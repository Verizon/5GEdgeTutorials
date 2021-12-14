
# Two resources depend on this:
# EKS cluster (which needs to grant permissions to this role)
# EC2 EKS worker nodes (which use these roles to access EKS)
resource "aws_iam_role" "worker_role" {
  name = "${var.cluster_name}-wl-workers"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Sid    = ""
        Principal = {
          Service = "ec2.amazonaws.com"
        }
      },
    ]
  })

  # Todo: switch to partition
  managed_policy_arns = [
    "arn:aws:iam::aws:policy/AmazonEKSWorkerNodePolicy",
    "arn:aws:iam::aws:policy/AmazonEKS_CNI_Policy",
    "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly",
  ]
}

resource "aws_iam_instance_profile" "worker_role" {
  name = "${var.cluster_name}-wl-workers"
  role = aws_iam_role.worker_role.name
}