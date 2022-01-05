using System;
using System.Net.Http;
using System.Threading.Tasks;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;
using NBomber.Plugins.Network.Ping;

namespace Wavelength.Server.PerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
	        using (var httpClientHandler = new HttpClientHandler()) 
	        {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                var httpClient = new HttpClient(httpClientHandler);
                var step = Step.Create("fetchAuctions", async context => {

                    var response = await httpClient.GetAsync("https://192.168.50.225:9001/api/v1/Auction", context.CancellationToken);
                    return response.IsSuccessStatusCode
                        ? Response.Ok(statusCode: (int)response.StatusCode, sizeBytes: response.ToNBomberResponse().SizeBytes)
                        : Response.Fail(statusCode: (int)response.StatusCode);
                });

                var scenario = ScenarioBuilder
                                .CreateScenario("auctionsScenario", step)
                                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                                .WithLoadSimulations(
                                    Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromSeconds(30))
                                );

                // creates ping plugin that brings additional reporting data
                var pingPluginConfig = PingPluginConfig.CreateDefault(new[] { "192.168.50.225" });
                var pingPlugin = new PingPlugin(pingPluginConfig);

                NBomberRunner
                    .RegisterScenarios(scenario)
                    .WithWorkerPlugins(pingPlugin)
                    .Run();

                httpClient.Dispose();
                httpClient = null;
            }
	    }
	}
}
