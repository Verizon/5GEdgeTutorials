# AWS Wavelength (Verizon 5G Edge) Terraform Module for Redis
Use this Terraform Module to get started building your first Redis cluster at the network edge on AWS Wavelength

## Overview
- VPC with subnets in AWS Wavelength Zone(s) and Parent Region (i.e., Availability Zones)
- EKS Cluster (control plane) deployed to Parent Region
- EKS Nodes deployed to Wavelength Zone(s)
- Redis Enterprise Database scheduled to AWS Wavelength

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

You should see the completed configuration after roughly 20-25 minutes (should look something like this):
```
Apply complete! Resources: 57 added, 0 changed, 0 destroyed.
```

Next, to deploy cluster resources, you will need to direct kubectl to the appropriate config file. To do so, run the following:
```
export KUBECONFIG="kubeconfig_wavelength-eks-Cluster"
```
Note that if you have changed the name of your EKS cluster to `test-cluster`, as an example, adjust the command above to `kubeconfig_test-cluster`.


## Deploying Your First Redis Enterprise Cluster
In this module, `./redis.sh` is automatically applied by the Terraform configuration to deploy your first workload, including:
- Create a RedisEnterpriseCluster(REC) within the "redis-demo" namespace
- Create a RedisEnterpriseDatabase (REDB)