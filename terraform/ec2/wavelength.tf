// The contents of this file are Copyright (c) 2021 Verizon. 
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is in the root directory.

# Create the VPC
resource "aws_vpc" "tf-vpc" {
  cidr_block           = "10.0.0.0/16"
  instance_tenancy     = "default"
  enable_dns_support   = true
  enable_dns_hostnames = true
  tags = {
    Name = "wavelength-vpc"
  }
}

# Create subnet in parent region
resource "aws_subnet" "tf_region_subnet" {
  vpc_id            = aws_vpc.tf-vpc.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = var.availability_zone
  tags = {
    Name = "wavelength-region-subnet"
  }
}

# Create subnet in Wavelength Zone
resource "aws_subnet" "tf_wl_subnet" {
  vpc_id            = aws_vpc.tf-vpc.id
  cidr_block        = "10.0.2.0/24"
  availability_zone = var.wavelength_zone
  tags = {
    Name = "wavelength-edge-subnet"
  }
}

# Create security group for parent region resources
resource "aws_security_group" "parent_security_group" {
  vpc_id      = aws_vpc.tf-vpc.id
  name        = "bastion-sg"
  description = "Security group for bastion host in parent region"

  ingress {
    cidr_blocks = ["0.0.0.0/0"]
    protocol    = "tcp"
    from_port   = 22
    to_port     = 22
  }
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  tags = {
    Name = "wavelength-bastion-sg"
  }
}

# Create security group for edge resources
resource "aws_security_group" "edge_security_group" {
  vpc_id      = aws_vpc.tf-vpc.id
  name        = "edge-sg"
  description = "Security group for Wavelength Zone resources"
  ingress {
    security_groups = [aws_security_group.parent_security_group.id]
    protocol        = "tcp"
    from_port       = 22
    to_port         = 22
  }
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  tags = {
    Name = "wavelength-edge-sg"
  }
}


# Create Internet Gateway
resource "aws_internet_gateway" "tf_internet_gw" {
  vpc_id = aws_vpc.tf-vpc.id
  tags = {
    Name = "tf Internet Gateway"
  }
}

# Create Carrier Gateway
resource "aws_ec2_carrier_gateway" "tf_carrier_gateway" {
  vpc_id = aws_vpc.tf-vpc.id
  tags = {
    Name = "tf-carrier-gw"
  }
}

# Create the Route Table for Region
resource "aws_route_table" "region_route_table" {
  vpc_id = aws_vpc.tf-vpc.id
  tags = {
    Name = "Region Route Table"
  }
}

# Create the Route Table for Wavelength Zone
resource "aws_route_table" "WLZ_route_table" {
  vpc_id = aws_vpc.tf-vpc.id
  tags = {
    Name = "Wavelength Zone Route Table"
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

# Setup WLZ Route Table
resource "aws_route" "WLZ_route" {
  route_table_id         = aws_route_table.WLZ_route_table.id
  destination_cidr_block = "0.0.0.0/0"
  carrier_gateway_id     = aws_ec2_carrier_gateway.tf_carrier_gateway.id
}

# Associate Route Table with subnet for WLZ
resource "aws_route_table_association" "wavelength_route_association" {
  subnet_id      = aws_subnet.tf_wl_subnet.id
  route_table_id = aws_route_table.WLZ_route_table.id
}


# Create AMI Mapping for Bastion Host (Amazon Linux 2)
variable "bastion_ami" {
  type = map(string)
  default = {
    "us-east-1" = "ami-02e136e904f3da870"
    "us-west-2" = "ami-013a129d325529d4d"
  }
}

# Create AMI Mapping for Wavelength Zone (Amazon Linux 2)
variable "wavelength_ami" {
  type = map(string)
  default = {
    "us-east-1" = "ami-0fc7dad598dd37e11"
    "us-west-2" = "ami-0045a9e5f2af86401"
  }
}

# Create EC2 instance in Wavelength Zone
resource "aws_instance" "edge_instance" {
  ami             = lookup(var.wavelength_ami, var.region)
  instance_type   = var.instance_type
  subnet_id       = aws_subnet.tf_wl_subnet.id
  key_name        = var.key_pair
  security_groups = [aws_security_group.edge_security_group.id]
  tags = {
    Name = "wavelength-edge-instance"
  }
}

# Create Carrier IP address in Wavelength Zone
resource "aws_eip" "tf-wavelength-ip" {
  vpc                  = true
  network_border_group = var.wavelength_zone
}

# Attach Carrier IP address to Wavelength Zone instance
resource "aws_eip_association" "eip_assoc" {
  instance_id   = aws_instance.edge_instance.id
  allocation_id = aws_eip.tf-wavelength-ip.id
}

# Create Bastion Host
resource "aws_instance" "bastion_host_instance" {
  count                       = var.bastion_host ? 1 : 0
  ami                         = lookup(var.bastion_ami, var.region)
  instance_type               = var.instance_type
  subnet_id                   = aws_subnet.tf_region_subnet.id
  key_name                    = var.key_pair
  security_groups             = [aws_security_group.parent_security_group.id]
  associate_public_ip_address = true
  tags = {
    Name = "wavelength-bastion-host"
  }
}