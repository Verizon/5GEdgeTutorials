#!/bin/bash

# Load environment variables
AWS_ACCESS_KEY="<your-aws-access-key>" 
AWS_SECRET_KEY="<your-secret-key>"
EDS_KEY="<your-eds-api-key>"
EDS_SECRET="<your-eds-secret-key>"

# Set this field to run demo workload
RUN_TEST_WORKLOAD=true

# Create CA for admission controller
kubectl create secret generic eds-webhook -n default --from-file=key.pem=certs/tls.key --from-file=cert.pem=certs/tls.cert

# Apply RBAC permissions for EDS webhook
kubectl apply -f rbac.yaml

# Provide AWS Credentials
echo -n "aws_access_key_id = ${AWS_ACCESS_KEY}" > ./aws-credentials.txt
echo -e "\naws_secret_access_key = ${AWS_SECRET_KEY}" >> ./aws-credentials.txt
kubectl create secret generic aws-cred -n default --from-file=./aws-credentials.txt

# Provide EDS Credentials
echo -n "${EDS_KEY}" > ./eds-key.txt
echo -n "${EDS_SECRET}" > ./eds-secret.txt
kubectl create secret generic vzeds --from-file=secret=eds-secret.txt --from-file=key=eds-key.txt

# Apply Manifest & Validation Webhook
kubectl apply -f manifest.yaml
kubectl apply -f validation_service.yaml

# Conduct Testing
if [ "$RUN_TEST_WORKLOAD" == true ]; then
	kubectl apply -f test_deployment.yaml
	kubectl get configmap nginx-service.edge -o jsonpath="{.data}"
	exit 1
fi
