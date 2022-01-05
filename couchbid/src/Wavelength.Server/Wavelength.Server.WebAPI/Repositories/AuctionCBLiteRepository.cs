using Couchbase.Lite.Query;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Wavelength.Core.DataAccessObjects;
using Wavelength.Core.DomainObjects;
using Wavelength.Core.DTO;
using Wavelength.Server.WebAPI.Services;

namespace Wavelength.Server.WebAPI.Repositories
{
    public class AuctionCBLiteRepository
        : IAuctionRepository
    {
        private readonly CouchbaseLiteService _couchbaseLiteService;

        public AuctionCBLiteRepository(CouchbaseLiteService couchbaseLiteService)
        {
            _couchbaseLiteService = couchbaseLiteService;
        }

        public Task<IEnumerable<string>> CloseEndedAuctions()
        {
            throw new System.NotImplementedException();
        }

        public Task CreateAuction(System.Guid documentId, AuctionItem newAuctionItem)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DeactivateAuction(System.Guid documentId)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DeleteAuction(System.Guid documentId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<AuctionItems> GetAuctionItems(int limit = 50, int skip = 0)
        {
            if (_couchbaseLiteService.AuctionDatabase is not null)
            {
                var n1qlQuery = $"SELECT * FROM _ AS item WHERE documentType='auction' AND isActive = true LIMIT {limit} OFFSET {skip}";
                var items = new List<AuctionItem>();
                var stopWatch = new Stopwatch();
                using (var query = QueryBuilder.Select(SelectResult.All())
                        .From(DataSource.Database(_couchbaseLiteService.AuctionDatabase))
                        .Where(Expression.Property("documentType").EqualTo(Expression.String("auction"))
                        .And(Expression.Property("isActive").EqualTo(Expression.Boolean(true))))
                        .Limit(Expression.Int(limit))) 

                {
                    stopWatch.Start();
                    foreach (var result in query.Execute())
                    {
                        var json = JsonConvert.SerializeObject(result);
                        json = json.Replace("auctions", "item");
                        var dto = JsonConvert.DeserializeObject<CBLiteAuctionItemDTO>(json);
                        if (dto.Item is not null)
                        {
                            items.Add(dto.Item);
                        }
                    }
                    stopWatch.Stop();
                }
                return new AuctionItems(items, $"{stopWatch.Elapsed.TotalMilliseconds}ms", $"{stopWatch.Elapsed.TotalMilliseconds}ms");
            }
            return new AuctionItems(new List<AuctionItem>(), string.Empty, string.Empty);
        }
    }
}
