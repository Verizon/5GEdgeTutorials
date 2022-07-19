# Edge Discovery Service Admission Controller 
Use this Admission Controller for 5G Edge Kubernetes environments to ensure that your edge service registry is up-to-date with the latest services exposed through the carrier network.

## Use Cases
When deploying Kubernetes workloads in mobile edge compute environments, multiple edge zones may be utilized simultaneously. However, how can ephemeral workloads be automatically connected to your edge service discovery service, responsible for routing mobile clients to the optimal edge zone?

Using admission controllers, the creation and deletion of NodePorts can be automatically forwarded to the Edge Discovery Service. To learn more about the Edge Discovery Service, visit [the docs](https://www.verizon.com/business/5g-edge-portal/documentation/verizon-5g-edge-discovery-service.html). 


## Getting Started
```
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
```