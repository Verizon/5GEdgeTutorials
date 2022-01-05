using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Wavelength.Server.WebAPI.Repositories;

namespace Wavelength.Server.WebAPI.Features.Auctions.DeactivateAuctionCommand
{
    public class RequestHandler
        : IRequestHandler<RequestCommand, bool>
    {
        private readonly IAuctionRepository _repository;
        public RequestHandler(
            IAuctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(
            RequestCommand request, 
            CancellationToken cancellationToken)
        {
            return await _repository.DeactivateAuction(request.Id);
        }
    }
}
