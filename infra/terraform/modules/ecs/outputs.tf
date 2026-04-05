output "cluster_name" { value = aws_ecs_cluster.this.name }
output "cluster_arn" { value = aws_ecs_cluster.this.arn }
output "execution_role_arn" { value = aws_iam_role.execution.arn }
output "task_role_arn" { value = aws_iam_role.task.arn }
output "api_service_name" { value = aws_ecs_service.api.name }
output "frontend_service_name" { value = aws_ecs_service.frontend.name }
output "api_log_group" { value = aws_cloudwatch_log_group.api.name }
output "frontend_log_group" { value = aws_cloudwatch_log_group.frontend.name }
