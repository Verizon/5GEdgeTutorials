#!/bin/sh
docker exec -it cb-server-7 couchbase-cli cluster-init -c 127.0.0.1 \
--cluster-username Administrator \
--cluster-password password \
--services data,index,query \
--cluster-ramsize 1024 \
--cluster-index-ramsize 256 