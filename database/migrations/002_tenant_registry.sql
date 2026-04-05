-- SampleTech Insurance Platform — Tenant Registry
-- Migration: 002_tenant_registry
-- Adds the public.tenants table that tracks all provisioned tenant schemas.
-- This table lives in the public (platform) schema, NOT in any tenant schema.

BEGIN;

CREATE TABLE IF NOT EXISTS tenants (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name         VARCHAR(255) NOT NULL,
  schema_name  VARCHAR(100) UNIQUE NOT NULL,
  db_user      VARCHAR(100) NOT NULL,
  status       VARCHAR(20) NOT NULL DEFAULT 'active'
                 CHECK (status IN ('active', 'suspended', 'deprovisioning', 'deprovisioned')),
  plan         VARCHAR(50) NOT NULL DEFAULT 'standard',
  settings     JSONB NOT NULL DEFAULT '{}',
  created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_tenants_status ON tenants(status);
CREATE INDEX idx_tenants_schema_name ON tenants(schema_name);

COMMENT ON TABLE tenants IS
  'Platform-level tenant registry. One row per provisioned tenant. '
  'The schema_name column maps 1:1 to a PostgreSQL schema: tenant_<id>.';

COMMIT;
