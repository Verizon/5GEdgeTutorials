using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Java.Net;
using Wavelength.Services;

namespace Wavelength.Droid.Services
{
    public class ConnectivityService
        : BaseConnectivityService
    {
        public override async Task<bool> IsReachable(string host, int msTimeout = 5000)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException(nameof(host));


            return await Task.Run(() =>
            {
                bool reachable;
                try
                {
                    reachable = InetAddress.GetByName(host).IsReachable(msTimeout);
                }
                catch (UnknownHostException uhex)
                {
                    Debug.WriteLine("Unable to reach: " + host + " Error: " + uhex);
                    reachable = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to reach: " + host + " Error: " + ex);
                    reachable = false;
                }
                return reachable;
            });
        }

        public override async Task<bool> IsRemoteReachable(string host, int port = 80, int msTimeout = 5000)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException(nameof(host));

            host = host.Replace("http://www.", string.Empty).
              Replace("http://", string.Empty).
              Replace("https://www.", string.Empty).
              Replace("https://", string.Empty).
              TrimEnd('/');

            return await Task.Run(async () =>
            {
                try
                {
                    var tcs = new TaskCompletionSource<InetSocketAddress>();
                    new System.Threading.Thread(() =>
                    {
                        /* this line can take minutes when on wifi with poor or none internet connectivity
                        and Task.Delay solves it only if this is running on new thread (Task.Run does not help) 
			            Also depending on which android version you are running this might through an exception when it times out, 
			            hence the try catch.  Yes I know - why can't it be consistant, but that's like asking how many licks 
			            does it take to get to the center of a tooties roll pop - the world might never know...
			            LABEAAA 11/16/2021  */
                        InetSocketAddress result = new InetSocketAddress(host, port);

                        if (!tcs.Task.IsCompleted)
                            tcs.TrySetResult(result);

                    }).Start();

                    await Task.Run(async () =>
                    {
                        await Task.Delay(msTimeout);

                        if (!tcs.Task.IsCompleted)
                            tcs.TrySetResult(null);
                    });

                    var sockaddr = await tcs.Task;

                    if (sockaddr == null)
                        return false;

                    using (var sock = new Socket())
                    {
                        await sock.ConnectAsync(sockaddr, msTimeout);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to reach: " + host + " Error: " + ex);
                }

                return false;
            });
        }
    }
}
