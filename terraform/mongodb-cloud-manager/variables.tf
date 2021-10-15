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

variable "keyName" {
  type        = string
  description = "Name of your AWS key"
  default     = "test_key"
}

variable "mmsGroupId" {
  type        = string
  description = "Name of your MongoDB organization ID"
  sensitive   = true
}

variable "mmsApiKey" {
  type        = string
  description = "Your MongoDB API key"
  sensitive   = true
}

variable "atlas_public_key" {
  type        = string
  description = "Your MongoDB Atlas public key"
  sensitive   = true
}

variable "atlas_private_key" {
  type        = string
  description = "Your MongoDB Atlas private key"
  sensitive   = true
}

variable "agentNodeCount" {
  type        = number
  description = "MongoDB cluster size"
  default     = 3
}

variable "agentInstanceType" {
  type        = string
  description = "Instance type of agent nodes"
  default     = "t3.medium"
  validation {
    condition     = contains(["t3.medium", "t3.xlarge", "r5.2xlarge", "g4dn.2xlarge"], var.agentInstanceType)
    error_message = "Valid values for instance types are t3.medium, t3.xlarge, r5.2xlarge, and g4dn.2xlarge."
  }
}