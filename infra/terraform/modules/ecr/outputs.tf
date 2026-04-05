output "repository_urls" {
  description = "Map of service name to ECR repository URL"
  value       = { for k, v in aws_ecr_repository.this : k => v.repository_url }
}

output "registry_id" {
  description = "AWS account ID that owns the ECR registries"
  value       = values(aws_ecr_repository.this)[0].registry_id
}

output "kms_key_arn" {
  value = aws_kms_key.ecr.arn
}
