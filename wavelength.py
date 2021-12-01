import boto3
from botocore.exceptions import ClientError
import json
import sys
import typer

MIN_PYTHON = (3, 10)
assert (
    sys.version_info >= MIN_PYTHON
), f"requires Python {'.'.join([str(n) for n in MIN_PYTHON])} or newer"

SSH_PERMISSION = {
    "IpProtocol": "tcp",
    "FromPort": 22,
    "ToPort": 22,
}

ANY_IP_PERMISSION = {
    "IpProtocol": "tcp",
    "FromPort": 0,
    "ToPort": 65535,
    "IpRanges": [{"CidrIp": "0.0.0.0/0", "Description": "Any IP"}],
}

VZ_CENTOS7_AMI = "ami-0348652b6078c2c1d"
AWS_CENTOS7_AMI = "ami-0348652b6078c2c1d"

app = typer.Typer()
ec2_client = boto3.client("ec2")
ec2_resource = boto3.resource("ec2")
iam_client = boto3.client("iam")


@app.command()
def deploy(
    management_ips: str = typer.Argument(
        ..., help="IPv4 CIDR addresses allowed to connect to the bastion host"
    ),
    iperf_port: int = typer.Option(
        5001, help="Port to allow TCP and UDP traffic in for iperf"
    ),
    wavelength_ips: str = typer.Option(
        "174.205.0.0/16",
        help="IPv4 CIDR addresses allowed to connect to the Wavelength host",
    ),
    wl_zone_name: str = typer.Option(
        "us-west-2-wl1-phx-wlz-1",
        "--wavelength-zone",
        help="Wavelength zone to deploy the Wavelength node into",
    ),
    vpc_name: str = typer.Option(
        "wavelength-testing", "--vpc", help="Name for the VPC to create"
    ),
    instance_profile_name: str = typer.Option(
        "wl-testing-profile",
        "--instance-profile",
        help="Name for the IAM instance profile to create",
    ),
    iam_role_name: str = typer.Option(
        "wl-testing-role", "--iam-role", help="Name for the IAM role to create"
    ),
    keypair_name: str = typer.Option(
        "wl-testing-keypair",
        "--keypair",
        help="Name for the SSH keypair to create. (Existing keypair with this name will be DELETED.)",
    ),
    key_file: str = typer.Option(
        "wl-testing.pem", help="Filename to use to save the SSH public key"
    ),
    bastion_type: str = typer.Option(
        "t2.micro", help="Machine type for the bastion host"
    ),
    wavelength_type: str = typer.Option(
        "t3.medium", help="Machine type for the Wavelength node"
    ),
):
    """
    Create a test node in AWS Wavelength with iperf installed and running, an EC2 bastion host from which you can SSH into the Wavelegth node, and other AWS infrastructure to support them.
    """
    typer.echo("Creating VPC")
    vpc = ec2_resource.create_vpc(CidrBlock="10.0.0.0/16")
    vpc.create_tags(Tags=[{"Key": "Name", "Value": vpc_name}])
    vpc.wait_until_available()

    typer.echo("Creating internet gateway")
    internet_gw = ec2_resource.create_internet_gateway()
    vpc.attach_internet_gateway(InternetGatewayId=internet_gw.id)

    typer.echo("Creating route table")
    vpc_route_table = vpc.create_route_table()
    vpc_route_table.create_route(
        DestinationCidrBlock="0.0.0.0/0",
        GatewayId=internet_gw.id,
    )

    typer.echo("Creating public subnet")
    public_subnet = vpc.create_subnet(CidrBlock="10.0.1.0/24")
    vpc_route_table.associate_with_subnet(SubnetId=public_subnet.id)

    typer.echo("Creating carrier gateway")
    carrier_gw = ec2_client.create_carrier_gateway(VpcId=vpc.id)
    carrier_route_table = vpc.create_route_table()
    carrier_route_table.create_route(
        DestinationCidrBlock="0.0.0.0/0",
        CarrierGatewayId=carrier_gw["CarrierGateway"]["CarrierGatewayId"],
    )

    typer.echo("Creating wavelength subnet")
    wl_subnet = ec2_resource.create_subnet(
        CidrBlock="10.0.2.0/26", AvailabilityZone=wl_zone_name, VpcId=vpc.id
    )
    carrier_route_table.associate_with_subnet(SubnetId=wl_subnet.id)

    typer.echo("Creating security groups")
    wavelength_address_block = {
        "CidrIp": wavelength_ips,
        "Description": "Carrier-side IPs",
    }
    management_address_block = {
        "CidrIp": management_ips,
        "Description": "Management IPs",
    }
    icmp_permission = {
        "IpProtocol": "icmp",
        "FromPort": -1,
        "ToPort": -1,
        "IpRanges": [wavelength_address_block, management_address_block],
    }
    iperf_tcp_permission = {
        "IpProtocol": "tcp",
        "FromPort": iperf_port,
        "ToPort": iperf_port,
        "IpRanges": [wavelength_address_block, management_address_block],
    }
    iperf_udp_permission = iperf_tcp_permission | {"IpProtocol": "udp"}

    bastion_sg = ec2_resource.create_security_group(
        VpcId=vpc.id, GroupName="BastionSG", Description="Allow SSH and iperf in"
    )
    ec2_client.authorize_security_group_ingress(
        GroupId=bastion_sg.id,
        IpPermissions=[
            SSH_PERMISSION
            | {"IpRanges": [wavelength_address_block, management_address_block]},
            icmp_permission,
            iperf_tcp_permission,
            iperf_udp_permission,
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
                "IpRanges": [wavelength_address_block],
                "UserIdGroupPairs": [{"GroupId": bastion_sg.id}],
            },
            icmp_permission,
            iperf_tcp_permission,
            iperf_udp_permission,
        ],
    )
    ec2_client.authorize_security_group_egress(
        GroupId=wl_sg.id, IpPermissions=[ANY_IP_PERMISSION]
    )

    typer.echo("Creating key pair")
    # We can't recover the old private key, so remove it
    ec2_client.delete_key_pair(KeyName=keypair_name)
    keypair = ec2_client.create_key_pair(KeyName=keypair_name)

    with open(key_file, "w") as private_key_file:
        private_key_file.write(keypair["KeyMaterial"])
    # TODO: Set file permissions

    with open("scripts/iperf.sh") as user_data_file:
        user_data = user_data_file.read()

    typer.echo("Creating bastion instance")
    bastion_instance = ec2_resource.create_instances(
        MinCount=1,
        MaxCount=1,
        ImageId=VZ_CENTOS7_AMI,
        InstanceType=bastion_type,
        KeyName=keypair_name,
        NetworkInterfaces=[
            {
                "DeviceIndex": 0,
                "SubnetId": public_subnet.id,
                "AssociatePublicIpAddress": True,
                "Groups": [bastion_sg.group_id],
            }
        ],
        UserData=user_data,
    )[0]

    carrier_ip = ec2_client.allocate_address(
        Domain="vpc", NetworkBorderGroup=wl_zone_name
    )
    carrier_intf = ec2_client.create_network_interface(
        SubnetId=wl_subnet.id,
        Groups=[wl_sg.group_id],
    )
    carrier_intf_id = carrier_intf["NetworkInterface"]["NetworkInterfaceId"]
    ec2_client.associate_address(
        AllocationId=carrier_ip["AllocationId"], NetworkInterfaceId=carrier_intf_id
    )

    typer.echo("Creating Wavelength instance")
    wavelength_instance = ec2_resource.create_instances(
        MinCount=1,
        MaxCount=1,
        ImageId=AWS_CENTOS7_AMI,
        InstanceType=wavelength_type,
        KeyName=keypair_name,
        NetworkInterfaces=[
            {
                "DeviceIndex": 0,
                "NetworkInterfaceId": carrier_intf_id,
            }
        ],
        Placement={"AvailabilityZone": wl_zone_name},
        UserData=user_data,
    )[0]

    typer.echo("Creating IAM role")
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
            RoleName=iam_role_name, AssumeRolePolicyDocument=ec2_policy
        )
    except iam_client.exceptions.EntityAlreadyExistsException:
        pass

    iam_client.attach_role_policy(
        RoleName=iam_role_name,
        PolicyArn="arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore",
    )

    try:
        iam_client.create_instance_profile(InstanceProfileName=instance_profile_name)
    except iam_client.exceptions.EntityAlreadyExistsException:
        pass

    iam_client.add_role_to_instance_profile(
        InstanceProfileName=instance_profile_name, RoleName=iam_role_name
    )

    typer.echo("Associating role")
    bastion_instance.wait_until_running()
    ec2_client.associate_iam_instance_profile(
        IamInstanceProfile={"Name": instance_profile_name},
        InstanceId=bastion_instance.id,
    )

    wavelength_instance.wait_until_running()
    ec2_client.associate_iam_instance_profile(
        IamInstanceProfile={"Name": instance_profile_name},
        InstanceId=wavelength_instance.id,
    )

    typer.secho("Done!", fg=typer.colors.GREEN)
    bastion_instance.reload()
    typer.echo("SSH to the bastion host:")
    typer.secho(
        f"    ssh -i {key_file} -o IdentitiesOnly=yes ec2-user@{bastion_instance.public_ip_address}",
        bold=True,
    )
    typer.echo("iperf to the Wavelength node:")
    typer.secho(
        f"    iperf3 --client {carrier_ip['CarrierIp']} --port {iperf_port}", bold=True
    )


