# Verizon 5G Edge Demos & Tutorials
Tutorials, starter guides, and simple projects to launch your first application at the network edge.
> Brought to you by the Verizon 5G Edge Developer Relations Team

**What is 5G Edge?**

Verizon 5G Edge with AWS Wavelength brings the power of the AWS Cloud closer to mobile and connected devices at the edge of the Verizon 4G and 5G networks. That means developers can build apps with low latency using familiar AWS services, APIs and tools via seamless extension of your Amazon Virtual Private Cloud.

To learn more, check out the [AWS Wavelength page](https://aws.amazon.com/wavelength/).

## Table of Contents

- [Background](#background)
- [Install](#install)
- [Usage](#usage)
- [Contribute](#contribute)
- [License](#license)

## Background

We want to make it easier than ever to develop applications for the network edge. To that end, we've created a number of starter projects that get your AWS Wavelength infrastructure up-and-running in seconds. From there, feel free to experiment with some of the starter applications we've developed, including the following:

 - Automation of your first EC2 Instance on AWS Wavelength using Boto3
 - Automation of your first EKS Cluster on AWS Wavelength using Boto3
 - More projects coming soon!

## Install

To run the demos above, ensure you have Python3 and have the latest version of Boto3 downloaded

```
pip install boto3
```

Please note that the vast majority of the demos require an active AWS account and a set of authentication credentials. To learn how to generate AWS access keys, check out the Boto3 documentation [here](https://boto3.amazonaws.com/v1/documentation/api/latest/guide/quickstart.html).

## Usage


**Boto3 tutorials**

After generating your AWS access and secret access keys, navigate to the `sdk-tutorials` folder and you can go ahead an run either of the Boto3 infrastructure automation documents.

```
python EC2botoTutorial.py
python EKSbotoTutorial.py
```

**CloudFormation templates**

Within the `cloudformation-templates` folder, you can find a variety of popular infrastructure patterns within AWS Wavelength, including:
- EC2 instance in a Wavelength Zone with auto-assigned Carrier IP
- ECS cluster (EC2 Launch Type) with a task scheduled to Wavelength Zone

From the CLI, create a CloudFormation stack using the following:
```
aws cloudformation create-stack \
--stack-name myWavelengthStack \
--template-body <selected-cfn-file> \
--parameters ParameterKey=EnvironmentName,ParameterValue=WavelengthCloudFormation \
```

For EKS Clusters in Wavelength Zones, please note the following:
- For Public Cluster, ECR/EC2 endpoints are Optional but you must manually attach Carrier IP to public-facing instaces
- For Private Cluster, change Public Endpoint Access to False and Private Endpoint Access to True in the EKS Console; EC2/ECR Endpoints are mandatory

Regardless of cluster endpoint access, you must authenticate to Cluster using aws-auth ConfigMap:

```
eks_node_profile=$(aws cloudformation describe-stacks --stack-name $stack_name --query "Stacks[0].Outputs[?OutputKey=='NodeInstanceRole'].OutputValue" --output text)
cat > aws-auth-cm.yaml <<EOF
apiVersion: v1
kind: ConfigMap
metadata:
   name: aws-auth
   namespace: kube-system
data:
  mapRoles: |
    - rolearn: $eks_node_profile
      username: system:node:{{EC2PrivateDNSName}}
      groups:
        - system:bootstrappers
        - system:nodes
EOF
kubectl apply -f aws-auth-cm.yaml
```

## Contribute

Please refer to [the contributing.md file](Contributing.md) for information about how to get involved. We welcome issues, questions, and pull requests.

## Maintainers
- Robbie Belson: robert.belson@verizon.com

## License
- This project is licensed under the terms of the [Apache 2.0](LICENSE-Apache-2.0) open source license. Please refer to [LICENSE](LICENSE) for the full terms.
