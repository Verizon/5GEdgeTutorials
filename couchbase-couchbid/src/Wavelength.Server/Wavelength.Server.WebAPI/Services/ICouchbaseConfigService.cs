using Wavelength.Core.Models;

namespace Wavelength.Server.WebAPI.Services
{
    public interface ICouchbaseConfigService
    {
        CouchbaseConfig Config { get; }

        public void InitConfig();
    }
}