def get_vpc_by_name(name: str):
    vpcs = ec2_client.describe_vpcs(Filters=[{"Name": "tag:Name", "Values": [name]}])[
        "Vpcs"
    ]
    num_vpcs = len(vpcs)
    if num_vpcs == 1:
        return ec2_resource.Vpc(vpcs[0]["VpcId"])
    elif num_vpcs:
        typer.echo(f"Too many VPCs ({num_vpcs}) with name '{name}'", err=True)
    else:
        typer.echo(f"No VPCs with name '{name}'", err=True)
    raise typer.Exit(code=1)


def get_vpc_nics(vpc_id: str):
    response = ec2_client.describe_network_interfaces(
        Filters=[{"Name": "vpc-id", "Values": [vpc_id]}]
    )
    nic_ids = [nic["NetworkInterfaceId"] for nic in response["NetworkInterfaces"]]
    return nic_ids


def get_carrier_ip_from_instance(instance):
    for interface in instance["NetworkInterfaces"]:
        if "Association" in interface and "CarrierIp" in interface["Association"]:
            # Assumes only one carrier IP per instance
            return interface["Association"]["CarrierIp"]
    return None


def get_allocation_ids_from_public_ips(public_ips: list[str]):
    response = ec2_client.describe_addresses(PublicIps=public_ips)
    return [address["AllocationId"] for address in response["Addresses"]]


