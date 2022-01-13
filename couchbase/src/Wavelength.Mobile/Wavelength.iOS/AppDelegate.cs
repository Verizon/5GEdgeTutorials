using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UIKit;
using Wavelength.iOS.Network;
using Wavelength.iOS.Services;
using Wavelength.Models;
using Wavelength.Repository;
using Wavelength.Services;

namespace Wavelength.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            global::Xamarin.Forms.FormsMaterial.Init();
            var formsApp = new App();
            Startup.Init(ConfigureServices, formsApp.RegisterServices);

            LoadApplication(formsApp);
            return base.FinishedLaunching(app, options);
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IConnectivityService, ConnectivityService>();
            services.AddSingleton<IHttpClientHandlerFactory, HttpClientHandlerIOSFactory>();
            services.AddSingleton<IHttpClientFactory, HttpClientFactory>();
        }
    }
}
