variable "profile" {
  type        = string
  description = "The name of your AWS crendential profile, found most often in ~/.aws/credentials"
  default     = "default"
}

variable "region" {
  type        = string
  default     = "us-east-1"
  description = "The AWS region to deploy your Wavelength configuration."
  validation {
    condition     = contains(["us-east-1", "us-west-2"], var.region)
    error_message = "Valid values for regions supporting Wavelength Zones are us-east-1 and us-west-2."
  }
}

variable "wavelength_zone" {
  type        = string
  default     = "us-east-1-wl1-bos-wlz-1"
  description = "The Wavelength Zone for your EC2 instance."
  validation {
    condition     = contains(["us-east-1-wl1-bos-wlz-1", "us-east-1-wl1-nyc-wlz-1", "us-east-1-wl1-was-wlz-1", "us-east-1-wl1-atl-wlz-1", "us-east-1-wl1-mia-wlz-1", "us-east-1-wl1-dfw-wlz-1", "us-east-1-wl1-iah-wlz-1", "us-east-1-wl1-chi-wlz-1", "us-west-2-wl1-sfo-wlz-1", "us-west-2-wl1-sea-wlz-1", "us-west-2-wl1-las-wlz-1", "us-west-2-wl1-phx-wlz-1", "us-west-2-wl1-den-wlz-1"], var.wavelength_zone)
    error_message = "Please enter a valid Wavelength Zone."
  }
}

variable "availability_zone" {
  type        = string
  default     = "us-east-1a"
  description = "The Availability Zone for your EC2 instance."
}

variable "instance_type" {
  type    = string
  default = "t3.medium"
  validation {
    condition     = contains(["t3.medium", "t3.xlarge", "r5.2xlarge", "g4dn.2xlarge"], var.instance_type)
    error_message = "Valid values for instance types are t3.medium, t3.xlarge, r5.2xlarge, and g4dn.2xlarge."
  }
}

variable "key_pair" {
  type        = string
  default     = "test_key"
  description = "The name of your EC2 key pair."
}

variable "bastion_host" {
  type        = bool
  default     = true
  description = "Indicate whether bastion host is desired to connect to Wavelength Zone instance."
}




