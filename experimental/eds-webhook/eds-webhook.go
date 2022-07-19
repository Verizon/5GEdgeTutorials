package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"net/http"
	"github.com/golang/glog"
	"k8s.io/api/admission/v1beta1"
	v1 "k8s.io/api/core/v1"
	metav1 "k8s.io/apimachinery/pkg/apis/meta/v1"
)

//WebhookServerHandler listen to admission requests and serve responses
type WebhookServerHandler struct {
}

func (gs *WebhookServerHandler) service(w http.ResponseWriter, r *http.Request) {


	// Body and path validation
	var body []byte
	if r.Body != nil {
		if data, err := ioutil.ReadAll(r.Body); err == nil {
			body = data
		}
	}
	if len(body) == 0 {
		glog.Error("empty body")
		http.Error(w, "empty body", http.StatusBadRequest)
		return
	}
	glog.Info("Received admission controller request for service")
	//glog.Infof(string(body))

	if r.URL.Path != "/service" {
		glog.Error("no service to validate")
		http.Error(w, "no validate", http.StatusBadRequest)
		return
	}

	// admission review api and json binding
	arRequest := v1beta1.AdmissionReview{}
	if err := json.Unmarshal(body, &arRequest); err != nil {
		glog.Error("incorrect body")
		http.Error(w, "incorrect body", http.StatusBadRequest)
	}
     
	var raw [] byte
	if arRequest.Request.Operation == "DELETE"{
	     raw = arRequest.Request.OldObject.Raw 
		}else {
	      raw = arRequest.Request.Object.Raw
		}
	
	service := v1.Service{}
	if err := json.Unmarshal(raw, &service); err != nil {
		glog.Error("error deserializing service")
		return
	}

	//Register/ Deregister service from eds if the service is Nodeport
	if service.Spec.Type == "NodePort" {
		selector := service.Spec.Selector
		nodeport:= service.Spec.Ports[0].NodePort

		glog.Infof("service %s has been found with type NodePort", service.Name)
		if arRequest.Request.Operation == "DELETE" {
			glog.Infof("Delete Operation! Deregistering serviceendpoint and deleting configmap")
			deleteServiceendpointfromEDS(selector, service.Name)
		} else {
			glog.Infof("Registering serviceendpoint and creating or updating configmap")
			findPodsAndRegistertoEDS(selector, service.Name, nodeport)
		}

	}



	// Preparing response for kuberntetes admission controller
	status := true
	message := "Success!"
	arResponse := v1beta1.AdmissionReview{

		Response: &v1beta1.AdmissionResponse{
			UID:     arRequest.Request.UID,
			Allowed: status,
			Result: &metav1.Status{
				Message: message,
			},
		},
	}
	resp, err := json.Marshal(arResponse)
	if err != nil {
		glog.Errorf("Can't encode response: %v", err)
		http.Error(w, fmt.Sprintf("could not encode response: %v", err), http.StatusInternalServerError)
	}

	// Writing the response back to kubernetes admission controller
	glog.Infof("Ready to write reponse ...")
	if _, err := w.Write(resp); err != nil {
		glog.Errorf("Can't write response: %v", err)
		http.Error(w, fmt.Sprintf("could not write response: %v", err), http.StatusInternalServerError)
	}
}
