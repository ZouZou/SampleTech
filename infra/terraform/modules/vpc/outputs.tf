output "vpc_id" {
  description = "VPC ID"
  value       = aws_vpc.this.id
}

output "vpc_cidr" {
  description = "VPC CIDR block"
  value       = aws_vpc.this.cidr_block
}

output "public_subnet_ids" {
  description = "Public subnet IDs (one per AZ)"
  value       = aws_subnet.public[*].id
}

output "private_subnet_ids" {
  description = "Private (app) subnet IDs (one per AZ)"
  value       = aws_subnet.private[*].id
}

output "isolated_subnet_ids" {
  description = "Isolated (DB) subnet IDs (one per AZ)"
  value       = aws_subnet.isolated[*].id
}

output "alb_security_group_id" {
  description = "Security group ID for the Application Load Balancer"
  value       = aws_security_group.alb.id
}

output "app_security_group_id" {
  description = "Security group ID for app-tier containers"
  value       = aws_security_group.app.id
}

output "db_security_group_id" {
  description = "Security group ID for the RDS instance"
  value       = aws_security_group.db.id
}
