using System;
using System.Threading.Tasks;
using Wavelength.Core.DomainObjects;

namespace Wavelength.Server.WebAPI.Repositories
{
    public interface IBidRepository
    {
        public Task<(decimal DbExecutionTime, decimal DbElapsedTime)> CreateBid(string documentId, Bid bid);
    }
}
