# HAProxy on Verizon 5G Edge with AWS Wavelength
Use this Terraform Module to get started building your first active-active load balancer at the network edge on AWS Wavelength

## Overview

- **Infrastructure**
  - VPC with single subnet in AWS Wavelength Zone
  - Active-active load balancer with auto-attached Carrier IP addresses
  - Auto scaling group of web fleet without Carrier IP
- **Software**
  - HAProxy Enterprise Edition (HAPEE) 2.2 from AWS Marketplace


## Installation

Install the terraform backend using `terraform init` and then deploy the configuration.

```bash
terraform apply -auto-approve
```

**NOTE:**ensure that the AWS provider is **at least** `v3.29.0` to ensure support for AWS Wavelength.

Next, be sure to populate the `hapee-config.sh` file with your AWS access key and secret access key. This is critical for the HAPEE Data Plane API to extract tags for your EC2 instances to auto-register targets to your load balancer.
To seamlessly populate your keys, populate your keys in `secret.tfvars` file and run `./config.sh` or manually ediy lines 148 and 164 of `hapee-config.sh`.

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | 3.51.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_aws"></a> [aws](#provider\_aws) | 3.51.0 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [aws_autoscaling_group.asg_web](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/autoscaling_group) | resource |
| [aws_ec2_carrier_gateway.tf_carrier_gw](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/ec2_carrier_gateway) | resource |
| [aws_eip.hapee_node_eip1](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/eip) | resource |
| [aws_iam_instance_profile.eip_instance_profile](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/iam_instance_profile) | resource |
| [aws_iam_role.eip_role](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/iam_role) | resource |
| [aws_iam_role_policy.eip_role_policy](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/iam_role_policy) | resource |
| [aws_instance.hapee_node](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/instance) | resource |
| [aws_internet_gateway.tf_internet_gw](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/internet_gateway) | resource |
| [aws_launch_template.web_launch_template](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/launch_template) | resource |
| [aws_route.region_route](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route) | resource |
| [aws_route_table.region_route_table](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route_table) | resource |
| [aws_route_table.wavelength_route_table](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route_table) | resource |
| [aws_route_table_association.region_route_association](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route_table_association) | resource |
| [aws_route_table_association.wavelength_route_association](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route_table_association) | resource |
| [aws_security_group.hapee_node_sg](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/security_group) | resource |
| [aws_security_group.web_node_sg](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/security_group) | resource |
| [aws_subnet.tf_region_subnet](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/subnet) | resource |
| [aws_subnet.tf_wl_subnet](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/subnet) | resource |
| [aws_vpc.tf_vpc](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/vpc) | resource |
| [aws_ami.hapee_aws_amis](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/data-sources/ami) | data source |
| [aws_iam_policy_document.eip_policy](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/data-sources/iam_policy_document) | data source |
| [aws_iam_policy_document.instance_assume_role_policy](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/data-sources/iam_policy_document) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_amis_linux2"></a> [amis\_linux2](#input\_amis\_linux2) | Create AMI Mapping for Amazon Linux 2 | `map(string)` | <pre>{<br>  "us-east-1": "ami-0fc7dad598dd37e11",<br>  "us-west-2": "ami-0045a9e5f2af86401"<br>}</pre> | no |
| <a name="input_aws_hapee_instance_type"></a> [aws\_hapee\_instance\_type](#input\_aws\_hapee\_instance\_type) | Default AWS instance type for HAPEE node; must be t3.medium, t3.xlarge, or r5.2xlarge to support AWS Wavelength. | `string` | `"t3.medium"` | no |
| <a name="input_aws_region"></a> [aws\_region](#input\_aws\_region) | This is the AWS region for your deployment; must be either us-east-1 or us-west-2 to support Wavelength Zones. | `string` | `"us-east-1"` | no |
| <a name="input_aws_web_instance_type"></a> [aws\_web\_instance\_type](#input\_aws\_web\_instance\_type) | Default AWS instance type for HAPEE node; must be t3.medium, t3.xlarge, or r5.2xlarge to support AWS Wavelength. | `string` | `"t3.medium"` | no |
| <a name="input_backend_size"></a> [backend\_size](#input\_backend\_size) | Size of backend Auto Scaling group | `number` | `1` | no |
| <a name="input_key_name"></a> [key\_name](#input\_key\_name) | SSH key pair to use for AWS instances. | `string` | `"test_key"` | no |
| <a name="input_region_subnet"></a> [region\_subnet](#input\_region\_subnet) | This is the AWS Availability Zone for your deployment. | `string` | `"us-east-1a"` | no |
| <a name="input_wavelength_subnet"></a> [wavelength\_subnet](#input\_wavelength\_subnet) | This is the AWS Wavelength Zone for your deployment. | `string` | `"us-east-1-wl1-nyc-wlz-1"` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_node1_interface_allocs_id"></a> [node1\_interface\_allocs\_id](#output\_node1\_interface\_allocs\_id) | n/a |
| <a name="output_node1_interface_public_IPs"></a> [node1\_interface\_public\_IPs](#output\_node1\_interface\_public\_IPs) | n/a |
| <a name="output_node_primary_interface_IDs"></a> [node\_primary\_interface\_IDs](#output\_node\_primary\_interface\_IDs) | n/a |
| <a name="output_nodes_private_IPs"></a> [nodes\_private\_IPs](#output\_nodes\_private\_IPs) | n/a |
<!-- END_TF_DOCS -->
