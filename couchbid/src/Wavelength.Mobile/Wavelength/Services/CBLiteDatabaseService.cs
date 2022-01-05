using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using Couchbase.Lite.Sync;
using Wavelength.Constants;
using Wavelength.Models;
using Microsoft.Extensions.DependencyInjection;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Wavelength.Services
{
    public class CBLiteDatabaseService
        : ICBLiteDatabaseService
    {
        private readonly IConnectivityService _connectivityService;
        private Replicator _auctionReplicator;
        private ListenerToken _replicatonChangeToken;
        
        private readonly string _databaseName;
        public string DatabaseName  => _databaseName; 

        private readonly string _directoryPath;
        public string DatabaseDirectoryPath => _directoryPath;
        
	    private string _deviceId;
	    public string DeviceId => _deviceId;
	    
		public string RestApiUri { get; private set; }
        public string SyncGatewayUri { get; private set; } 
        public string DatacenterLocation { get; private set; }
        
        public string LastReplicatorStatus { get; private set; }
	    public string IndexCount { get; private set; }
        public bool IsDatabaseInitialized { get; private set; }
        public Database AuctionDatabase { get; private set; }
		
        public CBLiteDatabaseService(IConnectivityService connectivityService)
        {
            _connectivityService = connectivityService;
            _databaseName = "Auctions";
            _directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (Xamarin.Essentials.Preferences.ContainsKey(Constants.Preferences.DeviceIdKey))
			{
				_deviceId = Xamarin.Essentials.Preferences.Get(Constants.Preferences.DeviceIdKey, "");
				if (_deviceId == string.Empty) 
				{
					_deviceId = Guid.NewGuid().ToString();
					Xamarin.Essentials.Preferences.Set(Constants.Preferences.DeviceIdKey, _deviceId);
				}
			}
			else 
			{
				_deviceId = Guid.NewGuid().ToString();
				Xamarin.Essentials.Preferences.Set(Constants.Preferences.DeviceIdKey, _deviceId);
			}
            IsDatabaseInitialized = false;
        }

        public void InitDatabase()
        {
            Database.Log.Console.Domains = Couchbase.Lite.Logging.LogDomain.All;
            Database.Log.Console.Level = Couchbase.Lite.Logging.LogLevel.Verbose;

            if (AuctionDatabase is null)
            {
                var databaseConfig = new DatabaseConfiguration()
                {
                    Directory = _directoryPath 
                };
                AuctionDatabase = new Database(_databaseName, databaseConfig);

                //setup replication
                Task.Run(async () =>
                {
	                await CreateIndexes();
                    await SetupReplication();
                    IsDatabaseInitialized = true;
                });
            }
        }

        public void CloseDatabase()
        {
            if (AuctionDatabase is not null)
            {
                _auctionReplicator.Stop();

                AuctionDatabase.Close();
                AuctionDatabase.Dispose();
            }
        }

        public void DeleteDatabase() 
	    { 
            if (Database.Exists(_databaseName, _directoryPath)) 
	        {
                //remove replicator
		        _auctionReplicator.RemoveChangeListener(_replicatonChangeToken);
                _auctionReplicator.Stop();
		        _auctionReplicator.Dispose();

                //validate the database is closed first
                AuctionDatabase.Close();
                AuctionDatabase.Dispose();
                Database.Delete(_databaseName, _directoryPath); 
	        }
	    }

        private async Task CreateIndexes()
        {
	        if (AuctionDatabase is not null)
	        {
		        var indexes = AuctionDatabase.GetIndexes();
		        if (!indexes.Contains(Indexes.DocumentType))
		        {
					AuctionDatabase.CreateIndex(
				        Indexes.DocumentType,
						IndexBuilder.ValueIndex(ValueIndexItem.Expression(Expression.Property(ExpressionProperties.DocumentType)))
					);
		        }
		        if (!indexes.Contains(Indexes.AuctionItemBids))
		        {
					var index = IndexBuilder.ValueIndex(
						ValueIndexItem.Expression(Expression.Property(ExpressionProperties.DocumentType)),
						ValueIndexItem.Expression(Expression.Property(ExpressionProperties.AuctionId))
					);
					AuctionDatabase.CreateIndex(Indexes.AuctionItemBids, index);
				}
				if (!indexes.Contains(Indexes.DocumentTypeIsActive))
				{
					var index = IndexBuilder.ValueIndex(
						ValueIndexItem.Expression(Expression.Property(ExpressionProperties.DocumentType)),
						ValueIndexItem.Expression(Expression.Property(ExpressionProperties.IsActive))
					);
					AuctionDatabase.CreateIndex(Indexes.DocumentTypeIsActive, index);
				}

				IndexCount = AuctionDatabase.GetIndexes().Count.ToString();
	        }
	        await Task.CompletedTask;
        }

        private async Task SetupReplication() 
	    {
            if (_auctionReplicator is null)
            {
                await CalculateSyncGatewayEndpoint();
                var urlEndpoint = new URLEndpoint(new Uri(SyncGatewayUri));

				try
				{
					var authenticator = new BasicAuthenticator(RestUri.SyncGatewayUsername, RestUri.SyncGatewayPassword);
					var replicationConfig = new ReplicatorConfiguration(AuctionDatabase, urlEndpoint);
					replicationConfig.ReplicatorType = ReplicatorType.Pull;
					replicationConfig.Continuous = true;
					replicationConfig.Heartbeat = TimeSpan.FromSeconds(30);
					replicationConfig.EnableAutoPurge = true;
					replicationConfig.Authenticator = authenticator;

					//need to hack in the pinned certificate stuff to fix issue with Android
					//this is probably more secure than iOS but a pain in the butt none the less
					//and yes I should do this in iOS too but that is just going to add regression onto
					//something that already works.  LABEAAA 11/16/2021
					if (Xamarin.Forms.Device.RuntimePlatform == Device.Android)
					{
						var pcs = Startup.ServiceProvider.GetService<IPinnedCertificateService>();
						var bytes = pcs.GetCertificateFromAssets();
						replicationConfig.PinnedServerCertificate = new X509Certificate2(bytes);
					}

					_auctionReplicator = new Replicator(replicationConfig);

					_replicatonChangeToken = _auctionReplicator.AddChangeListener((o, e) =>
					{
						if (e.Status.Error != null)
						{
							Console.WriteLine($"**Replication ERROR Message: {e.Status.Error.Message} StackTrace: {e.Status.Error.StackTrace}");

							Messaging.Instance.Publish(Messages.ReplicationError, e.Status.Error);
						}

						if (e.Status.Progress.Completed > 0 || e.Status.Progress.Total > 0)
						{
							Messaging.Instance.Publish(Messages.ReplicationProgressUpdate, new ProgressStatus
							{
								Total = e.Status.Progress.Total,
								Completed = e.Status.Progress.Completed
							});
						}
						switch (e.Status.Activity)
						{
							case ReplicatorActivityLevel.Busy:
								LastReplicatorStatus = Messages.ReplicationStatus.Busy;
								Messaging.Instance.Publish(Messages.ReplicationChangeStatus, Messages.ReplicationStatus.Busy);
								break;
							case ReplicatorActivityLevel.Connecting:
								LastReplicatorStatus = Messages.ReplicationStatus.Connecting;
								Messaging.Instance.Publish(Messages.ReplicationChangeStatus, Messages.ReplicationStatus.Connecting);
								break;
							case ReplicatorActivityLevel.Offline:
								LastReplicatorStatus = Messages.ReplicationStatus.Offline;
								Messaging.Instance.Publish(Messages.ReplicationChangeStatus, Messages.ReplicationStatus.Offline);
								break;
							case ReplicatorActivityLevel.Stopped:
								LastReplicatorStatus = Messages.ReplicationStatus.Stopped;
								Messaging.Instance.Publish(Messages.ReplicationChangeStatus, Messages.ReplicationStatus.Stopped);
								break;
							default:
								LastReplicatorStatus = Messages.ReplicationStatus.Idle;
								Messaging.Instance.Publish(Messages.ReplicationChangeStatus, Messages.ReplicationStatus.Idle);
								break;
						}
					});
					_auctionReplicator.Start();
                }
				catch (Exception ex) 
				{
					Console.WriteLine($"Error - no replication, *sigh*  Future Dev please fix me:  {ex.Message}");
				}
            }
	    }

        private async Task CalculateSyncGatewayEndpoint() 
	    {
			DatacenterLocation = Labels.DatacenterLocationCloud;
			SyncGatewayUri = $@"{RestUri.CloudSyncGatewayProtocol}://{RestUri.CloudSyncGatewayUrl}:{RestUri.CloudSyncGatewayPort}/{RestUri.CloudSyncGatewayEndpoint}";
			RestApiUri = $@"{RestUri.CloudServerProtocol}://{RestUri.CloudServerBaseUrl}:{RestUri.CloudServerPort}";
			try
			{
				var isWavelengthApiAvailable = await _connectivityService.IsRemoteReachable(
												RestUri.WavelengthServerBaseUrl,
												RestUri.WavelengthServerPort);

				var isWavelengthSyncGatewayAvailable = await _connectivityService.IsRemoteReachable(
												RestUri.WavelengthSyncGatewayUrl,
												RestUri.WavelengthSyncGatewayPort);

				if (isWavelengthApiAvailable && isWavelengthSyncGatewayAvailable)
				{
					DatacenterLocation = Labels.DatacenterLocationWavelength;
					SyncGatewayUri = $@"{RestUri.WavelengthSyncGatewayProtocol}://{RestUri.WavelengthSyncGatewayUrl}:{RestUri.WavelengthSyncGatewayPort}/{RestUri.WavelengthSyncGatewayEndpoint}";
					RestApiUri = $@"{RestUri.WavelengthServerProtocol}://{RestUri.WavelengthServerBaseUrl}:{RestUri.WavelengthServerPort}";
				}
			}
			catch (Exception ex) 
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
	    }
    }
}