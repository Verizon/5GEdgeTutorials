import boto3
import json
import sys

MIN_PYTHON = (3, 10)
assert (
    sys.version_info >= MIN_PYTHON
), f"requires Python {'.'.join([str(n) for n in MIN_PYTHON])} or newer"

WL_AZ = "us-west-2-wl1-phx-wlz-1"

INSEEGO_IP = {"CidrIp": "174.205.236.186/32", "Description": "Inseego IP"}
VERIZON_IP = {"CidrIp": "168.149.161.46/32", "Description": "Verizon IP"}

SSH_PERMISSION = {
    "IpProtocol": "tcp",
    "FromPort": 22,
    "ToPort": 22,
}

IPERF_PERMISSION = {
    "IpProtocol": "tcp",
    "FromPort": 5001,
    "ToPort": 5001,
}

ANY_IP_PERMISSION = {
    "IpProtocol": "tcp",
    "FromPort": 0,
    "ToPort": 65535,
    "IpRanges": [{"CidrIp": "0.0.0.0/0", "Description": "Any IP"}],
}

VZ_CENTOS7_AMI = "ami-0d2d88144bce4de9c"
AWS_CENTOS7_AMI = "ami-0348652b6078c2c1d"

BASTION_INSTANCE_TYPE = "t3.medium"
WAVELENGTH_INSTANCE_TYPE = "t3.medium"

EC2_INSTANCE_PROFILE_NAME = "botoInstanceProfile"
IAM_ROLE_NAME = "botoRole"
KEYPAIR_NAME = "boto-keypair"


