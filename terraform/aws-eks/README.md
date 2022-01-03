# AWS Wavelength (Verizon 5G Edge) Terraform Module for EKS
Use this Terraform Module to get started building your first EKS cluster at the network edge on AWS Wavelength

## Overview
- VPC with subnets in AWS Wavelength Zone(s) and Parent Region (i.e., Availability Zones)
- EKS Cluster (control plane) deployed to Parent Region
- EKS Nodes deployed to Wavelength Zone(s)

## Getting Started
Visit the `variables.tf` file to edit your desired region (using the `region` variable) as well as the name of your EC2 key pair within that region (using the `worker_key_name` variable).

Next, edit the `wavelength_zones` variable to adjust the deployment of your EKS Node Groups. This metadata consists of a list of Wavelength Zones, including the following information:
- `availability_zone`: canonical name of the Wavelength Zone (e.g., us-east-1-wl1-bos-wlz-1)
- `availability_zone_id`: availability zone ID of the Wavelength Zone (e.g., use1-wl1-bos-wlz1)
- `worker_nodes`: desired number of nodes within that Wavelength Zone (e.g., 2)
- `cidr_block`: CIDR range for the corresponding Wavelength Zone subnet

Next, initialize Terraform within your working directory and create a preview of your deployment changes.
```
terraform init
terraform plan
```

Next, apply the configuration by running `terraform apply`.

You should see the completed configuration after roughly 12-15 minutes (should look something like this):
```
Apply complete! Resources: 55 added, 0 changed, 0 destroyed.
```

Next, to deploy cluster resources, you will need to direct kubectl to the appropriate config file. To do so, run the following:
```
export KUBECONFIG="kubeconfig_wavelength-eks-Cluster"
```
Note that if you have changed the name of your EKS cluster to `test-cluster`, as an example, adjust the command above to `kubeconfig_test-cluster`.


## Deploy Your First Workload
Navigate to the **demo** folder (`cd demo`) and run `./init.sh` to deploy your first workload. This workload runs a simple "Hello World" web container with the following configuration:
- Web deployment in `us-east-1-wl1-nyc-wlz-1` subnet; NodePort exposed on port 30007
- Web deployment in `us-east-1-wl1-bos-wlz-1` subnet; NodePort exposed on port 30008

To edit the Wavelength Zone node selector, edit lines 23 and 61 of `deployment.yaml`.

To destroy the workload, run `kubectl delete -f deployment.yaml` to destroy the resources.

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | >= 3.40.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_aws"></a> [aws](#provider\_aws) | 3.58.0 |

## Modules

