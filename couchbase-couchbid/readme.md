# Wavelength with Couchbase Demo  

## Development Requirements

- Docker setup and running on your computer
- Visual Studio for Windows or Mac with Xamarin installed

## Couchbase Server

These scripts are compatible with Couchbase Server 7.02 and Sync Gateway 2.8.2.  

### Local Debugging Environment Setup

To run locally you need to use the scripts found in the env-setup directory in this order:

- docker-create-bridge.sh
- docker-cb-server.sh
- docker-init-cluster.sh
- init-docker-env-data.sh
- sync-gateway.sh
 
### Local Debugging of ASP.NET WebAPI and Xamarin App 

TODO - explain process

## Sync Gateway

You can use the logs-sync-gateway.sh to validate the configuration is working with Sync Gateway.

## Cloudformation Template
Provided `cloudformation.template.yaml` creates an example stack that can be deployed to an edge zone.
The stack depends on 2 pre-existing subnets -- one in a regular aws zone (cloud subnet) and one in an edge zone (edge subnet).
The following components will be created:
- a 3-instance couchbase cluster: `couchbase-node-001 ... couchbase-node-003`
- `couchbid` bucket used to store application data
- Couchbid API instance: `cloud-couchbid-api`
- Cloud Sync Gateway instance: `cloud-sync-gateway`
- Edge Sync Gateway instance: `edge-sync-gateway`

The template uses simplified security model and is not intended to be used for production deployments.

