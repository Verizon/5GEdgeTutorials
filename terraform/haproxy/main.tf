// The contents of this file are Copyright (c) 2021 Verizon.
// This file is subject to the terms and conditions defined in
// file 'LICENSE', which can found in the root of the repository.
// Original file Copyright (c) 2019 HAProxy, used under Apache 2.0 license.

provider "aws" {
  region = var.aws_region
}

# Lookup latest HAPEE Ubuntu AMI
data "aws_ami" "hapee_aws_amis" {
  filter {
    name   = "name"
    values = ["*hapee-ubuntu*"]
  }
  most_recent = true
  owners      = ["aws-marketplace"]
}


// Default VPC definition
resource "aws_vpc" "tf_vpc" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_hostnames = true
  tags = {
    Name = "hapee_test_vpc"
  }
}

# Create subnet in Wavelength Zone
resource "aws_subnet" "tf_wl_subnet" {
  vpc_id            = aws_vpc.tf_vpc.id
  cidr_block        = "10.0.0.0/24"
  availability_zone = var.wavelength_subnet
  tags = {
    Name = "hapee_test_subnet"
  }
}

# Create subnet in Parent Region
resource "aws_subnet" "tf_region_subnet" {
  vpc_id            = aws_vpc.tf_vpc.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = var.region_subnet
  tags = {
    Name = "hapee-region-subnet"
  }
}

# Create Internet Gateway
resource "aws_internet_gateway" "tf_internet_gw" {
  vpc_id = aws_vpc.tf_vpc.id
  tags = {
    Name = "tf Internet Gateway"
  }
}


# Define Carrier Gateway
resource "aws_ec2_carrier_gateway" "tf_carrier_gw" {
  vpc_id = aws_vpc.tf_vpc.id
  tags = {
    Name = "hapee_test_cg"
  }
}


# Create the Route Table for Region
resource "aws_route_table" "region_route_table" {
  vpc_id = aws_vpc.tf_vpc.id
  tags = {
    Name = "Region Route Table"
  }
}

# Setup Region Route Table
resource "aws_route" "region_route" {
  route_table_id         = aws_route_table.region_route_table.id
  destination_cidr_block = "0.0.0.0/0"
  gateway_id             = aws_internet_gateway.tf_internet_gw.id
}

# Associate Route Table with subnet
resource "aws_route_table_association" "region_route_association" {
  subnet_id      = aws_subnet.tf_region_subnet.id
  route_table_id = aws_route_table.region_route_table.id
}


# Define Wavelength Route Table
resource "aws_route_table" "wavelength_route_table" {
  vpc_id = aws_vpc.tf_vpc.id

  route {
    cidr_block         = "0.0.0.0/0"
    carrier_gateway_id = aws_ec2_carrier_gateway.tf_carrier_gw.id
  }

  tags = {
    Name = "hapee_test_route_table"
  }
}

# Routing table association for Wavelength subnet
resource "aws_route_table_association" "wavelength_route_association" {
  subnet_id      = aws_subnet.tf_wl_subnet.id
  route_table_id = aws_route_table.wavelength_route_table.id
}

# Security group for backend
resource "aws_security_group" "web_node_sg" {
  name        = "web_node_sg"
  description = "Instance Web SG: pass SSH, permit HTTP only from HAPEE"
  vpc_id      = aws_vpc.tf_vpc.id

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    self        = true
  }

  ingress {
    from_port       = 80
    to_port         = 80
    protocol        = "tcp"
    security_groups = ["${aws_security_group.hapee_node_sg.id}"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    self        = true
  }

  tags = {
    Name = "hapee_web_node_sg"
  }
}

# Security group for HAPEE LB nodes
resource "aws_security_group" "hapee_node_sg" {
  name        = "hapee_node_sg"
  description = "Instance HAPEE SG: pass SSH, HTTP, HTTPS and Dashboard traffic by default"
  vpc_id      = aws_vpc.tf_vpc.id

  ingress {
    from_port   = 3
    to_port     = 0
    protocol    = "icmp"
    cidr_blocks = ["0.0.0.0/0"]
    self        = true
  }

  ingress {
    from_port   = 8
    to_port     = 0
    protocol    = "icmp"
    cidr_blocks = ["0.0.0.0/0"]
    self        = true
  }

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    self        = true
  }

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    self        = true
  }

  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    self        = true
  }

  ingress {
    from_port = 0
    to_port   = 0
    protocol  = "112"
    self      = true
  }

  ingress {
    from_port   = 9022
    to_port     = 9022
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    self        = true
  }

  ingress {
    from_port   = 9023
    to_port     = 9023
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    self        = true
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    self        = true
  }

  tags = {
    Name = "hapee_node_sg"
  }
}