def get_carrier_gw_ids(vpc_id: str):
    response = ec2_client.describe_carrier_gateways(
        Filters=[{"Name": "vpc-id", "Values": [vpc_id]}]
    )
    carrier_gw_ids = [
        carrier_gw["CarrierGatewayId"] for carrier_gw in response["CarrierGateways"]
    ]
    return carrier_gw_ids


def get_subnet_ids(vpc_id: str):
    response = ec2_client.describe_subnets(
        Filters=[{"Name": "vpc-id", "Values": [vpc_id]}]
    )
    subnet_ids = [subnet["SubnetId"] for subnet in response["Subnets"]]
    return subnet_ids


def get_security_group_ids(vpc_id: str, include_default_sg=False):
    response = ec2_client.describe_security_groups(
        Filters=[{"Name": "vpc-id", "Values": [vpc_id]}]
    )
    security_group_ids = [
        security_group["GroupId"]
        for security_group in response["SecurityGroups"]
        if include_default_sg or security_group["GroupName"] != "default"
    ]
    return security_group_ids


@app.command()
def teardown(
    vpc_name: str = typer.Option(
        "wavelength-testing", "--vpc", help="Name for the VPC to tear down"
    ),
):
    """
    Destroy the named VPC and related components (as created by the deploy command).
    """
    vpc = get_vpc_by_name(vpc_name)

    iam_instance_profile_arns = set()
    instance_ids = []

    typer.echo("Terminating instances")
    for instance in vpc.instances.all():
        if instance.iam_instance_profile:
            iam_instance_profile_arns.add(instance.iam_instance_profile["Arn"])
        addresses = ec2_client.describe_addresses(
            Filters=[{"Name": "instance-id", "Values": [instance.id]}]
        )["Addresses"]
        for address in addresses:
            if "CarrierIp" in address:
                typer.echo("  Releasing carrier IP")
                ec2_client.disassociate_address(AssociationId=address["AssociationId"])
                ec2_client.release_address(
                    AllocationId=address["AllocationId"],
                    NetworkBorderGroup=address["NetworkBorderGroup"],
                )
        instance.terminate()
        instance_ids.append(instance.id)

    typer.echo("Waiting on instance terminations")
    if instance_ids:
        waiter = ec2_client.get_waiter("instance_terminated")
        waiter.wait(InstanceIds=instance_ids)

    typer.echo("Deleting network interfaces")
    nic_ids = get_vpc_nics(vpc.id)
    for nic_id in nic_ids:
        ec2_client.delete_network_interface(NetworkInterfaceId=nic_id)

    typer.echo("Deleting internet gateways")
    for gw in vpc.internet_gateways.all():
        vpc.detach_internet_gateway(InternetGatewayId=gw.id)
        gw.delete()

    typer.echo("Deleting carrier gateways")
    for carrier_gw_id in get_carrier_gw_ids(vpc.id):
        ec2_client.delete_carrier_gateway(CarrierGatewayId=carrier_gw_id)

    typer.echo("Deleting route tables")
    for route_table in vpc.route_tables.all():
        is_main_route_table = False
        for route_table_association in route_table.associations:
            if route_table_association.main:
                is_main_route_table = True
            else:
                route_table_association.delete()
        if not is_main_route_table:
            route_table.delete()

    typer.echo("Deleting subnets")
    for subnet_id in get_subnet_ids(vpc.id):
        ec2_client.delete_subnet(SubnetId=subnet_id)

    typer.echo("Deleting security group dependencies")
    for security_group_id in get_security_group_ids(vpc.id):
        try:
            ec2_client.delete_security_group(GroupId=security_group_id)
        except ClientError as e:
            if e.response["Error"]["Code"] == "DependencyViolation":
                pass
            else:
                raise e

    typer.echo("Deleting remaining security groups")
    for security_group_id in get_security_group_ids(vpc.id):
        ec2_client.delete_security_group(GroupId=security_group_id)

    typer.echo("Deleting VPC")
    vpc.delete()

    typer.echo("Deleting IAM instance profiles")
    iam_role_names = set()
    for instance_profile_arn in iam_instance_profile_arns:
        instance_profile_name = instance_profile_arn.split("/")[-1]
        instance_profile = iam_client.get_instance_profile(
            InstanceProfileName=instance_profile_name
        )["InstanceProfile"]
        for role in instance_profile["Roles"]:
            iam_role_names.add(role["RoleName"])
            iam_client.remove_role_from_instance_profile(
                InstanceProfileName=instance_profile_name, RoleName=role["RoleName"]
            )
        iam_client.delete_instance_profile(InstanceProfileName=instance_profile_name)

    typer.echo("Deleting IAM roles")
    for role_name in iam_role_names:
        for policy in iam_client.list_attached_role_policies(RoleName=role_name)[
            "AttachedPolicies"
        ]:
            iam_client.detach_role_policy(
                RoleName=role_name, PolicyArn=policy["PolicyArn"]
            )
        iam_client.delete_role(RoleName=role_name)

    # TODO: Delete local key file

    typer.secho("Done!", fg=typer.colors.GREEN)


if __name__ == "__main__":
    app()
