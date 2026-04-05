# SampleTech — Terraform Remote State Bootstrap
# Run ONCE manually with local state, before any environment apply.
# Creates the S3 bucket + DynamoDB table used as the Terraform backend.
#
# Usage:
#   cd infra/terraform/bootstrap
#   terraform init
#   terraform apply
#
# After apply, all other environments can use the S3 backend.

terraform {
  required_version = ">= 1.7"
  required_providers {
    aws = { source = "hashicorp/aws", version = "~> 5.0" }
  }
  # Intentionally no S3 backend here — bootstrap uses local state
}

provider "aws" {
  region = var.aws_region
}

data "aws_caller_identity" "current" {}

locals {
  bucket_name = "sampletech-tfstate-${data.aws_caller_identity.current.account_id}"
}

# ── S3 Bucket for State ───────────────────────────────────────────────────────

resource "aws_s3_bucket" "tfstate" {
  bucket        = local.bucket_name
  force_destroy = false # Never accidentally destroy state

  tags = {
    Project   = "sampletech"
    ManagedBy = "terraform"
    Purpose   = "terraform-remote-state"
  }
}

resource "aws_s3_bucket_versioning" "tfstate" {
  bucket = aws_s3_bucket.tfstate.id
  versioning_configuration { status = "Enabled" }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "tfstate" {
  bucket = aws_s3_bucket.tfstate.id
  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "aws:kms"
    }
    bucket_key_enabled = true
  }
}

resource "aws_s3_bucket_public_access_block" "tfstate" {
  bucket                  = aws_s3_bucket.tfstate.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_lifecycle_configuration" "tfstate" {
  bucket = aws_s3_bucket.tfstate.id

  rule {
    id     = "expire-old-versions"
    status = "Enabled"
    filter {}
    noncurrent_version_expiration { noncurrent_days = 90 }
  }
}

# ── DynamoDB for State Locking ────────────────────────────────────────────────

resource "aws_dynamodb_table" "tfstate_lock" {
  name         = "sampletech-tfstate-lock"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "LockID"

  attribute {
    name = "LockID"
    type = "S"
  }

  server_side_encryption {
    enabled = true
  }

  point_in_time_recovery {
    enabled = true
  }

  tags = {
    Project   = "sampletech"
    ManagedBy = "terraform"
    Purpose   = "terraform-state-lock"
  }
}

variable "aws_region" {
  type    = string
  default = "us-east-1"
}

output "state_bucket_name" {
  value = aws_s3_bucket.tfstate.bucket
}

output "lock_table_name" {
  value = aws_dynamodb_table.tfstate_lock.name
}

output "backend_config" {
  value = <<-EOT
    # Add this to your environment's terraform block:
    backend "s3" {
      bucket         = "${aws_s3_bucket.tfstate.bucket}"
      region         = "${var.aws_region}"
      dynamodb_table = "${aws_dynamodb_table.tfstate_lock.name}"
      encrypt        = true
    }
  EOT
}
