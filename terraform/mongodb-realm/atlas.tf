# Initialize MongoDB Atlas Provider
provider "mongodbatlas" {
  public_key  = var.atlas_public_key
  private_key = var.atlas_private_key
}

#Create MongoDB Atlas project
# resource "mongodbatlas_project" "realm_project" {
#   name   = "realm-project"
#   org_id = var.atlas_org_id
# }

resource "mongodbatlas_cluster" "cluster-test" {
  project_id   = var.group_id
  name         = "Wavelength-IoT-Demo-Cluster"
  cluster_type = "REPLICASET"
  replication_specs {
    num_shards = 1
    regions_config {
      region_name     = "US_EAST_1"
      electable_nodes = 3
      priority        = 7
      read_only_nodes = 0
    }
  }
  provider_backup_enabled      = true
  auto_scaling_disk_gb_enabled = true
  mongo_db_major_version       = "4.4"

  //Provider Settings "block"
  provider_name               = "AWS"
  disk_size_gb                = 2
  provider_instance_size_name = "M10"
}

## After Atlas Cluster launches, run init.sh to populate user data for EC2 instance
resource "null_resource" "init_db_sync" {
  provisioner "local-exec" {
    command = "./init.sh"
  }
  depends_on = [ #Make sure that Atlas cluster is called before init.sh (because it requires DB to exist)
    mongodbatlas_cluster.cluster-test
  ]
}
