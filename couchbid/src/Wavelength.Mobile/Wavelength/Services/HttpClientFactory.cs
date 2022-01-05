using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Wavelength.Services
{
    public class HttpClientFactory
        : IHttpClientFactory
    {
        private readonly IHttpClientHandlerFactory _httpClientHandlerFactory;
        private HttpClient _httpClient;

        public HttpClientFactory(
	        IHttpClientHandlerFactory httpClientHandlerFactory)
        {
            _httpClientHandlerFactory = httpClientHandlerFactory;
        }

        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }

        public HttpClient GetHttpClient(string baseUri)
        {
            _httpClient = new HttpClient(_httpClientHandlerFactory.GetHandler())
            {
                BaseAddress = new Uri(baseUri)
            };
            _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            return _httpClient;
        }
    }
}
