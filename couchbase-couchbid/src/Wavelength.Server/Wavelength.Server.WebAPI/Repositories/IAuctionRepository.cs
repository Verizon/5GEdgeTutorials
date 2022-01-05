using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wavelength.Core.DataAccessObjects;
using Wavelength.Core.DomainObjects;

namespace Wavelength.Server.WebAPI.Repositories
{
    public interface IAuctionRepository
    {
        Task<AuctionItems> GetAuctionItems(int limit, int skip);
        Task<IEnumerable<string>> CloseEndedAuctions();
        Task CreateAuction(Guid documentId, AuctionItem newAuctionItem);
        Task<bool> DeactivateAuction(Guid documentId);
        Task<bool> DeleteAuction(Guid documentId);
    }
}
