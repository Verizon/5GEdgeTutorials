using System;
using Wavelength.Core.Models;

namespace Wavelength.Core.DataAccessObjects
{
    public class BidDAO
    {
        public BidDAO()
        {
            PerformanceMetrics = new Metrics();
        }

        public string DocumentType { get; set; } = "Bid";
        public Guid DocumentId { get; set; }
        public Guid DeviceId { get; set; }
        public Guid BidId { get; set; }
        public Guid AuctionId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public DateTimeOffset Received { get; set; }
        public DateTimeOffset Sent { get; set; }
        public Metrics PerformanceMetrics { get; set; } 
    }
}
