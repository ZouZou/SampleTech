variable "name" { type = string }
variable "aws_region" { type = string }

variable "private_subnet_ids" { type = list(string) }
variable "app_security_group_id" { type = string }

variable "api_image" { type = string }
variable "frontend_image" { type = string }
variable "image_tag" { type = string; default = "latest" }

variable "api_target_group_arn" { type = string }
variable "frontend_target_group_arn" { type = string }

variable "db_connection_string_secret_arn" { type = string }
variable "jwt_key_secret_arn" { type = string }
variable "secret_arns" { type = list(string) }
variable "secrets_kms_key_arn" { type = string }

variable "aspnet_environment" {
  type    = string
  default = "Production"
}

variable "frontend_origin" { type = string }

variable "api_cpu" { type = number; default = 512 }
variable "api_memory" { type = number; default = 1024 }
variable "api_desired_count" { type = number; default = 2 }
variable "api_max_count" { type = number; default = 10 }

variable "frontend_cpu" { type = number; default = 256 }
variable "frontend_memory" { type = number; default = 512 }
variable "frontend_desired_count" { type = number; default = 2 }

variable "use_spot" {
  description = "Use FARGATE_SPOT for cost savings (not recommended for production)"
  type        = bool
  default     = false
}

variable "log_retention_days" { type = number; default = 90 }

variable "tags" { type = map(string); default = {} }
