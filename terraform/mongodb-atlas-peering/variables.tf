variable "profile" {
  type        = string
  description = "AWS Credentials Profile to use"
  default     = "default"
}

variable "region" {
  type        = string
  default     = "us-east-1"
  description = "This is the AWS region."
}

variable "aws_account_id" {
  description = "My AWS Account ID"
}



variable "key_pair" {
  type        = string
  default     = "test_key"
  description = "This is the name of an existing EC2 Key Pair"
}

variable "atlas_org_id" {
  type        = string
  description = "This is the MongoDB Atlas Organization ID, found in Organization Settings in the Console"
}

variable "atlas_public_key" {
  type        = string
  description = "This is the public key for the MongoDB Atlas API"
  sensitive   = true
}

variable "atlas_private_key" {
  type        = string
  description = "This is the private key for the MongoDB Atlas API"
  sensitive   = true
}

variable "group_id" {
  type        = string
  description = "This is the project ID for your MongoDB Atlas"
  sensitive   = true
}

variable "atlas_vpc_cidr" {
  description = "Atlas project CIDR"
}

variable "altas_region" {
  type        = string
  default     = "US_EAST_1"
  description = "This is the Atlas AWS region."
}



variable "app_instance_size" {
  type        = string
  default     = "t3.medium"
  description = "This is the instance type for the app edge instances"
}

variable "atlas_dbuser" {
  type        = string
  default     = "appUser"
  description = "This is the database user for the atlas cluster"
}

variable "atlas_dbpassword" {
  type        = string
  description = "This is the database password for the atlas cluster"
  sensitive   = true
}



variable "wavelength_zones" {
  description = "This is the wavelength zone subnet metadata"
  default = {
    nyc = {
      availability_zone    = "us-east-1-wl1-nyc-wlz-1",
      availability_zone_id = "use1-wl1-nyc-wlz1",
      asg_size             = 1,
      cidr_block           = "10.0.2.0/24"
    },
    bos = {
      availability_zone    = "us-east-1-wl1-bos-wlz-1",
      availability_zone_id = "use1-wl1-bos-wlz1",
      asg_size             = 1,
      cidr_block           = "10.0.3.0/24"
    },
    was = {
      availability_zone    = "us-east-1-wl1-was-wlz-1",
      availability_zone_id = "use1-wl1-was-wlz1",
      asg_size             = 0,
      cidr_block           = "10.0.4.0/24"
    },
    atl = {
      availability_zone    = "us-east-1-wl1-atl-wlz-1",
      availability_zone_id = "use1-wl1-atl-wlz1",
      asg_size             = 0,
      cidr_block           = "10.0.5.0/24"
    },
    mia = {
      availability_zone    = "us-east-1-wl1-mia-wlz-1",
      availability_zone_id = "use1-wl1-mia-wlz1",
      asg_size             = 0,
      cidr_block           = "10.0.6.0/24"
    },
  }
}

variable "availability_zones" {
  description = "This is the availability zone subnet metadata"
  default = {
    az1 = {
      availability_zone_id = "use1-az1"
      cidr_block           = "10.0.0.0/24"
    },
    az2 = {
      availability_zone_id = "use1-az2"
      cidr_block           = "10.0.1.0/24"
    }
  }
}
