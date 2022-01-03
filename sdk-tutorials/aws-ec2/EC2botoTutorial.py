# AWS Wavelength Tutorial for Boto
# Copyright Verizon
# Licensed under Apache 2.0. See license file in root for terms.

import boto3
import time
def launchWavelengthInfra():
    print("Launching Wavelength Zone..")

    # create VPC
    vpc = client.create_vpc(CidrBlock='10.0.2.0/24',
        TagSpecifications=[
            {'ResourceType': 'vpc','Tags': [{'Key': 'Name','Value': 'WL_Test_VPC'}]},
            ]
        )
    time.sleep(1) #Provide time for VPC to launch
    print("VPC launch complete...")
    vpc_id=vpc["Vpc"]["VpcId"]

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
    subnet = client.create_subnet(CidrBlock='10.0.2.0/26',
        TagSpecifications=[{'ResourceType': 'subnet','Tags': [{'Key': 'Name','Value': 'WL_Test_Subnet'}]}],
        AvailabilityZone='us-east-1-wl1-bos-wlz-1',
        VpcId=vpc_id
        )
    subnet_id=subnet["Subnet"]["SubnetId"]
    print("Subnet (Wavelength Zone) created..")

    ##Associate route table to subnet
    route_table = ec2.RouteTable(rt_id)
    route_table_association = route_table.associate_with_subnet(SubnetId=subnet_id)
    print("Association of route table to subnet complete..")

    # Create security group
    securityGroup = ec2.create_security_group(
        GroupName='Enabled_ICMP', Description='Security group with ICMP access', VpcId=vpc_id)
    securityGroup.authorize_ingress(
        CidrIp='0.0.0.0/0',
        IpProtocol='icmp',
        FromPort=-1,
        ToPort=-1
    )
    print(securityGroup.id)
    print("Security group initialization complete...")


    ##Allocate IP address
    ipAddress = client.allocate_address(
        Domain='vpc',
        NetworkBorderGroup='us-east-1-wl1-bos-wlz-1',
    )
    print("IP address allocated in NBG...")
    print(ipAddress)

    ##Create ENI
    eni=client.create_network_interface(SubnetId=subnet_id)
    eni_id=eni["NetworkInterface"]["NetworkInterfaceId"]
    print("ENI created..")

    ##Associate IP to ENI
    assoc = client.associate_address(
    AllocationId=ipAddress["AllocationId"],
    NetworkInterfaceId=eni_id,
    )
    print("Association of Carrier IP to ENI complete..")


    #Create EC2 Instance in Wavelength Zone
    instance = ec2.create_instances(
        ImageId='ami-0947d2ba12ee1ff75',
        InstanceType='t3.medium',
        MaxCount=1,
        MinCount=1,
        NetworkInterfaces=[{'DeviceIndex': 0,"NetworkInterfaceId":eni_id}]
    )
    print("EC2 instance deployed...")

if __name__ == "__main__":
    client = boto3.client('ec2',aws_access_key_id="your-key",
    aws_secret_access_key="your-secret",region_name='us-east-1')
    ec2 = boto3.resource('ec2',aws_access_key_id="your-key",
    aws_secret_access_key="your-secret",region_name='us-east-1')
    launchWavelengthInfra()
