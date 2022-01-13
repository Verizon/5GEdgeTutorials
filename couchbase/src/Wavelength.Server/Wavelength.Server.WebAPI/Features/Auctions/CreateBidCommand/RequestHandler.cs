using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wavelength.Core.DataAccessObjects;
using Wavelength.Core.DomainObjects;
using Wavelength.Core.Models;
using Wavelength.Server.WebAPI.Repositories;
using Wavelength.Server.WebAPI.Services;

namespace Wavelength.Server.WebAPI.Features.Auction.CreateBidCommand
{
    public class RequestHandler
        : IRequestHandler<RequestCommand, BidDAO>
    {
        private readonly CouchbaseConfig _couchbaseConfig;
        private readonly IBidRepository _bidRepository;
    
        public RequestHandler(
            ICouchbaseConfigService configService,
            IBidRepository bidRepository)
            
        {
            _bidRepository = bidRepository;
            _couchbaseConfig = configService.Config;
        }

        public async Task<BidDAO> Handle(
                RequestCommand request,
                CancellationToken cancellationToken)
        {
            //calculate location
            var location = string.Empty;
            switch (_couchbaseConfig.Location)
            {
                case CouchbaseConfig.LocationCloud:
                    location = CouchbaseConfig.LocationCloudName;
                    break;
                case CouchbaseConfig.LocationDev:
                    location = CouchbaseConfig.LocationDevName;
                    break;
                default:
                    location = CouchbaseConfig.LocationWavelengthName;
                    break;
            }
            //calculate aprox network latency for request
            var documentId = Guid.NewGuid();

            //create the bid to write to the database
            var bid = new Bid()
            {
                DocumentId = documentId,
                AuctionId = request.Id,
                BidId = request.BidId,
                DeviceId = request.DeviceId,
                IsActive = true,
                LocationName = location,
                Received = request.Received,
                Sent = request.Sent
            };
            var results = await _bidRepository.CreateBid(documentId.ToString(), bid);
            var dao = bid.ToBidDAO();
            dao.PerformanceMetrics.DbElapsedTime = results.DbElapsedTime;
            dao.PerformanceMetrics.DbExecutionTime = results.DbExecutionTime;
            return dao; 
        }
    }
}
