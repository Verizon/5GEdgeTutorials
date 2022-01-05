using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using Wavelength.Models;
using Wavelength.Services;
using Xamarin.Forms;
using Newtonsoft.Json;

namespace Wavelength.Repository
{
    public class CBLiteAuctionRepository 
	: ICBLiteAuctionRepository
    {
        private readonly ICBLiteDatabaseService _databaseService;
        
        private readonly IQuery _queryAuctions;
        private ListenerToken _queryAuctionsToken;

        private IQuery _queryBidsOnAuction;
        private ListenerToken _queryBidsOnAuctionToken;
        
        private readonly IQuery _queryAuctionCount;
        private ListenerToken _queryAuctionCountToken;
        
        private readonly IQuery _queryBidCount; 
        private ListenerToken _queryBidCountToken;

        public CBLiteAuctionRepository(ICBLiteDatabaseService databaseService)
        {
            _databaseService = databaseService;
            if (!_databaseService.IsDatabaseInitialized) 
	        {
                _databaseService.InitDatabase();
	        }
            
            //setup auction live queries
            _queryAuctions = _databaseService.AuctionDatabase.CreateQuery(@"SELECT * FROM _ as AuctionItem WHERE documentType = 'auction'");

            //setup counts for information screens
            _queryAuctionCount = _databaseService.AuctionDatabase.CreateQuery(@"SELECT COUNT(*) FROM _ as Total WHERE documentType='auction'");
            _queryBidCount = _databaseService.AuctionDatabase.CreateQuery(@"SELECT COUNT(*) FROM _ as Total WHERE documentType='bid'");
        }
        
        // **
        // ** Auction Section - used for Live Queries for Auctions and Bids 
        // ** 

        public void RegisterAuctionLiveQuery(Command<IEnumerable<AuctionItem>> onAuctionItemUpdate)
        {
            _queryAuctionsToken = _queryAuctions.AddChangeListener((o, args) =>
            {
                var items = new List<AuctionItem>();
                var allResults = args.Results.AllResults();
                foreach (var result in allResults)
                {
                    var auctionItem = JsonConvert.DeserializeObject<AuctionItemDao>(result.ToJSON())?.AuctionItem;
                    if (auctionItem is not null)
                    {
                        items.Add(auctionItem);   
                    }
                } 
                onAuctionItemUpdate.Execute(items.OrderBy(x=>x.StopTime));
            });
        }

        public void DeRegisterAuctionLiveQuery()
        {
            _queryAuctions.RemoveChangeListener(_queryAuctionsToken);   
        }

        public void RegisterBidsLiveQuery(Command<IEnumerable<Bid>> onBidItemsUpdate, string auctionId)
        {
            //can't cache because this needs to change all the time between screens
            _queryBidsOnAuction = QueryBuilder.Select(SelectResult.All())
                .From(DataSource.Database(_databaseService.AuctionDatabase))
                .Where(Expression.Property("documentType").EqualTo(Expression.String("bid"))
                    .And(Expression.Property("auctionId").EqualTo(Expression.String(auctionId))));
            
            //handle live query
            _queryBidCountToken = _queryBidsOnAuction.AddChangeListener((o, args) =>
            {
                var items = new List<Bid>();
                var allResults = args.Results.AllResults();
                foreach (var result in allResults)
                {
                    var json = result.ToJSON();
                    var bidItem = JsonConvert.DeserializeObject<BidDao>(json)?.Auctions;
                    if (bidItem is not null)
                    {
                        TimeSpan ts = bidItem.Received - bidItem.Sent;
                        bidItem.TimeSpanRaw = ts.TotalMilliseconds;
                        bidItem.TimeSpan = $"{ts.TotalMilliseconds} ms";
                        items.Add(bidItem);
                    }
                }
                onBidItemsUpdate.Execute(items.OrderBy(x=>x.Sent));
            });
        }

        public void DeRegisterBidsLiveQuery(string auctionId)
        {
            _queryBidsOnAuction.RemoveChangeListener(_queryBidsOnAuctionToken);    
            _queryBidsOnAuction.Dispose();
            _queryBidsOnAuction = null;
        }
        
        
       // **
       // ** Information Section - used for InformationPage
       // ** 
        public void RegisterAuctionCount(Command<int> updateAuctionCount)
        {
            _queryAuctionCountToken = _queryAuctionCount.AddChangeListener((sender, args) =>
            {
                var allResults = args.Results.AllResults();
                foreach (var result in allResults)
                {
                    updateAuctionCount.Execute(result[0].Int);
                }
            });
        }

        public void DeregisterAuctionCount()
        {
            _queryAuctionCount.RemoveChangeListener(_queryAuctionCountToken);
        }
        
        public void RegisterBidCount(Command<int> updateBidCount)
        {
            _queryBidCountToken = _queryBidCount.AddChangeListener((sender, args) =>
            {
                var allResults = args.Results.AllResults();
                foreach (var result in allResults)
                {
                    updateBidCount.Execute(result[0].Int);
                }
            });
        }

        public void DeregisterBidCount()
        {
            _queryBidCount.RemoveChangeListener(_queryBidCountToken);
        }
    }
}