#!/usr/bin/env bash
# SampleTech Insurance Platform — Tenant Provisioning Script
#
# Creates a new schema-per-tenant in PostgreSQL:
#   1. Creates the tenant schema  (tenant_<tenant_id>)
#   2. Creates a dedicated DB user with access scoped to that schema
#   3. Runs all migration SQL files against the tenant schema
#   4. Inserts a record into the public.tenants registry
#   5. Seeds default admin user for the tenant
#
# Usage:
#   ./provision-tenant.sh \
#     --tenant-id  acme-corp \
#     --tenant-name "Acme Insurance Corp" \
#     --admin-email admin@acme.com \
#     --db-host     <rds-endpoint> \
#     --db-port     5432 \
#     --db-name     sampletech \
#     --db-admin-user sampletech_admin \
#     [--migrations-dir ../../database/migrations]
#
# Env vars (alternative to flags):
#   PGPASSWORD — master DB password (never pass on command line)
#   TENANT_DB_PASSWORD — per-tenant user password (generated if not set)
#
# Exit codes: 0 = success, 1 = validation error, 2 = DB error

set -euo pipefail

# ── Defaults ──────────────────────────────────────────────────────────────────
DB_PORT=5432
DB_NAME="sampletech"
DB_ADMIN_USER="sampletech_admin"
MIGRATIONS_DIR="$(dirname "$0")/../../database/migrations"

# ── Argument Parsing ──────────────────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
  case "$1" in
    --tenant-id)       TENANT_ID="$2";       shift 2 ;;
    --tenant-name)     TENANT_NAME="$2";     shift 2 ;;
    --admin-email)     ADMIN_EMAIL="$2";     shift 2 ;;
    --db-host)         DB_HOST="$2";         shift 2 ;;
    --db-port)         DB_PORT="$2";         shift 2 ;;
    --db-name)         DB_NAME="$2";         shift 2 ;;
    --db-admin-user)   DB_ADMIN_USER="$2";   shift 2 ;;
    --migrations-dir)  MIGRATIONS_DIR="$2";  shift 2 ;;
    *) echo "ERROR: Unknown flag: $1" >&2; exit 1 ;;
  esac
done

# ── Validation ────────────────────────────────────────────────────────────────
require() { [[ -n "${!1:-}" ]] || { echo "ERROR: --${1//_/-} is required" >&2; exit 1; }; }
require TENANT_ID
require TENANT_NAME
require ADMIN_EMAIL
require DB_HOST

# Tenant ID must be lowercase alphanumeric + hyphens, max 60 chars
if ! [[ "$TENANT_ID" =~ ^[a-z0-9][a-z0-9-]{0,58}[a-z0-9]$|^[a-z0-9]$ ]]; then
  echo "ERROR: tenant-id must be lowercase alphanumeric with hyphens (e.g. acme-corp)" >&2
  exit 1
fi

# Derive safe schema name (replace hyphens with underscores for Postgres identifier)
SCHEMA_NAME="tenant_$(echo "$TENANT_ID" | tr '-' '_')"
DB_USER="${SCHEMA_NAME}_user"

echo "==> Provisioning tenant: $TENANT_ID"
echo "    Schema:  $SCHEMA_NAME"
echo "    DB user: $DB_USER"
echo "    DB host: $DB_HOST:$DB_PORT/$DB_NAME"

# ── Generate per-tenant DB password if not provided ───────────────────────────
if [[ -z "${TENANT_DB_PASSWORD:-}" ]]; then
  TENANT_DB_PASSWORD="$(openssl rand -base64 32 | tr -d '/+=')"
  echo "    Generated DB password for $DB_USER (store in Secrets Manager)"
fi

# ── psql helper ───────────────────────────────────────────────────────────────
psql_admin() {
  psql \
    --host="$DB_HOST" \
    --port="$DB_PORT" \
    --dbname="$DB_NAME" \
    --username="$DB_ADMIN_USER" \
    --no-password \
    --tuples-only \
    --no-align \
    "$@"
}

psql_admin_file() {
  psql \
    --host="$DB_HOST" \
    --port="$DB_PORT" \
    --dbname="$DB_NAME" \
    --username="$DB_ADMIN_USER" \
    --no-password \
    "$@"
}

# ── Guard: check if tenant already exists ────────────────────────────────────
echo "==> Checking for existing tenant..."
EXISTING=$(psql_admin -c "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = '$SCHEMA_NAME';" 2>/dev/null || echo "0")
if [[ "$EXISTING" == "1" ]]; then
  echo "ERROR: Schema '$SCHEMA_NAME' already exists. Use reprovision-tenant.sh to update." >&2
  exit 2
fi

# ── Step 1: Create schema ─────────────────────────────────────────────────────
echo "==> Creating schema $SCHEMA_NAME..."
psql_admin -c "CREATE SCHEMA IF NOT EXISTS \"$SCHEMA_NAME\";"

