using System;
using System.Collections;
using System.Collections.Generic;
namespace Wavelength.Models
{
    public class AuctionItemsDao
    {
        public IEnumerable<AuctionItem> Items { get; set; }

        public string DbQueryExecutionTime { get; set; }
        public string DbQueryElapsedTime { get; set; }
        public double ApiOverheadTime { get; set; }
        public double NetworkOverheadTime { get; set; }
    }
}
