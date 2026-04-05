output "alb_arn" { value = aws_lb.this.arn }
output "alb_dns_name" { value = aws_lb.this.dns_name }
output "alb_zone_id" { value = aws_lb.this.zone_id }
output "api_target_group_arn" { value = aws_lb_target_group.api.arn }
output "frontend_target_group_arn" { value = aws_lb_target_group.frontend.arn }
output "https_listener_arn" { value = aws_lb_listener.https.arn }
output "alb_logs_bucket" { value = aws_s3_bucket.alb_logs.bucket }