# ── Step 2: Create tenant-scoped DB user ──────────────────────────────────────
echo "==> Creating DB user $DB_USER..."
psql_admin <<-SQL
  DO \$\$
  BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '$DB_USER') THEN
      CREATE ROLE "$DB_USER" LOGIN PASSWORD '$TENANT_DB_PASSWORD';
    END IF;
  END
  \$\$;

  -- Grant usage on schema only — no cross-tenant data access
  GRANT USAGE ON SCHEMA "$SCHEMA_NAME" TO "$DB_USER";
  GRANT CREATE ON SCHEMA "$SCHEMA_NAME" TO "$DB_USER";
  ALTER ROLE "$DB_USER" SET search_path TO "$SCHEMA_NAME";

  -- Default privileges: future tables/sequences in this schema are accessible
  ALTER DEFAULT PRIVILEGES IN SCHEMA "$SCHEMA_NAME"
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO "$DB_USER";
  ALTER DEFAULT PRIVILEGES IN SCHEMA "$SCHEMA_NAME"
    GRANT USAGE, SELECT ON SEQUENCES TO "$DB_USER";
SQL

# ── Step 3: Run migrations in tenant schema ───────────────────────────────────
echo "==> Running migrations in schema $SCHEMA_NAME..."
MIGRATIONS_DIR="$(realpath "$MIGRATIONS_DIR")"

if [[ ! -d "$MIGRATIONS_DIR" ]]; then
  echo "ERROR: Migrations directory not found: $MIGRATIONS_DIR" >&2
  exit 1
fi

# Create migration tracking table in the tenant schema
psql_admin <<-SQL
  CREATE TABLE IF NOT EXISTS "$SCHEMA_NAME".schema_migrations (
    version     VARCHAR(255) PRIMARY KEY,
    applied_at  TIMESTAMPTZ NOT NULL DEFAULT now()
  );
SQL

for migration_file in $(ls "$MIGRATIONS_DIR"/*.sql 2>/dev/null | sort); do
  version=$(basename "$migration_file" .sql)

  already_applied=$(psql_admin -c \
    "SELECT COUNT(*) FROM \"$SCHEMA_NAME\".schema_migrations WHERE version = '$version';" \
    2>/dev/null || echo "0")

  if [[ "$already_applied" == "1" ]]; then
    echo "    [SKIP] $version (already applied)"
    continue
  fi

  echo "    [APPLY] $version"

  # Run migration inside the tenant schema search path
  psql_admin_file \
    -v search_path="\"$SCHEMA_NAME\"" \
    -c "SET search_path TO \"$SCHEMA_NAME\";" \
    -f "$migration_file" \
    --single-transaction

  psql_admin -c \
    "INSERT INTO \"$SCHEMA_NAME\".schema_migrations (version) VALUES ('$version');"
done

# ── Step 4: Register tenant in public registry ────────────────────────────────
echo "==> Registering tenant in public.tenants..."
psql_admin <<-SQL
  INSERT INTO public.tenants (
    id,
    name,
    schema_name,
    db_user,
    status,
    created_at,
    updated_at
  ) VALUES (
    '$(python3 -c "import uuid; print(uuid.uuid4())" 2>/dev/null || uuidgen | tr '[:upper:]' '[:lower:]')',
    '$TENANT_NAME',
    '$SCHEMA_NAME',
    '$DB_USER',
    'active',
    now(),
    now()
  )
  ON CONFLICT (schema_name) DO NOTHING;
SQL

# ── Step 5: Seed default admin user for tenant ────────────────────────────────
echo "==> Seeding default admin user: $ADMIN_EMAIL..."
TEMP_PASSWORD="$(openssl rand -base64 16)"
# Password hash: bcrypt via psql extension or use a placeholder for rotation
# In production, the admin should reset on first login.
psql_admin <<-SQL
  SET search_path TO "$SCHEMA_NAME";

  INSERT INTO users (
    email, password_hash, role, first_name, last_name, is_active
  ) VALUES (
    '$ADMIN_EMAIL',
    -- Placeholder hash — user must reset password on first login
    'MUST_RESET_ON_FIRST_LOGIN',
    'admin',
    'Tenant',
    'Admin',
    true
  )
  ON CONFLICT (email) DO NOTHING;
SQL

# ── Step 6: Store tenant DB password in Secrets Manager ──────────────────────
echo "==> Storing DB credentials in AWS Secrets Manager..."
SECRET_NAME="sampletech/${TENANT_ID}/db-user"

if aws secretsmanager describe-secret --secret-id "$SECRET_NAME" &>/dev/null; then
  echo "    [UPDATE] Secret $SECRET_NAME already exists — updating value"
  aws secretsmanager put-secret-value \
    --secret-id "$SECRET_NAME" \
    --secret-string "{\"username\":\"$DB_USER\",\"password\":\"$TENANT_DB_PASSWORD\",\"schema\":\"$SCHEMA_NAME\"}"
else
  echo "    [CREATE] Creating secret $SECRET_NAME"
  aws secretsmanager create-secret \
    --name "$SECRET_NAME" \
    --description "Per-tenant DB credentials for $TENANT_NAME ($TENANT_ID)" \
    --secret-string "{\"username\":\"$DB_USER\",\"password\":\"$TENANT_DB_PASSWORD\",\"schema\":\"$SCHEMA_NAME\"}"
fi

echo ""
echo "==> Tenant '$TENANT_ID' provisioned successfully!"
echo "    Schema:       $SCHEMA_NAME"
echo "    DB user:      $DB_USER"
echo "    Admin email:  $ADMIN_EMAIL"
echo "    Secret:       $SECRET_NAME"
echo ""
echo "    IMPORTANT: Force password reset for $ADMIN_EMAIL before handing off to tenant."
