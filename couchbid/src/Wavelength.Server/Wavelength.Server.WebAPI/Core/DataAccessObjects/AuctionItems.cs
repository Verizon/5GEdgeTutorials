using System.Collections.Generic;

namespace Wavelength.Core.DataAccessObjects
{
    public class AuctionItems
	{
		public IList<DomainObjects.AuctionItem> Items { get; set; }
		public string DbQueryExecutionTime { get; set;}
		public string DbQueryElapsedTime { get; set;}

		public AuctionItems(
			IList<DomainObjects.AuctionItem> items,
			string dbQueryExecutionTime, 
			string dbQueryElapsedTime)
		{
			Items = items;
			DbQueryExecutionTime = dbQueryExecutionTime;
			DbQueryElapsedTime = dbQueryElapsedTime;
		}
	}
}
