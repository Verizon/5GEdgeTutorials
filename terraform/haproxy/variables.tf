variable "aws_region" {
  description = "This is the AWS region for your deployment; must be either us-east-1 or us-west-2 to support Wavelength Zones."
  default     = "us-east-1"
  type        = string
}

variable "wavelength_subnet" {
  description = "This is the AWS Wavelength Zone for your deployment."
  default     = "us-east-1-wl1-nyc-wlz-1"
  type        = string
}

variable "region_subnet" {
  description = "This is the AWS Availability Zone for your deployment."
  default     = "us-east-1a"
  type        = string
}

// Load balancer instance type
variable "aws_hapee_instance_type" {
  description = "Default AWS instance type for HAPEE node; must be t3.medium, t3.xlarge, or r5.2xlarge to support AWS Wavelength."
  default     = "t3.medium"
  type        = string
}

// Backend instance type
variable "aws_web_instance_type" {
  description = "Default AWS instance type for HAPEE node; must be t3.medium, t3.xlarge, or r5.2xlarge to support AWS Wavelength."
  default     = "t3.medium"
}

// Key pair
variable "key_name" {
  description = "SSH key pair to use for AWS instances."
  default     = "test_key"
  type        = string
}

// Backend ASG Size
variable "backend_size" {
  description = "Size of backend Auto Scaling group"
  default     = 1
}
