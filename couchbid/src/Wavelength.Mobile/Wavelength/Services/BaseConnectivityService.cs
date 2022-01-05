using System;
using System.Threading.Tasks;

namespace Wavelength.Services
{
    public abstract class BaseConnectivityService 
	    : IConnectivityService, IDisposable
    {
        private bool _disposing = false;

        public abstract Task<bool> IsReachable(string host, int msTimeout = 5000);

        public abstract Task<bool> IsRemoteReachable(string host, int port = 80, int msTimeout = 5000);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BaseConnectivityService()
        {
            Dispose(false);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposing)
            {
                if (disposing)
                {
                    //dispose only
                }

                _disposing = true;
            }
        }
    }
}
