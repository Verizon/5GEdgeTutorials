using System.Net.Http;

namespace Wavelength.Services
{
    public interface IHttpClientHandlerFactory
    {
        HttpClientHandler GetHandler();
    }
}
