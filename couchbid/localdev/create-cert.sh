sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout localhost.key -out localhost.crt -config localhost.conf -passin pass:G@rb@g3P@123~$#
sudo openssl pkcs12 -export -out localhost.pfx -inkey localhost.key -in localhost.crt
sudo openssl rsa -in localhost.key -out localhost-private.key
sudo openssl rsa -in localhost.key -pubout -out localhost-public.key
