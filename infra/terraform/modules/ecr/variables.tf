variable "name" {
  type = string
}

variable "ecs_task_execution_role_arns" {
  description = "List of IAM role ARNs allowed to pull from ECR"
  type        = list(string)
  default     = []
}

variable "tags" {
  type    = map(string)
  default = {}
}
