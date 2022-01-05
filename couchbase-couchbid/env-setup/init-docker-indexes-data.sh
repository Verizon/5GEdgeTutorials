#!/bin/bash
# create the cluster

#create the bucket via REST API
curl -v -X POST http://localhost:8091/pools/default/buckets \
-u Administrator:password \
-d name=wavelength \
-d ramQuotaMB=512

sleep 2
#add sync gateway user
docker exec -it cb-server-7 couchbase-cli user-manage \
--cluster http://127.0.0.1 \
--username Administrator \
--password password \
--set \
--rbac-username syncgateway \
--rbac-password password \
--roles mobile_sync_gateway[*] \
--auth-domain local

#create indexes

#main index for main screen getting auctions
sleep 8
curl -v http://localhost:8093/query/service \
  -u Administrator:password \
  -d "statement=CREATE INDEX idx_wavelength_auction_active on wavelength(isActive, documentType) WHERE documentType='auction' AND isActive = true" 

#index for calculating any auctions that need to be closed
sleep 4
curl -v http://localhost:8093/query/service \
    -u Administrator:password \
    -d "statement=CREATE INDEX adv_stopTime_documentType_isActive_isWinnerCalculated ON wavelength(stopTime) WHERE documentType = 'auction' AND isActive = true AND isWinnerCalculated = false"

sleep 5 
curl -v http://localhost:8093/query/service \
  -u Administrator:password \
  -d "statement=CREATE INDEX idx_wavelength_document_type_auction_isWinnerCalculated on wavelength(isActive, stopTime, isWinnerCalculated) WHERE documentType='auction'"

#load sample data
sleep 5
curl -v http://localhost:8093/query/service  \
  -u Administrator:password \
  -d "statement=CREATE INDEX idx_locationName_received_documentType_bid ON wavelength(locationName, received, documentType) WHERE documentType='bid' AND locationName='AWS Wavelength - Verizon'"

docker cp sample-documents-server.json \
cb-server-7:/sample-documents-server.json

docker exec -it cb-server-7 /opt/couchbase/bin/cbimport json --format list \
  -c http://localhost:8091 -u Administrator -p password \
  -d "file:///sample-documents-server.json" -b 'wavelength' -g %documentId%