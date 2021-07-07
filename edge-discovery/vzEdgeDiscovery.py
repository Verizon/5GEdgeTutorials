import requests
import json
import base64
import random

#Authenticate to EDS
def authenticate(appKey,secretKey):
  print("Authenticating to EDS...")
  token=base64.b64encode((str(appKey)+":"+str(secretKey)).encode('ascii')).decode('ascii')
  url = "https://5gedge.verizon.com/api/ts/v1/oauth2/token"
  payload='grant_type=client_credentials'
  headers = {
    'Authorization': 'Basic '+str(token),
    'Content-Type': 'application/x-www-form-urlencoded',
  }
  response = requests.request("POST", url, headers=headers, data=payload)
  output=response.json()
  access_token=output["access_token"]
  print(f"Your access token: {access_token}")
  return access_token

#Create Service Profile
def createServiceProfile(accessToken,maxLatency):
  print("\nCreating service profile with network performance parameters...")
  url = "https://5gedge.verizon.com/api/mec/eds/serviceprofiles"
  payload = json.dumps({
    "clientType": "Analytics",
    "ecspFilter": "Verizon",
    "clientSchedule": "time windows",
    "clientServiceArea": "BOSTON",
    "networkResources": {
      "minBandwidthKbits": 700,
      "serviceContinuitySupport": True,
      "maxRequestRate": 5000,
      "maxLatencyMs": int(maxLatency),
      "minAvailability": 10
    },
    "computeResources": {
      "GPU": {
        "minCoreClockMHz": 100,
        "minMemoryClockMHz": 0,
        "minBandwidthGBs": 0,
        "minTFLOPS": 0
      },
      "minRAMGB": 10,
      "minStorageGB": 500
    },
    "properties": {
      "type": "string",
      "data": {}
    }
  })
  headers = {
    'Authorization': 'Bearer '+str(accessToken),
    'Content-Type': 'application/json',
  }
  response = requests.request("POST", url, headers=headers, data=payload)
  output=response.json()
  serviceProfileId=output["serviceProfileId"]
  print(f"Your Service Profile ID: {serviceProfileId}")
  return serviceProfileId


#Update Service Profile to reflect different latency profile
def updateServiceProfile(accessToken,maxLatency,serviceProfileId):
  print("\nUpdating service profile with network performance parameters...")
  url = "https://5gedge.verizon.com/api/mec/eds/serviceprofiles/"+str(serviceProfileId)
  payload = json.dumps({
    "clientType": "Analytics",
    "ecspFilter": "Verizon",
    "clientSchedule": "time windows",
    "clientServiceArea": "OREGON",
    "networkResources": {
      "minBandwidthKbits": 700,
      "serviceContinuitySupport": True,
      "maxRequestRate": 5000,
      "maxLatencyMs": int(maxLatency),
      "minAvailability": 10
    },
    "computeResources": {
      "GPU": {
        "minCoreClockMHz": 100,
        "minMemoryClockMHz": 0,
        "minBandwidthGBs": 0,
        "minTFLOPS": 0
      },
      "minRAMGB": 10,
      "minStorageGB": 500
    },
    "properties": {
      "type": "string",
      "data": {}
    }
  })
  headers = {
    'Authorization': 'Bearer '+str(accessToken),
    'Content-Type': 'application/json'
  }

  response = requests.request("PUT", url, headers=headers, data=payload)
  output=response.json()
  print(f"Your Updated Service Profile ID: {serviceProfileId} was successfully updated to new latency threshold: {maxLatency}ms")
  return serviceProfileId


def deleteServiceProfile(accessToken,serviceProfileId):
  print("\nDeleting service profile...")
  url = "https://5gedge.verizon.com/api/mec/eds/serviceprofiles/"+str(serviceProfileId)
  payload={}
  headers = {
    'Authorization': 'Bearer '+str(accessToken)
  }
  response = requests.request("DELETE", url, headers=headers, data=payload)
  print(response.text)
  print(f"Your Updated service profile (serviceProfileId={serviceProfileId}) was successfully deleted")


