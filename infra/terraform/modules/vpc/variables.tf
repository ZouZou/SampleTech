variable "name" {
  description = "Name prefix for all resources"
  type        = string
}

variable "vpc_cidr" {
  description = "CIDR block for the VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "single_nat_gateway" {
  description = "Use a single NAT gateway (cost-saving for staging; set false for production HA)"
  type        = bool
  default     = false
}

variable "flow_log_retention_days" {
  description = "CloudWatch Logs retention for VPC flow logs (SOC 2 minimum: 365)"
  type        = number
  default     = 365
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
