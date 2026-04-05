# Staging environment — non-secret values only.
# Secrets (ACM cert ARN, etc.) are passed via GitHub Actions secrets / CI env.

aws_region     = "us-east-1"
vpc_cidr       = "10.1.0.0/16"
staging_domain = "staging.sampletech.io"

alert_email_addresses = [
  # "devops@sampletech.io"
]
