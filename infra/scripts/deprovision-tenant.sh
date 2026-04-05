#!/usr/bin/env bash
# SampleTech Insurance Platform — Tenant Deprovisioning Script
#
# Safely removes a tenant's schema and associated resources.
# Requires explicit --confirm flag to prevent accidents.
# Creates a backup snapshot before deletion (can be suppressed with --no-backup).
#
# Usage:
#   ./deprovision-tenant.sh \
#     --tenant-id  acme-corp \
#     --db-host    <rds-endpoint> \
#     --confirm
#
# This is a DESTRUCTIVE operation. All tenant data will be permanently deleted.

set -euo pipefail

DB_PORT=5432
DB_NAME="sampletech"
DB_ADMIN_USER="sampletech_admin"
CONFIRM=false
NO_BACKUP=false
BACKUP_DIR="/tmp/sampletech-tenant-backups"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --tenant-id)      TENANT_ID="$2";      shift 2 ;;
    --db-host)        DB_HOST="$2";        shift 2 ;;
    --db-port)        DB_PORT="$2";        shift 2 ;;
    --db-name)        DB_NAME="$2";        shift 2 ;;
    --db-admin-user)  DB_ADMIN_USER="$2";  shift 2 ;;
    --backup-dir)     BACKUP_DIR="$2";     shift 2 ;;
    --confirm)        CONFIRM=true;        shift ;;
    --no-backup)      NO_BACKUP=true;      shift ;;
    *) echo "ERROR: Unknown flag: $1" >&2; exit 1 ;;
  esac
done

require() { [[ -n "${!1:-}" ]] || { echo "ERROR: --${1//_/-} is required" >&2; exit 1; }; }
require TENANT_ID
require DB_HOST

SCHEMA_NAME="tenant_$(echo "$TENANT_ID" | tr '-' '_')"
DB_USER="${SCHEMA_NAME}_user"

if [[ "$CONFIRM" != "true" ]]; then
  echo "ERROR: This operation will PERMANENTLY DELETE all data for tenant '$TENANT_ID'."
  echo "       Pass --confirm to proceed."
  exit 1
fi

echo "==> Deprovisioning tenant: $TENANT_ID"
echo "    Schema: $SCHEMA_NAME"
echo "    DB user: $DB_USER"

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

# ── Verify schema exists ──────────────────────────────────────────────────────
EXISTING=$(psql_admin -c "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = '$SCHEMA_NAME';" 2>/dev/null || echo "0")
if [[ "$EXISTING" != "1" ]]; then
  echo "ERROR: Schema '$SCHEMA_NAME' not found. Nothing to deprovision." >&2
  exit 2
fi

# ── Backup before deletion ────────────────────────────────────────────────────
if [[ "$NO_BACKUP" != "true" ]]; then
  echo "==> Creating backup of schema $SCHEMA_NAME..."
  mkdir -p "$BACKUP_DIR"
  BACKUP_FILE="$BACKUP_DIR/${TENANT_ID}-$(date +%Y%m%d-%H%M%S).sql"

  PGPASSWORD="${PGPASSWORD:-}" pg_dump \
    --host="$DB_HOST" \
    --port="$DB_PORT" \
    --username="$DB_ADMIN_USER" \
    --schema="$SCHEMA_NAME" \
    --no-owner \
    --no-acl \
    "$DB_NAME" > "$BACKUP_FILE"

  echo "    Backup written to: $BACKUP_FILE"
fi

# ── Mark tenant inactive in registry ─────────────────────────────────────────
echo "==> Marking tenant inactive in public.tenants..."
psql_admin -c "UPDATE public.tenants SET status = 'deprovisioning', updated_at = now() WHERE schema_name = '$SCHEMA_NAME';"

# ── Drop schema ───────────────────────────────────────────────────────────────
echo "==> Dropping schema $SCHEMA_NAME (CASCADE)..."
psql_admin -c "DROP SCHEMA IF EXISTS \"$SCHEMA_NAME\" CASCADE;"

# ── Remove DB user ────────────────────────────────────────────────────────────
echo "==> Removing DB user $DB_USER..."
psql_admin <<-SQL
  REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA "$SCHEMA_NAME" FROM "$DB_USER";
  DROP ROLE IF EXISTS "$DB_USER";
SQL

# ── Remove from public registry ───────────────────────────────────────────────
echo "==> Removing tenant from public.tenants registry..."
psql_admin -c "DELETE FROM public.tenants WHERE schema_name = '$SCHEMA_NAME';"

# ── Archive Secrets Manager secret ───────────────────────────────────────────
echo "==> Archiving Secrets Manager secret..."
SECRET_NAME="sampletech/${TENANT_ID}/db-user"
if aws secretsmanager describe-secret --secret-id "$SECRET_NAME" &>/dev/null; then
  aws secretsmanager delete-secret \
    --secret-id "$SECRET_NAME" \
    --recovery-window-in-days 30
  echo "    Secret scheduled for deletion in 30 days: $SECRET_NAME"
fi

echo ""
echo "==> Tenant '$TENANT_ID' deprovisioned."
if [[ "$NO_BACKUP" != "true" ]]; then
  echo "    Backup retained at: $BACKUP_FILE"
fi
