# SampleTech Insurance Platform — Monitoring Module
# CloudWatch dashboards, log metric filters, and alerting.
# Covers API errors, latency, ECS health, and RDS.

terraform {
  required_providers {
    aws = { source = "hashicorp/aws", version = "~> 5.0" }
  }
}

# ── SNS Topic for Alerts ──────────────────────────────────────────────────────

resource "aws_sns_topic" "alerts" {
  name              = "${var.name}-alerts"
  kms_master_key_id = "alias/aws/sns"
  tags              = var.tags
}

resource "aws_sns_topic_subscription" "email" {
  count     = length(var.alert_email_addresses)
  topic_arn = aws_sns_topic.alerts.arn
  protocol  = "email"
  endpoint  = var.alert_email_addresses[count.index]
}

# ── Log Metric Filters ────────────────────────────────────────────────────────

resource "aws_cloudwatch_log_metric_filter" "api_5xx" {
  name           = "${var.name}-api-5xx-errors"
  log_group_name = var.api_log_group
  pattern        = "{ $.StatusCode >= 500 }"

  metric_transformation {
    name      = "Api5xxErrors"
    namespace = "SampleTech/${var.environment}"
    value     = "1"
    unit      = "Count"
  }
}

resource "aws_cloudwatch_log_metric_filter" "api_4xx" {
  name           = "${var.name}-api-4xx-errors"
  log_group_name = var.api_log_group
  pattern        = "{ $.StatusCode >= 400 && $.StatusCode < 500 }"

  metric_transformation {
    name      = "Api4xxErrors"
    namespace = "SampleTech/${var.environment}"
    value     = "1"
    unit      = "Count"
  }
}

resource "aws_cloudwatch_log_metric_filter" "api_latency" {
  name           = "${var.name}-api-latency"
  log_group_name = var.api_log_group
  pattern        = "{ $.ElapsedMs = * }"

  metric_transformation {
    name      = "ApiResponseTimeMs"
    namespace = "SampleTech/${var.environment}"
    value     = "$.ElapsedMs"
    unit      = "Milliseconds"
  }
}

resource "aws_cloudwatch_log_metric_filter" "auth_failures" {
  name           = "${var.name}-auth-failures"
  log_group_name = var.api_log_group
  pattern        = "{ $.StatusCode = 401 || $.StatusCode = 403 }"

  metric_transformation {
    name      = "AuthFailures"
    namespace = "SampleTech/${var.environment}"
    value     = "1"
    unit      = "Count"
  }
}

# ── CloudWatch Alarms ─────────────────────────────────────────────────────────

resource "aws_cloudwatch_metric_alarm" "api_error_rate_high" {
  alarm_name          = "${var.name}-api-5xx-rate-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "Api5xxErrors"
  namespace           = "SampleTech/${var.environment}"
  period              = 60
  statistic           = "Sum"
  threshold           = 10
  treat_missing_data  = "notBreaching"
  alarm_description   = "API 5xx error rate exceeds 10/min — possible service outage"
  alarm_actions       = [aws_sns_topic.alerts.arn]
  ok_actions          = [aws_sns_topic.alerts.arn]

  tags = var.tags
}

resource "aws_cloudwatch_metric_alarm" "api_latency_p95" {
  alarm_name          = "${var.name}-api-latency-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 3
  metric_name         = "ApiResponseTimeMs"
  namespace           = "SampleTech/${var.environment}"
  period              = 60
  extended_statistic  = "p95"
  threshold           = 2000 # 2s p95 — alert before SLA breach
  treat_missing_data  = "notBreaching"
  alarm_description   = "API p95 latency exceeds 2s"
  alarm_actions       = [aws_sns_topic.alerts.arn]

  tags = var.tags
}

resource "aws_cloudwatch_metric_alarm" "auth_failures_spike" {
  alarm_name          = "${var.name}-auth-failures-spike"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 1
  metric_name         = "AuthFailures"
  namespace           = "SampleTech/${var.environment}"
  period              = 300
  statistic           = "Sum"
  threshold           = 50
  treat_missing_data  = "notBreaching"
  alarm_description   = "High rate of auth failures — possible credential stuffing attack"
  alarm_actions       = [aws_sns_topic.alerts.arn]

  tags = var.tags
}

