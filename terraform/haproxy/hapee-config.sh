Content-Type: multipart/mixed; boundary="//"
MIME-Version: 1.0

--//
Content-Type: text/cloud-config; charset="us-ascii"
MIME-Version: 1.0
Content-Transfer-Encoding: 7bit
Content-Disposition: attachment; filename="cloud-config.txt"

#cloud-config
cloud_final_modules:
- [scripts-user, always]

--//
Content-Type: text/x-shellscript; charset="us-ascii"
MIME-Version: 1.0
Content-Transfer-Encoding: 7bit
Content-Disposition: attachment; filename="userdata.txt"

#!/bin/bash
exec > >(tee /var/log/user-data.log|logger -t user-data -s 2>/dev/console) 2>&1
cd /home/ubuntu
touch test.txt
cat <<EOF>>hapee-lb.cfg
# _md5hash=8eb1c7699cef2a85744582af0f48318d
# _version=6
# Dataplaneapi managed File
# changing file directly can cause a conflict if dataplaneapi is running
# Dataplaneapi managed File
# changing file directly can cause a conflict if dataplaneapi is running
# Dataplaneapi managed File
# changing file directly can cause a conflict if dataplaneapi is running
# Dataplaneapi managed File
# changing file directly can cause a conflict if dataplaneapi is running
# Dataplaneapi managed File
# changing file directly can cause a conflict if dataplaneapi is running
#---------------------------------------------------------------------
# Example configuration for a possible web application.
# The full configuration documentation is available online:
#    https://www.haproxy.com/documentation/hapee/
# Or in the hapee-2.2r1-lb-doc package.
#---------------------------------------------------------------------

# ---------------------------------------------------------------------
# Process-global settings
# ---------------------------------------------------------------------
global
  # module-load        hapee-lb-update.so
  # module-load        hapee-lb-sanitize.so
  daemon
  chroot /var/empty
  user hapee-lb
  group hapee
  pidfile /var/run/hapee-2.2/hapee-lb.pid
  stats socket /var/run/hapee-2.2/hapee-lb.sock user hapee-lb group hapee mode 660 level admin expose-fd listeners
  stats timeout 10m
  log 127.0.0.1 local0
  log 127.0.0.1 local1 notice
  log-send-hostname
  module-path /opt/hapee-2.2/modules

# ---------------------------------------------------------------------
# Common defaults that all the 'listen' and 'backend' sections will
# use if not designated in their block
# ---------------------------------------------------------------------
defaults
  mode http
  log global
  option httplog
  option redispatch
  option dontlognull
  option forwardfor except 127.0.0.0/8
  timeout connect 10s
  timeout client 300s
  timeout server 300s
  retries 3

userlist hapee-dataplaneapi
  user admin insecure-password admin

# ---------------------------------------------------------------------
# main frontend which forwards to the backends
# ---------------------------------------------------------------------
frontend fe_main
  bind *:80 # direct HTTP access
  use_backend aws-us-east-1-my-service-wavelength-test-80

backend aws-us-east-1-my-service-wavelength-test-80


# ---------------------------------------------------------------------
# backend dedicated to serve the statistics page
# ---------------------------------------------------------------------
backend be_stats
  stats uri /hapee-stats
EOF

# set -a
# . ./secret.tfvars
# set +a
# echo $access_key
# echo $secret_key


cat << EOF >> dataplaneapi.hcl
config_version = 2

name = "upward_civet"

mode = "single"

status = ""

dataplaneapi {
  host = "0.0.0.0"
  port = 5556

  userlist {
    userlist = "hapee-dataplaneapi"
  }

  resources {
    maps_dir      = "/etc/hapee-2.2/maps"
    ssl_certs_dir = "/etc/hapee-2.2/ssl"
    spoe_dir      = "/etc/hapee-2.2/spoe"
  }

  advertised {
    api_address = ""
    api_port    = 0
  }
}

haproxy {
  config_file = "/etc/hapee-2.2/hapee-lb.cfg"
  haproxy_bin = "/opt/hapee-2.2/sbin/hapee-lb"

  reload {
    reload_delay = 5
    reload_cmd   = "/bin/systemctl reload hapee-2.2-lb"
    restart_cmd  = "/bin/systemctl restart hapee-2.2-lb"
  }
}

service_discovery {
  aws_regions = [
    {
      AccessKeyID = "<your-access-key>"

      Allowlist = [
        {
          Key   = "tag-key"
          Value = "wavelength-target"
        },
      ]

      Description                = "Production environment"
      Enabled                    = true
      ID                         = "4e6b04b7-f80d-479d-9f4c-dafecd5de079"
      IPV4Address                = "private"
      Name                       = "my-service"
      Region                     = "us-east-1"
      RetryTimeout               = 10
      SecretAccessKey            = "<your-secret-key>"
      ServerSlotsBase            = 10
      ServerSlotsGrowthIncrement = 0
      ServerSlotsGrowthType      = "exponential"
    },
  ]
}

log {
  log_to    = "stdout"
  log_level = "trace"
}
EOF

# systemctl stop apt-daily.service
# systemctl kill --kill-who=all apt-daily.service
# systemctl stop apt-daily.timer
# systemctl stop apt-daily-upgrade.timer

sudo cp hapee-lb.cfg /etc/hapee-2.2/
sudo cp dataplaneapi.hcl /etc/hapee-extras/

sudo systemctl enable hapee-extras-dataplaneapi
sudo systemctl restart hapee-extras-dataplaneapi
sudo systemctl restart hapee-2.2-lb.service
