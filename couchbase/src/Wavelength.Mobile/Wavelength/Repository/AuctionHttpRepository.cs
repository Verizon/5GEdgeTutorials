using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wavelength.Models;
using Wavelength.Services;

namespace Wavelength.Repository
{
    public class AuctionHttpRepository
        : IAuctionHttpRepository
    {
        private readonly ICBLiteDatabaseService _databaseService;
        private readonly HttpClient _wavelengthHttpClient;
        private readonly HttpClient _cloudHttpClient;
        
        private string _wavelengthApiBaseUri;
        private string _cloudApiBaseUri;
        
        public AuctionHttpRepository(
            ICBLiteDatabaseService databaseService,
	        Services.IHttpClientFactory httpClientFactory)
        {
            _databaseService = databaseService;
            CalculateUrls();
            //cache http clients for performance
            _wavelengthHttpClient = httpClientFactory.GetHttpClient(_wavelengthApiBaseUri);
            _cloudHttpClient = httpClientFactory.GetHttpClient(_cloudApiBaseUri);
        }

        public async Task<IEnumerable<BidResult>> PostBidAsync(
            BidDto bid, 
            CancellationToken token)
        {
            var items = new List<BidResult>();
            var postUri = new Uri(Constants.RestUri.PostBid, UriKind.Relative);
            using var request = new HttpRequestMessage(HttpMethod.Post, postUri);
            if (_databaseService.DatacenterLocation == Constants.Labels.DatacenterLocationWavelength)
            {
                using var wavelengthRequest = new HttpRequestMessage(HttpMethod.Post, postUri);
                await PostBidContentAsync(bid, token, _wavelengthHttpClient, wavelengthRequest, items);
            }
            await PostBidContentAsync(bid, token, _cloudHttpClient, request, items);
            return items;
        }

        private async Task PostBidContentAsync(
                BidDto bid, 
                CancellationToken token, 
                HttpClient httpClient,
                HttpRequestMessage request, 
                List<BidResult> items)
        {

            var stopWatch = new Stopwatch();
            using var content = GetHttpContent(bid);
            request.Content = content;
            stopWatch.Start();
            using var response = await httpClient 
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
                .ConfigureAwait(false);
            stopWatch.Stop();
            response.EnsureSuccessStatusCode();
            var resultsJson = await response.Content.ReadAsStringAsync();
            var bidResult = JsonConvert.DeserializeObject<BidResult>(resultsJson);
            if (bidResult is not null)
            {
                bidResult.PerformanceMetrics.TotalHttpLatency = stopWatch.Elapsed.TotalMilliseconds; 
                TimeSpan ts = bidResult.Received - bidResult.Sent;
                bidResult.TimeSpanRaw = ts.TotalMilliseconds;
                bidResult.TimeSpan = $"{ts.TotalMilliseconds} ms";
                items.Add(bidResult);
                Console.WriteLine($"***REST API*** DocumentId:  {bidResult.DocumentId}");
                Console.WriteLine($"***REST API*** BidId:  {bidResult.BidId}");
                Console.WriteLine($"***REST API*** AuctionId:  {bidResult.AuctionId}");
                Console.WriteLine($"***REST API*** Metric API SendDateTime: {bidResult.PerformanceMetrics.ApiSendDateTime}");
                Console.WriteLine($"***REST API*** Metric API Latency: {bidResult.PerformanceMetrics.ApiLatency} ms");
                Console.WriteLine($"***REST API*** Metric API DbExecutionTime: {bidResult.PerformanceMetrics.DbExecutionTime} ms");
                Console.WriteLine($"***REST API*** Metric API DbElapsedTime: {bidResult.PerformanceMetrics.DbElapsedTime} ms");
                Console.WriteLine($"***REST API*** Http Request: {bidResult.PerformanceMetrics.TotalHttpLatency} ms");
            }
        }
        
        public async Task<AuctionItemsDao> GetItemsAsync(bool forceRefresh = false)
        {
            var uri = new Uri(Constants.RestUri.GetAuctions, UriKind.Relative);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri
            };
            try
            {
                var _httpClient = (_databaseService.DatacenterLocation == Constants.Labels.DatacenterLocationWavelength)
                                    ? _wavelengthHttpClient
                                    : _cloudHttpClient;
                //test performance
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var response = await _httpClient
                    .GetAsync(request.RequestUri, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false);
                stopWatch.Stop();

                //parse results
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var results = JsonConvert.DeserializeObject<AuctionItemsDao>(json);
                    results.NetworkOverheadTime = stopWatch.Elapsed.TotalMilliseconds - results.ApiOverheadTime;
                    return results;
                }
            }
            catch (Exception ex)
            {
                //todo logging
                throw ex;
            }
            return new AuctionItemsDao();
        }
        
        private HttpContent GetHttpContent(BidDto bidDto)
        {
            HttpContent httpContent = null;
            var rand = new Random();
            bidDto.Sent = DateTimeOffset.Now;
            var ms = new MemoryStream();
            SerializeJsonIntoStream(bidDto, ms);
            ms.Seek(0, SeekOrigin.Begin);
            httpContent = new StreamContent(ms);
            /*
               **too slow** - going to use custom stream instead
            var json = JsonConvert.SerializeObject(bidDto);
            httpContent = new StringContent(json); 
            */
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return httpContent;
        }
        
        private void SerializeJsonIntoStream(BidDto value, Stream stream)
        {
            using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jtw, value);
                jtw.Flush();
            }
        }

        private void CalculateUrls()
        {
            _cloudApiBaseUri = $@"{Constants.RestUri.CloudServerProtocol}://{Constants.RestUri.CloudServerBaseUrl}:{Constants.RestUri.CloudServerPort}";
            _wavelengthApiBaseUri = $@"{Constants.RestUri.WavelengthServerProtocol}://{Constants.RestUri.WavelengthServerBaseUrl}:{Constants.RestUri.WavelengthServerPort}";
        }
    }
}
