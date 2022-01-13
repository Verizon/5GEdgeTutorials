using System;
using System.Net.Http;
using Wavelength.Services;

namespace Wavelength.Droid.Network
{
    public class HttpClientHandlerAndroidFactory
       : IHttpClientHandlerFactory
    {
        public HttpClientHandlerAndroidFactory() { }

        public HttpClientHandler GetHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            #if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Contains("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
            #endif
            return handler;
        }
    }
}
