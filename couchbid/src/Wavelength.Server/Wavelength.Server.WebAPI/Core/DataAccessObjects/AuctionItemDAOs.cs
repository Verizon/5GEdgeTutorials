using System.Collections.Generic;
using Wavelength.Core.Models;

namespace Wavelength.Core.DataAccessObjects
{
    public class AuctionItemDAOs
    {
        public IEnumerable<AuctionItemDAO> Items { get; set; }
        public Metrics PerformanceMetrics { get; set; }

        public AuctionItemDAOs(IEnumerable<AuctionItemDAO> items)
        {
            Items = items;
            PerformanceMetrics = new Metrics();
        }
    }
}
