#!/bin/bash
echo "stopping sync-gateway wavelength..."
docker stop sync-gateway-wavelength
echo "deleting sync-gateway wavelength..."
docker rm sync-gateway-wavelength
echo "Running sync-gateway wavelength.."
docker run -p 4984-4985:4984-4985 --network demo --name sync-gateway-wavelength -d -v `pwd`/sync-gateway-config.json:/etc/sync_gateway/sync_gateway.json couchbase/sync-gateway:2.8.2-enterprise -adminInterface :4985 /etc/sync_gateway/sync_gateway.json

