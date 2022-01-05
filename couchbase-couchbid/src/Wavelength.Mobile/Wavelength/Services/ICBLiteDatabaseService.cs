using Couchbase.Lite;

namespace Wavelength.Services
{
    public interface ICBLiteDatabaseService
    {
	    //information display
	    string DeviceId { get; }
        string DatacenterLocation { get; }
		string IndexCount { get;  }
		
	    //database management
        bool IsDatabaseInitialized { get; }
        Database AuctionDatabase { get; }
        string DatabaseName { get; }
        string DatabaseDirectoryPath { get; }
        string RestApiUri { get; }
        string SyncGatewayUri { get;  }
        string LastReplicatorStatus { get; }

        void InitDatabase();
        void CloseDatabase();
        void DeleteDatabase();
    }
}