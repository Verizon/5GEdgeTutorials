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
