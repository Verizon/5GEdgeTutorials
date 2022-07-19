package main

import (
	"context"

	"github.com/golang/glog"

	metav1 "k8s.io/apimachinery/pkg/apis/meta/v1"

	"k8s.io/client-go/kubernetes"
	"k8s.io/client-go/rest"
)

func deleteServiceendpointfromEDS(selector map[string]string, appName string) string {
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

	if cm, err := clientset.CoreV1().ConfigMaps("default").Get(context.TODO(), appName+".edge", metav1.GetOptions{}); err != nil {
		//cm, _ :=
		glog.Info(err)

		glog.Infof("Couldnt find configmap ")

	} else {

		glog.Infof("Deregistering service endpoint from EDS:")
		glog.Infof(cm.Data["edsServiceEndpointID"])
		result := DeleteService(cm.Data["edsServiceEndpointID"])
		glog.Infof(result)
		glog.Infof(" Deleting configmap")
		err1 := clientset.CoreV1().ConfigMaps("default").Delete(context.TODO(), cm.Name, metav1.DeleteOptions{})
		glog.Info(err1)

	}
	return ""
}
