# MongoDB Atlas connected  to AWS Wavelength with VPC peering
Terraform module for Verizon 5G Edge with AWS Wavelength running MongoDB Atlas VPC and peering it to a Wavelength VPC with Wavelength AZs being able to privately connect.

**What is Atlas?**
Atlas is a multi-cloud application data platform with integrated suite of cloud database and data services to accelerate and simplify how you build with data.
Learn more about Atlas on the [MongoDB website](https://www.mongodb.com/atlas).



## Prerequisites
- Create an account on [MongoDB Cloud](https://cloud.mongodb.com), and extract your public and private keys. 
- Have your AWS account details and profile ready to work.

Use these values to populate your `secret.tfvars` file for the Terraform module.

```
atlas_public_key = <your-public-key>
atlas_private_key = <your-private-key>
atlas_org_id = <your-org-id>
group_id = <your-project-id>
atlas_dbpassword = <atlas-database-password>
aws_account_id = <AWS-account-number>
atlas_vpc_cidr = <Atlas-CIDR-block>
```

Additionally, ensure your AWS credentials are stored in your default profile. To do so, run `aws configure` or visit the CLI documentation to learn more.

## Getting Started
To get started, apply the infrastructure template for the MongoDB Atlas Cluster. 

```
terraform init
terraform apply -var-file="secret.tfvars"
```
