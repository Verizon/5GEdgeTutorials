#!/bin/bash
export KUBECONFIG="kubeconfig_wavelength-eks-Cluster"
kubectl get nodes


git clone https://github.com/RedisLabs/redis-enterprise-k8s-docs.git

kubectl create namespace redis-demo
kubectl config set-context --current --namespace=redis-demo
kubectl apply -f https://raw.githubusercontent.com/RedisLabs/redis-enterprise-k8s-docs/6.2.8-15/bundle.yaml 
kubectl apply -f redis-enterprise-k8s-docs/examples/v1/rec.yaml

sleep 10


cat << EOF > /tmp/redis-enterprise-database.yml
apiVersion: app.redislabs.com/v1alpha1
kind: RedisEnterpriseDatabase
metadata:
  name: redis-enterprise-database
spec:
  memorySize: 100MB
EOF
kubectl apply -f /tmp/redis-enterprise-database.yml