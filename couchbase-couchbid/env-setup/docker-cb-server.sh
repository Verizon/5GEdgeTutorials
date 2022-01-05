#!/bin/bash
echo "stopping cb-server 7.02 image ..."
docker stop cb-server-7
echo "deleting cb-server 7.02 image ..."
docker rm cb-server-7
echo "Running cb-server 7.02..."
docker run -d --name cb-server-7 --network demo -p 8091-8097:8091-8097 -p 9100-9122:9100-9122 -p 11209-11210:11209-11210 -p 21100:21100 -p 21200:21200 -p 21300:21300 couchbase:enterprise-7.0.2
