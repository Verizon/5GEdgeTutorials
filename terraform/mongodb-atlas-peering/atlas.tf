# Initialize MongoDB Atlas Provider
provider "mongodbatlas" {
  public_key  = var.atlas_public_key
  private_key = var.atlas_private_key
}

# Create MongoDB Atlas project
# resource "mongodbatlas_project" "wavelength_project" {
#   name   = "wavelength-project"
#   org_id = var.atlas_org_id
# }



resource "mongodbatlas_cluster" "cluster-test" {
  project_id   = var.group_id
  name         = "Wavelength-Cluster"
  cluster_type = "REPLICASET"
  replication_specs {
    num_shards = 1
    regions_config {
      region_name     = var.altas_region
      electable_nodes = 3
      priority        = 7
      read_only_nodes = 0
    }
  }
  cloud_backup      = true
  auto_scaling_disk_gb_enabled = true
  mongo_db_major_version       = "5.0"

  //Provider Settings "block"
  provider_name               = "AWS"
  disk_size_gb                = 2
  provider_instance_size_name = "M10"
}

resource "mongodbatlas_database_user" "db_user" {
  username = var.atlas_dbuser
  password = var.atlas_dbpassword
  auth_database_name = "admin"
  project_id = var.group_id # mongodbatlas_project.wavelength_project.id
  roles{
    role_name = "readWrite"
    database_name = "admin"
  }
}

resource "mongodbatlas_network_container" "atlas_container" {
  atlas_cidr_block = var.atlas_vpc_cidr
  project_id = var.group_id # mongodbatlas_project.wavelength_project.id
  provider_name = "AWS"
  region_name = var.altas_region
}

resource "mongodbatlas_network_peering" "aws-atlas" {
  accepter_region_name = var.region
  project_id = var.group_id # mongodbatlas_project.wavelength_project.id
  container_id = mongodbatlas_network_container.atlas_container.container_id
  provider_name = "AWS"
  route_table_cidr_block = aws_vpc.tf-vpc.cidr_block
  vpc_id = aws_vpc.tf-vpc.id
  aws_account_id = var.aws_account_id
} 

## After Atlas Cluster launches, run init.sh to populate user data for EC2 instance
##resource "null_resource" "init_db_sync" {
##  provisioner "local-exec" {
##    command = "./init.sh"
##  }
##  depends_on = [ #Make sure that Atlas cluster is called before init.sh (because it requires DB to exist)
##    mongodbatlas_cluster.cluster-test
##  ]
## }
