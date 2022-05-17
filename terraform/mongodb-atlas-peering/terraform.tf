terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "3.51.0"
    }
    mongodbatlas = {
      source  = "mongodb/mongodbatlas"
      version = "1.3.1"
    }
  }
}

