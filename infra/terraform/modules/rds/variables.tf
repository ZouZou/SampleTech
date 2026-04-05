variable "name" {
  description = "Name prefix for all resources"
  type        = string
}

variable "subnet_ids" {
  description = "List of isolated subnet IDs for the DB subnet group"
  type        = list(string)
}

variable "security_group_id" {
  description = "Security group ID to attach to the RDS instance"
  type        = string
}

variable "instance_class" {
  description = "RDS instance class"
  type        = string
  default     = "db.t3.medium"
}

variable "db_name" {
  description = "Initial database name"
  type        = string
  default     = "sampletech"
}

variable "db_username" {
  description = "Master database username"
  type        = string
  default     = "sampletech_admin"
}

variable "db_password" {
  description = "Master database password (sourced from Secrets Manager in practice)"
  type        = string
  sensitive   = true
}

variable "allocated_storage_gb" {
  description = "Initial storage allocation in GB"
  type        = number
  default     = 20
}

variable "max_allocated_storage_gb" {
  description = "Maximum autoscaling storage in GB"
  type        = number
  default     = 200
}

variable "multi_az" {
  description = "Enable Multi-AZ deployment (required for production)"
  type        = bool
  default     = false
}

variable "backup_retention_days" {
  description = "Automated backup retention period in days (SOC 2 minimum: 7)"
  type        = number
  default     = 7
}

variable "deletion_protection" {
  description = "Prevent accidental deletion"
  type        = bool
  default     = true
}

variable "skip_final_snapshot" {
  description = "Skip final snapshot on deletion (false for production)"
  type        = bool
  default     = false
}

variable "performance_insights_retention_days" {
  description = "Performance Insights retention period (7 or 731)"
  type        = number
  default     = 7
}

variable "max_connections_alarm_threshold" {
  description = "Number of DB connections that triggers an alarm"
  type        = number
  default     = 100
}

variable "alarm_sns_arn" {
  description = "SNS topic ARN for CloudWatch alarms (null to disable)"
  type        = string
  default     = null
}

variable "tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default     = {}
}
