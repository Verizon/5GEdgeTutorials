using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Wavelength.Repository;
using Wavelength.Services;
using Wavelength.ViewModels;
using Xamarin.Essentials;

namespace Wavelength
{
    public static class Startup
    {
        public static IServiceProvider ServiceProvider { get; set;  }

        public static void Init(
	                Action<HostBuilderContext, IServiceCollection> nativeConfigureServices, 
	                Action<HostBuilderContext, IServiceCollection> appConfigureServices)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("Wavelength.appsettings.json"))
            {
                var host = new HostBuilder()
                    .ConfigureHostConfiguration(configure =>
                    {
                        // Tell the host configuration where to file the file (this is required for Xamarin apps)
                        configure.AddCommandLine(new string[] {$"ContentRoot={FileSystem.AppDataDirectory}"});
                        //read in the configuration file!
                        configure.AddJsonStream(stream);
                    }).ConfigureServices((context, services) =>
                    {
                        nativeConfigureServices(context, services);
                        appConfigureServices(context, services);
                        services.AddHttpClient();
                        ConfigureServices(context, services);
                    }).ConfigureLogging(logBuilder =>
                    {
                        logBuilder.AddConsole(console =>
                        {
                        });
                    }).Build();
                ServiceProvider = host.Services;
            }     
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<ICBLiteDatabaseService, CBLiteDatabaseService>();
            services.AddSingleton<ICBLiteAuctionRepository, CBLiteAuctionRepository>();
            services.AddSingleton<IAuctionHttpRepository, AuctionHttpRepository>();
            
            //setup viewmodels
            services.AddTransient<AboutViewModel>();
            services.AddTransient<ItemsViewModel>();
            services.AddTransient<ItemDetailViewModel>();
        }
    }
}