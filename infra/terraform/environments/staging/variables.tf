variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "vpc_cidr" {
  type    = string
  default = "10.1.0.0/16"
}

variable "acm_certificate_arn" {
  description = "ACM certificate ARN for the staging domain (must be in us-east-1 for CloudFront or same region as ALB)"
  type        = string
}

variable "staging_domain" {
  description = "Staging domain (e.g. staging.sampletech.io)"
  type        = string
  default     = "staging.sampletech.io"
}

variable "image_tag" {
  description = "Docker image tag to deploy"
  type        = string
  default     = "latest"
}

variable "alert_email_addresses" {
  description = "On-call email addresses for CloudWatch alarm notifications"
  type        = list(string)
  default     = []
}

variable "route53_zone_id" {
  description = "Route 53 hosted zone ID (leave empty to skip DNS record)"
  type        = string
  default     = ""
}
