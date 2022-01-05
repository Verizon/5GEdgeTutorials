using MediatR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wavelength.Core.DataAccessObjects;
using Wavelength.Server.WebAPI.Repositories;

namespace Wavelength.Server.WebAPI.Features.Auctions.GetAuctionsQuery
{
    public class RequestHandler
		: IRequestHandler<RequestQuery, AuctionItemDAOs>
	{
		private readonly IAuctionRepository _auctionRepository;
		private readonly IFormatProvider _cachedProvider = NumberFormatInfo.CurrentInfo;
		public RequestHandler(
			IAuctionRepository auctionRepository)
		{
			_auctionRepository = auctionRepository;
		}

		public async Task<AuctionItemDAOs> Handle(
			RequestQuery request, 
			CancellationToken cancellationToken)
		{
			var daos = new List<AuctionItemDAO>();
			var items = await _auctionRepository.GetAuctionItems(request.Limit, request.Skip);
			//convert to data access object
			items.Items.ToList().ForEach(item => 
			{
				daos.Add(item.ToAuctionItemDAO());
			});
			var results = new AuctionItemDAOs(daos);

			results.PerformanceMetrics.DbElapsedTime =
				StringToDecimal(items.DbQueryElapsedTime);
			results.PerformanceMetrics.DbExecutionTime =
				StringToDecimal(items.DbQueryExecutionTime);

			return results;
		}

		private static decimal StringToDecimal(string input)
        {
			long n = 0;
			//calculate trimming off ms from returned string from CB Server
			var length = input.Length - 2;
			var decimalPosition = length - 2; 
			for (var index = 0; index < length; index++)
            {
				char c = input[index];
				if (c == '.')
					decimalPosition = index + 1;
				else
					n = (n * 10) + (int)(c - '0');
            }
			return new decimal(
					(int)n, 
					(int)(n >> 32), 
					0, 
					false, 
					(byte)(length - decimalPosition)
					);
        }
	}
}
