package main

import (
	"context"
	"encoding/json"
	"fmt"
	"os"
	"strconv"

	"github.com/VerizonDev/vzeds"
)

func RegisterService(awsRes []AWSPrivateIPResponse, appName string, nodeport int32) (string, error) {
	edsclient := vzeds.NewClient("")

	// to be changed to secrets
	edsclient.GetAuth(os.Getenv("VZEDS_API_KEY"), os.Getenv("VZEDS_API_SECRET"))

	ctx := context.Background()

	var param vzeds.GetMECPlatformsParam

	// pick a region
	region_res, err := edsclient.GetRegions(ctx)
	if err != nil {
		return "", err
	}
	param.Region = region_res.Regions[0].Name

	// set a subscriber density value
	param.SubscriberDensity = 50000

	// create a service profile and use that ID
	svcprof_param := vzeds.DefaultCreateServiceProfileParam()
	svcprof, err := edsclient.CreateServiceProfile(ctx, &svcprof_param)
	if err != nil {
		return "", err
	}
	param.ServiveProfileId = svcprof.ServiceProfileId

	// query get MEC platforms to pick an ern
	// mec, err := edsclient.GetMECPlatforms(ctx, &param)
	// if err != nil {
	// 	return "", err
	// }

	// // fmt.Printf("EDS replied with %d MEC platforms, use the first one's ern: %s\n",
	// // 	len(mec.MECPlatforms), mec.MECPlatforms[0].Ern)

	var svcep []vzeds.ServiceEndpointDescription
    
	for index, ar := range awsRes {
		// register a service endpoint
		svcep_param := vzeds.ServiceEndpointDescription{
			EdgeHostedService: vzeds.EdgeHostedService{
				Ern:                         ar.WavelengthZone,
				ApplicationServerProviderID: "AWS",
				ApplicationID:               appName + "-" + strconv.Itoa(index), //ar.AppID,
				ServiceEndpoint: vzeds.ServiceEndpoint{
					Fqdn:        "example.com",
					URI:         "http://1.1.1.1",
					IPv4Address: ar.CarrierIP, // to be changed by caller
					IPv6Address: "::1",
					Port:        vzeds.StringInt(nodeport), // to be changed by caller
				},
				ServiceDescription: "EKS wavelength application "+appName,
			},
			ServiceProfileID: svcprof.ServiceProfileId,
		}

		svcep = append(svcep, svcep_param)
	}

	var sv vzeds.RegisterServiceEndpointsParam = svcep

	param_json, err := json.MarshalIndent(sv, "", "  ")
	if err != nil {
		return "", err
	}
	fmt.Printf("Registering service endpoint with following parameters:\n%s\n", param_json)

	svcep_res, err := edsclient.RegisterServiceEndpoints(ctx, &sv)
	if err != nil {
		return "", err
	}

	fmt.Printf("Registered service endpoints with ID %s\n", svcep_res.ServiceEndpointsID)
	return svcep_res.ServiceEndpointsID, nil
}

func DeleteService(serviceID string) string {
	edsclient := vzeds.NewClient("")

	edsclient.GetAuth(os.Getenv("VZEDS_API_KEY"), os.Getenv("VZEDS_API_SECRET"))
	ctx := context.Background()

	_, err := edsclient.DeleteServiceEndpoints(ctx, serviceID)
	if err != nil {
		return err.Error()
	}

	return "Service Deleted successfully"
}

func GetService(serviceID string) *vzeds.GetServiceEndpointsResponse {
	edsclient := vzeds.NewClient("")

	edsclient.GetAuth(os.Getenv("VZEDS_API_KEY"), os.Getenv("VZEDS_API_SECRET"))
	ctx := context.Background()

	res, err := edsclient.GetServiceEndPointsById(ctx, serviceID)
	if err != nil {
		fmt.Println(err)
	}

	fmt.Println(res)
	return res
}
