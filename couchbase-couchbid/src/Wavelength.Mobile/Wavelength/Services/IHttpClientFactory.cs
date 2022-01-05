using System;
using System.Net.Http;

namespace Wavelength.Services
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient(string baseUri);
        HttpClient GetHttpClient();
    }
}
