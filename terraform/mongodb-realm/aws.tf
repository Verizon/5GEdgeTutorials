
provider "aws" {
  profile = "default"
  region  = "us-east-1"
}


# Create the VPC
resource "aws_vpc" "tf-vpc" {
  cidr_block           = "10.0.0.0/16"
  instance_tenancy     = "default"
  enable_dns_support   = true
  enable_dns_hostnames = true
  tags = {
    Name = "wavelength-vpc"
  }
  depends_on = [ #Make sure that Atlas cluster and ./init.sh have both executed
    null_resource.init_db_sync
  ]
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

# Create subnet for each wavelength zone
resource "aws_subnet" "wavelength_subnets" {
  for_each = var.wavelength_zones

  vpc_id = aws_vpc.tf-vpc.id

  cidr_block           = each.value.cidr_block
  availability_zone_id = each.value.availability_zone_id

  tags = {
    Name = "wavelength-subnet-${each.key}"
  }
}

# Create subnet for each availability zone
resource "aws_subnet" "region_subnets" {
  for_each = var.availability_zones

  vpc_id = aws_vpc.tf-vpc.id

  cidr_block           = each.value.cidr_block
  availability_zone_id = each.value.availability_zone_id

  tags = {
    Name = "region-subnet-${each.key}"
  }
}

# Create region route table
resource "aws_route_table" "region_route_table" {
  vpc_id = aws_vpc.tf-vpc.id
  tags = {
    Name = "Region Route Table"
  }
}

# Create region default route
resource "aws_route" "region_route" {
  route_table_id         = aws_route_table.region_route_table.id
  destination_cidr_block = "0.0.0.0/0"
  gateway_id             = aws_internet_gateway.tf_internet_gw.id
}

# Associate region route table
resource "aws_route_table_association" "region_route_associations" {
  for_each       = var.availability_zones
  subnet_id      = aws_subnet.region_subnets[each.key].id
  route_table_id = aws_route_table.region_route_table.id
}

# Create Wavelength route table
resource "aws_route_table" "WLZ_route_table" {
  vpc_id = aws_vpc.tf-vpc.id
  tags = {
    Name = "Wavelength Zone Route Table"
  }
}

# Create Wavelength default route
resource "aws_route" "WLZ_route" {
  route_table_id         = aws_route_table.WLZ_route_table.id
  destination_cidr_block = "0.0.0.0/0"
  carrier_gateway_id     = aws_ec2_carrier_gateway.tf_carrier_gateway.id
}

# Associate Wavelength route table
resource "aws_route_table_association" "wavelength_route_associations" {
  for_each       = var.wavelength_zones
  subnet_id      = aws_subnet.wavelength_subnets[each.key].id
  route_table_id = aws_route_table.WLZ_route_table.id
}

# Create AMI Mapping for Amazon Linux 2
variable "amis" {
  type = map(string)
  default = {
    "us-east-1" = "ami-04d29b6f966df1537"
    "us-west-2" = "ami-0e472933a1395e172"
  }
}

# Create security group
resource "aws_security_group" "edge_security_group" {
  vpc_id      = aws_vpc.tf-vpc.id
  name        = "realm-sg"
  description = "Security group for Realm instance at the edge"

  ingress {
    cidr_blocks = ["0.0.0.0/0"]
    protocol    = "-1"
    from_port   = 0
    to_port     = 0
  }
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  tags = {
    Name = "Realm Edge Security Group"
  }
}

# Create launch template for edge deployment
resource "aws_launch_template" "realm_launch_template" {
  name          = "realm_launch_template_nyc"
  image_id      = lookup(var.amis, var.region)
  instance_type = var.realm_instance_size
  key_name      = var.key_pair
  user_data     = filebase64("./db_sync_setup.sh")
  network_interfaces {
    associate_carrier_ip_address = true
    device_index                 = 0
    security_groups              = [aws_security_group.edge_security_group.id]
  }
  tags = {
    Name = "Realm Launch Template"
  }
}

# Create auto scaling group for WLZ resources
resource "aws_autoscaling_group" "asg" {
  for_each            = var.wavelength_zones
  desired_capacity    = each.value.asg_size
  max_size            = 5
  min_size            = 0
  vpc_zone_identifier = [for subnet in aws_subnet.wavelength_subnets : subnet.id]

  launch_template {
    id      = aws_launch_template.realm_launch_template.id
    version = "$Latest"
  }
  tag {
    key                 = "Name"
    value               = "Wavelength-Node-${each.key}"
    propagate_at_launch = true
  }
  tag {
    key                 = "app"
    value               = "wavelength-mongodb-realm"
    propagate_at_launch = true
  }
}

# After Realm instances launches, populate the edge discovery service
resource "null_resource" "init_EDS" {
  provisioner "local-exec" {
    command = "./edsInit.sh"
    environment = {
      appKey = var.appKey
      secretKey = var.secretKey
  }
  }
  depends_on = [ #Make sure that Realm ASG is created
    aws_autoscaling_group.asg
  ]
}
