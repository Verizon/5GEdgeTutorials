using System;

namespace Wavelength.Models
{
    public class Bid
    {
        public string DocumentType { get; set; }
        public string DocumentId { get; set; }
        public string DeviceId { get; set; }
        public string BidId { get; set; }
        public Guid AuctionId { get; set; }
        
        public DateTimeOffset Received { get; set; } 
        public DateTimeOffset Sent { get; set; } 
        public string TimeSpan { get; set; }
        public double TimeSpanRaw { get; set; }
        public string LocationName { get; set; }
        public bool IsActive { get; set; }

        public BidDto ToBidDTO()
        {
            return new BidDto()
            {
                DeviceId = DeviceId,
                BidId =  BidId,
                Id = AuctionId.ToString()
            };
        }
    }
}