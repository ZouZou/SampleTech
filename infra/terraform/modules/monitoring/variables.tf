variable "name" { type = string }
variable "environment" { type = string }

variable "api_log_group" { type = string }
variable "alb_arn_suffix" { type = string }
variable "ecs_cluster_name" { type = string }
variable "ecs_api_service_name" { type = string }
variable "rds_instance_id" { type = string }

variable "alert_email_addresses" {
  description = "Email addresses to notify for CloudWatch alarms"
  type        = list(string)
  default     = []
}

variable "tags" { type = map(string); default = {} }
