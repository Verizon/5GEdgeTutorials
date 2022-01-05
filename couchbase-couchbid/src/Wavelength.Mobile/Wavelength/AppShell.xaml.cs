using System;
using System.Collections.Generic;
using Wavelength.Services;
using Wavelength.ViewModels;
using Wavelength.Views;
using Xamarin.Forms;

namespace Wavelength
{
    public partial class AppShell 
	    : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
        }
    }
}
