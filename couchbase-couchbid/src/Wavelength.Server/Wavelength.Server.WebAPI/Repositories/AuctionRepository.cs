using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Query;
using Couchbase.KeyValue;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wavelength.Core.DataAccessObjects;
using Wavelength.Core.DomainObjects;
using Wavelength.Core.Models;
using Wavelength.Server.WebAPI.Core.Exceptions;
using Wavelength.Server.WebAPI.Providers;
using Wavelength.Server.WebAPI.Services;

namespace Wavelength.Server.WebAPI.Repositories
{
	public class AuctionRepository
		: IAuctionRepository
	{
		private readonly IClusterProvider _clusterProvider;
		private readonly ILogger<AuctionRepository> _logger;
		private readonly IWavelengthBucketProvider _bucketProvider;
		private readonly CouchbaseConfig _couchbaseConfig;

		public AuctionRepository(
			IClusterProvider clusterProvider,
			IWavelengthBucketProvider bucketProvider,
			ILogger<AuctionRepository> logger,
			ICouchbaseConfigService configuration)
		{
			_clusterProvider = clusterProvider;
			_bucketProvider = bucketProvider;
			_logger = logger;
			_couchbaseConfig = configuration.Config;
		}

		public async Task CreateAuction(Guid documentId, AuctionItem newAuctionItem)
		{
			var bucket = await _bucketProvider.GetBucketAsync();
			if (_couchbaseConfig.CollectionName is string collectionName)
			{
				var collection = await bucket.CollectionAsync(collectionName);
				var result = await collection.InsertAsync<AuctionItem>(
					documentId.ToString(),
					newAuctionItem,
					options =>
					{
						if (_couchbaseConfig.DurabilityPersistToMajority)
						{
							options.Durability(DurabilityLevel.PersistToMajority);
						}
						else
						{
							options.Durability(DurabilityLevel.None);
						}
					});
				if (result is not null)
					return;
				throw new CreateAuctionError();
			}
			else
			{
				throw new CouchbaseConfigurationValueNullException();
			}
		}

		public async Task<AuctionItems> GetAuctionItems(
			int limit = 25,
			int skip = 0)
		{
			//assume index is created on the server or this will go very poor
			//if running slow, check this index to make sure it's created
			//CREATE INDEX document_type_idx on `wavelength` (documentType, isActive)

			//validate we can see the proper config
			if (_clusterProvider is not null
				&& _couchbaseConfig is not null
				&& _couchbaseConfig.BucketName is not null)
			{
				//query the database and get items back
				var cluster = await _clusterProvider.GetClusterAsync();

				//Query the database - see full documenation
				//https://docs.couchbase.com/dotnet-sdk/current/howtos/n1ql-queries-with-sdk.html
				var query = GetActiveAuctionsQuery(limit, skip);
				var queryOptions = GetQueryOptions();
				var results = await cluster
										.QueryAsync<AuctionItem>(query, queryOptions)
										.ConfigureAwait(false);
				if (results is not null
					&& results?.MetaData?.Status == QueryStatus.Success)
				{
					var listAuctions = await results.Rows.ToListAsync<AuctionItem>();
					if (listAuctions is not null && listAuctions.Count > 0)
					{
						var auctionItems = new AuctionItems(
							listAuctions,
							results.MetaData.Metrics.ExecutionTime,
							results.MetaData.Metrics.ElapsedTime);
						return auctionItems;
					}
				}
			}
			return new AuctionItems(new List<AuctionItem>(),
									string.Empty,
									string.Empty);
		}

		public async Task<bool> DeactivateAuction(Guid documentId)
		{
			var bucket = await _bucketProvider.GetBucketAsync();
			if (_couchbaseConfig.CollectionName is string collectionName)
			{
				var collection = await bucket.CollectionAsync(collectionName);
				var result = await collection.GetAsync(documentId.ToString());
				var auctionitem = result.ContentAs<AuctionItem>();
				if (auctionitem is not null)
				{
					auctionitem.IsActive = false;
					_ = await collection.ReplaceAsync<AuctionItem>(
						documentId.ToString(),
						auctionitem,
						options =>
						{
							if (_couchbaseConfig.DurabilityPersistToMajority)
							{
								options.Durability(DurabilityLevel.PersistToMajority);
							}
							else
							{
								options.Durability(DurabilityLevel.None);
							}
						});
					return true;
				}
			}
			else
			{
				throw new CouchbaseConfigurationValueNullException();
			}
			//should never get here in perfect world
			return false;
		}

		public async Task<bool> DeleteAuction(Guid documentId)
        {
			var bucket = await _bucketProvider.GetBucketAsync();
			if (_couchbaseConfig.CollectionName is string collectionName)
			{
				var collection = await bucket.CollectionAsync(collectionName);
				await collection.RemoveAsync(documentId.ToString(), options =>
				{
					if (_couchbaseConfig.DurabilityPersistToMajority)
					{
						options.Durability(DurabilityLevel.PersistToMajority);
					}
					else
					{
						options.Durability(DurabilityLevel.None);
					}
				});
				return true;
			}
			else
			{
				throw new CouchbaseConfigurationValueNullException();
			}
		}

		public async Task<IEnumerable<string>> CloseEndedAuctions()
		{
			//validate we can see the proper config
			if (_clusterProvider is not null
				&& _couchbaseConfig is not null
				&& _couchbaseConfig.BucketName is not null)
            {
                return await CloseAuctions().ConfigureAwait(false);
            }
			return new List<string>();
		}

		private async Task<List<string>> CloseAuctions()
        {
			var auctionsClosed = new List<string>();
			//query the database to find any auctions to close 
			var cluster = await _clusterProvider.GetClusterAsync();
			if (cluster is not null) 
			{
				var auctionResults = await GetAuctionsToClose(cluster);
				if (auctionResults.Count > 0)
				{
					foreach (var auction in auctionResults
						.Where(x => x.StopTime <= DateTimeOffset.Now).ToList()) 
					{
						var winningBidder = await GetWinningBidder(cluster, auction);
						var didClose = await CloseAuction(auction, winningBidder);
						if (didClose && auction.DocumentId is Guid item)
						{
							auctionsClosed.Add(item.ToString());
						}
					}
				}
			}
			return auctionsClosed;
        }

        private async Task<bool> CloseAuction(
			AuctionItem auction, 
			Bid? winningBidder)
        {
			if (winningBidder == null)
				return false;
			if (auction.DocumentId is Guid documentId)
			{
				auction.IsWinnerCalculated = true;
				auction.WinnerDeviceId = winningBidder.DeviceId;
				var bucket = await _bucketProvider
									.GetBucketAsync()
									.ConfigureAwait(false);
				var collection = await bucket.CollectionAsync(
											_couchbaseConfig.CollectionName 
											?? "_default");
				var updateResults = await collection
										.ReplaceAsync<AuctionItem>(documentId.ToString(), auction);
				if (updateResults != null)
					return true;
			}
			return false;
        }

		//find winning bidder
		private async Task<Bid?> GetWinningBidder(
			ICluster cluster,
			AuctionItem auction)
		{
			if (auction.StopTime is DateTimeOffset stopTime)
			{
				var queryOptions = GetQueryOptions();
				var sbFindBidder = new StringBuilder("SELECT b.* FROM ");
				sbFindBidder.Append($"{_couchbaseConfig.BucketName} b ");
				sbFindBidder.Append($"WHERE b.documentType = 'bid' ");
				sbFindBidder.Append($"AND b.locationName = '{CouchbaseConfig.LocationWavelengthName}' ");
				sbFindBidder.Append($"AND b.received <= '{stopTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}' ");
				sbFindBidder.Append($"ORDER BY b.received DESC LIMIT 1");
				var winningBidderQuery = sbFindBidder.ToString();
				var bidderResults = await cluster
									.QueryAsync<Bid>(winningBidderQuery, queryOptions);
				if (bidderResults is not null
					&& bidderResults?.MetaData?.Status == QueryStatus.Success)
				{
					var bidders = await bidderResults.ToListAsync<Bid>().ConfigureAwait(false);
					if (bidders.Any())
					{
						return bidders.First<Bid>();
					}
				}
			}
			return null;
		}
		private async Task<List<AuctionItem>> GetAuctionsToClose(
			ICluster cluster)
        {
			var queryOptions = GetQueryOptions();	
			var auctionSb = new StringBuilder("SELECT a.* FROM ");
			auctionSb.Append($"{_couchbaseConfig.BucketName} a ");
			auctionSb.Append("WHERE a.documentType = 'auction' ");
			auctionSb.Append("AND ");
			auctionSb.Append("a.isActive = true ");
			auctionSb.Append("AND ");
			auctionSb.Append("a.isWinnerCalculated = false ");
			auctionSb.Append("AND ");
			auctionSb.Append($"a.stopTime <= '{DateTimeOffset.Now.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}' ");

			var auctionQuery = auctionSb.ToString();
			var auctionResults = await cluster
					.QueryAsync<AuctionItem>(auctionQuery, queryOptions)
					.ConfigureAwait(false);
			if (auctionResults is not null &&
				auctionResults?.MetaData?.Status == QueryStatus.Success)
			{
                return await auctionResults!
                        .Rows
                        .ToListAsync<AuctionItem>()
                        .ConfigureAwait(false);
            }
			return new List<AuctionItem>();
		}

		private string GetActiveAuctionsQuery(
			int limit, 
			int skip)
		{
			var sb = new StringBuilder("SELECT a.* FROM ");
			sb.Append($"{_couchbaseConfig.BucketName} a ");
			sb.Append("WHERE a.documentType = 'auction' ");
			sb.Append("AND ");
			sb.Append("a.isActive = true ");
			sb.Append($"LIMIT {limit} ");
			sb.Append($"OFFSET {skip} ");
			return sb.ToString();
		}

		private QueryOptions GetQueryOptions()
		{
			var queryOptions = new QueryOptions().Metrics(true);
			queryOptions.ScanConsistency(QueryScanConsistency.RequestPlus);
			queryOptions.Readonly(true);
			return queryOptions;
		}
    }
}
