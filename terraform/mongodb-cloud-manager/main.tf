// The contents of this file are Copyright (c) 2021 Verizon.
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is in the root directory.

provider "aws" {
  profile = "default"
  region  = var.region
}


resource "aws_security_group" "AgentNode" {
  name        = "agent_sg"
  vpc_id      = aws_vpc.tf-vpc.id
  description = "Allow ingress for RS"

  ingress = [
    {
      description      = "SSH Traffic"
      from_port        = 22
      to_port          = 22
      protocol         = "tcp"
      cidr_blocks      = ["0.0.0.0/0"]
      ipv6_cidr_blocks = null
      prefix_list_ids  = null
      security_groups  = null
      self             = null
    },
    {
      description      = "RDP"
      from_port        = 3389
      to_port          = 3389
      protocol         = "tcp"
      cidr_blocks      = ["0.0.0.0/0"]
      ipv6_cidr_blocks = null
      prefix_list_ids  = null
      security_groups  = null
      self             = null
    },
    {
      description      = "MongoDB Connection"
      from_port        = 27017
      to_port          = 27017
      protocol         = "tcp"
      cidr_blocks      = ["0.0.0.0/0"]
      ipv6_cidr_blocks = null
      prefix_list_ids  = null
      security_groups  = null
      self             = null
    }
  ]
  egress = [
    {
      description      = "All Ports/Protocols"
      from_port        = 0
      to_port          = 0
      protocol         = "-1"
      cidr_blocks      = ["0.0.0.0/0"]
      ipv6_cidr_blocks = ["::/0"]
      prefix_list_ids  = null
      security_groups  = null
      self             = null
    }
  ]
}


# Template File for MongoDB User Data for CloudManager Creation
data "template_file" "user_data" {
  template = file("./userDataCloudManager.tpl")
  vars = {
    mmsGroupId = var.mmsGroupId
    mmsApiKey = var.mmsApiKey
  }
}

# Create AMI Mapping for Wavelength Zone (Amazon Linux 2)
variable "wavelength_ami" {
  type = map(string)
  default = {
    "us-east-1" = "ami-0c2b8ca1dad447f8a"
    "us-west-2" = "ami-083ac7c7ecf9bb9b0"
  }
}

resource "aws_instance" "AgentNode" {
  ami                    = lookup(var.wavelength_ami, var.region)
  instance_type          = var.agentInstanceType
  subnet_id              = aws_subnet.tf_wl_subnet.id
  vpc_security_group_ids = [aws_security_group.AgentNode.id]
  key_name               = var.keyName
  count                  = var.agentNodeCount
  user_data_base64       = base64encode(data.template_file.user_data.rendered)
  tags = {
    Name = "MongoDB Node"
  }
}


# After Atlas Cluster launches, run createCluster.sh to create MongoDB instance
resource "null_resource" "init_db" {
  provisioner "local-exec" {
    command = "./createCluster.sh"
    environment = {
      mmsGroupId = var.mmsGroupId
      mmsApiKey = var.mmsApiKey
      atlas_private_key = var.atlas_private_key
      atlas_public_key = var.atlas_public_key
      node_a = aws_instance.AgentNode[0].private_dns
      node_b = aws_instance.AgentNode[1].private_dns
      node_c = aws_instance.AgentNode[2].private_dns
   }
  }
  depends_on = [ #Make sure that Atlas cluster is called before init.sh (because it requires DB to exist)
    aws_instance.AgentNode[0],
    aws_instance.AgentNode[1],
    aws_instance.AgentNode[2]
  ]
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
  name   = "mongodb_eip_role_policy"
  role   = aws_iam_role.eip_role.id
  policy = data.aws_iam_policy_document.eip_policy.json
}

# IAM instance profile - EIP instance profile
resource "aws_iam_instance_profile" "eip_instance_profile" {
  name = "mongodb_instance_profile"
  role = aws_iam_role.eip_role.id
}

# Carrier IP allocation for each MongoDB instance
resource "aws_eip" "mongodb_node_eip" {
  count                = var.agentNodeCount
  network_interface    = element(aws_instance.AgentNode.*.primary_network_interface_id, count.index)
  vpc                  = true
  network_border_group = var.wavelength_zone
}
