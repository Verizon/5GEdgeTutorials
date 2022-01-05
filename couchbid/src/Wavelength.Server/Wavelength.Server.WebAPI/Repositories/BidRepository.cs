using Couchbase.KeyValue;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Wavelength.Core.DomainObjects;
using Wavelength.Server.WebAPI.Core.Exceptions;
using Wavelength.Server.WebAPI.Providers;

namespace Wavelength.Server.WebAPI.Repositories
{
    public class BidRepository
        : IBidRepository
    {
        private readonly IWavelengthBucketProvider _bucketProvider; 
        public BidRepository(
            IWavelengthBucketProvider bucketProvider) 
        {
            _bucketProvider = bucketProvider;
        }

        public async Task<(decimal DbExecutionTime, decimal DbElapsedTime)> CreateBid(
            string documentId, 
            Bid bid)
        {
            var stopWatch = new Stopwatch();
            var insertStopWatch = new Stopwatch();
            try
            {
                stopWatch.Start();
                var bucket = await _bucketProvider.GetBucketAsync();
                var collection = await bucket.DefaultCollectionAsync();

                using (var auctionResults = await collection.GetAsync(bid.AuctionId.ToString(), options => {
                    options.Timeout(TimeSpan.FromSeconds(2));
                }))
                {
                    //valdiate auction exists
                    if (auctionResults is not null)
                    {
                        var auction = auctionResults.ContentAs<AuctionItem>();
                        //validate auction is still running
                        if (auction is not null
                            && auction.IsActive == true
                            && auction?.StopTime >= DateTimeOffset.Now) 
                        {
                            insertStopWatch.Start();
                            var result = await collection.InsertAsync<Bid>(documentId, bid);
                            insertStopWatch.Stop();
                            stopWatch.Stop();
                            return (
                                DbExecutionTime: new decimal(insertStopWatch.Elapsed.TotalMilliseconds),
                                DbElapsedTime: new decimal(stopWatch.Elapsed.TotalMilliseconds));
                        }
                        throw new AuctionEndedException();
                    }
                    insertStopWatch.Stop();
                    stopWatch.Stop();
                    throw new AuctionNotFoundException();
                }
            }
            catch (Exception)
            {
                stopWatch.Stop();
                insertStopWatch.Stop();
                throw;
            }
        }
    }
}
