using System;
using Wavelength.Core.DTO;

namespace Wavelength.Core.DomainObjects
{
    public class Bid
        : DomainBase
    {
        public override string? DocumentType { get; set; } = "bid";
        public Guid DeviceId { get; set; }
        public Guid BidId { get; set; }
        public Guid AuctionId { get; set; }
        public DateTimeOffset Received { get; set; }
        public DateTimeOffset Sent { get; set; }
        public string LocationName { get; set; } = string.Empty;

        public DataAccessObjects.BidDAO ToBidDAO()
        {
            return new DataAccessObjects.BidDAO()
            {
                DeviceId = DeviceId,
                DocumentId = DocumentId ?? Guid.NewGuid(),
                BidId = BidId,
                AuctionId = AuctionId,
                Received = Received,
                Sent = Sent,
                LocationName = LocationName,
                PerformanceMetrics = new Models.Metrics() 
            };
        }
    }
}
