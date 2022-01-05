using System;
namespace Wavelength.Core.Models
{
    public class CouchbaseConfig
    {
        public const string Section = "Couchbase";

        public const string ModeServer = "Server";
        public const string ModeCBLite = "CBLite";

        public const string LocationDev = "Development";
        public const string LocationDevName = "Development";

        public const string LocationCloud = "Cloud";
        public const string LocationCloudName = "AWS - Cloud";

        public const string LocationWavelength = "Wavelength";
        public const string LocationWavelengthName = "AWS Wavelength - Verizon";

        public string? Location { get; set; }
        public string? Mode { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ConnectionString { get; set; }
        public bool UseSsl { get; set; } = true;
        public string? RestEndpoint { get; set; }
        public string? BucketName { get; set; }
        public string? ScopeName { get; set; }
        public string? CollectionName { get; set; }

        public string? DatabaseName { get; set; }
        public string? SyncGatewayUri { get; set; }
        public string? SyncGatewayUsername { get; set; }
        public string? SyncGatewayPassword { get; set; }
        public string? ClosingCode { get; set; }
        public bool DurabilityPersistToMajority { get; set; }
    }
}
