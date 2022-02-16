#!/bin/bash -xe

sudo su
yum update -y
yum install -y iperf3

amazon-linux-extras install -y nginx1

# TODO: templatize and take URL, filename, ports as arguments

nginx
iperf3 --server --daemon --port 5001

cd /usr/share/nginx/html/ && curl -L -o sample.4ds https://079413ce-bd22-4260-8be6-8b040540a21d.client-api.unity3dusercontent.com/client_api/v1/buckets/6d9e0224-b376-437b-9f0c-ca4cfc1fe443/entries/343a3095-8ceb-4743-b0eb-ffaba39239a5/versions/701327a2-18ea-4942-834c-dac5466a76c2/content/
