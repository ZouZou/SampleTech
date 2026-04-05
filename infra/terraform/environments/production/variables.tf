variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "vpc_cidr" {
  type    = string
  default = "10.0.0.0/16"
}

variable "acm_certificate_arn" {
  type = string
}

variable "production_domain" {
  type    = string
  default = "app.sampletech.io"
}

variable "image_tag" {
  type    = string
  default = "latest"
}

variable "alert_email_addresses" {
  type    = list(string)
  default = []
}

variable "route53_zone_id" {
  type    = string
  default = ""
}
