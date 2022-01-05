using Couchbase.Lite.Query;

namespace Wavelength.Constants
{
    public static class Indexes
    {
        public const string DocumentType = nameof(DocumentType);
        public const string AuctionItemBids = nameof(AuctionItemBids);
        public const string DocumentTypeIsActive = nameof(DocumentTypeIsActive);
    }
    
    public static class ExpressionProperties
    {
        public const string DocumentType = "documentType";
        public const string AuctionId = "auctionId";
        public const string IsActive = "isActive";
    }
}