# Confluent 5G Edge Module
Get started on Confluent with Verizon 5G Edge today using AWS Wavelength, Confluent for Kubernetes, and any Verizon 4G, 5G, or CAT-M IoT device.

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | >= 3.40.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_aws"></a> [aws](#provider\_aws) | 3.63.0 |
| <a name="provider_helm"></a> [helm](#provider\_helm) | 2.3.0 |
| <a name="provider_kubernetes"></a> [kubernetes](#provider\_kubernetes) | 2.6.0 |
| <a name="provider_local"></a> [local](#provider\_local) | 2.1.0 |

## Modules

| Name | Source | Version |
|------|--------|---------|
| <a name="module_eks_cluster"></a> [eks\_cluster](#module\_eks\_cluster) | terraform-aws-modules/eks/aws | n/a |

## Resources

| Name | Type |
|------|------|
| [aws_autoscaling_group.workers](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/autoscaling_group) | resource |
| [aws_ec2_carrier_gateway.tf_carrier_gateway](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ec2_carrier_gateway) | resource |
| [aws_iam_instance_profile.worker_role](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_instance_profile) | resource |
| [aws_iam_role.worker_role](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role) | resource |
| [aws_internet_gateway.tf_internet_gw](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/internet_gateway) | resource |
| [aws_launch_template.worker_launch_template](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/launch_template) | resource |
| [aws_route.WLZ_route](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route) | resource |
| [aws_route.region_route](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route) | resource |
| [aws_route_table.WLZ_route_table](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table) | resource |
| [aws_route_table.region_route_table](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table) | resource |
| [aws_route_table_association.WLZ_route_association](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table_association) | resource |
| [aws_route_table_association.region_route_association](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table_association) | resource |
| [aws_route_table_association.region_route_association_2](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table_association) | resource |
| [aws_security_group_rule.edge_confluent_1](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/security_group_rule) | resource |
| [aws_subnet.tf_region_subnet](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/subnet) | resource |
| [aws_subnet.tf_region_subnet_2](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/subnet) | resource |
| [aws_subnet.tf_wl_subnet](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/subnet) | resource |
| [aws_vpc.tf_vpc](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/vpc) | resource |
| [helm_release.confluent_for_kubernetes](https://registry.terraform.io/providers/hashicorp/helm/latest/docs/resources/release) | resource |
| [kubernetes_namespace.confluent](https://registry.terraform.io/providers/hashicorp/kubernetes/latest/docs/resources/namespace) | resource |
| [local_file.manifest](https://registry.terraform.io/providers/hashicorp/local/latest/docs/resources/file) | resource |
| [aws_eks_cluster.cluster](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/eks_cluster) | data source |
| [aws_eks_cluster_auth.cluster](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/eks_cluster_auth) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_availability_zone_1"></a> [availability\_zone\_1](#input\_availability\_zone\_1) | This is the first Availability Zone for the EKS control plane. | `string` | `"us-east-1a"` | no |
| <a name="input_availability_zone_2"></a> [availability\_zone\_2](#input\_availability\_zone\_2) | This is the second Availability Zone for the EKS control plane. | `string` | `"us-east-1b"` | no |
| <a name="input_cfk_version"></a> [cfk\_version](#input\_cfk\_version) | This is the version of Confluent for Kubernetes. | `string` | `"2.1.0"` | no |
| <a name="input_cluster_name"></a> [cluster\_name](#input\_cluster\_name) | n/a | `string` | `"wavelength"` | no |
| <a name="input_cp_version"></a> [cp\_version](#input\_cp\_version) | This is the version of the Confluent Platform. | `string` | `"6.2.1"` | no |
| <a name="input_domain"></a> [domain](#input\_domain) | n/a | `string` | `"lab.local"` | no |
| <a name="input_node_group_s3_bucket_url"></a> [node\_group\_s3\_bucket\_url](#input\_node\_group\_s3\_bucket\_url) | This is the S3 object URL of the EKS node group with auto-attached Carrier IPs. | `string` | `"https://wavelengthtutorials.s3.amazonaws.com/wlz-eks-node-group.yaml"` | no |
| <a name="input_profile"></a> [profile](#input\_profile) | AWS Credentials Profile to use | `string` | `"default"` | no |
| <a name="input_region"></a> [region](#input\_region) | This is the AWS region. | `string` | `"us-east-1"` | no |
| <a name="input_require_imdsv2"></a> [require\_imdsv2](#input\_require\_imdsv2) | n/a | `bool` | `true` | no |
| <a name="input_wavelength_zone"></a> [wavelength\_zone](#input\_wavelength\_zone) | This is the Wavelength Zone to deploy the EKS node group. | `string` | `"us-east-1-wl1-nyc-wlz-1"` | no |
| <a name="input_worker_image_id"></a> [worker\_image\_id](#input\_worker\_image\_id) | Create AMI Mapping for Wavelength Zone (EKS 1.21) | `map(string)` | <pre>{<br>  "us-east-1": "ami-0193ebf9573ebc9f7",<br>  "us-west-2": "ami-0bb07d9c8d6ca41e8"<br>}</pre> | no |
| <a name="input_worker_instance_type"></a> [worker\_instance\_type](#input\_worker\_instance\_type) | This is the EC2 instance type for the EKS worker nodes. | `string` | `"t3.xlarge"` | no |
| <a name="input_worker_key_name"></a> [worker\_key\_name](#input\_worker\_key\_name) | This is your EC2 key name. | `string` | `"test_key"` | no |
| <a name="input_worker_nodegroup_name"></a> [worker\_nodegroup\_name](#input\_worker\_nodegroup\_name) | This is the AMI for the EKS worker nodes. | `string` | `"Wavelength-Node-Group"` | no |
| <a name="input_worker_volume_size"></a> [worker\_volume\_size](#input\_worker\_volume\_size) | This is the volume size (GB) of the EBS volumes for the EKS worker nodes. | `number` | `20` | no |

## Outputs

No outputs.
<!-- END_TF_DOCS -->