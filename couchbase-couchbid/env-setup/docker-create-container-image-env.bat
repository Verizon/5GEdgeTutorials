cd ../src/Wavelength.Server/Wavelength.Server.WebAPI 
echo "setting up cloud image on port 9000"
docker stop cb-demo-cloud 
docker rm cb-demo-cloud 
docker volume rm cb-wavelength-demo-api-cloud 
docker build -t cb-wavelength-demo-api-cloud . -f Dockerfile
docker run -d ^
--network demo ^
--env-file env-cloud ^
-p 9000:80 ^
--name cb-demo-cloud cb-wavelength-demo-api-cloud ^

echo "setting up wavelength image on port 9001"
docker stop cb-demo-wavelength 
docker rm cb-demo-wavelength
docker volume rm cb-wavelength-demo-api-wavelength
docker build -t cb-wavelength-demo-api-wavelength . -f Dockerfile
docker run -d ^
--network demo ^
--env-file env-wavelength ^
-p 9001:80 ^
--name cb-demo-wavelength cb-wavelength-demo-api-wavelength ^