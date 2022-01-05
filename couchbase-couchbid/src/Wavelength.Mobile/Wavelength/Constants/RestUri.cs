using System;
namespace Wavelength.Constants
{
    public static class RestUri
    {
        //public const string CloudServerProtocol = "http";
        public const string CloudServerProtocol = "https";
        public const string CloudServerBaseUrl = "wavelength-oregon.couchbase.live";
        //public const string CloudServerBaseUrl = "192.168.50.225";
        public const int CloudServerPort = 443;

        public const string CloudSyncGatewayProtocol = "wss";
        public const string CloudSyncGatewayUrl = "wavelength-gateway-oregon.couchbase.live";
        public const int CloudSyncGatewayPort = 4984;
        public const string CloudSyncGatewayEndpoint = "wavelength";
        
        //public const string WavelengthServerProtocol = "http";
        public const string WavelengthServerProtocol = "https";
        //public const string WavelengthServerBaseUrl = "192.168.50.225";
        public const string WavelengthServerBaseUrl = "wavelength-las.couchbase.live";
        public const int WavelengthServerPort = 443;

        public const string WavelengthSyncGatewayProtocol = "wss";
        //public const string WavelengthSyncGatewayUrl = "192.168.50.225";
        public const string WavelengthSyncGatewayUrl = "wavelength-gateway-las.couchbase.live";
        public const int WavelengthSyncGatewayPort = 4984;
        public const string WavelengthSyncGatewayEndpoint = "wavelength";

        public const string SyncGatewayUsername = "wavelength";
        public const string SyncGatewayPassword = "df7bc96dca91bc3a2efab3250981a76f08afb822";
        
        public const string GetAuctions = "/api/v1/Auction";
        public const string PostBid = "/api/v1/Auction/Bid";

        public const string AuthCode = "uuddlrlrbaba.P1";


    }
}
