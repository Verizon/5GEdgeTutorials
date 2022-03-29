import abc

class VzEdgeDiscoveryInterface(metaclass=abc.ABCMeta):

    #Authenticate to EDS
    @abc.abstractmethod
    def authenticate(self, app_key, secret_key):
        '''code to do authenticate and return access token'''
        raise NotImplementedError

    #Create Service Profile
    @abc.abstractmethod
    def create_service_profile(self, access_token, max_latency):
        '''creation of service profile and return service_profile_id'''
        raise NotImplementedError

    #Update Service Profile to reflect different latency profile
    @abc.abstractmethod
    def update_service_profile(self, access_token, max_latency, service_profile_id):
        '''updation of service profile'''
        raise NotImplementedError

    @abc.abstractmethod
    def delete_service_profile(self, access_token, service_profile_id):
        '''deletion of service profile'''
        raise NotImplementedError

    #Create Service Registry for Edge Records
    @abc.abstractmethod
    def create_service_registry(self, access_token, service_profile_id, carrier_ips, availability_zones, application_id, fqdns):
        '''creation of service registry'''
        raise NotImplementedError

    @abc.abstractmethod
    def update_service_registry(self, access_token, service_endpoints_id, carrier_ips, availability_zones, application_id):
        '''update service registry'''
        raise NotImplementedError

    @abc.abstractmethod
    def delete_service_registry(self, access_token, service_endpoints_id):
        '''delete service registry'''
        raise NotImplementedError

    @abc.abstractmethod
    def discover_closest_edge_zone(self, access_token, service_endpoints_id, ue_identity):
        '''discover closest edge zone'''
        raise NotImplementedError