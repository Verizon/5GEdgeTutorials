# 5G Edge Tutorials
This repository is home to CloudFormation templates, tutorials, code samples, and quick starts for 5G Edge infrastructure and applications.


## CloudFormation Templates
In the `cfn` subfolder, we maintain CloudFormation template for popular edge configurations, including:
- EC2 instance in a Wavelength Zone
- ECS cluster in a Wavelength Zone (control plane in parent region, task deployed to Wavelength)
- EKS cluster in a Wavelength Zone (control plane in parent region, pods deployed to Wavelength)
- Outpost configuration (in-development)



## Tutorials
For installation, Use the package manager [pip](https://pip.pypa.io/en/stable/) to install boto3.

```bash
pip install boto3
```


For Auto Scaling tutorial:

```
python push_VZ_metrics.py
python add_alarm.py
python add_asg.py
```

For Hotdog/Not Hotdog tutorial:
- Launch EKS cluster using CFN template
- Schedule API/Inference/Web deployments + corresponding service
```
kubectl apply -f infDeployment.yaml
kubectl apply -f apiDeployment.yaml
kubectl apply -f webDeployment.yaml
```

For Boto3 tutorials:
- To automate EC2 instance creation, `python wavelengthEC2.py`
- To automate EKS cluster, `python EKSBotoTutorial.py`

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
