output "endpoint" {
  description = "RDS instance endpoint (host:port)"
  value       = "${aws_db_instance.this.address}:${aws_db_instance.this.port}"
}

output "address" {
  description = "RDS instance hostname"
  value       = aws_db_instance.this.address
}

output "port" {
  description = "RDS instance port"
  value       = aws_db_instance.this.port
}

output "db_name" {
  description = "Initial database name"
  value       = aws_db_instance.this.db_name
}

output "kms_key_arn" {
  description = "KMS key ARN used for RDS encryption"
  value       = aws_kms_key.rds.arn
}

output "instance_id" {
  description = "RDS instance identifier"
  value       = aws_db_instance.this.identifier
}
