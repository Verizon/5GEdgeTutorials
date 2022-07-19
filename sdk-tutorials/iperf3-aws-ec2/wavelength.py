import boto3
from botocore.exceptions import ClientError
import json
import os
from rich.live import Live
from rich.table import Table
import sys
import time
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

HTTP_PERMISSION = {
    "IpProtocol": "tcp",
    "FromPort": 80,
    "ToPort": 80,
}

ANY_IP_PERMISSION = {
    "IpProtocol": "tcp",
    "FromPort": 0,
    "ToPort": 65535,
    "IpRanges": [{"CidrIp": "0.0.0.0/0", "Description": "Any IP"}],
}

CURL_FORMAT_FILE = "./curl-format.txt"
SAMPLE_FILE = "sample.4ds"

INSTANCE_STATE_STYLE = {
    "pending": "gray",
    "running": "green",
    "shutting-down": "red",
    "terminated": "bold red",
    "stopping": "red",
    "stopped": "bold red",
}

INSTANCE_STATUS_STYLE = {
    "ok": "green",
    "impaired": "bold red",
    "insufficient-data": "yellow",
    "not-applicaable": "gray",
    "initializing": "gray",
}

app = typer.Typer()
ec2_client = boto3.client("ec2")
ec2_resource = boto3.resource("ec2")
iam_client = boto3.client("iam")


def bailout(messages=[]):
    typer.secho("Stopping build.", fg=typer.colors.RED, bold=True)
    typer.secho("Rerun this command with the 'teardown' option to try to clean up.")
    for msg in messages:
        typer.secho(msg)
    raise typer.Exit(code=1)


def instance_status_table(instances: dict[str, str]) -> tuple[Table, bool]:
    instance_ids = list(instances.keys())
    table = Table()
    for column in ["Instance", "State", "Status"]:
        table.add_column(column)

    response = ec2_client.describe_instance_status(InstanceIds=instance_ids)
    all_ok = True
    for row in response["InstanceStatuses"]:
        instance_state = row["InstanceState"]["Name"]
        instance_status = row["InstanceStatus"]["Status"]
        table.add_row(
            instances[row["InstanceId"]],
            f"[{INSTANCE_STATE_STYLE[instance_state]}]{instance_state}",
            f"[{INSTANCE_STATUS_STYLE[instance_status]}]{instance_status}",
        )
        all_ok &= instance_status == "ok"
    return (table, all_ok)


def monitor_startup(instances: dict[str, str], interval=5, max_iterations=180) -> bool:
    iterations = 0
    with Live(None, auto_refresh=False) as live:
        while iterations < max_iterations:
            table, all_ok = instance_status_table(instances)
            live.update(table)
            live.refresh()
            if all_ok:
                return True
            iterations += 1
            time.sleep(interval)
    return False


