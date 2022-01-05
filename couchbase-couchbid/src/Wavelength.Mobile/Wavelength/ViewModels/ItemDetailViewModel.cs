using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Wavelength.Models;
using Wavelength.Repository;
using Wavelength.Services;
using Xamarin.Forms;

namespace Wavelength.ViewModels
{
    [QueryProperty(nameof(Item), nameof(Item))]
    public class ItemDetailViewModel 
        : BaseViewModel
    {
        private IAuctionHttpRepository _auctionHttpRepository;
        private ICBLiteAuctionRepository _auctionRepository;
        private ICBLiteDatabaseService _databaseService;
        
        private AuctionItem _auctionItem; 
        
        private string _item;
        public string Item 
	    {   get => _item;
            set 
	        {
                if (value.Contains(" 00:00\""))
                {
                    _item = value.Replace(" 00:00", "");
                }
                else
                {
                    _item = value;
                }
                LoadItem();
	        } 
	    }

        public string Id { get; set; }

        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

	    private string _displayStartTime;
	    public string DisplayStartTime 
	    {
	        get => _displayStartTime;
	        set => SetProperty(ref _displayStartTime, value);
	    }

        private string _displayStopTime;
        public string DisplayStopTime
        {
            get => _displayStopTime;
            set => SetProperty(ref _displayStopTime, value);
        }

        private bool _isAuctionActive;
        public bool IsAuctionActive
        {
            get => _isAuctionActive;
            set => SetProperty(ref _isAuctionActive, value);
        }
        
        public Command BidOnItemCommand { get; }
        public Command<IEnumerable<Bid>> BidItemsUpdateCommand { get;  }
        public ObservableCollection<Bid> Items { get; private set; }

        public ItemDetailViewModel()
        {
            Items = new ObservableCollection<Bid>();
            _auctionRepository = Startup.ServiceProvider.GetService<ICBLiteAuctionRepository>();
            _databaseService = Startup.ServiceProvider.GetService<ICBLiteDatabaseService>();
            _auctionHttpRepository = Startup.ServiceProvider.GetService<IAuctionHttpRepository>();
            
            BidItemsUpdateCommand = new Command<IEnumerable<Bid>>(OnBidItemsUpdate);
            BidOnItemCommand = new Command(OnBidOnItem);
        }
        
        public void OnDisappearing()
        {
            _auctionRepository.DeRegisterBidsLiveQuery(_auctionItem.DocumentId);       
        }
        
        private void LoadItem()
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    //todo - research better way to do this
                    _auctionItem = JsonConvert.DeserializeObject<AuctionItem>(Item);

                    //register for live query of bids
                    if (_auctionItem is not null)
                    {
                        _auctionRepository.RegisterBidsLiveQuery(BidItemsUpdateCommand, _auctionItem?.DocumentId);
                        //set UI items
                        Text = _auctionItem.Title;
                        ImageUrl = _auctionItem.ImageUrl;
                        IsAuctionActive = !_auctionItem.IsWinnerCalculated;
                        if (IsAuctionActive)
                        {
                            DisplayStopTime = $"End {_auctionItem.StopTime.Humanize()}";
                        }
                        else
                        {
                            if (_auctionItem.WinnerDeviceId.ToString() == _databaseService.DeviceId)
                            {
                                DisplayStopTime = $"You Won this Auction - show this to someone at the booth!";
                            }
                            else
                            {
                                DisplayStopTime = $"Auction is over";
                            }
                        }
                    }
                });
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }
        
        private void OnBidItemsUpdate(IEnumerable<Bid> bidItems)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                IsBusy = true;
                //clear out previous items
                if (Items.Count > 0)
                {
                    Items.Clear();
                }
                //add items
                foreach (var item in bidItems
                    .OrderBy(x => x.TimeSpanRaw)
                    .ThenBy(x => x.Received))
                {
                    Items.Add(item);
                }
                IsBusy = false;
            });
        }
        
        private async void OnBidOnItem()
        {
            try
            {
                var bid = new BidDto();
                bid.Id = _auctionItem.DocumentId;
                bid.BidId = Guid.NewGuid().ToString();
                bid.DeviceId = _databaseService.DeviceId;
                var results = await _auctionHttpRepository.PostBidAsync(bid, CancellationToken.None);
                if (results is not null)
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
