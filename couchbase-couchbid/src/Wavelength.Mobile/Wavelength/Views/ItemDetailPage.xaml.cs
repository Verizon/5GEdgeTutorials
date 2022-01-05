using System.ComponentModel;
using Xamarin.Forms;
using Wavelength.ViewModels;

namespace Wavelength.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        private readonly ItemDetailViewModel _viewModel;
        public ItemDetailPage()
        {
            InitializeComponent();
            _viewModel = new ItemDetailViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnDisappearing(); 
        }
    }
}
