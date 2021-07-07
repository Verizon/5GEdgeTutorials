import vzEdgeDiscovery
import random

####Test EDS###########################################################################################
access_token=vzEdgeDiscovery.authenticate(appKey="<your-key>",secretKey="<your-secret-key>")
closestZone=vzEdgeDiscovery.discoverClosestEdgeZone(accessToken=access_token,serviceEndpointsId="<your-service-endpoints-id",UEIdentity="174.249.33.62")
########################################################################################################
