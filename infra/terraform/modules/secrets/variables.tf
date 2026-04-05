variable "name" { type = string }
variable "db_host" { type = string }
variable "db_name" { type = string; default = "sampletech" }
variable "db_username" { type = string; default = "sampletech_admin" }

variable "reader_role_arns" {
  description = "IAM role ARNs that may read these secrets (ECS execution roles)"
  type        = list(string)
}

variable "recovery_window_days" {
  description = "Days before a deleted secret is permanently removed"
  type        = number
  default     = 30
}

variable "tags" { type = map(string); default = {} }
