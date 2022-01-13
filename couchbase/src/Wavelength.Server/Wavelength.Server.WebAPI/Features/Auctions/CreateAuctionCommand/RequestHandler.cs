using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Wavelength.Core.DataAccessObjects;
using Wavelength.Core.DomainObjects;
using Wavelength.Server.WebAPI.Repositories;

namespace Wavelength.Server.WebAPI.Features.Auctions.CreateAuctionCommand
{
    public class RequestHandler
        : IRequestHandler<RequestCommand, AuctionItemDAO>
    {
        private readonly IAuctionRepository _repository;
        public RequestHandler(IAuctionRepository repository)
        {
            _repository = repository;
        }

        public async Task<AuctionItemDAO> Handle(RequestCommand request, CancellationToken cancellationToken)
        {
            var documentId = System.Guid.NewGuid();
            var newAuctionItem = new AuctionItem()
            {
                DocumentId = documentId,
                Title = request.Title,
                ImageUrl = request.ImageUrl,
                StopTime = request.StopTime,
                IsWinnerCalculated = false,
                IsActive = true,
            };
            await _repository.CreateAuction(documentId, newAuctionItem);
            return newAuctionItem.ToAuctionItemDAO();
        }
    }
}
