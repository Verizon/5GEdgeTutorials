# SDK Tutorials for 5G Edge

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