using System;
using System.Net.Http;

namespace Wavelength.Services
{
    public class HttpClientHandlerFactory
        : IHttpClientHandlerFactory
    {
        public HttpClientHandler GetHandler()
        {
            return new HttpClientHandler
            {
                UseProxy = true,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
        }
    }
}
