using System;
using System.Net.Http;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wavelength.Droid.Network;
using Wavelength.Services;
using Wavelength.Repository;
using Wavelength.Droid.Services;
using Couchbase.Lite;
using System.IO;

namespace Wavelength.Droid
{
    [Activity(
	    Label = "Couchbid", 
	    Icon = "@mipmap/ic_launcher_round", 
	    Theme = "@style/MainTheme", 
	    MainLauncher = true, 
	    ConfigurationChanges = 
	        ConfigChanges.ScreenSize | 
	        ConfigChanges.Orientation | 
	        ConfigChanges.UiMode | 
	        ConfigChanges.ScreenLayout | 
	        ConfigChanges.SmallestScreenSize)]
    public class MainActivity 
	    : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity 
			
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			Couchbase.Lite.Support.Droid.Activate(this);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
			FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
			var formsApp = new App();
            Startup.Init(ConfigureServices, formsApp.RegisterServices);
			LoadApplication(formsApp);
			global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);
		}
        
        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
	        services.AddSingleton<IHttpClientHandlerFactory, HttpClientHandlerAndroidFactory>();
	        services.AddSingleton<Wavelength.Services.IHttpClientFactory, HttpClientFactory>();
	        services.AddSingleton<IAuctionHttpRepository, AuctionHttpRepository>();

	        var pinnedCertService = new PinnedCertificateService(this.Assets);
	        services.AddSingleton<IPinnedCertificateService>(pinnedCertService);
	        services.AddSingleton<IConnectivityService, ConnectivityService>();
		}
        
        public override void OnRequestPermissionsResult(
	        int requestCode, 
	        string[] permissions, 
	        [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(
		        requestCode, 
		        permissions, 
				grantResults);
        }
    }
}