# IAM policy document - Assume role policy
data "aws_iam_policy_document" "instance_assume_role_policy" {
  statement {
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["ec2.amazonaws.com"]
    }
  }
}

# IAM policy document - EIP permissions policy
data "aws_iam_policy_document" "eip_policy" {
  statement {
    sid = "1"

    actions = [
      "ec2:DescribeAddresses",
      "ec2:AllocateAddress",
      "ec2:ReleaseAddress",
      "ec2:DescribeInstances",
      "ec2:AssociateAddress",
      "ec2:DisassociateAddress",
      "ec2:DescribeNetworkInterfaces",
      "ec2:AssignPrivateIpAddresses",
      "ec2:UnassignPrivateIpAddresses",
    ]

    resources = ["*"]
  }
}

# IAM role for EIP 
resource "aws_iam_role" "eip_role" {
  name               = "hapee_eip_role"
  assume_role_policy = data.aws_iam_policy_document.instance_assume_role_policy.json
}

# IAM role policy - EIP role policy
resource "aws_iam_role_policy" "eip_role_policy" {
  name   = "hapee_eip_role_policy"
  role   = aws_iam_role.eip_role.id
  policy = data.aws_iam_policy_document.eip_policy.json
}

# IAM instance profile - EIP instance profile
resource "aws_iam_instance_profile" "eip_instance_profile" {
  name = "hapee_instance_profile"
  role = aws_iam_role.eip_role.id
}


# Create AMI Mapping for Amazon Linux 2
variable "amis_linux2" {
  type = map(string)
  default = {
    "us-east-1" = "ami-0fc7dad598dd37e11"
    "us-west-2" = "ami-0045a9e5f2af86401"
  }
}

# Create launch template for backend
resource "aws_launch_template" "web_launch_template" {
  name                   = "realm_launch_template_nyc"
  image_id               = lookup(var.amis_linux2, var.aws_region)
  instance_type          = var.aws_web_instance_type
  key_name               = var.key_name
  vpc_security_group_ids = ["${aws_security_group.web_node_sg.id}"]
}

# Create auto scaling group for backend
resource "aws_autoscaling_group" "asg_web" {
  name                = "HAPEE Backend ASG"
  vpc_zone_identifier = [aws_subnet.tf_wl_subnet.id]
  max_size            = 3
  min_size            = 0
  desired_capacity    = 2
  launch_template {
    id      = aws_launch_template.web_launch_template.id
    version = "$Latest"
  }
  tag {
    key                 = "Name"
    value               = "hapee_web_node"
    propagate_at_launch = true
  }
  tag {
    key                 = "HAProxy:Service:Name"
    value               = "wavelength-test"
    propagate_at_launch = true
  }
  tag {
    key                 = "HAProxy:Service:Port"
    value               = 80
    propagate_at_launch = true
  }
  tag {
    key                 = "HAProxy:Instance:Port"
    value               = 80
    propagate_at_launch = true
  }
  tag {
    key                 = "wavelength-target"
    value               = "true"
    propagate_at_launch = true
  }
}

# Instance definition for HAPEE LB nodes
resource "aws_instance" "hapee_node" {
  count = 1

  instance_type        = var.aws_hapee_instance_type
  ami                  = data.aws_ami.hapee_aws_amis.id
  key_name             = var.key_name
  iam_instance_profile = aws_iam_instance_profile.eip_instance_profile.id

  vpc_security_group_ids = ["${aws_security_group.hapee_node_sg.id}"]
  subnet_id              = aws_subnet.tf_wl_subnet.id

  user_data = filebase64("lb-config.sh")

  tags = {
    Name = "hapee_lb_node"
  }
}

# Carrier IP allocation for each HAPEE LB instance
resource "aws_eip" "hapee_node_eip1" {
  count                = var.backend_size
  network_interface    = element(aws_instance.hapee_node.*.primary_network_interface_id, count.index)
  vpc                  = true
  network_border_group = var.wavelength_subnet
}