def deployWL():
    ec2_client = boto3.client("ec2")
    ec2_resource = boto3.resource("ec2")
    iam_client = boto3.client("iam")

    # TODO: Unique VPC name (by timestamp?)
    # TODO: Tag VPC?
    vpc = ec2_resource.create_vpc(CidrBlock="10.0.0.0/16")
    vpc.wait_until_exists()
    vpc.wait_until_available()

    internet_gw = ec2_resource.create_internet_gateway()
    vpc.attach_internet_gateway(InternetGatewayId=internet_gw.id)

    vpc_route_table = vpc.create_route_table()
    vpc_route_table.create_route(
        DestinationCidrBlock="0.0.0.0/0",
        GatewayId=internet_gw.id,
    )

    public_subnet = vpc.create_subnet(CidrBlock="10.0.1.0/24")
    vpc_route_table.associate_with_subnet(SubnetId=public_subnet.id)

    carrier_gw = ec2_client.create_carrier_gateway(VpcId=vpc.id)
    carrier_route_table = vpc.create_route_table()
    carrier_route_table.create_route(
        DestinationCidrBlock="0.0.0.0/0",
        CarrierGatewayId=carrier_gw["CarrierGateway"]["CarrierGatewayId"],
    )

    wl_subnet = ec2_resource.create_subnet(
        CidrBlock="10.0.2.0/26", AvailabilityZone=WL_AZ, VpcId=vpc.id
    )
    carrier_route_table.associate_with_subnet(SubnetId=wl_subnet.id)

    bastion_sg = ec2_resource.create_security_group(
        VpcId=vpc.id, GroupName="BastionSG", Description="Allow SSH and iperf in"
    )
    ec2_client.authorize_security_group_ingress(
        GroupId=bastion_sg.id,
        IpPermissions=[
            SSH_PERMISSION | {"IpRanges": [INSEEGO_IP, VERIZON_IP]},
            IPERF_PERMISSION | {"IpRanges": [INSEEGO_IP, VERIZON_IP]},
        ],
    )
    ec2_client.authorize_security_group_egress(
        GroupId=bastion_sg.id, IpPermissions=[ANY_IP_PERMISSION]
    )

    wl_sg = ec2_resource.create_security_group(
        VpcId=vpc.id, GroupName="WavelengthSG", Description="Allow SSH and iperf in"
    )
    ec2_client.authorize_security_group_ingress(
        GroupId=wl_sg.id,
        IpPermissions=[
            SSH_PERMISSION
            | {
                "IpRanges": [INSEEGO_IP],
                "UserIdGroupPairs": [{"GroupId": bastion_sg.id}],
            },
            IPERF_PERMISSION | {"IpRanges": [INSEEGO_IP]},
        ],
    )
    ec2_client.authorize_security_group_egress(
        GroupId=wl_sg.id, IpPermissions=[ANY_IP_PERMISSION]
    )

    # We can't recover the old private key, so remove it
    ec2_client.delete_key_pair(KeyName=KEYPAIR_NAME)
    keypair = ec2_client.create_key_pair(KeyName=KEYPAIR_NAME)

    with open("boto-keypair.pem", "w") as private_key_file:
        private_key_file.write(keypair["KeyMaterial"])

    # TODO: User data (startup scripts)

    bastion_instance = ec2_resource.create_instances(
        MinCount=1,
        MaxCount=1,
        ImageId=VZ_CENTOS7_AMI,
        InstanceType=BASTION_INSTANCE_TYPE,
        NetworkInterfaces=[
            {
                "DeviceIndex": 0,
                "SubnetId": public_subnet.id,
                "AssociatePublicIpAddress": True,
                "Groups": [bastion_sg.group_id],
            }
        ],
    )[0]

    carrier_ip = ec2_client.allocate_address(Domain="vpc", NetworkBorderGroup=WL_AZ)
    carrier_intf = ec2_client.create_network_interface(SubnetId=wl_subnet.id)
    carrier_intf_id = carrier_intf["NetworkInterface"]["NetworkInterfaceId"]
    ec2_client.associate_address(
        AllocationId=carrier_ip["AllocationId"], NetworkInterfaceId=carrier_intf_id
    )

    wavelength_instance = ec2_resource.create_instances(
        MinCount=1,
        MaxCount=1,
        ImageId=AWS_CENTOS7_AMI,
        InstanceType=WAVELENGTH_INSTANCE_TYPE,
        NetworkInterfaces=[{"DeviceIndex": 0, "NetworkInterfaceId": carrier_intf_id}],
        Placement={"AvailabilityZone": WL_AZ},
    )[0]

    try:
        ec2_policy = json.dumps(
            {
                "Version": "2012-10-17",
                "Statement": [
                    {
                        "Effect": "Allow",
                        "Principal": {"Service": "ec2.amazonaws.com"},
                        "Action": "sts:AssumeRole",
                    }
                ],
            }
        )
        iam_client.create_role(
            RoleName=IAM_ROLE_NAME, AssumeRolePolicyDocument=ec2_policy
        )
    except iam_client.exceptions.EntityAlreadyExistsException:
        pass

    iam_client.attach_role_policy(
        RoleName=IAM_ROLE_NAME,
        PolicyArn="arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore",
    )

    try:
        iam_client.create_instance_profile(
            InstanceProfileName=EC2_INSTANCE_PROFILE_NAME
        )
    except iam_client.exceptions.EntityAlreadyExistsException:
        pass

    iam_client.add_role_to_instance_profile(
        InstanceProfileName=EC2_INSTANCE_PROFILE_NAME, RoleName=IAM_ROLE_NAME
    )

    bastion_instance.wait_until_running()
    ec2_client.associate_iam_instance_profile(
        IamInstanceProfile={"Name": EC2_INSTANCE_PROFILE_NAME},
        InstanceId=bastion_instance.id,
    )

    wavelength_instance.wait_until_running()
    ec2_client.associate_iam_instance_profile(
        IamInstanceProfile={"Name": EC2_INSTANCE_PROFILE_NAME},
        InstanceId=wavelength_instance.id,
    )


# TODO: Release elastic IPs
# TODO: Delete NICs
# TODO: Terminate EC2 instances
# TODO: Delete VPC
# TODO: Delete IAM policies
# TODO: Delete IAM role

if __name__ == "__main__":
    deployWL()
