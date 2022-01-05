using System;
namespace Wavelength.Core.DomainObjects
{
    public class AuctionItem 
        : DomainBase
    {
        public override string? DocumentType { get; set; } = "auction";
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public DateTimeOffset? StopTime { get; set; }
        public bool IsWinnerCalculated { get; set; }
        public Guid? WinnerDeviceId { get; set; }

        public DataAccessObjects.AuctionItemDAO ToAuctionItemDAO()
        {
            return new DataAccessObjects.AuctionItemDAO
            {
                Id = DocumentId.ToString() ?? Guid.NewGuid().ToString(),
                ImageUrl = ImageUrl ?? "",
                Title = Title ?? "",
                StopTime = StopTime ?? DateTime.Now,
                IsWinnerCalculated = IsWinnerCalculated,
                WinnerDeviceId = WinnerDeviceId 
            };
        }
    }
}
