import cdk = require('@aws-cdk/core');
import ec2 = require('@aws-cdk/aws-ec2');
import iam = require('@aws-cdk/aws-iam');
import * as autoscaling from '@aws-cdk/aws-autoscaling';
import * as ecs from '@aws-cdk/aws-ecs';

require('dotenv').config();

const config = {
  env: {
    account: process.env.AWS_ACCOUNT_NUMBER,
    region: process.env.AWS_REGION,
  },
};

// Availability Zone & Wavelength Zone Assignment
const wavelengthAZ: string = 'us-west-2-wl1-sfo-wlz-1';
const parentRegionAZ: string = 'us-west-2a';

export class WlEc2Stack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {

    // Add environment config
    super(scope, id, { ...props, env: config.env });

    // Create new IAM role with AmazonEC2ContainerServiceforEC2Role Policy equivalent
    const role = new iam.Role(this, 'wlz-ec2-demo-role', {
      assumedBy: new iam.ServicePrincipal('ec2.amazonaws.com'),
    });
    role.addToPolicy(new iam.PolicyStatement({
      resources: ['*'],
      actions: ["ec2:DescribeTags",
                "ecs:CreateCluster",
                "ecs:DeregisterContainerInstance",
                "ecs:DiscoverPollEndpoint",
                "ecs:Poll",
                "ecs:RegisterContainerInstance",
                "ecs:StartTelemetrySession",
                "ecs:UpdateContainerInstancesState",
                "ecs:Submit*",
                "ecr:GetAuthorizationToken",
                "ecr:BatchCheckLayerAvailability",
                "ecr:GetDownloadUrlForLayer",
                "ecr:BatchGetImage",
                "logs:CreateLogStream",
                "logs:PutLogEvents"],
    }));
    //  Create instance profile from IAM role
    const instanceProfile = new iam.CfnInstanceProfile(
      this,
      'wlz-instance-profile',
      {
        roles: [role.roleName],
      }
    );


    // Create VPC with single public subnet for parent region
    const vpc = new ec2.Vpc(this, 'AppVPC', {
      cidr: '10.0.0.0/16',
      maxAzs: 1,
      subnetConfiguration: [
        {
          subnetType: ec2.SubnetType.PUBLIC,
          name: 'Public',
          cidrMask: 24,
        }
      ],
    });

    // Create Wavelength Zone Private Subnet
    const wlPrivateSubnet = new ec2.PrivateSubnet(this, 'wlz-private-subnet', {
      availabilityZone: wavelengthAZ,
      cidrBlock: '10.0.2.0/26',
      vpcId: vpc.vpcId,
      mapPublicIpOnLaunch: false,
    });

    // Add Wavelength Zone Subnet to VPC
    vpc.privateSubnets.push(wlPrivateSubnet);

    // Create Carrier Gateway
    const cagw = new ec2.CfnCarrierGateway(this, 'wlz-ecs-cagw', {
      vpcId: vpc.vpcId,
    });

    // Attach carrier gateway as default route to Wavelength Zone Subnet's route table
    new ec2.CfnRoute(this, 'wlz-route', {
      destinationCidrBlock: '0.0.0.0/0',
      routeTableId: wlPrivateSubnet.routeTable.routeTableId,
      carrierGatewayId: cagw.ref,
    });

    // Create Security Group
    const securityGroup = new ec2.SecurityGroup(this, 'wlz-sg', {
      vpc: vpc,
      allowAllOutbound: true,
      securityGroupName: 'wlz-sg',
    });

    // Upate security group rules to allow inbound traffic on specific ports
    securityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(22),
      'Allows SSH access from bastion'
    );
    securityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(80),
      'Allows HTTP access from carrier network'
    );


    // Define ECS-optimized image for Launch Template
    const image = new ecs.EcsOptimizedAmi();

    // Create Launch Template for Auto Scaling group to reference
    const wlLaunchTemplate = new ec2.CfnLaunchTemplate(
      this,
      'wl-launch-template',
      {
        launchTemplateName: 'wl-launch-template',
        launchTemplateData: {
          networkInterfaces: [
            {
              deviceIndex: 0,
              associateCarrierIpAddress: true,
              groups: [securityGroup.securityGroupId],
              deleteOnTermination: true,
              subnetId: wlPrivateSubnet.subnetId!,
            },
          ],
          imageId: image.getImage(this).imageId,
          instanceType: 't3.medium',
          keyName: 'wl-cdk-demo', // <= make sure to create a new EC2 KeyPair to enable SSH access
          iamInstanceProfile: { arn: instanceProfile.attrArn },
          userData: cdk.Fn.base64(
            `#!/bin/bash -xe
          echo ECS_CLUSTER=ECSCluster >> /etc/ecs/ecs.config`
            )
        },
      }
    );


    // Create Auto Scaling Group
    const wl_asg = new autoscaling.AutoScalingGroup(this, 'MyFleet', {
      instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.MEDIUM),
      machineImage: new ecs.EcsOptimizedAmi(),
      updateType: autoscaling.UpdateType.REPLACING_UPDATE,
      desiredCapacity: 1,
      vpc: vpc,
    });


    // Since Launch Templates are not currently paramter for ASGs, use cfnAsg
    // Source: aws-cdk issue # 67344: https://github.com/aws/aws-cdk/issues/6734
    const cfnAsg = wl_asg.node.defaultChild as autoscaling.CfnAutoScalingGroup;
    wl_asg.node.tryRemoveChild('LaunchConfig');
    cfnAsg.launchConfigurationName = undefined;

    cfnAsg.mixedInstancesPolicy = {
        launchTemplate: {
            launchTemplateSpecification: {
                launchTemplateId: wlLaunchTemplate.ref,
                version: wlLaunchTemplate.attrDefaultVersionNumber,
            },
        },
    };

    // Create ECS Cluster
    const ecsCluster = new ecs.Cluster(this, 'ECSCluster', {
      clusterName: "ECSCluster",
      vpc });

    // Add Capacity Provider to ECS Cluster, as .addAutoScalingGroup() is now deprecated method
    const capacityProvider = new ecs.AsgCapacityProvider(this, 'AsgCapacityProvider', { autoScalingGroup: wl_asg });
    ecsCluster.addAsgCapacityProvider(capacityProvider);

    // Create Task Definition with Host Network Mode
    const taskDefinition = new ecs.Ec2TaskDefinition(this, 'TaskDef', {
      networkMode: ecs.NetworkMode.HOST
    });

    // Configure Sample Container with Port Mappings
    taskDefinition.addContainer('DefaultContainer', {
      image: ecs.ContainerImage.fromRegistry("amazon/amazon-ecs-sample"),
      memoryLimitMiB: 512,
      portMappings: [{ containerPort: 80, hostPort: 80 }]
    });

    // Deploy ECS Service to Capacity Provider
    const ecsService = new ecs.Ec2Service(this, 'Service', {
      cluster: ecsCluster,
      taskDefinition,
       capacityProviderStrategies: [
      {
      capacityProvider: capacityProvider.capacityProviderName,
      weight: 1,
    }
  ],
    });

  }
}

const app = new cdk.App();
new WlEc2Stack(app, 'WlEc2Stack', {});
app.synth();
