# AWS Wavelength Tutorial for Boto
# Copyright Verizon
# Licensed under Apache 2.0. See license file in root for terms.

import boto3
import time
import json
def launchEKS():
    # create VPC
    wlVpc = client.create_vpc(CidrBlock='10.0.0.0/16',
        TagSpecifications=[
            {'ResourceType': 'vpc','Tags': [{'Key': 'Name','Value': 'wl-vpc'}]},
            ]
        )
    time.sleep(1) #Provide time for VPC to launch
    print("VPC launch complete...")
    vpc_id=wlVpc["Vpc"]["VpcId"]

#*************** Parent region infrastructure*******************
    #Create first subnet in parent region for control plane

    #Create a route table and a public route
    main_route_table = client.create_route_table(VpcId=vpc_id)
    main_rt_id=main_route_table["RouteTable"]["RouteTableId"]
    print("Main route table created...")
    region_subnet_2 = client.create_subnet(CidrBlock='10.0.0.0/24',
        TagSpecifications=[{'ResourceType': 'subnet','Tags': [{'Key': 'Name','Value': 'region-subnet2'}]}],
        AvailabilityZone='us-east-1a',
        VpcId=vpc_id
        )
    print("Parent region subnet launched..")
    region_subnet_id_2=region_subnet_2["Subnet"]["SubnetId"]
    response=client.modify_subnet_attribute(
        MapPublicIpOnLaunch={
            'Value': True
        },
        SubnetId=region_subnet_id_2
    )
    igw = client.create_internet_gateway(
        TagSpecifications=[{'ResourceType':'internet-gateway','Tags': [{'Key': 'Name','Value': 'region-igw'}]}]
    )
    print("IGW creation complete..")
    igw_id=igw["InternetGateway"]["InternetGatewayId"]
    vpc=ec2.Vpc(vpc_id)
    response = vpc.attach_internet_gateway(
        InternetGatewayId=igw_id,
    )
    main_route = client.create_route(
        DestinationCidrBlock='0.0.0.0/0',
        GatewayId=igw_id,
        RouteTableId=main_rt_id
    )
    key = client.create_key_pair(KeyName='eksKey')
    keyMaterial=key["KeyMaterial"]
    ec2_key=client.describe_key_pairs(KeyNames=['eksKey'])
    print("Key pair complete..")


