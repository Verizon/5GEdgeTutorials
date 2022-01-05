using Wavelength.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Wavelength.Views
{
    public partial class ItemsPage : ContentPage
    {
        private readonly ItemsViewModel _viewModel;

        public ItemsPage()
        {
            try
            {
                InitializeComponent();
                _viewModel = Startup.ServiceProvider.GetService<ItemsViewModel>();
                BindingContext = _viewModel;
            }
            catch (System.Exception ex) 
	        {
                System.Console.WriteLine($"{ex.Message} {ex.StackTrace}");
	        }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnDisappearing();
            
        }
    }
}
