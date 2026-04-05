# SampleTech Insurance Platform — Staging Environment
# Cost-optimized: single NAT gateway, single-AZ RDS, FARGATE_SPOT where possible.
# Mirrors production config to catch environment-parity bugs before they hit prod.

terraform {
  required_version = ">= 1.7"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  backend "s3" {
    bucket         = "sampletech-tfstate"
    key            = "staging/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "sampletech-tfstate-lock"
    encrypt        = true
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = local.tags
  }
}

locals {
  name        = "sampletech-staging"
  environment = "staging"

  tags = {
    Project     = "sampletech"
    Environment = local.environment
    ManagedBy   = "terraform"
  }
}

# ── VPC ───────────────────────────────────────────────────────────────────────

module "vpc" {
  source = "../../modules/vpc"

  name               = local.name
  vpc_cidr           = var.vpc_cidr
  single_nat_gateway = true # Cost saving — acceptable for staging
  tags               = local.tags
}

# ── Secrets ───────────────────────────────────────────────────────────────────
# Bootstrap phase: secrets created before ECS (execution role ARN not yet known).
# Use a two-pass apply: first apply creates secrets + ECS roles,
# second pass wires execution role ARN into secret policies.

module "secrets" {
  source = "../../modules/secrets"

  name             = local.name
  db_host          = module.rds.address
  db_name          = "sampletech"
  db_username      = "sampletech_admin"
  reader_role_arns = [module.ecs.execution_role_arn]
  tags             = local.tags

  depends_on = [module.rds, module.ecs]
}

# ── RDS ───────────────────────────────────────────────────────────────────────

module "rds" {
  source = "../../modules/rds"

  name              = local.name
  subnet_ids        = module.vpc.isolated_subnet_ids
  security_group_id = module.vpc.db_security_group_id

  instance_class           = "db.t3.micro"
  allocated_storage_gb     = 20
  max_allocated_storage_gb = 100

  db_password = module.secrets.db_password
  multi_az    = false # Staging: single-AZ for cost

  backup_retention_days    = 7
  deletion_protection      = false # Allow teardown of staging
  skip_final_snapshot      = true

  alarm_sns_arn = module.monitoring.alert_sns_arn
  tags          = local.tags
}

# ── ECR ───────────────────────────────────────────────────────────────────────

module "ecr" {
  source = "../../modules/ecr"

  name                         = "sampletech" # Shared with production — one registry
  ecs_task_execution_role_arns = [module.ecs.execution_role_arn]
  tags                         = local.tags
}

# ── ALB ───────────────────────────────────────────────────────────────────────

module "alb" {
  source = "../../modules/alb"

  name                  = local.name
  vpc_id                = module.vpc.vpc_id
  public_subnet_ids     = module.vpc.public_subnet_ids
  alb_security_group_id = module.vpc.alb_security_group_id
  acm_certificate_arn   = var.acm_certificate_arn
  deletion_protection   = false
  log_retention_days    = 90
  tags                  = local.tags
}

# ── ECS ───────────────────────────────────────────────────────────────────────

module "ecs" {
  source = "../../modules/ecs"

  name       = local.name
  aws_region = var.aws_region

  private_subnet_ids    = module.vpc.private_subnet_ids
  app_security_group_id = module.vpc.app_security_group_id

  api_image      = module.ecr.repository_urls["api"]
  frontend_image = module.ecr.repository_urls["frontend"]
  image_tag      = var.image_tag

  api_target_group_arn      = module.alb.api_target_group_arn
  frontend_target_group_arn = module.alb.frontend_target_group_arn

  db_connection_string_secret_arn = module.secrets.db_credentials_secret_arn
  jwt_key_secret_arn              = module.secrets.jwt_key_secret_arn
  secret_arns                     = module.secrets.all_secret_arns
  secrets_kms_key_arn             = module.secrets.kms_key_arn

  aspnet_environment = "Staging"
  frontend_origin    = "https://${var.staging_domain}"

  api_cpu            = 512
  api_memory         = 1024
  api_desired_count  = 1
  api_max_count      = 4

  frontend_cpu           = 256
  frontend_memory        = 512
  frontend_desired_count = 1

  use_spot           = true # FARGATE_SPOT for cost savings in staging
  log_retention_days = 30

  tags = local.tags
}

# ── Monitoring ────────────────────────────────────────────────────────────────

module "monitoring" {
  source = "../../modules/monitoring"

  name        = local.name
  environment = local.environment

  api_log_group        = module.ecs.api_log_group
  alb_arn_suffix       = module.alb.alb_arn
  ecs_cluster_name     = module.ecs.cluster_name
  ecs_api_service_name = module.ecs.api_service_name
  rds_instance_id      = module.rds.instance_id

  alert_email_addresses = var.alert_email_addresses
  tags                  = local.tags
}

# ── Route 53 (optional — skip if managing DNS externally) ────────────────────

resource "aws_route53_record" "staging" {
  count   = var.route53_zone_id != "" ? 1 : 0
  zone_id = var.route53_zone_id
  name    = var.staging_domain
  type    = "A"

  alias {
    name                   = module.alb.alb_dns_name
    zone_id                = module.alb.alb_zone_id
    evaluate_target_health = true
  }
}
