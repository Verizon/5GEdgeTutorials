using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wavelength.Server.WebAPI.Repositories;

namespace Wavelength.Server.WebAPI.Features.Auctions.CloseAuctionsCommand
{
    public class RequestHandler
        : IRequestHandler<RequestCommand, IEnumerable<string>>
    {
        private readonly IAuctionRepository _auctionRepository;
        public RequestHandler(
            IAuctionRepository auctionRepository)
        {
            _auctionRepository = auctionRepository;
        }

        public async Task<IEnumerable<string>> Handle(
            RequestCommand request, 
            CancellationToken cancellationToken)
        {
            return await _auctionRepository.CloseEndedAuctions();
        }
    }
}
