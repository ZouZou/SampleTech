# SampleTech Insurance Platform — Secrets Module
# Stores sensitive configuration in AWS Secrets Manager.
# KMS-encrypted. Rotation-ready. Zero secrets in source code or env files.

terraform {
  required_providers {
    aws  = { source = "hashicorp/aws", version = "~> 5.0" }
    random = { source = "hashicorp/random", version = "~> 3.0" }
  }
}

# ── KMS key for Secrets Manager ───────────────────────────────────────────────

resource "aws_kms_key" "secrets" {
  description             = "${var.name} Secrets Manager encryption key"
  deletion_window_in_days = 30
  enable_key_rotation     = true
  tags                    = merge(var.tags, { Name = "${var.name}-secrets-kms" })
}

resource "aws_kms_alias" "secrets" {
  name          = "alias/${var.name}-secrets"
  target_key_id = aws_kms_key.secrets.key_id
}

# ── DB Credentials ────────────────────────────────────────────────────────────

resource "random_password" "db_password" {
  length           = 32
  special          = true
  override_special = "!#$%^&*()-_=+[]{}<>"
}

resource "aws_secretsmanager_secret" "db_credentials" {
  name                    = "${var.name}/db/credentials"
  description             = "RDS master credentials for ${var.name}"
  kms_key_id              = aws_kms_key.secrets.arn
  recovery_window_in_days = var.recovery_window_days
  tags                    = var.tags
}

resource "aws_secretsmanager_secret_version" "db_credentials" {
  secret_id = aws_secretsmanager_secret.db_credentials.id

  secret_string = jsonencode({
    username          = var.db_username
    password          = random_password.db_password.result
    connection_string = "Host=${var.db_host};Port=5432;Database=${var.db_name};Username=${var.db_username};Password=${random_password.db_password.result};SSL Mode=Require;Trust Server Certificate=false"
  })

  lifecycle {
    ignore_changes = [secret_string]
  }
}

# ── JWT Signing Key ───────────────────────────────────────────────────────────

resource "random_password" "jwt_key" {
  length  = 64
  special = false # Base64-safe characters only
}

resource "aws_secretsmanager_secret" "jwt_key" {
  name                    = "${var.name}/app/jwt-key"
  description             = "JWT signing key for ${var.name} API"
  kms_key_id              = aws_kms_key.secrets.arn
  recovery_window_in_days = var.recovery_window_days
  tags                    = var.tags
}

resource "aws_secretsmanager_secret_version" "jwt_key" {
  secret_id     = aws_secretsmanager_secret.jwt_key.id
  secret_string = jsonencode({ jwt_key = random_password.jwt_key.result })

  lifecycle {
    ignore_changes = [secret_string]
  }
}

# ── IAM Policy: allow specific roles to read secrets ─────────────────────────

resource "aws_secretsmanager_secret_policy" "db_credentials" {
  secret_arn = aws_secretsmanager_secret.db_credentials.arn

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowECSExecution"
        Effect = "Allow"
        Principal = {
          AWS = var.reader_role_arns
        }
        Action   = ["secretsmanager:GetSecretValue"]
        Resource = "*"
      },
      {
        Sid    = "DenyPublicAccess"
        Effect = "Deny"
        Principal = { AWS = "*" }
        Action    = "secretsmanager:*"
        Resource  = "*"
        Condition = {
          StringNotEquals = {
            "aws:PrincipalArn" = var.reader_role_arns
          }
          Bool = {
            "aws:ViaAWSService" = "false"
          }
        }
      }
    ]
  })
}

resource "aws_secretsmanager_secret_policy" "jwt_key" {
  secret_arn = aws_secretsmanager_secret.jwt_key.arn

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowECSExecution"
        Effect = "Allow"
        Principal = {
          AWS = var.reader_role_arns
        }
        Action   = ["secretsmanager:GetSecretValue"]
        Resource = "*"
      }
    ]
  })
}
