using MediatR;
using Wavelength.Core.DataAccessObjects;

namespace Wavelength.Server.WebAPI.Features.Auctions.GetAuctionsQuery
{
    public class RequestQuery :
        IRequest<AuctionItemDAOs>
    {
        public int Limit { get; set; } = 25;
        public int Skip { get; set; }
    }
}