#*************** Wavelength Zone Infrastructure********************************
    # Create carrier gateway
    cgw = client.create_carrier_gateway(VpcId=vpc_id)
    cgw_id=cgw["CarrierGateway"]["CarrierGatewayId"]
    print("Carrier gateway launch complete...")

    #Create a route table and a public route
    route_table = client.create_route_table(VpcId=vpc_id)
    rt_id=route_table["RouteTable"]["RouteTableId"]
    print("Route table created...")

    route = client.create_route(
        DestinationCidrBlock='0.0.0.0/0',
        CarrierGatewayId=cgw_id,
        RouteTableId=rt_id
    )
    print("Route to carrier gateway complete...")

    #Create subnet
    subnet = client.create_subnet(CidrBlock='10.0.1.0/24',
        TagSpecifications=[{'ResourceType': 'subnet','Tags': [{'Key': 'Name','Value': 'WL_Test_Subnet'}]}],
        AvailabilityZone='us-east-1-wl1-nyc-wlz-1',
        VpcId=vpc_id
        )
    subnet_id=subnet["Subnet"]["SubnetId"]
    print("Subnet (Wavelength Zone) created..")

    ##Associate route table to subnet
    route_table = ec2.RouteTable(rt_id)
    route_table_association = route_table.associate_with_subnet(SubnetId=subnet_id)
    print("Association of route table to subnet complete..")

    # Create security group
    wlSecurityGroup = ec2.create_security_group(
        GroupName='Enabled_ICMP', Description='Security group with ICMP access', VpcId=vpc_id)
    wlSecurityGroup.authorize_ingress(
        CidrIp='0.0.0.0/0',
        IpProtocol='icmp',
        FromPort=-1,
        ToPort=-1
    )
    print("Security group initialization complete...")

    #Create second subnet in parent region for control plane (multi-AZ)
    region_subnet = client.create_subnet(CidrBlock='10.0.2.0/24',
        TagSpecifications=[{'ResourceType': 'subnet','Tags': [{'Key': 'Name','Value': 'region-subnet1'}]}],
        AvailabilityZone='us-east-1b',
        VpcId=vpc_id
        )
    region_subnet_id=region_subnet["Subnet"]["SubnetId"]
    print("Subnet (Parent Region) created..")

    # Create security group for parent region subnet and authorize HTTPS
    regionSecurityGroup = ec2.create_security_group(
        GroupName='region-eks-cluster-sg',
        TagSpecifications=[{'ResourceType': 'security-group','Tags': [{'Key': 'Name','Value': 'region-sg'}]}],
        Description='Security group for EKS cluster',
        VpcId=vpc_id)
    regionSecurityGroup.authorize_ingress(
        CidrIp='0.0.0.0/0',
        IpProtocol='tcp',
        FromPort=443,
        ToPort=443
    )
    # regionSecurityGroupId=regionSecurityGroup["GroupId"]
    regionSecurityGroupId=regionSecurityGroup.id
    print("Parent region security group complete..")

    #Create S3 gateway endpoint
    s3Endpoint = client.create_vpc_endpoint(
    VpcEndpointType='Gateway',
    VpcId=vpc_id,
    ServiceName='com.amazonaws.us-east-1.s3',
    RouteTableIds=[rt_id],
    TagSpecifications=[{'ResourceType': 'vpc-endpoint','Tags': [{'Key': 'Name','Value': 's3-gw-endpoint'}]}],
    )
    s3EndpointId=s3Endpoint["VpcEndpoint"]["VpcEndpointId"]
    print("S3 endpoint complete..")

    #Enable DNS support and DNS hsotnames
    vpc=ec2.Vpc(vpc_id)
    response = vpc.modify_attribute(EnableDnsHostnames={'Value': True})
    response = vpc.modify_attribute(EnableDnsSupport={'Value': True})
    print("VPC attribute modification complete..")

    #Create EC2 interface endpoint to communicate to control plane
    ec2Endpoint = client.create_vpc_endpoint(
    VpcEndpointType='Interface',
    VpcId=vpc_id,
    ServiceName='com.amazonaws.us-east-1.ec2',
    SubnetIds=[region_subnet_id],
    SecurityGroupIds=[regionSecurityGroupId],
    TagSpecifications=[{'ResourceType': 'vpc-endpoint','Tags': [{'Key': 'Name','Value': 'wl-ec2-endpoint'}]}],
    )
    ec2EndpointId=ec2Endpoint["VpcEndpoint"]["VpcEndpointId"]
    print("EC2 endpoint complete..")

    #Create ECR interface endpoint to communicate to ECR
    ecrEndpoint = client.create_vpc_endpoint(
    VpcEndpointType='Interface',
    VpcId=vpc_id,
    ServiceName='com.amazonaws.us-east-1.ecr.dkr',
    SubnetIds=[region_subnet_id],
    SecurityGroupIds=[regionSecurityGroupId],
    TagSpecifications=[{'ResourceType': 'vpc-endpoint','Tags': [{'Key': 'Name','Value': 'wl-ecr-endpoint'}]}],
    )
    ecrEndpointId=ecrEndpoint["VpcEndpoint"]["VpcEndpointId"]
    print("ECR endpoint complete..")

    #Create ECR (API) interface endpoint to communicate to ECR
    ecrApiEndpoint = client.create_vpc_endpoint(
    VpcEndpointType='Interface',
    VpcId=vpc_id,
    ServiceName='com.amazonaws.us-east-1.ecr.api',
    SubnetIds=[region_subnet_id],
    SecurityGroupIds=[regionSecurityGroupId],
    TagSpecifications=[{'ResourceType': 'vpc-endpoint','Tags': [{'Key': 'Name','Value': 'wl-ecr-api-endpoint'}]}],
    )
    ecrApiEndpointId=ecrApiEndpoint["VpcEndpoint"]["VpcEndpointId"]
    print("ECR API endpoint complete..")

    eks_trust_json=json.dumps({
      "Version": "2012-10-17",
      "Statement": [
        {
          "Effect": "Allow",
          "Principal": {
            "Service": "eks.amazonaws.com"
          },
          "Action": "sts:AssumeRole"
        }
      ]
    })
    #Create IAM role for EKS cluster creation
    eksRole = iam.create_role(
    RoleName='wavelength-eks-role',
    AssumeRolePolicyDocument=eks_trust_json,
    )
    eksRoleARN=eksRole["Role"]["Arn"]
    eksRoleName=eksRole["Role"]["RoleName"]
    print("IAM role creation complete..")


    #Attach EKS cluster policy to IAM role
    response = iam.attach_role_policy(
    RoleName=eksRoleName,
    PolicyArn='arn:aws:iam::aws:policy/AmazonEKSClusterPolicy'
    )
    print("IAM policy attachment complete..")

    #Create Private EKS cluster
    eksCluster = eks.create_cluster(
    name='wavelength_eks',
    version='1.17',
    roleArn=eksRoleARN,
    resourcesVpcConfig={
        'subnetIds': [
            region_subnet_id,region_subnet_id_2
        ],
        'securityGroupIds': [
            regionSecurityGroupId
        ],
        'endpointPublicAccess': True,
        'endpointPrivateAccess': True
    },
    )
    print("Private EKS cluster init complete..")

    #Given it takes 15min to launch EKS cluster, evaluate completion progress
    clusterLaunch=False
    while (clusterLaunch==False):
        try:
            eksCluster=eks.describe_cluster(name='wavelength_eks')
            print(eksCluster["cluster"])
            apiServer=eksCluster["cluster"]["endpoint"]
            certificateAuthority=eksCluster["cluster"]["certificateAuthority"]["data"]
            clusterLaunch=True
        except:
            print("Cluster creation not complete. Waiting another minute..")
            time.sleep(60)

    #Get API server
    # eksCluster=eks.describe_cluster(name='wavelength_eks')
    # apiServer=eksCluster["cluster"]["endpoint"]
    # certificateAuthority=eksCluster["cluster"]["certificateAuthority"]["data"]
    print("Nodegroup data retrieved..")

    #Launch self-managed worker node
    response = cfn.create_stack(
    StackName='wavelength-eks-node',
    TemplateURL='https://amazon-eks.s3.us-west-2.amazonaws.com/cloudformation/2020-08-12/amazon-eks-nodegroup.yaml',
    Parameters=[
        {
            'ParameterKey':'ClusterControlPlaneSecurityGroup',
            'ParameterValue': regionSecurityGroupId
        },
        {
            'ParameterKey':'NodeGroupName',
            'ParameterValue': 'wavelength-eks-nodegroup'
        },
        {
            'ParameterKey':'KeyName',
            'ParameterValue': 'eksKey'
        },
        {
            'ParameterKey':'Subnets',
            'ParameterValue': subnet_id
        },
        {
            'ParameterKey':'VpcId',
            'ParameterValue': vpc_id
        },
        {
            'ParameterKey':'ClusterName',
            'ParameterValue': "wavelength_eks"
        },
        {
            'ParameterKey':'NodeAutoScalingGroupDesiredCapacity',
            'ParameterValue': "1"
        },
        {
            'ParameterKey':'BootstrapArguments',
            'ParameterValue': '--apiserver-endpoint $api_server  --b64-cluster-ca $certificate_authority'
        }

    ],
    Capabilities=[
        'CAPABILITY_NAMED_IAM',
    ],
    Tags=[
        {
            'Key': 'Name',
            'Value': 'wavelengthEKSWorkerNode'
        },
    ],
    )

    stackComplete=False
    while (stackComplete==False):
        try:
            stack = cfn.describe_stacks(StackName='wavelength-eks-node')
            print(stack["Stacks"][0])
            print(stack["Stacks"][0]["StackStatus"])
            if stack["Stacks"][0]["StackStatus"]=="CREATE_COMPLETE":
                stackComplete=True
        except:
            print("CFN Stack creation not complete. Waiting another minute..")
            time.sleep(60)
    print("Stack creation complete..")

    ##Get CFN Outputs
    outputs=stack["Stacks"][0]["Outputs"]
    print(outputs)
    for k in outputs:
        if k["OutputKey"]=="NodeInstanceRole":
            nodeGroupProfile=k["OutputValue"]
    for k in outputs:
        if k["OutputKey"]=="NodeSecurityGroup":
            nodeSecurityGroup=k["OutputValue"]
    for k in outputs:
        if k["OutputKey"]=="NodeAutoScalingGroup":
            nodeASG=k["OutputValue"]
    print("CFN output retrieval complete...")

    #Get Instance ID/ENI ID of ASG node
    eksNodeInstanceId=asg.describe_auto_scaling_groups(AutoScalingGroupNames=[nodeASG])["AutoScalingGroups"][0]["Instances"][0]["InstanceId"]
    eksNodeEniId=client.describe_instances(InstanceIds=[eksNodeInstanceId])["Reservations"][0]["Instances"][0]["NetworkInterfaces"][0]["NetworkInterfaceId"]

    ##Allocate IP address
    ipAddress = client.allocate_address(
        Domain='vpc',
        NetworkBorderGroup='us-east-1-wl1-nyc-wlz-1',
    )
    print("IP address allocated in NBG...")
    ##Associate IP to ENI
    assoc = client.associate_address(
        AllocationId=ipAddress["AllocationId"],
        NetworkInterfaceId=eksNodeEniId,
    )
    print("Carrier IP association complete..")
    ##eksCarrierIp=client.describe_addresses(AllocationIds=[ipAddress]["AllocationId"])["Addresses"][0]["CarrierIp"]


if __name__ == "__main__":
    key=""
    secret=""
    client = boto3.client('ec2',aws_access_key_id=key,aws_secret_access_key=secret,region_name='us-east-1')
    ec2 = boto3.resource('ec2',aws_access_key_id=key,aws_secret_access_key=secret,region_name='us-east-1')
    iam = boto3.client('iam',aws_access_key_id=key,aws_secret_access_key=secret,region_name='us-east-1')
    eks = boto3.client('eks',aws_access_key_id=key,aws_secret_access_key=secret,region_name='us-east-1')
    asg = boto3.client('autoscaling',aws_access_key_id=key,aws_secret_access_key=secret,region_name='us-east-1')
    cfn = boto3.client('cloudformation',aws_access_key_id=key,aws_secret_access_key=secret,region_name='us-east-1')
    launchEKS()
