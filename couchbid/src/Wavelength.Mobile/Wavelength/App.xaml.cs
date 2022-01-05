using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xamarin.Forms;
using Wavelength.Services;
using Wavelength.Models;
using Wavelength.Repository;
using Xamarin.Essentials;
using Microsoft.Extensions.DependencyInjection;
using Wavelength.ViewModels;

namespace Wavelength
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        public void RegisterServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<ICBLiteDatabaseService, CBLiteDatabaseService>();
            services.AddSingleton<ICBLiteAuctionRepository, CBLiteAuctionRepository>();

            //load view models
            services.AddSingleton<ItemsViewModel>();
            services.AddTransient<InformationViewModel>();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