@app.command()
def deploy(
    management_ips: str = typer.Argument(
        ..., help="IPv4 CIDR addresses allowed to connect to the bastion host"
    ),
    iperf_port: int = typer.Option(
        5001, help="Port to allow TCP and UDP traffic in for iperf"
    ),
    bastion_az: str = typer.Option(
        "us-west-2c", help="Availability zone to deploy bastion host into"
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
    bastion_ami: str = typer.Option(
        "ami-0341aeea105412b57", help="AMI ID for the bastion host"
    ),
    wavelength_ami: str = typer.Option(
        "ami-0341aeea105412b57", help="AMI ID for the Wavelength instance"
    ),
    startup_script: str = typer.Option(
        "scripts/startup-amazon-linux.sh",
        help="Script to run when starting the instances for the first time",
    ),
    use_existing: bool = typer.Option(
        False,
        help="Use existing items where feasible, otherwise exit when existing items are encountered",
    ),
):
    """
    Create test nodes in AWS Wavelength and EC2 with iperf and NGINX installed
    and running, and other AWS infrastructure to support them. The EC2 instances
    is also a bastion host from which you can SSH into the Wavelegth node.
    """
    typer.secho("Creating VPC", bold=True)
    # TODO: Error out on existing VPC
    vpc = ec2_resource.create_vpc(CidrBlock="10.0.0.0/16")
    vpc.create_tags(Tags=[{"Key": "Name", "Value": vpc_name}])
    vpc.wait_until_available()

    typer.secho("Creating internet gateway", bold=True)
    internet_gw = ec2_resource.create_internet_gateway()
    vpc.attach_internet_gateway(InternetGatewayId=internet_gw.id)

    typer.secho("Creating route table", bold=True)
    vpc_route_table = vpc.create_route_table()
    vpc_route_table.create_route(
        DestinationCidrBlock="0.0.0.0/0",
        GatewayId=internet_gw.id,
    )

    typer.secho("Creating public subnet", bold=True)
    public_subnet = vpc.create_subnet(
        CidrBlock="10.0.1.0/24", AvailabilityZone=bastion_az
    )
    vpc_route_table.associate_with_subnet(SubnetId=public_subnet.id)

    typer.secho("Creating carrier gateway", bold=True)
    carrier_gw = ec2_client.create_carrier_gateway(VpcId=vpc.id)
    carrier_route_table = vpc.create_route_table()
    carrier_route_table.create_route(
        DestinationCidrBlock="0.0.0.0/0",
        CarrierGatewayId=carrier_gw["CarrierGateway"]["CarrierGatewayId"],
    )

    typer.secho("Creating wavelength subnet", bold=True)
    wl_subnet = ec2_resource.create_subnet(
        CidrBlock="10.0.2.0/26", AvailabilityZone=wl_zone_name, VpcId=vpc.id
    )
    carrier_route_table.associate_with_subnet(SubnetId=wl_subnet.id)

    typer.secho("Creating security groups", bold=True)
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
            HTTP_PERMISSION
            | {"IpRanges": [wavelength_address_block, management_address_block]},
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
            HTTP_PERMISSION | {"IpRanges": [wavelength_address_block]},
        ],
    )
    ec2_client.authorize_security_group_egress(
        GroupId=wl_sg.id, IpPermissions=[ANY_IP_PERMISSION]
    )

    typer.secho("Creating key pair", bold=True)
    # We can't recover the old private key, so remove it
    ec2_client.delete_key_pair(KeyName=keypair_name)
    keypair = ec2_client.create_key_pair(KeyName=keypair_name)

    with open(key_file, "w") as private_key_file:
        private_key_file.write(keypair["KeyMaterial"])
    os.chmod(key_file, 0o600)

    with open(startup_script) as user_data_file:
        user_data = user_data_file.read()

    typer.secho("Creating bastion instance", bold=True)
    bastion_instance = ec2_resource.create_instances(
        MinCount=1,
        MaxCount=1,
        ImageId=bastion_ami,
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

    typer.secho("Creating Wavelength instance", bold=True)
    wavelength_instance = ec2_resource.create_instances(
        MinCount=1,
        MaxCount=1,
        ImageId=wavelength_ami,
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

    typer.secho("Creating IAM role", bold=True)
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
        typer.secho("  IAM role already exists...", nl=False)
        if use_existing:
            typer.secho("ignoring (--use-existing)", fg=typer.colors.GREEN)
        else:
            typer.secho("failing (--no-use-existing)", fg=typer.colors.RED)
            bailout(
                messages=[
                    f"You might have to manually remove the IAM role '{iam_role_name}'"
                ]
            )

    iam_client.attach_role_policy(
        RoleName=iam_role_name,
        PolicyArn="arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore",
    )

    try:
        iam_client.create_instance_profile(InstanceProfileName=instance_profile_name)
    except iam_client.exceptions.EntityAlreadyExistsException:
        typer.secho("  Instance profile already exists...", nl=False)
        if use_existing:
            typer.secho("ignoring (--use-existing)", fg=typer.colors.GREEN)
        else:
            typer.secho("failing (--no-use-existing)", fg=typer.colors.RED)
            bailout(
                messages=[
                    f"You might have to manually remove the IAM role '{iam_role_name}' and EC2 instance profile '{instance_profile_name}'"
                ]
            )

    try:
        iam_client.add_role_to_instance_profile(
            InstanceProfileName=instance_profile_name, RoleName=iam_role_name
        )
    except iam_client.exceptions.LimitExceededException:
        typer.secho("  Role already has associated instance profile...", nl=False)
        if use_existing:
            typer.secho("ignoring (--use-existing)", fg=typer.colors.GREEN)
        else:
            typer.secho("failing (--no-use-existing)", fg=typer.colors.RED)
            bailout(
                messages=[
                    f"You might have to manually remove the IAM role '{iam_role_name}' and EC2 instance profile '{instance_profile_name}'"
                ]
            )

    typer.secho("Associating role", bold=True)
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

    typer.secho("Waiting on instance startups", bold=True)
    all_ok = monitor_startup(
        {
            bastion_instance.id: "Bastion",
            wavelength_instance.id: "Wavelength",
        }
    )

    if all_ok:
        typer.secho("Done!", fg=typer.colors.GREEN, bold=True)
    else:
        typer.secho(
            "Timed out waiting for instance startus", fg=typer.colors.YELLOW, bold=True
        )

    bastion_instance.reload()
    bastion_ip = bastion_instance.public_ip_address
    wavelength_ip = carrier_ip["CarrierIp"]

    #  TODO: Get this from the instance
    bastion_username = "ec2-user"

    typer.echo("SSH to the bastion host:")
    typer.secho(
        f"    ssh -i {key_file} -o IdentitiesOnly=yes {bastion_username}@{bastion_ip}",
        bold=True,
    )
    typer.echo("iperf to the Wavelength node:")
    typer.secho(f"    iperf3 --client {wavelength_ip} --port {iperf_port}", bold=True)
    typer.echo("Download sample file from the Wavelength node:")
    typer.secho(
        f'    curl -w"@{CURL_FORMAT_FILE}" -o /dev/null http://{wavelength_ip}/{SAMPLE_FILE}',
        bold=True,
    )
    typer.echo("Download sample file from the EC2 node:")
    typer.secho(
        f'    curl -w"@{CURL_FORMAT_FILE}" -o /dev/null http://{bastion_ip}/{SAMPLE_FILE}',
        bold=True,
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

    typer.secho("Terminating instances", bold=True)
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

    typer.secho("Waiting on instance terminations", bold=True)
    if instance_ids:
        waiter = ec2_client.get_waiter("instance_terminated")
        waiter.wait(InstanceIds=instance_ids)

    typer.secho("Deleting network interfaces", bold=True)
    nic_ids = get_vpc_nics(vpc.id)
    for nic_id in nic_ids:
        ec2_client.delete_network_interface(NetworkInterfaceId=nic_id)

    typer.secho("Deleting internet gateways", bold=True)
    for gw in vpc.internet_gateways.all():
        vpc.detach_internet_gateway(InternetGatewayId=gw.id)
        gw.delete()

    typer.secho("Deleting carrier gateways", bold=True)
    for carrier_gw_id in get_carrier_gw_ids(vpc.id):
        ec2_client.delete_carrier_gateway(CarrierGatewayId=carrier_gw_id)

    typer.secho("Deleting route tables", bold=True)
    for route_table in vpc.route_tables.all():
        is_main_route_table = False
        for route_table_association in route_table.associations:
            if route_table_association.main:
                is_main_route_table = True
            else:
                route_table_association.delete()
        if not is_main_route_table:
            route_table.delete()

    typer.secho("Deleting subnets", bold=True)
    for subnet_id in get_subnet_ids(vpc.id):
        ec2_client.delete_subnet(SubnetId=subnet_id)

    typer.secho("Deleting security group dependencies", bold=True)
    for security_group_id in get_security_group_ids(vpc.id):
        try:
            ec2_client.delete_security_group(GroupId=security_group_id)
        except ClientError as e:
            if e.response["Error"]["Code"] == "DependencyViolation":
                pass
            else:
                raise e

    typer.secho("Deleting remaining security groups", bold=True)
    for security_group_id in get_security_group_ids(vpc.id):
        ec2_client.delete_security_group(GroupId=security_group_id)

    typer.secho("Deleting VPC", bold=True)
    vpc.delete()

    typer.secho("Deleting IAM instance profiles", bold=True)
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

    typer.secho("Deleting IAM roles", bold=True)
    for role_name in iam_role_names:
        for policy in iam_client.list_attached_role_policies(RoleName=role_name)[
            "AttachedPolicies"
        ]:
            iam_client.detach_role_policy(
                RoleName=role_name, PolicyArn=policy["PolicyArn"]
            )
        iam_client.delete_role(RoleName=role_name)

    typer.secho("Done!", fg=typer.colors.GREEN, bold=True)


if __name__ == "__main__":
    app()
