cd ../src/Wavelength.Server/Wavelength.Server.WebAPI 
echo "setting up cloud image on port 9000"
docker stop cb-demo-cloud 
docker rm cb-demo-cloud 
docker volume rm cb-wavelength-demo-api-cloud 
docker build -t cb-wavelength-demo-api-cloud . -f Dockerfile
docker run -d ^
--network demo ^
-p 9000:80 ^
--name cb-demo-cloud cb-wavelength-demo-api-cloud ^
-e cbsettings_CBLocation='Cloud' ^
-e cbsettings_CBMode='Server' ^
-e cbsettings_CBUsername='Administrator' ^
-e cbsettings_CBPassword='password' ^
-e cbsettings_CBConnectionString='couchbase://cb-server-7' ^
-e cbsettings_CBBucketName='wavelength' ^
-e cbsettings_CBScopeName='_default' ^
-e cbsettings_CBCollectionName='_default' ^
-e cbsettings_CBDatabaseName='auctions' ^
-e cbsettings_CBSyncGatewayUri='ws://sync-gateway-wavelength:4984/wavelength' ^
-e cbsettings_CBSyncGatewayUsername='demo' ^
-e cbsettings_CBSyncGatewayPassword='password' ^
-e cbsettings_CBUseSsl=false ^
-e cbsettings_CBClosingCode='123' ^
-e cbsettings_CBDurabilityPersistToMajority=true

echo "setting up wavelength image on port 9001"
docker stop cb-demo-wavelength 
docker rm cb-demo-wavelength
docker volume rm cb-wavelength-demo-api-wavelength
docker build -t cb-wavelength-demo-api-wavelength . -f Dockerfile
docker run -d ^
--network demo ^
-p 9001:80 ^
--name cb-demo-wavelength cb-wavelength-demo-api-wavelength ^
--name cb-demo-cloud cb-wavelength-demo-api-cloud ^
-e cbsettings_CBLocation='Wavelength' ^
-e cbsettings_CBMode='Server' ^
-e cbsettings_CBUsername='Administrator' ^
-e cbsettings_CBPassword='password' ^
-e cbsettings_CBConnectionString='couchbase://cb-server-7' ^
-e cbsettings_CBBucketName='wavelength' ^
-e cbsettings_CBScopeName='_default' ^
-e cbsettings_CBCollectionName='_default' ^
-e cbsettings_CBDatabaseName='auctions' ^
-e cbsettings_CBSyncGatewayUri='ws://sync-gateway-wavelength:4984/wavelength' ^
-e cbsettings_CBSyncGatewayUsername='demo' ^
-e cbsettings_CBSyncGatewayPassword='password' ^
-e cbsettings_CBUseSsl=false ^
-e cbsettings_CBClosingCode='123' ^
-e cbsettings_CBDurabilityPersistToMajority=true