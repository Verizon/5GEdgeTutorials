curl -v -X POST http://localhost:8091/pools/default/buckets -u Administrator:password -d name=wavelength -d ramQuotaMB=512

timeout /t 2 
docker exec -it cb-server-7 couchbase-cli user-manage --cluster http://127.0.0.1 --username Administrator --password password --set --rbac-username syncgateway --rbac-password password --roles mobile_sync_gateway[*] --auth-domain local

timeout /t 8 
curl -v -X POST http://localhost:8093/query/service -u Administrator:password -d "statement=CREATE INDEX idx_wavelength_document_type on wavelength(documentType)"

timeout /t 5 
curl -v -X  POST http://localhost:8093/query/service  -u Administrator:password -d "statement=CREATE INDEX idx_wavelength_auction_active on wavelength(isActive)"

docker cp sample-documents-server.json cb-server-7:/sample-documents-server.json

docker exec -it cb-server-7 /opt/couchbase/bin/cbimport json --format list -c http://localhost:8091 -u Administrator -p password -d "file:///sample-documents-server.json" -b "wavelength" -g "%documentId%"