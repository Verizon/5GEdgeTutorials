using Couchbase.Lite;
using Couchbase.Lite.Query;
using Couchbase.Lite.Sync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Wavelength.Core.Exceptions;
using Wavelength.Core.Models;

namespace Wavelength.Server.WebAPI.Services
{
    public class CouchbaseLiteService
	{
		private readonly CouchbaseConfig _couchbaseConfig;
		private readonly ILogger<CouchbaseLiteService> _logger;

		private Database? _db;
		public Database? AuctionDatabase 
		{
			get => _db;
		}    
		
		private Replicator? _replicator;
		public Replicator? DatabaseReplicator 
		{
			get => _replicator;
		}

		public CouchbaseLiteService(
			ILogger<CouchbaseLiteService> logger,
			IConfiguration configuration) 
		{
			_logger = logger;
			//get config from JSON file configuration
			_couchbaseConfig = new CouchbaseConfig();
			configuration.GetSection(CouchbaseConfig.Section).Bind(_couchbaseConfig);
#if DEBUG
			Database.Log.Console.Domains = Couchbase.Lite.Logging.LogDomain.All;
			Database.Log.Console.Level = Couchbase.Lite.Logging.LogLevel.Verbose;
#endif
		}

		public void InitDatabase(string filePath)
		{
			try
			{
				var fullPath = string.Empty;
				if (Environment.OSVersion.Platform == PlatformID.Unix 
					|| Environment.OSVersion.Platform == PlatformID.Other)
				{
					fullPath = $"{filePath}/databases";
				}
				else
				{
					fullPath = $"{filePath}\\databases";
				}
				var databaseName = _couchbaseConfig?.DatabaseName;
				_db = new Database(databaseName, new DatabaseConfiguration { Directory = fullPath });

				//create indexex 
				CreateIndexes();

				//start replication
				InitReplication();
			}
			catch (Exception ex) 
			{
				_logger.LogError(ex.Message, ex.StackTrace);
			}

		}

		private void CreateIndexes()
		{ 
			if (_db is not null) 
			{
				var indexes = _db.GetIndexes();
				if (!indexes.Contains(Constants.Indexes.DocumentTypeName)) 
				{
					var documentTypeIndex = IndexBuilder.ValueIndex(
					ValueIndexItem.Expression(Expression.Property(Constants.Indexes.DocumentTypeProperty)));
					_db.CreateIndex(Constants.Indexes.DocumentTypeName, documentTypeIndex);

					var isActiveIndex = IndexBuilder.ValueIndex(
					ValueIndexItem.Expression(Expression.Property(Constants.Indexes.IsActiveProperty)));
					_db.CreateIndex(Constants.Indexes.IsActiveName, isActiveIndex);
				}
			}
		}

		private void InitReplication() 
		{
			if (_db is not null 
				&& _couchbaseConfig.SyncGatewayUri is not null
				&& _couchbaseConfig.SyncGatewayUsername is not null 
				&& _couchbaseConfig.SyncGatewayPassword is not null)
			{
				var repConfig = new ReplicatorConfiguration(_db, new URLEndpoint(new Uri(_couchbaseConfig.SyncGatewayUri)));
				var repAuth = new BasicAuthenticator(_couchbaseConfig.SyncGatewayUsername, _couchbaseConfig.SyncGatewayPassword);

				repConfig.Authenticator = repAuth;
				repConfig.Continuous = true;
				repConfig.ReplicatorType = ReplicatorType.PushAndPull;
				
				_replicator = new Replicator(repConfig);
				_replicator.Start();
			}
			else 
			{
				throw new SyncGatewayConfigMissingException();
			}

		}

		public void StopReplication() 
		{ 
			if (_replicator is not null) 
			{
				_replicator.Stop();
			}
		}

		public void Dispose() 
		{
			if (_replicator is not null)
			{
				_replicator.Dispose();
			}
			if (_db is not null) 
			{
				_db.Close();
				_db.Dispose();
			}
			
		}
	}
}
