using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using Wavelength.Models;
using Wavelength.Views;
using Wavelength.Repository;
using Newtonsoft.Json;

namespace Wavelength.ViewModels
{
    public class ItemsViewModel 
	    : BaseViewModel, IViewModel
    {
        private readonly IAuctionHttpRepository _auctionHttpRepository;
        private readonly ICBLiteAuctionRepository _auctionRepository;

        private AuctionItem _selectedItem;
        
        public Command<IEnumerable<AuctionItem>> AuctionItemsUpdatedCommand { get; }
        public ObservableCollection<AuctionItem> Items { get; private set; }
        public Command<AuctionItem> ItemTapped { get; }

        public ItemsViewModel(ICBLiteAuctionRepository auctionRepository)
        {
            Title = "Auctions";
            _auctionRepository = auctionRepository;
            _auctionHttpRepository = DependencyService.Get<IAuctionHttpRepository>();
            Items = new ObservableCollection<AuctionItem>();
            
            //setup live query 
            AuctionItemsUpdatedCommand = new Command<IEnumerable<AuctionItem>>(OnAuctionItemsUpdate);
            
            //handle navigation when item is selected
            ItemTapped = new Command<AuctionItem>(OnItemSelected);
        }

        private void OnAuctionItemsUpdate(IEnumerable<AuctionItem> auctionItems)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    IsBusy = true;
                    Items.Clear();
                    if (Items.Count > 0)
                    {
                        Items.Clear();
                    }
                    //add items
                    foreach (var item in auctionItems)
                    {
                        Items.Add(item);
                    }
                    IsBusy = false;
                });
            }
            catch (Exception ex) 
	        {
                Console.WriteLine($"{ex.Message}");
	        }
        }
        
        public void OnAppearing()
        {
            SelectedItem = null;
            _auctionRepository.RegisterAuctionLiveQuery(AuctionItemsUpdatedCommand);
        }

        public void OnDisappearing()
        {
            _auctionRepository.DeRegisterAuctionLiveQuery();    
        }

        public AuctionItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        async void OnItemSelected(AuctionItem item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.Item)}={JsonConvert.SerializeObject(item)}");
        }
    }
}
