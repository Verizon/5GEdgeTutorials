# Terraform Module for Edge IoT Architecture on MongoDB using MongoDB Realm, Atlas, and Verizon 5G Edge
> Brought to you by the Verizon 5G Edge and MongoDB teams


## Prerequisites
- Create an account on MongoDB, and extract your public and private keys. 
- Create an account on the [Verizon 5G Edge Portal](https://www.verizon.com/business/5g-edge-portal) and extract your appKey and secretKey.

Use these values to populate your `secret.tfvars` file for the Terraform module.

```
atlas_public_key = <your-public-key>
atlas_private_key = <your-private-key>
group_id = <your-org-id>
appKey = <your-eds-app-key>
secretKey = <your-edge-secret-key>

```

Additionally, ensure your AWS credentials are stored in your default profile. To do so, run `aws configure` or visit the CLI documentation to learn more.

## Getting Started
To get started, apply the infrastructure template for the MongoDB Atlas Cluster. 

```
terraform apply -var-file="secret.tfvars"
```

## Configure Client Device

Next, create your MQTT producer that will act as the synthetic data generator to send data to the Realm database deployed on the Wavelength Zone.
From your mobile device, Raspberry Pi, or other mobile client, navigate to your shell and run the following.

```
git clone https://github.com/graboskyc/MQTTProducer.git && 
cd MQTTProducer
```

Here, update the `.env` file to include the carrier IP address of the Wavelength Instance,your Realm App ID, and the topic name.

BROKER= "<your-ip-address>" #Replace with Carrier IP address of MongoDB Realm Node
CLIENTID="Realm App ID"
TOPIC="IOTDataPoint" #Keep as-is

Lastly, `./build.sh` script to start the container. At this point, go to your cloud console, in your project, open up the requisite Realm app and the data points from this producer should start populating.