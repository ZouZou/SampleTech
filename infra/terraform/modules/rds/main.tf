# SampleTech Insurance Platform — RDS Module
# PostgreSQL 16, schema-per-tenant multi-tenancy.
# Production: Multi-AZ + encrypted + enhanced monitoring.
# Staging: single-AZ, same config otherwise for parity.

terraform {
  required_providers {
    aws = { source = "hashicorp/aws", version = "~> 5.0" }
  }
}

# ── Subnet Group ─────────────────────────────────────────────────────────────

resource "aws_db_subnet_group" "this" {
  name        = "${var.name}-db-subnet-group"
  subnet_ids  = var.subnet_ids
  description = "RDS subnet group for ${var.name} (isolated subnets)"
  tags        = var.tags
}

# ── Parameter Group ───────────────────────────────────────────────────────────

resource "aws_db_parameter_group" "this" {
  name   = "${var.name}-pg16"
  family = "postgres16"

  # Force SSL connections (SOC 2 requirement)
  parameter {
    name  = "rds.force_ssl"
    value = "1"
  }

  # Enable query logging for audit trail
  parameter {
    name  = "log_connections"
    value = "1"
  }

  parameter {
    name  = "log_disconnections"
    value = "1"
  }

  parameter {
    name  = "log_duration"
    value = "0"
  }

  # Log slow queries (>1s) for performance monitoring
  parameter {
    name  = "log_min_duration_statement"
    value = "1000"
  }

  parameter {
    name  = "log_statement"
    value = "ddl"
  }

  tags = var.tags
}

# ── KMS key for RDS encryption (SOC 2 — encryption at rest) ──────────────────

resource "aws_kms_key" "rds" {
  description             = "${var.name} RDS encryption key"
  deletion_window_in_days = 30
  enable_key_rotation     = true
  tags                    = merge(var.tags, { Name = "${var.name}-rds-kms" })
}

resource "aws_kms_alias" "rds" {
  name          = "alias/${var.name}-rds"
  target_key_id = aws_kms_key.rds.key_id
}

# ── Enhanced Monitoring IAM Role ──────────────────────────────────────────────

resource "aws_iam_role" "rds_monitoring" {
  name = "${var.name}-rds-monitoring-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = { Service = "monitoring.rds.amazonaws.com" }
    }]
  })

  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "rds_monitoring" {
  role       = aws_iam_role.rds_monitoring.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonRDSEnhancedMonitoringRole"
}

# ── RDS Instance ──────────────────────────────────────────────────────────────

resource "aws_db_instance" "this" {
  identifier = var.name

  engine         = "postgres"
  engine_version = "16.3"
  instance_class = var.instance_class

  allocated_storage     = var.allocated_storage_gb
  max_allocated_storage = var.max_allocated_storage_gb
  storage_type          = "gp3"
  storage_encrypted     = true
  kms_key_id            = aws_kms_key.rds.arn

  db_name  = var.db_name
  username = var.db_username
  password = var.db_password

  db_subnet_group_name   = aws_db_subnet_group.this.name
  vpc_security_group_ids = [var.security_group_id]
  parameter_group_name   = aws_db_parameter_group.this.name
  publicly_accessible    = false

  # High Availability
  multi_az = var.multi_az

  # Backups (SOC 2 — availability)
  backup_retention_period   = var.backup_retention_days
  backup_window             = "03:00-04:00"
  maintenance_window        = "sun:04:00-sun:05:00"
  copy_tags_to_snapshot     = true
  delete_automated_backups  = false

  # Protection
  deletion_protection      = var.deletion_protection
  skip_final_snapshot      = var.skip_final_snapshot
  final_snapshot_identifier = var.skip_final_snapshot ? null : "${var.name}-final-snapshot"

  # Monitoring
  enabled_cloudwatch_logs_exports = ["postgresql", "upgrade"]
  monitoring_interval             = 60
  monitoring_role_arn             = aws_iam_role.rds_monitoring.arn
  performance_insights_enabled    = true
  performance_insights_kms_key_id = aws_kms_key.rds.arn
  performance_insights_retention_period = var.performance_insights_retention_days

  # Auto minor version upgrades (controlled upgrade window)
  auto_minor_version_upgrade = true

  tags = var.tags
}

# ── CloudWatch Alarms ─────────────────────────────────────────────────────────

resource "aws_cloudwatch_metric_alarm" "rds_cpu" {
  alarm_name          = "${var.name}-rds-cpu-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "CPUUtilization"
  namespace           = "AWS/RDS"
  period              = 300
  statistic           = "Average"
  threshold           = 80
  alarm_description   = "RDS CPU utilization exceeds 80%"
  alarm_actions       = var.alarm_sns_arn != null ? [var.alarm_sns_arn] : []

  dimensions = { DBInstanceIdentifier = aws_db_instance.this.identifier }
  tags       = var.tags
}

resource "aws_cloudwatch_metric_alarm" "rds_storage" {
  alarm_name          = "${var.name}-rds-storage-low"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = 1
  metric_name         = "FreeStorageSpace"
  namespace           = "AWS/RDS"
  period              = 300
  statistic           = "Average"
  threshold           = 5368709120 # 5 GB in bytes
  alarm_description   = "RDS free storage below 5 GB"
  alarm_actions       = var.alarm_sns_arn != null ? [var.alarm_sns_arn] : []

  dimensions = { DBInstanceIdentifier = aws_db_instance.this.identifier }
  tags       = var.tags
}

resource "aws_cloudwatch_metric_alarm" "rds_connections" {
  alarm_name          = "${var.name}-rds-connections-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "DatabaseConnections"
  namespace           = "AWS/RDS"
  period              = 60
  statistic           = "Average"
  threshold           = var.max_connections_alarm_threshold
  alarm_description   = "RDS connection count is unusually high"
  alarm_actions       = var.alarm_sns_arn != null ? [var.alarm_sns_arn] : []

  dimensions = { DBInstanceIdentifier = aws_db_instance.this.identifier }
  tags       = var.tags
}
