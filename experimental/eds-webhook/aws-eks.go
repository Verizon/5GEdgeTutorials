package main

import (
	"context"
	"fmt"
	"github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/ec2"
	"github.com/aws/aws-sdk-go-v2/service/ec2/types"
	"github.com/golang/glog"
)

// EC2DescribeInstancesAPI defines the interface for the DescribeInstances function.
// We use this interface to test the function using a mocked service.
type EC2DescribeInstancesAPI interface {
	DescribeInstances(ctx context.Context,
		params *ec2.DescribeInstancesInput,
		optFns ...func(*ec2.Options)) (*ec2.DescribeInstancesOutput, error)
}

// GetInstances retrieves information about your Amazon Elastic Compute Cloud (Amazon EC2) instances.
// Inputs:
//     c is the context of the method call, which includes the AWS Region.
//     api is the interface that defines the method call.
//     input defines the input arguments to the service call.
// Output:
//     If success, a DescribeInstancesOutput object containing the result of the service call and nil.
//     Otherwise, nil and an error from the call to DescribeInstances.
func GetInstances(c context.Context, api EC2DescribeInstancesAPI, input *ec2.DescribeInstancesInput) (*ec2.DescribeInstancesOutput, error) {
	return api.DescribeInstances(c, input)
}

func DescribeInstancesCmd(privateIP []string) (awsResponse []AWSPrivateIPResponse) {

	cfg, err := config.LoadDefaultConfig(context.TODO())
	client := ec2.NewFromConfig(cfg)

	var filters []types.Filter

	f1Name := "private-ip-address"
	filter := types.Filter{Name: &f1Name,
		Values: privateIP,
	}

	filters = append(filters, filter)
	input := &ec2.DescribeInstancesInput{
		Filters: filters,
	}

	result, err := GetInstances(context.TODO(), client, input)
	if err != nil {
		fmt.Println("Got an error retrieving information about your Amazon EC2 instances:")
		fmt.Println(err)
		return
	}

	for _, r := range result.Reservations {
		glog.Infof("Reservation ID: " + *r.ReservationId)
		glog.Infof("Instance IDs:")
		for _, i := range r.Instances {
			var res AWSPrivateIPResponse
			
			glog.Infof("   " + *i.InstanceId)
			if i.Placement != nil && i.Placement.AvailabilityZone != nil {
				res.WavelengthZone = *i.Placement.AvailabilityZone
				glog.Infof("WavelengthZone: " + res.WavelengthZone)
			}
			for _, n := range i.NetworkInterfaces {
				if n.Association != nil && n.Association.CarrierIp != nil {
					res.CarrierIP = *n.Association.CarrierIp
					glog.Infof("Carrier IP Address: " + res.CarrierIP)
				}
			}
			awsResponse = append(awsResponse, res)
		}
	}
	return awsResponse
}
