# New Relic / Pixie on Verizon 5G Edge
Get started on New Relic with Verizon 5G Edge today using AWS Wavelength, Amazon EKS, Pixie, and New Relic.

In this tutorial, use Terraform to deploy Pixie's Helm chart on Kubernetes to AWS Wavelength.

## Getting Started
1. Fork the code repository for the [Pixie<>5G Edge Terraform module](https://github.com/bpschmitt/vz-newrelic-5gedge.git)

```
git clone https://github.com/bpschmitt/vz-newrelic-5gedge.git
cd vz-newrelic-5gedge
```

2. Next, initialize Terraform within your working directory and create a preview of your deployment changes.

```
terraform init
terraform plan
```

3. Next, edit `terraform.tfvars.example` with any specific configuration details, such as your EKS cluster name, and specific Wavelength Zone(s) of interest. Note that you must login to your New Relic console to retrieve your `nr_license_key`, `pixie_api_key`, and `pixie_deploy_key`.

```
mv terraform.tfvars.example terraform.tfvars
```

4. Apply the configuration by running `terraform apply.`


## Contributors
Thank you to Brad Schmitt, Sheffield Wong and the entire New Relic team for their support. To learn more about New Relic, visit [their website](https://www.pixie.io/).