#Create Service Registry for Edge Records
def createServiceRegistry(accessToken,serviceProfileId,carrierIps,availabilityZones,applicationId):
  print("\nCreating service registry with Carrier IP information...")
  url = "https://5gedge.verizon.com/api/mec/eds/serviceendpoints"
  payload=[]
  for i in range(len(carrierIps)):
    newRecord= {
      "edgeHostedService": {
        "ern": str(availabilityZones[i]),
        "serviceEndpoint": {
          "URI": "http://api/test/id",
          "FQDN": "aceg1357.com",
          "IPv4Address": str(carrierIps[i]),
          "IPv6Address": "2001:0db8:5b96:0000:0000:426f:8e17:642a",
          "port": 80
        },
        "applicationServerProviderId": "AWS",
        "applicationId": str(applicationId),
        "serviceDescription": "Test application on Verizon 5G Edge"
      },
      "serviceProfileID": str(serviceProfileId)
      }
    payload.append(newRecord)
  payload=json.dumps(payload)

  headers = {
    'Authorization': 'Bearer '+str(accessToken),
    'Content-Type': 'application/json',
  }
  response = requests.request("POST", url, headers=headers, data=payload)
  output=response.json()
  serviceEndpointsId=output["serviceEndpointsId"]
  print(f"Your Service Endpoints ID: {serviceEndpointsId}")
  print(f"Your Application ID: {applicationId}")

  return serviceEndpointsId


def updateServiceRegistry(accessToken,serviceEndpointsId,carrierIps,availabilityZones,applicationId):
  print("\nUpdating service registry with Carrier IP information...")
  url = "https://5gedge.verizon.com/api/mec/eds/serviceendpoints/"+str(serviceEndpointsId)
  payload=[]
  for i in range(len(carrierIps)):
    newRecord= {
    "ern": str(availabilityZones[i]),
    "serviceEndpoint": {
      "URI": "http://api/test/id",
      "FQDN": "aceg1357.com",
      "IPv4Address": str(carrierIps[i]),
      "IPv6Address": "2001:0db8:5b96:0000:0000:426f:8e17:642a",
      "port": 80
    },
    "applicationServerProviderId": "AWS",
    "applicationId": str(applicationId),
    "serviceDescription": "Test application on Verizon 5G Edge"
    }
    payload.append(newRecord)
  payload=json.dumps(payload)
  headers = {
      'Authorization': 'Bearer '+str(accessToken),
      'Content-Type': 'application/json',
  }
  response = requests.request("PUT", url, headers=headers, data=payload)
  print(f"Your service registry (serviceEndpointsId={serviceEndpointsId}) was successfully updated")
  return serviceEndpointsId

def deleteServiceRegistry(accessToken,serviceEndpointsId):
  print("\nDeleting service registry...")
  url = "https://5gedge.verizon.com/api/mec/eds/serviceendpoints/"+str(serviceEndpointsId)
  payload={}
  headers = {
    'Authorization': 'Bearer '+str(accessToken),
  }
  response = requests.request("DELETE", url, headers=headers, data=payload)
  print(response.text)
  print(f"Your service registry (serviceEndpointsId={serviceEndpointsId}) was successfully deleted")

def discoverClosestEdgeZone(accessToken,serviceEndpointsId,UEIdentity):
  print("\nSelecting closest Mobile Edge Computing (MEC) endpoint...")
  url = "https://5gedge.verizon.com/api/mec/eds/serviceendpoints?serviceEndpointsIds="+str(serviceEndpointsId)+"&UEIdentityType=IPAddress&UEIdentity="+str(UEIdentity)
  payload={}
  headers = {
    'Authorization': 'Bearer '+str(accessToken)
  }
  response = requests.request("GET", url, headers=headers, data=payload)
  output=response.json()
  closestEdgeZone=output["serviceEndpoints"][0]["ern"]
  print(f"Your Closest Edge Zone: {closestEdgeZone}")
  closestIP=output["serviceEndpoints"][0]["serviceEndpoint"]["IPv4Address"]
  print(f"Your Closest IP Address: {closestIP}")
  return closestEdgeZone,closestIP
