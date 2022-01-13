using Microsoft.Extensions.Configuration;
using Wavelength.Core.Models;

namespace Wavelength.Server.WebAPI.Services
{
    public class CouchbaseConfigService
        : ICouchbaseConfigService
    {
        private readonly IConfiguration _configuration;
        public CouchbaseConfig Config { get; private set; } 

        public CouchbaseConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
            Config = new CouchbaseConfig();
        }

        public void InitConfig()
        {
            Config.BucketName = _configuration.GetValue<string>("CBBucketName");
            Config.CollectionName = _configuration.GetValue<string>("CBCollectionName");
            Config.ConnectionString = _configuration.GetValue<string>("CBConnectionString");
            Config.DatabaseName = _configuration.GetValue<string>("CBDatabaseName");
            Config.Location = _configuration.GetValue<string>("CBLocation");
            Config.Mode = _configuration.GetValue<string>("CBMode");
            Config.Password = _configuration.GetValue<string>("CBPassword");
            Config.RestEndpoint = _configuration.GetValue<string>("CBRestEndpoint");
            Config.ScopeName = _configuration.GetValue<string>("CBScopeName");
            Config.SyncGatewayPassword = _configuration.GetValue<string>("CBSyncGatewayPassword");
            Config.SyncGatewayUri = _configuration.GetValue<string>("CBSyncGatewayUri");
            Config.SyncGatewayUsername = _configuration.GetValue<string>("CBSyncGatewayUsername");
            Config.Username = _configuration.GetValue<string>("CBUsername");
            Config.UseSsl = _configuration.GetValue<bool>("CBUseSsl");
            Config.ClosingCode = _configuration.GetValue<string>("CBClosingCode");
            Config.DurabilityPersistToMajority = _configuration.GetValue<bool>("CBDurabilityPersistToMajority");
        }
    }
}
