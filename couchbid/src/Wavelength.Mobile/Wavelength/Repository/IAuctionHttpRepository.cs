using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wavelength.Models;

namespace Wavelength.Repository
{
    public interface IAuctionHttpRepository
    {
        Task<AuctionItemsDao> GetItemsAsync(bool forceRefresh = false);
        Task<IEnumerable<BidResult>> PostBidAsync(BidDto bid, CancellationToken token);
    }
}
