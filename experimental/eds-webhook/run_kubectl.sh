kubectl apply -f manifest.yaml
kubectl apply -f validation_service.yaml

## bind default service account to view cluster 

# kubectl create clusterrolebinding default-view --clusterrole=view --serviceaccount=default:default

# ## Testing
#sleep 30

#kubectl apply -f test_deployment.yaml
# kubectl run nginx1 --image=nginx
# kubectl expose pod nginx1 --port=80 --name nginx-service1 --type=NodePort 

# kubectl run nginx2 --image=nginx
# kubectl expose pod nginx2 --port=80 --name nginx-service2 --type=NodePort 


# kubectl expose pod nginx --port=80 --name nginx-svc --type=NodePort 