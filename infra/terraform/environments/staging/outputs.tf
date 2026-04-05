output "alb_dns_name" {
  description = "ALB DNS name — point your staging CNAME here"
  value       = module.alb.alb_dns_name
}

output "ecr_api_url" {
  description = "ECR URL for API image"
  value       = module.ecr.repository_urls["api"]
}

output "ecr_frontend_url" {
  description = "ECR URL for frontend image"
  value       = module.ecr.repository_urls["frontend"]
}

output "ecs_cluster_name" {
  value = module.ecs.cluster_name
}

output "api_service_name" {
  value = module.ecs.api_service_name
}

output "frontend_service_name" {
  value = module.ecs.frontend_service_name
}

output "dashboard_name" {
  value = module.monitoring.dashboard_name
}