| Name | Source | Version |
|------|--------|---------|
| <a name="module_eks_cluster"></a> [eks\_cluster](#module\_eks\_cluster) | terraform-aws-modules/eks/aws | n/a |

## Resources

| Name | Type |
|------|------|
| [aws_autoscaling_group.region_workers](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/autoscaling_group) | resource |
| [aws_autoscaling_group.wavelength_workers](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/autoscaling_group) | resource |
| [aws_ec2_carrier_gateway.tf_carrier_gateway](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ec2_carrier_gateway) | resource |
| [aws_iam_instance_profile.worker_role](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_instance_profile) | resource |
| [aws_iam_role.worker_role](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role) | resource |
| [aws_internet_gateway.tf_internet_gw](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/internet_gateway) | resource |
| [aws_launch_template.region_launch_template](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/launch_template) | resource |
| [aws_launch_template.worker_launch_template](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/launch_template) | resource |
| [aws_route.WLZ_route](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route) | resource |
| [aws_route.region_route](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route) | resource |
| [aws_route_table.WLZ_route_table](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table) | resource |
| [aws_route_table.region_route_table](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table) | resource |
| [aws_route_table_association.WLZ_route_associations](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table_association) | resource |
| [aws_route_table_association.region_route_associations](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table_association) | resource |
| [aws_security_group_rule.edge_confluent_1](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/security_group_rule) | resource |
| [aws_subnet.region_subnets](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/subnet) | resource |
| [aws_subnet.wavelength_subnets](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/subnet) | resource |
| [aws_vpc.tf_vpc](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/vpc) | resource |
| [aws_eks_cluster.cluster](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/eks_cluster) | data source |
| [aws_eks_cluster_auth.cluster](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/eks_cluster_auth) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_availability_zones"></a> [availability\_zones](#input\_availability\_zones) | This is the metadata for your parent region subnets. | `map` | <pre>{<br>  "az1": {<br>    "availability_zone_id": "use1-az1",<br>    "cidr_block": "10.0.1.0/24"<br>  },<br>  "az2": {<br>    "availability_zone_id": "use1-az2",<br>    "cidr_block": "10.0.2.0/24"<br>  }<br>}</pre> | no |
| <a name="input_cluster_name"></a> [cluster\_name](#input\_cluster\_name) | This is the name of your EKS cluster deployed to the parent region. | `string` | `"wavelength-eks-Cluster"` | no |
| <a name="input_node_group_s3_bucket_url"></a> [node\_group\_s3\_bucket\_url](#input\_node\_group\_s3\_bucket\_url) | This is the S3 object URL of the EKS node group with auto-attached Carrier IPs. | `string` | `"https://wavelengthtutorials.s3.amazonaws.com/wlz-eks-node-group.yaml"` | no |
| <a name="input_profile"></a> [profile](#input\_profile) | AWS Credentials Profile to use | `string` | `"default"` | no |
| <a name="input_region"></a> [region](#input\_region) | This is the AWS region. | `string` | `"us-east-1"` | no |
| <a name="input_wavelength_zones"></a> [wavelength\_zones](#input\_wavelength\_zones) | This is the metadata for your Wavelength Zone subnets. | `map` | <pre>{<br>  "atl": {<br>    "availability_zone": "us-east-1-wl1-atl-wlz-1",<br>    "availability_zone_id": "use1-wl1-atl-wlz1",<br>    "cidr_block": "10.0.13.0/24",<br>    "worker_nodes": 0<br>  },<br>  "bos": {<br>    "availability_zone": "us-east-1-wl1-bos-wlz-1",<br>    "availability_zone_id": "use1-wl1-bos-wlz1",<br>    "cidr_block": "10.0.11.0/24",<br>    "worker_nodes": 1<br>  },<br>  "mia": {<br>    "availability_zone": "us-east-1-wl1-mia-wlz-1",<br>    "availability_zone_id": "use1-wl1-mia-wlz1",<br>    "cidr_block": "10.0.14.0/24",<br>    "worker_nodes": 0<br>  },<br>  "nyc": {<br>    "availability_zone": "us-east-1-wl1-nyc-wlz-1",<br>    "availability_zone_id": "use1-wl1-nyc-wlz1",<br>    "cidr_block": "10.0.10.0/24",<br>    "worker_nodes": 1<br>  },<br>  "was": {<br>    "availability_zone": "us-east-1-wl1-was-wlz-1",<br>    "availability_zone_id": "use1-wl1-was-wlz1",<br>    "cidr_block": "10.0.12.0/24",<br>    "worker_nodes": 0<br>  }<br>}</pre> | no |
| <a name="input_worker_image_id"></a> [worker\_image\_id](#input\_worker\_image\_id) | This is the AMI ID for the EKS-optimized AMI. | `map(string)` | <pre>{<br>  "us-east-1": "ami-0193ebf9573ebc9f7",<br>  "us-west-2": "ami-0bb07d9c8d6ca41e8"<br>}</pre> | no |
| <a name="input_worker_instance_type"></a> [worker\_instance\_type](#input\_worker\_instance\_type) | This is the EC2 instance type for the EKS worker nodes. | `string` | `"t3.xlarge"` | no |
| <a name="input_worker_key_name"></a> [worker\_key\_name](#input\_worker\_key\_name) | This is your EC2 key name. | `string` | `"test_key"` | no |
| <a name="input_worker_nodegroup_name"></a> [worker\_nodegroup\_name](#input\_worker\_nodegroup\_name) | This is the name for the EKS worker nodes. | `string` | `"Wavelength-Node-Group"` | no |
| <a name="input_worker_volume_size"></a> [worker\_volume\_size](#input\_worker\_volume\_size) | This is the volume size (GB) of the EBS volumes for the EKS worker nodes. | `number` | `20` | no |

## Outputs

No outputs.
<!-- END_TF_DOCS -->