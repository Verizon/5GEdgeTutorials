using System.Collections.Generic;

namespace Wavelength.Core.DataAccessObjects
{
    public class AppEnvironmentDAO 
	{
		public string Name { get; set;} = string.Empty;
		public string Mode { get; set; } = string.Empty;
		public string DbConnectionString { get; set; } = string.Empty;
		public string ScopeName { get; set; } = string.Empty;
		public string CollectionName { get; set; } = string.Empty;
		public string BucketName { get; set; } = string.Empty;
		public string SyncGatewayUri { get; set; } = string.Empty;
		public bool IsDurabilityPersistToMajority { get; set; } = false;
	}
}
