provider "helm" {
  kubernetes {
    host                   = data.aws_eks_cluster.cluster.endpoint
    cluster_ca_certificate = base64decode(data.aws_eks_cluster.cluster.certificate_authority[0].data)
    token                  = data.aws_eks_cluster_auth.cluster.token
  }
}

resource "helm_release" "datadog_monitor" {
  name       = "datadog-monitor"
  repository = "https://helm.datadoghq.com"
  chart      = "datadog"

  namespace  = "datadog"
  create_namespace = true

  values = [
    file("my-values.yaml")
  ]
  
  set {
    name = "datadog.apiKey"
    value = var.dd_api
  }
}


# Configure the Datadog provider
provider "datadog" {
  api_key = var.dd_api
  app_key = var.dd_app
}


resource "datadog_integration_aws" "main" {
  account_id  = var.accountId
  role_name   = "DatadogAWSIntegrationRole"
}

resource "aws_iam_role" "datadog" {
  name               = "DatadogAWSIntegrationRole"
  assume_role_policy = jsonencode(
    {
       Version = "2012-10-17"
      Statement = [
        {
          Effect = "Allow"
          Principal = {
            AWS = "arn:aws:iam::464622532012:root"
          }
          Action = "sts:AssumeRole"
          Condition = {
            StringEquals = {
              "sts:ExternalId" = datadog_integration_aws.main.external_id
            }
          }
        }
      ]
    }
  )

  inline_policy {
    name = "datadog-policy"
    policy = file("datadog-policy.json")
  }
}