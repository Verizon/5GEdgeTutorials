import requests
import json
import base64
import random
import configparser

config = configparser.ConfigParser()
config.read('/resources/configuration.properties')

edge_url="https://5gedge.verizon.com"

from vz_edge_discovery_interface import VzEdgeDiscoveryInterface
from custom_exception import CustomException
class VzEdgeDiscovery(VzEdgeDiscoveryInterface):

    def handle_custom_exception(self,error):
        """Catch custom exception globally, serialize into JSON, and respond with 400."""
        payload = dict(())
        payload['status_code'] = error.status_code
        payload['message'] = error.message
        return json.dumps(payload)

    #Authenticate to EDS
    def authenticate(self, app_key, secret_key):
        print("Authenticating to EDS...")
        token=base64.b64encode((str(app_key)+":"+str(secret_key)).encode('ascii')).decode('ascii')
        url = edge_url+"/api/ts/v1/oauth2/token"
        print("url............."+url)
        payload='grant_type=client_credentials'
        headers = {
            'Authorization': 'Basic '+str(token),
            'Content-Type': 'application/x-www-form-urlencoded'
        }
        try:
            response='None'
            response = requests.request("POST", url, headers=headers, data=payload)
            print(f"Your authentication response : {response}")
            output=response.json()
            #print(response.text)
            access_token=output["access_token"]
            print(f"Your access token: {access_token}")
            return access_token
        except Exception as e:
            message="E10001 - Error getting access token for authentication: "+str(e)
            if response == 'None':
                badRequest=CustomException(message, "503")
            else:
                badRequest=CustomException(message, response.status_code)
                
            return self.handle_custom_exception(badRequest)
 

    #Create Service Profile
    def create_service_profile(self, 
                               access_token, 
                               max_latency):
        print("\nCreating service profile with network performance parameters...")

        url =edge_url+"/api/mec/eds/serviceprofiles"

        payload = json.dumps({
            "clientType": "Analytics",
            "ecspFilter": "Verizon",
            "clientSchedule": "time windows",
            "clientServiceArea": "BOSTON",
            "networkResources": {
                "minBandwidthKbits": 700,
                "serviceContinuitySupport": True,
                "maxRequestRate": 5000,
                "maxLatencyMs": int(max_latency),
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
        'Authorization': 'Bearer '+str(access_token),
        'Content-Type': 'application/json'
        }
        response='None'
        try: 
            response = requests.request("POST", url, headers=headers, data=payload)
            print(f"Your Create Service Profile response : {response}")           
            if response.status_code == 200:  # Only 200 Error Check
                output = response.json()
                #print(response.text)
                serviceProfileId=output["serviceProfileId"]
                
                if len(serviceProfileId) !=0:
                    print(f"Your Service Profile ID: {serviceProfileId}")
                    return serviceProfileId
                else:
                    raise CustomException('E10002: ServiceProfileId value is missing in the response', 'E10002')
            else:
                print("\nE10001 - Error Creating service profile with network performance parameters...") 
                output=response.json()
                raise CustomException("Error Creating service profile with network performance parameters "+output["message"], response.status_code)
        except CustomException as e:
            message="E10001 - Error Creating service profile with network performance parameter: "+str(e)
            if response == 'None':
                customException=CustomException(message, "503")
            else:
                customException=CustomException(message, e.status_code)            
            return self.handle_custom_exception(customException)
        
        except Exception as e:
            print("\nE10001 - Error Creating service profile with network performance parameters..."+str(e)) 
            message="E10001 - Error Creating service profile with network performance parameter: "+str(e)
            if response == 'None':
                customException=CustomException(message, "503")
            else:
                customException=CustomException(message, response.status_code)              
            return self.handle_custom_exception(customException)

    #Update Service Profile to reflect different latency profile
    def update_service_profile(self, access_token, 
                               max_latency, service_profile_id):
        print("\nUpdating service profile with network performance parameters...")
        
        url = edge_url+"/api/mec/eds/serviceprofiles/"+str(service_profile_id)
        
        payload = json.dumps({
            "clientType": "Analytics",
            "ecspFilter": "Verizon",
            "clientSchedule": "time windows",
            "clientServiceArea": "OREGON",
            "networkResources": {
                "minBandwidthKbits": 700,
                "serviceContinuitySupport": True,
                "maxRequestRate": 5000,
                "maxLatencyMs": int(max_latency),
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
            'Authorization': 'Bearer '+str(access_token),
            'Content-Type': 'application/json'
        }
        response='None'
        try:
            response = requests.request("PUT", url, headers=headers, data=payload)
            print(f"Your Update Service Profile response : {response}")
            if response.status_code == 200:
                output = response.json()
                #print(response.text)
                print(f"Your Updated Service Profile ID: {service_profile_id} was successfully updated to new latency threshold: {max_latency}ms")
                return service_profile_id
            else:
                output=response.json()
                raise CustomException("E10010 - Error updating Service Profile ID: {service_profile_id} to the new latency threshold: {max_latency}ms : "+output["message"], response.status_code)
        except CustomException as e:
            if response == 'None':
                message="E10010 - Error updating Service Profile ID: {service_profile_id}  "+str(e)
                customException=CustomException(message, "503")
            else:
                customException=CustomException(e.message, e.status_code)            
            return self.handle_custom_exception(customException)

        except Exception as e:
            message="E10010 - Error updating Service Profile ID: {service_profile_id} to the new latency threshold: {max_latency} ms "+str(e)
            if response == 'None':
                customException=CustomException(message, "503")
            else:
                customException=CustomException(message, response.status_code)               
            return self.handle_custom_exception(customException)


    def delete_service_profile(self, access_token, service_profile_id):
        print("\nDeleting service profile...")

        url = edge_url+"/api/mec/eds/serviceprofiles/"+str(service_profile_id)

        payload={}

        headers = {
            'Authorization': 'Bearer '+str(access_token)
        }
        response='None'
        try:
            response = requests.request("DELETE", url, headers=headers, data=payload)
            print(f"Your Delete Service Registry response : {response}")           
            if response.status_code == 200:
                #print(response.text)
                print(f"Your Updated service profile (service_profile_id={service_profile_id}) was successfully deleted")
            else:     
                output=response.json()
                raise CustomException("E10020 - Error in deleting Service Profile ID: {service_profile_id} : "+output["message"], response.status_code)
        except CustomException as e:
            if response == 'None':
                message="E10020 - Error in deleting Service Profile ID: {service_profile_id}  "+str(e)
                customException=CustomException(message, "503")
            else:
                customException=CustomException(e.message, e.status_code)            
            return self.handle_custom_exception(customException)

        except Exception as e:
            message="E10020 - Error in deleting Service Profile ID: {service_profile_id}  "+str(e)
            if response == 'None':
                customException=CustomException(message, "503")
            else:
                customException=CustomException(message, response.status_code)               
            return self.handle_custom_exception(customException)

    #Create Service Registry for Edge Records
    def create_service_registry(self, access_token, service_profile_id, 
                                carrier_ips, availability_zones, application_id,
                                fqdns):
        print("\nCreating service registry with Carrier IP information...")

        url = edge_url+"/api/mec/eds/serviceendpoints"
        
        payload=[]

        for i in range(len(carrier_ips)):
            newRecord= {
                    "ern": str(availability_zones[i]),
                    "serviceEndpoint": {
                        "URI": "http://api/test/id",
                        "FQDN": str(fqdns[i]),
                        "IPv4Address": str(carrier_ips[i]),
                        "IPv6Address": "2001:0db8:5b96:0000:0000:426f:8e17:642a",
                        "port": 80
                    },
                    "applicationServerProviderId": "AWS",
                    "applicationId": str(application_id[i]),
                    "serviceDescription": "Test application on Verizon 5G Edge",               
                    "serviceProfileID": str(service_profile_id)
            }
            payload.append(newRecord)
        
        payload=json.dumps(payload)
        
        headers = {
            'Authorization': 'Bearer '+str(access_token),
            'Content-Type': 'application/json'
        }
        response='None'
        try:
            response = requests.request("POST", url, headers=headers, data=payload)
            print(f"Your Create Service Registry response : {response}")
            if response.status_code == 200:
                output=response.json()
                #print(response.text)
                service_endpoints_id=output["serviceEndpointsId"]
                print(f"Your Service Endpoints ID: {service_endpoints_id}")
                print(f"Your Application ID: {application_id}")
                return service_endpoints_id
            else:
                output=response.json()
                print(f"Your output ID: {output}")               
                if response.status_code == 404:          
                    raise CustomException("E10030: Error Creating service registry with Carrier IP information for the service endpoint id: {service_endpoints_id} : "+output["error"],response.status_code)
                else:         
                    raise CustomException("E10030: Error Creating service registry with Carrier IP information for the service endpoint id: {service_endpoints_id} : "+output["message"],response.status_code)
        except CustomException as e:
            if response == 'None':
                message="E10030: Error Creating service registry with Carrier IP information "+str(e)
                customException=CustomException(message, "503")
            else:
                customException=CustomException(e.message, e.status_code)            
            return self.handle_custom_exception(customException)

        except Exception as e:
            message="E10030: Error Creating service registry with Carrier IP information.."+str(e)
            if response == 'None':
                customException=CustomException(message, "503")
            else:
                customException=CustomException(message, response.status_code)               
            return self.handle_custom_exception(customException)

    def update_service_registry(self, access_token, service_endpoints_id, carrier_ips, availability_zones, application_id, service_profile_id):
        print("\nUpdating service registry with Carrier IP information for the service endpoints id: "+str(service_endpoints_id))

        url = edge_url+"/api/mec/eds/serviceendpoints/"+str(service_endpoints_id)
        
        payload=[]
        
        for i in range(len(carrier_ips)):
            newRecord= {
                "ern": str(availability_zones[i]),
                "serviceEndpoint": {
                    "URI": "http://api/test/id",
                    "FQDN": "aceg1357.com",
                    "IPv4Address": str(carrier_ips[i]),
                    "IPv6Address": "2001:0db8:5b96:0000:0000:426f:8e17:642a",
                    "port": 80
                },
                "applicationServerProviderId": "AWS",
                "applicationId": str(application_id[i]),
                "serviceDescription": "Test application on Verizon 5G Edge",
                "serviceProfileID": str(service_profile_id),
                "ern": "us-east-1-wl1-atl-wlz-1"
            }
            payload.append(newRecord)
        
        payload=json.dumps(payload)
        
        headers = {
            'Authorization': 'Bearer '+str(access_token),
            'Content-Type': 'application/json'
        }
        response='None'

        try:
            response = requests.request("PUT", url, headers=headers, data=payload)
            print(f"Your Update Service Registry response : {response}")
           # print(f"Your Service json response ID: {response.json()}")
            if response.status_code == 200:
                output=response.json()
                #print(response.text)
                #print(f"Your Service Endpoints ID: {service_endpoints_id}")
                print(f"Your Application ID: {application_id}")
                print(f"Your service registry (service_endpoints_id={service_endpoints_id}) was successfully updated")
                return service_endpoints_id
            else:
                output=response.json()
                print(f"Your Update service Registry Failed: {output}")
                raise CustomException("E10031: Error updating service registry with Carrier IP information for the service endpoint id: {service_endpoints_id} : "+output["message"],response.status_code)
        except CustomException as e:
            if response == 'None':
                message="E10031: Error updating service registry with Carrier IP information for the service endpoint id: {service_endpoints_id} "+str(e)
                customException=CustomException(message, "503")
            else:
                print(f"Your exception Registry ID: {output}")
                customException=CustomException(e.message, e.status_code)            
            return self.handle_custom_exception(customException)

        except Exception as e:
            message="E10031: Error updating service registry with Carrier IP information for the service endpoint id: {service_endpoints_id} "+str(e)
            if response == 'None':
                customException=CustomException(message, "503")
            else:
                customException=CustomException(message, response.status_code)               
            return self.handle_custom_exception(customException)

        #try:
        #    response = requests.request("PUT", url, headers=headers, data=payload)
        # 
        #   if response.status_code == 200:
        #        print(f"***************Your service registry (service_endpoints_id={service_endpoints_id}) was successfully updated")
        #        return service_endpoints_id
        #    else:
        #        print(f"E10030: Error Your service registry (service_endpoints_id={service_endpoints_id}) was not successfully  updated")
        #        return None
        #
        #except Exception as e:
        #    print(f"E10030: Error Your service registry (service_endpoints_id={service_endpoints_id}) was not successfully  updated")
        #    return None

    def delete_service_registry(self, access_token, service_endpoints_id):
        print("\nDeleting service registry..."+str(service_endpoints_id))
 
        url = edge_url+"/api/mec/eds/serviceendpoints/"+str(service_endpoints_id)
 
        payload={}
 
        headers = {
            'Authorization': 'Bearer '+str(access_token),
        }
        response='None'
        try: 
            response = requests.request("DELETE", url, headers=headers, data=payload)
            print(f"Your Delete Service Registry response : {response}")
            if response.status_code == 200:
                #print(response.text)        
                print(f"Your service registry (service_endpoints_id={service_endpoints_id}) was successfully deleted")
            else:
                print(f"E10032: Your service registry (service_endpoints_id={service_endpoints_id}) was not successfully deleted")
                output=response.json()
                raise CustomException("E10032: Error in deleting service registry with Carrier IP information for the service endpoint id: {service_endpoints_id} : "+output["message"],response.status_code)
        except CustomException as e:
            if response == 'None':
                message="E10032: Error in deleting service registry for the service endpoint id: {service_endpoints_id} "+str(e)
                customException=CustomException(message, "503")
            else:
                customException=CustomException(e.message, e.status_code)            
            return self.handle_custom_exception(customException)

        except Exception as e:
            message="E10032: Error in deleting service registry for the service endpoint id: {service_endpoints_id} "+str(e)
            if response == 'None':
                customException=CustomException(message, "503")
            else:
                customException=CustomException(message, response.status_code)               
            return self.handle_custom_exception(customException)

    def discover_closest_edge_zone(self, access_token, service_endpoints_id, ue_identity):
        print("\nSelecting closest Mobile Edge Computing (MEC) endpoint...")

        url = edge_url+"/api/mec/eds/serviceendpoints?serviceEndpointsIds="+str(service_endpoints_id)+"&UEIdentityType=IPAddress&UEIdentity="+str(ue_identity)
        # print(url)

        payload={}

        headers = {
            'Authorization': 'Bearer '+str(access_token)
        }
        response='None'
        try:
            response = requests.request("GET", url, headers=headers, data=payload)
            print(f"Your Discover Closest Edge Zone response : {response}")
            if response.status_code == 200:
                output=response.json()
                #print(response.text)
                closest_edge_zone=output["serviceEndpoints"][0]["ern"]
                print(f"Your Closest Edge Zone: {closest_edge_zone}")
                closest_ip=output["serviceEndpoints"][0]["serviceEndpoint"]["IPv4Address"]
                print(f"Your Closest IP Address: {closest_ip}")
                return closest_edge_zone,closest_ip
            else:
                output=response.json()
                raise CustomException("E10040: Error in identifying closest edge zone :"+output["message"],response.status_code)
        except CustomException as e:
            if response == 'None':
                message="E10040: Error in identifying closest edge zone "+str(e)
                customException=CustomException(message, "503")
            else:
                customException=CustomException(e.message, e.status_code)            
            return self.handle_custom_exception(customException)

        except Exception as e:
            message="E10040: Error in identifying closest edge zone "+str(e)
            if response == 'None':
                customException=CustomException(message, "503")
            else:
                customException=CustomException(message, response.status_code)               
            return self.handle_custom_exception(customException)


