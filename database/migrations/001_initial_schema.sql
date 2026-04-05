-- SampleTech Insurance Platform — Initial Schema
-- Migration: 001_initial_schema
-- Created: 2026-04-03

BEGIN;

-- Users (all roles)
CREATE TABLE users (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email       VARCHAR(255) UNIQUE NOT NULL,
  password_hash TEXT NOT NULL,
  role        VARCHAR(20) NOT NULL CHECK (role IN ('admin', 'underwriter', 'agent', 'broker', 'client')),
  first_name  VARCHAR(100) NOT NULL,
  last_name   VARCHAR(100) NOT NULL,
  is_active   BOOLEAN NOT NULL DEFAULT true,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Policies
CREATE TABLE policies (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  policy_number   VARCHAR(50) UNIQUE NOT NULL,
  client_id       UUID NOT NULL REFERENCES users(id),
  underwriter_id  UUID REFERENCES users(id),
  agent_id        UUID REFERENCES users(id),
  policy_type     VARCHAR(50) NOT NULL,  -- e.g. 'auto', 'home', 'life', 'commercial'
  status          VARCHAR(20) NOT NULL DEFAULT 'draft'
                    CHECK (status IN ('draft', 'quoted', 'active', 'cancelled', 'expired', 'renewed')),
  premium_cents   BIGINT NOT NULL DEFAULT 0,
  effective_date  DATE,
  expiry_date     DATE,
  terms           JSONB NOT NULL DEFAULT '{}',
  created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Claims
CREATE TABLE claims (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  claim_number    VARCHAR(50) UNIQUE NOT NULL,
  policy_id       UUID NOT NULL REFERENCES policies(id),
  claimant_id     UUID NOT NULL REFERENCES users(id),
  handler_id      UUID REFERENCES users(id),
  status          VARCHAR(20) NOT NULL DEFAULT 'submitted'
                    CHECK (status IN ('submitted', 'under_review', 'approved', 'denied', 'paid', 'closed')),
  incident_date   DATE NOT NULL,
  description     TEXT NOT NULL,
  amount_claimed_cents BIGINT NOT NULL DEFAULT 0,
  amount_approved_cents BIGINT,
  created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Audit log — append-only, never update/delete
CREATE TABLE audit_log (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  actor_id    UUID REFERENCES users(id),
  action      VARCHAR(100) NOT NULL,
  entity_type VARCHAR(50) NOT NULL,
  entity_id   UUID NOT NULL,
  old_data    JSONB,
  new_data    JSONB,
  ip_address  INET,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Indexes
CREATE INDEX idx_policies_client_id ON policies(client_id);
CREATE INDEX idx_policies_status ON policies(status);
CREATE INDEX idx_claims_policy_id ON claims(policy_id);
CREATE INDEX idx_claims_status ON claims(status);
CREATE INDEX idx_audit_log_entity ON audit_log(entity_type, entity_id);
CREATE INDEX idx_audit_log_actor ON audit_log(actor_id);

COMMIT;
