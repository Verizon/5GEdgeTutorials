# AWS Wavelength (Verizon 5G Edge) Terraform Module for EC2
Use this Terraform Module to get started building your first EC2 instance at the network edge on AWS Wavelength

## Overview
  - VPC with subnet in AWS Wavelength Zone and Parent Region (i.e., Availability Zone)
  - Bastion host deployed to parent region (optionally)
  - Web app deployed to Wavelength Zone

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
| [aws_ec2_carrier_gateway.tf_carrier_gateway](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/ec2_carrier_gateway) | resource |
| [aws_eip.tf-wavelength-ip](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/eip) | resource |
| [aws_eip_association.eip_assoc](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/eip_association) | resource |
| [aws_instance.bastion_host_instance](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/instance) | resource |
| [aws_instance.edge_instance](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/instance) | resource |
| [aws_internet_gateway.tf_internet_gw](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/internet_gateway) | resource |
| [aws_route.WLZ_route](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route) | resource |
| [aws_route.region_route](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route) | resource |
| [aws_route_table.WLZ_route_table](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route_table) | resource |
| [aws_route_table.region_route_table](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route_table) | resource |
| [aws_route_table_association.region_route_association](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route_table_association) | resource |
| [aws_route_table_association.wavelength_route_association](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/route_table_association) | resource |
| [aws_security_group.edge_security_group](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/security_group) | resource |
| [aws_security_group.parent_security_group](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/security_group) | resource |
| [aws_subnet.tf_region_subnet](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/subnet) | resource |
| [aws_subnet.tf_wl_subnet](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/subnet) | resource |
| [aws_vpc.tf-vpc](https://registry.terraform.io/providers/hashicorp/aws/3.51.0/docs/resources/vpc) | resource |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_availability_zone"></a> [availability\_zone](#input\_availability\_zone) | The Availability Zone for your EC2 instance. | `string` | `"us-east-1a"` | no |
| <a name="input_bastion_ami"></a> [bastion\_ami](#input\_bastion\_ami) | Create AMI Mapping for Bastion Host (Amazon Linux 2) | `map(string)` | <pre>{<br>  "us-east-1": "ami-02e136e904f3da870",<br>  "us-west-2": "ami-013a129d325529d4d"<br>}</pre> | no |
| <a name="input_bastion_host"></a> [bastion\_host](#input\_bastion\_host) | Indicate whether bastion host is desired to connect to Wavelength Zone instance. | `bool` | `true` | no |
| <a name="input_instance_type"></a> [instance\_type](#input\_instance\_type) | n/a | `string` | `"t3.medium"` | no |
| <a name="input_key_pair"></a> [key\_pair](#input\_key\_pair) | The name of your EC2 key pair. | `string` | `"test_key"` | no |
| <a name="input_profile"></a> [profile](#input\_profile) | The name of your AWS crendential profile, found most often in ~/.aws/credentials | `string` | `"default"` | no |
| <a name="input_region"></a> [region](#input\_region) | The AWS region to deploy your Wavelength configuration. | `string` | `"us-east-1"` | no |
| <a name="input_wavelength_ami"></a> [wavelength\_ami](#input\_wavelength\_ami) | Create AMI Mapping for Wavelength Zone (Amazon Linux 2) | `map(string)` | <pre>{<br>  "us-east-1": "ami-0fc7dad598dd37e11",<br>  "us-west-2": "ami-0045a9e5f2af86401"<br>}</pre> | no |
| <a name="input_wavelength_zone"></a> [wavelength\_zone](#input\_wavelength\_zone) | The Wavelength Zone for your EC2 instance. | `string` | `"us-east-1-wl1-bos-wlz-1"` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_bastion_private_ip"></a> [bastion\_private\_ip](#output\_bastion\_private\_ip) | The Private IP address of the bastion host in the parent region |
| <a name="output_bastion_public_ip"></a> [bastion\_public\_ip](#output\_bastion\_public\_ip) | The Public IP address of the bastion host in the parent region |
| <a name="output_wavelength_carrier_ip"></a> [wavelength\_carrier\_ip](#output\_wavelength\_carrier\_ip) | The Carrier IP address of the edge instance in the Wavelength Zone |
| <a name="output_wavelength_private_ip"></a> [wavelength\_private\_ip](#output\_wavelength\_private\_ip) | The Private IP address of the edge instance in the Wavelength Zone |
<!-- END_TF_DOCS -->
