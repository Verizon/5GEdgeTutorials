from vz_edge_discovery import VzEdgeDiscovery
import random

my_obj = VzEdgeDiscovery()

####Test EDS###########################################################################################
access_token=my_obj.authenticate(app_key="<your-key>",secret_key="<your-secret-key>")
closestZone=my_obj.discover_closest_edge_zone(access_token=access_token, service_endpoints_id="<your-service-endpoints-id", ue_identity="174.249.33.62")
########################################################################################################
