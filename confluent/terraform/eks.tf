data "aws_eks_cluster" "cluster" {
  name = module.eks_cluster.cluster_id
}

data "aws_eks_cluster_auth" "cluster" {
  name = module.eks_cluster.cluster_id
}

provider "kubernetes" {
  host                   = data.aws_eks_cluster.cluster.endpoint
  cluster_ca_certificate = base64decode(data.aws_eks_cluster.cluster.certificate_authority.0.data)
  token                  = data.aws_eks_cluster_auth.cluster.token

  experiments {
    manifest_resource = true
  }
}

module "eks_cluster" {
  source          = "terraform-aws-modules/eks/aws"
  cluster_name    = var.cluster_name
  cluster_version = "1.21"
  version         = "17.24.0"
  subnets = [for subnet in aws_subnet.region_subnets: subnet.id]

  vpc_id = aws_vpc.tf_vpc.id

  manage_aws_auth              = true
  manage_cluster_iam_resources = true
  manage_worker_iam_resources  = true

  worker_create_security_group                       = true
  worker_create_cluster_primary_security_group_rules = true

  write_kubeconfig = true

  map_roles = [
    {
      rolearn  = aws_iam_role.worker_role.arn
      username = "system:node:{{EC2PrivateDNSName}}"
      groups   = ["system:bootstrappers", "system:nodes"]

    }
  ]
}