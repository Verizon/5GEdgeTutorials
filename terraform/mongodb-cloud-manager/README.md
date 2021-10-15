# MongoDB Cloud Manager on AWS Wavelength
Terraform module for Verizon 5G Edge with AWS Wavelength running MongoDB Cloud Manager in AWS Wavelength Zone.

**What is Cloud Manager?**
Cloud Manager eliminates the guesswork in running MongoDB. Easily monitor your databases, automate administration tasks, and leverage cloud backups for your self-managed deployments.
Learn more about Cloud Manager on the [MongoDB website](https://www.mongodb.com/cloud/cloud-manager).


<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 0.14.9 |
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | ~> 3.53 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_aws"></a> [aws](#provider\_aws) | 3.60.0 |
| <a name="provider_null"></a> [null](#provider\_null) | 3.1.0 |
| <a name="provider_template"></a> [template](#provider\_template) | 2.2.0 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [aws_ec2_carrier_gateway.tf_carrier_gateway](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ec2_carrier_gateway) | resource |
| [aws_eip.mongodb_node_eip](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/eip) | resource |
| [aws_iam_instance_profile.eip_instance_profile](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_instance_profile) | resource |
| [aws_iam_role.eip_role](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role) | resource |
| [aws_iam_role_policy.eip_role_policy](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role_policy) | resource |
| [aws_instance.AgentNode](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/instance) | resource |
| [aws_internet_gateway.tf_internet_gw](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/internet_gateway) | resource |
| [aws_route.WLZ_route](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route) | resource |
| [aws_route.region_route](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route) | resource |
| [aws_route_table.WLZ_route_table](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table) | resource |
| [aws_route_table.region_route_table](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table) | resource |
| [aws_route_table_association.region_route_association](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table_association) | resource |
| [aws_route_table_association.wavelength_route_association](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/route_table_association) | resource |
| [aws_security_group.AgentNode](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/security_group) | resource |
| [aws_subnet.tf_region_subnet](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/subnet) | resource |
| [aws_subnet.tf_wl_subnet](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/subnet) | resource |
| [aws_vpc.tf-vpc](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/vpc) | resource |
| [null_resource.init_db](https://registry.terraform.io/providers/hashicorp/null/latest/docs/resources/resource) | resource |
| [aws_iam_policy_document.eip_policy](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |
| [aws_iam_policy_document.instance_assume_role_policy](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |
| [template_file.user_data](https://registry.terraform.io/providers/hashicorp/template/latest/docs/data-sources/file) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_agentInstanceType"></a> [agentInstanceType](#input\_agentInstanceType) | Instance type of agent nodes | `string` | `"t3.medium"` | no |
| <a name="input_agentNodeCount"></a> [agentNodeCount](#input\_agentNodeCount) | MongoDB cluster size | `number` | `3` | no |
| <a name="input_atlas_private_key"></a> [atlas\_private\_key](#input\_atlas\_private\_key) | Your MongoDB Atlas private key | `string` | n/a | yes |
| <a name="input_atlas_public_key"></a> [atlas\_public\_key](#input\_atlas\_public\_key) | Your MongoDB Atlas public key | `string` | n/a | yes |
| <a name="input_availability_zone"></a> [availability\_zone](#input\_availability\_zone) | The Availability Zone for your EC2 instance. | `string` | `"us-east-1a"` | no |
| <a name="input_keyName"></a> [keyName](#input\_keyName) | Name of your AWS key | `string` | `"test_key"` | no |
| <a name="input_mmsApiKey"></a> [mmsApiKey](#input\_mmsApiKey) | Your MongoDB API key | `string` | n/a | yes |
| <a name="input_mmsGroupId"></a> [mmsGroupId](#input\_mmsGroupId) | Name of your MongoDB organization ID | `string` | n/a | yes |
| <a name="input_region"></a> [region](#input\_region) | The AWS region to deploy your Wavelength configuration. | `string` | `"us-east-1"` | no |
| <a name="input_wavelength_ami"></a> [wavelength\_ami](#input\_wavelength\_ami) | Create AMI Mapping for Wavelength Zone (Amazon Linux 2) | `map(string)` | <pre>{<br>  "us-east-1": "ami-0c2b8ca1dad447f8a",<br>  "us-west-2": "ami-083ac7c7ecf9bb9b0"<br>}</pre> | no |
| <a name="input_wavelength_zone"></a> [wavelength\_zone](#input\_wavelength\_zone) | The Wavelength Zone for your EC2 instance. | `string` | `"us-east-1-wl1-bos-wlz-1"` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_database_nodes"></a> [database\_nodes](#output\_database\_nodes) | The Carrier IP addresses of your MongoDB database nodes. |
| <a name="output_node_hostnames"></a> [node\_hostnames](#output\_node\_hostnames) | The hostnames (Private DNS names) of your MongoDB database nodes. |
| <a name="output_nodes_private_IPs"></a> [nodes\_private\_IPs](#output\_nodes\_private\_IPs) | The Private IP addresses of your MongoDB database nodes. |
<!-- END_TF_DOCS -->