using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Wavelength.Services;

namespace Wavelength.iOS.Services
{
    public class ConnectivityService 
	    : BaseConnectivityService
    {
        public override async Task<bool> IsReachable(string host, int msTimeout = 5000)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException(nameof(host));


            return await IsRemoteReachable(host, 80, msTimeout);
        }

        public override async Task<bool> IsRemoteReachable(string host, int port = 80, int msTimeout = 5000)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException(nameof(host));

            host = host.Replace("http://www.", string.Empty).
                Replace("http://", string.Empty).
                Replace("https://www.", string.Empty).
                Replace("https://", string.Empty).
                Replace("ws://", string.Empty).
                Replace("wss://", string.Empty).
                TrimEnd('/');

            return await Task.Run(() =>
            {
                var reachable = false;

                try
                {
                    var clientDone = new ManualResetEvent(false);
                    var hostEntry = new DnsEndPoint(host, port);

                    using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
                    {
                        var socketEventArg = new SocketAsyncEventArgs { RemoteEndPoint = hostEntry };
                        socketEventArg.Completed += (s, e) =>
                        {
                            reachable = e.SocketError == SocketError.Success;
                            clientDone.Set();
                        };

                        clientDone.Reset();
                        socket.ConnectAsync(socketEventArg);
                        clientDone.WaitOne(msTimeout);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to reach: " + host + " Error: " + ex);
                }

                return reachable;
            });
        }
    }
}
