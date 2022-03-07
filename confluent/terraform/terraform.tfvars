cluster_name      = "wavelength"

# SSH Key (must already exist and be in the desired region
worker_key_name = "my-key-name"
domain          = "lab.com"
# zoneid          = "testzoneid"

# Update according to the desired WL Zones
wavelength_zones = {
  nyc = {
    availability_zone = "us-east-1-wl1-nyc-wlz-1",
    availability_zone_id = "use1-wl1-nyc-wlz1",
    worker_nodes = 2,
    cidr_block = "10.0.10.0/24"
    nodeport_offset = 30100
  },
  bos = {
    availability_zone = "us-east-1-wl1-bos-wlz-1",
    availability_zone_id = "use1-wl1-bos-wlz1",
    worker_nodes = 2,
    cidr_block = "10.0.11.0/24"
    nodeport_offset = 30200
  },
  was = {
    availability_zone = "us-east-1-wl1-was-wlz-1",
    availability_zone_id = "use1-wl1-was-wlz1",
    worker_nodes = 2,
    cidr_block = "10.0.12.0/24"
    nodeport_offset = 30300
  },
  atl = {
    availability_zone = "us-east-1-wl1-atl-wlz-1",
    availability_zone_id = "use1-wl1-atl-wlz1",
    worker_nodes = 2,
    cidr_block = "10.0.13.0/24"
    nodeport_offset = 30400
  },
  mia = {
    availability_zone = "us-east-1-wl1-mia-wlz-1",
    availability_zone_id = "use1-wl1-mia-wlz1",
    worker_nodes = 2,
    cidr_block = "10.0.14.0/24"
    nodeport_offset = 30500
  },
}