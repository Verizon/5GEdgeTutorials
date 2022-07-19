package main

import (
	"context"

	"encoding/json"

	"github.com/golang/glog"
	corev1 "k8s.io/api/core/v1"

	metav1 "k8s.io/apimachinery/pkg/apis/meta/v1"

	"k8s.io/client-go/kubernetes"
	"k8s.io/client-go/rest"
)

type AWSPrivateIPResponse struct {
	WavelengthZone string
	CarrierIP      string
	AppID          string
	InstanceID     string
}

func findPodsAndRegistertoEDS(selector map[string]string, appName string, nodeport int32) string {
	//****1. login to kubecluster

	config, err := rest.InClusterConfig()
	if err != nil {
		panic(err.Error())
	}
	// creates the clientset
	clientset, err := kubernetes.NewForConfig(config)
	if err != nil {
		panic(err.Error())
	}

	// get pods in all the namespaces by omitting namespace
	// Or specify namespace to get pods in particular namespace
	podsList, err := clientset.CoreV1().Pods("").List(context.TODO(), metav1.ListOptions{})
	if err != nil {
		panic(err.Error())
	}

	//****2. find pods with the selector and get their nodes

	var privateIPs []string
	for _, v := range podsList.Items {
		for i, j := range v.Labels {
			for m, n := range selector {
				if (i == m) && (j == n) {

					glog.Info("------------------------------------")
					glog.Infof(" Pod has been found with label %s : %s", i, j)
					glog.Infof(" Pod name is  %s", v.Name)
					glog.Infof(" This Pod is deployed on Node %s", v.Spec.NodeName)
					glog.Infof(" This Pod is deployed on Node Host IP %s", v.Status.HostIP)
					glog.Info("------------------------------------")

					privateIPs = append(privateIPs, v.Status.HostIP)

				}
			}
		}

	}

	////*****3. call AWS API and get carrier IP addresses

	awsAPIResponse := DescribeInstancesCmd(privateIPs)
	glog.Infof("AWS response: %#v", awsAPIResponse)
	//*****4. register IP addresses with eds and get service point

	edsServiceEndpointID, err := RegisterService(awsAPIResponse, appName, nodeport)

	if err != nil {
		glog.Info(err)
		return "Error in registering service endpoint"
	}
	glog.Infof("Registered service endpoint ID: ")

	//*****5. create configmap for storing service endpoints
	glog.Infof(edsServiceEndpointID)

	glog.Infof("Creating Configmap")
	selectorData, err := json.Marshal(selector)
	configMapData := make(map[string]string)

	configMapData["edsServiceEndpointID"] = edsServiceEndpointID
	configMapData["serviceName"] = appName
	configMapData["selector"] = string(selectorData)

	configMap := corev1.ConfigMap{
		TypeMeta: metav1.TypeMeta{
			Kind:       "ConfigMap",
			APIVersion: "v1",
		},
		ObjectMeta: metav1.ObjectMeta{
			Name: appName + ".edge",
		},
		Data: configMapData,
	}

	if _, err := clientset.CoreV1().ConfigMaps("default").Get(context.TODO(), appName+".edge", metav1.GetOptions{}); err != nil {
		//cm, _ :=
		glog.Info(err)
		glog.Infof(" create configmap")

		_, err1 := clientset.CoreV1().ConfigMaps("default").Create(context.TODO(), &configMap, metav1.CreateOptions{})
		glog.Info(err1)
	} else {
		//cm, _ :=
		glog.Infof(" update configmap")
		clientset.CoreV1().ConfigMaps("default").Update(context.TODO(), &configMap, metav1.UpdateOptions{})
	}
	return ""
}
