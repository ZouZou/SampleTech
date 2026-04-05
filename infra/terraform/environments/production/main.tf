# SampleTech Insurance Platform — Production Environment
# HA configuration: 3-AZ NAT gateways, Multi-AZ RDS, FARGATE (no spot).
# All resources have deletion protection enabled.

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
    key            = "production/terraform.tfstate"
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
  name        = "sampletech-production"
  environment = "production"

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
  single_nat_gateway = false # One NAT per AZ for HA
  tags               = local.tags
}

# ── Secrets ───────────────────────────────────────────────────────────────────

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

  instance_class           = "db.r7g.large"
  allocated_storage_gb     = 100
  max_allocated_storage_gb = 1000

  db_password = module.secrets.db_password
  multi_az    = true # Production: Multi-AZ for HA

  backup_retention_days    = 30    # SOC 2 requirement
  deletion_protection      = true
  skip_final_snapshot      = false

  performance_insights_retention_days = 731 # 2 years for compliance

  alarm_sns_arn = module.monitoring.alert_sns_arn
  tags          = local.tags
}

# ── ECR ───────────────────────────────────────────────────────────────────────

module "ecr" {
  source = "../../modules/ecr"

  name                         = "sampletech" # Same registry as staging
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
  deletion_protection   = true
  log_retention_days    = 365
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

  aspnet_environment = "Production"
  frontend_origin    = "https://${var.production_domain}"

  api_cpu           = 1024
  api_memory        = 2048
  api_desired_count = 2
  api_max_count     = 20

  frontend_cpu           = 512
  frontend_memory        = 1024
  frontend_desired_count = 2

  use_spot           = false # FARGATE (on-demand) — no spot interruptions in prod
  log_retention_days = 90

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

# ── Route 53 ──────────────────────────────────────────────────────────────────

resource "aws_route53_record" "production" {
  count   = var.route53_zone_id != "" ? 1 : 0
  zone_id = var.route53_zone_id
  name    = var.production_domain
  type    = "A"

  alias {
    name                   = module.alb.alb_dns_name
    zone_id                = module.alb.alb_zone_id
    evaluate_target_health = true
  }
}
