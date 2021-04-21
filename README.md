# Verizon 5G Edge Demos & Tutorials
Tutorials, starter guides, and simple projects to launch your first application at the network edge.
> Brought to you by the Verizon 5G Edge Developer Relations Team

**What is 5G Edge?**

Verizon 5G Edge and AWS Wavelength bring the power of the AWS Cloud closer to mobile and connected devices at the edge of the Verizon 4G and 5G networks. That means developers can build apps with ultralow latency, using familiar AWS services, APIs and tools via seamless extension of your Amazon Virtual Private Cloud.

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


**Boto3 tutorial**

After including your AWS access and secret access keys, you can go ahead an run the automation document.

```
python EC2botoTutorial.py
python EKSbotoTutorial.py
```

## Contribute

Please refer to [the contributing.md file](Contributing.md) for information about how to get involved. We welcome issues, questions, and pull requests.

## Maintainers
- Robbie Belson: robert.belson@verizon.com

## License
- This project is licensed under the terms of the [Apache 2.0](LICENSE-Apache-2.0) open source license. Please refer to [LICENSE](LICENSE) for the full terms.
