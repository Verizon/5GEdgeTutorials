# CloudFormation Templates for 5G Edge

Within the `cloudformation-templates` folder, you can find a variety of popular infrastructure patterns within AWS Wavelength, including:
- EC2 instance in a Wavelength Zone with auto-assigned Carrier IP
- ECS cluster (EC2 Launch Type) with a task scheduled to Wavelength Zone

From the CLI, create a CloudFormation stack using the following:
```
aws cloudformation create-stack \
--stack-name myWavelengthStack \
--template-body <selected-cfn-file> \
--parameters ParameterKey=EnvironmentName,ParameterValue=WavelengthCloudFormation \
```

For EKS Clusters in Wavelength Zones, please note the following:
- For Public Cluster, ECR/EC2 endpoints are Optional but you must manually attach Carrier IP to public-facing instaces
- For Private Cluster, change Public Endpoint Access to False and Private Endpoint Access to True in the EKS Console; EC2/ECR Endpoints are mandatory

Regardless of cluster endpoint access, you must authenticate to Cluster using aws-auth ConfigMap:

```
eks_node_profile=$(aws cloudformation describe-stacks --stack-name $stack_name --query "Stacks[0].Outputs[?OutputKey=='NodeInstanceRole'].OutputValue" --output text)
cat > aws-auth-cm.yaml <<EOF
apiVersion: v1
kind: ConfigMap
metadata:
   name: aws-auth
   namespace: kube-system
data:
  mapRoles: |
    - rolearn: $eks_node_profile
      username: system:node:{{EC2PrivateDNSName}}
      groups:
        - system:bootstrappers
        - system:nodes
EOF
kubectl apply -f aws-auth-cm.yaml
```