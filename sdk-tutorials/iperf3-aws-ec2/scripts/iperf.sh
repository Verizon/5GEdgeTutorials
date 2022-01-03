#!/bin/bash -xe

sudo su
yum update -y
yum install -y iperf3
iperf3 --server --daemon --port 5001
