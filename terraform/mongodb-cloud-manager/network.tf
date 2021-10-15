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