# ALB target 5xx
resource "aws_cloudwatch_metric_alarm" "alb_5xx" {
  alarm_name          = "${var.name}-alb-5xx"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "HTTPCode_ELB_5XX_Count"
  namespace           = "AWS/ApplicationELB"
  period              = 60
  statistic           = "Sum"
  threshold           = 5
  treat_missing_data  = "notBreaching"
  alarm_description   = "ALB is returning 5xx errors"
  alarm_actions       = [aws_sns_topic.alerts.arn]

  dimensions = {
    LoadBalancer = var.alb_arn_suffix
  }

  tags = var.tags
}

# ECS service running count
resource "aws_cloudwatch_metric_alarm" "ecs_api_running_tasks" {
  alarm_name          = "${var.name}-ecs-api-running-low"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = 1
  metric_name         = "RunningTaskCount"
  namespace           = "ECS/ContainerInsights"
  period              = 60
  statistic           = "Minimum"
  threshold           = 1
  treat_missing_data  = "breaching"
  alarm_description   = "API ECS service has no running tasks"
  alarm_actions       = [aws_sns_topic.alerts.arn]

  dimensions = {
    ClusterName = var.ecs_cluster_name
    ServiceName = var.ecs_api_service_name
  }

  tags = var.tags
}

# ── CloudWatch Dashboard ──────────────────────────────────────────────────────

resource "aws_cloudwatch_dashboard" "main" {
  dashboard_name = "${var.name}-operations"

  dashboard_body = jsonencode({
    widgets = [
      {
        type   = "metric"
        x      = 0; y = 0; width = 12; height = 6
        properties = {
          title  = "API Error Rate"
          period = 60
          stat   = "Sum"
          metrics = [
            ["SampleTech/${var.environment}", "Api5xxErrors", { label = "5xx Errors", color = "#d62728" }],
            ["SampleTech/${var.environment}", "Api4xxErrors", { label = "4xx Errors", color = "#ff7f0e" }]
          ]
          view  = "timeSeries"
          yAxis = { left = { min = 0 } }
        }
      },
      {
        type   = "metric"
        x      = 12; y = 0; width = 12; height = 6
        properties = {
          title  = "API Latency (p50 / p95 / p99)"
          period = 60
          metrics = [
            ["SampleTech/${var.environment}", "ApiResponseTimeMs", { stat = "p50", label = "p50" }],
            ["SampleTech/${var.environment}", "ApiResponseTimeMs", { stat = "p95", label = "p95", color = "#ff7f0e" }],
            ["SampleTech/${var.environment}", "ApiResponseTimeMs", { stat = "p99", label = "p99", color = "#d62728" }]
          ]
          view  = "timeSeries"
          yAxis = { left = { min = 0 } }
        }
      },
      {
        type   = "metric"
        x      = 0; y = 6; width = 8; height = 6
        properties = {
          title  = "ECS API — CPU & Memory"
          period = 60
          metrics = [
            ["ECS/ContainerInsights", "CpuUtilized", "ClusterName", var.ecs_cluster_name, "ServiceName", var.ecs_api_service_name, { stat = "Average", label = "CPU %" }],
            ["ECS/ContainerInsights", "MemoryUtilized", "ClusterName", var.ecs_cluster_name, "ServiceName", var.ecs_api_service_name, { stat = "Average", label = "Memory MB", yAxis = "right" }]
          ]
          view  = "timeSeries"
        }
      },
      {
        type   = "metric"
        x      = 8; y = 6; width = 8; height = 6
        properties = {
          title  = "RDS — CPU & Connections"
          period = 60
          metrics = [
            ["AWS/RDS", "CPUUtilization", "DBInstanceIdentifier", var.rds_instance_id, { stat = "Average", label = "CPU %" }],
            ["AWS/RDS", "DatabaseConnections", "DBInstanceIdentifier", var.rds_instance_id, { stat = "Average", label = "Connections", yAxis = "right" }]
          ]
          view = "timeSeries"
        }
      },
      {
        type   = "metric"
        x      = 16; y = 6; width = 8; height = 6
        properties = {
          title  = "Auth Failures (Possible Attack)"
          period = 300
          stat   = "Sum"
          metrics = [["SampleTech/${var.environment}", "AuthFailures", { color = "#9467bd" }]]
          view  = "timeSeries"
          yAxis = { left = { min = 0 } }
        }
      }
    ]
  })
}
