output "db_credentials_secret_arn" { value = aws_secretsmanager_secret.db_credentials.arn }
output "jwt_key_secret_arn" { value = aws_secretsmanager_secret.jwt_key.arn }
output "kms_key_arn" { value = aws_kms_key.secrets.arn }
output "all_secret_arns" {
  value = [
    aws_secretsmanager_secret.db_credentials.arn,
    aws_secretsmanager_secret.jwt_key.arn
  ]
}
# Expose generated password for RDS module use (marked sensitive)
output "db_password" {
  value     = random_password.db_password.result
  sensitive = true
}
