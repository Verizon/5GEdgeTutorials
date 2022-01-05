using System;
using System.Threading.Tasks;

namespace Wavelength.Services
{
    public interface IConnectivityService : IDisposable
    {
        /// <summary>
        /// Tests if a host name is pingable
        /// </summary>
        /// <param name="host">The host name can either be a machine name, such as "www.couchbase.com", 
	    /// or a textual representation of its IP address (127.0.0.1)</param>
        /// <param name="msTimeout">Timeout in milliseconds</param>
        /// <returns></returns>
        Task<bool> IsReachable(string host, int msTimeout = 5000);

        /// <summary>
        /// Tests if a remote host name is reachable
        /// </summary>
        /// <param name="host">Host name can be a remote IP or URL of website</param>
        /// <param name="port">Port to attempt to check is reachable.</param>
        /// <param name="msTimeout">Timeout in milliseconds.</param>
        /// <returns></returns>
        Task<bool> IsRemoteReachable(string host, int port = 80, int msTimeout = 5000);
    }
}
