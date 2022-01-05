using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wavelength.Models;
using Xamarin.Forms;

namespace Wavelength.Repository
{
    public interface ICBLiteAuctionRepository
    {
        void RegisterAuctionCount(Command<int> updateAuctionCount);
        void DeregisterAuctionCount();
        void RegisterBidCount(Command<int> updateBidCount);
        void DeregisterBidCount();
        
        //live queries for auctions and bids
        void RegisterAuctionLiveQuery(Command<IEnumerable<AuctionItem>> onAuctionItemUpdate);
        void DeRegisterAuctionLiveQuery();

        void RegisterBidsLiveQuery(Command<IEnumerable<Bid>> onBidItemsUpdate, string auctionId);
        void DeRegisterBidsLiveQuery(string auctionId);
    }
}