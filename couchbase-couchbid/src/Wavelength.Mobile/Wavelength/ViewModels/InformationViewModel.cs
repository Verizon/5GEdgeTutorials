using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Wavelength.Constants;
using Xamarin.Forms;

using Wavelength.Models;
using Wavelength.Views;
using Wavelength.Repository;
using Wavelength.Services;

namespace Wavelength.ViewModels
{
    public class InformationViewModel
        : BaseViewModel
    {
        private readonly ICBLiteDatabaseService _databaseService;
        private readonly ICBLiteAuctionRepository _auctionRepository;
        
        private Action<string> ReplicationStatusUpdateSubscription;
        private Action<Exception> ReplicationStatusErrorSubscription;
        private Action<ProgressStatus> ReplicationStatusProgressStatusUpdateSubscription;
        private bool _didShowReplicationError = false; 
        
        private string _indexCount;
        public string IndexCount
        {
            get => _indexCount;
            set => SetProperty(ref _indexCount, value);
        }

        private string _deviceId;

        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }
        
        private string _datacenterLocation;
        public string DatacenterLocation
        {
            get => _datacenterLocation;
            set => SetProperty(ref _datacenterLocation, value);
        }

        private string _replicationStatus;
        public string ReplicationStatus
        {
            get => _replicationStatus;
            set => SetProperty(ref _replicationStatus, value);
        }

        private string _syncGatewayUri;
        public string SyncGatewayUri
        {
            get => _syncGatewayUri;
            set => SetProperty(ref _syncGatewayUri, value);
        }
        
        private string _restApiUri;
        public string RestApiUri
        {
            get => _restApiUri;
            set => SetProperty(ref _restApiUri, value);
        }

        private string _databaseName;
        public string DatabaseName
        {
            get => _databaseName;
            set => SetProperty(ref _databaseName, value);
        }

        private int _auctionCount;
        public int AuctionCount
        {
            get => _auctionCount;
            set => SetProperty(ref _auctionCount, value);
        }
        
        private int _bidCount;
        public int BidCount
        {
            get => _bidCount;
            set => SetProperty(ref _bidCount, value);
        }
        
        public Command<int> UpdateAuctionCount { get; }
        public Command<int> UpdateBidCount { get; }
        public Command DeleteDatabase { get; }
        
        public InformationViewModel(ICBLiteDatabaseService databaseService,
                                    ICBLiteAuctionRepository auctionRepository)
        {
            _databaseService = databaseService;
            _auctionRepository = auctionRepository;
            
            //messaging subscription setup
            Messaging.Instance.Subscribe<string>(Messages.ReplicationChangeStatus, nameof(InformationViewModel), UpdateReplicationStatus); 
            Messaging.Instance.Subscribe<Exception>(Messages.ReplicationError, nameof(InformationViewModel), UpdateReplicationError); 
            DeleteDatabase = new Command(OnDeleteDatabase);
            
            //set events for updating auction count
            UpdateAuctionCount = new Command<int>(OnUpdateAuctionCount); 
            UpdateBidCount = new Command<int>(OnUpdateBidCount); 
            _auctionRepository.RegisterAuctionCount(UpdateAuctionCount);
            _auctionRepository.RegisterBidCount(UpdateBidCount);
            
            //default values
            Title = "Settings";
            IndexCount = "0";
             
            SetupFields();
        }

        public void OnDisappearing()
        {
            _auctionRepository.DeregisterAuctionCount();
            _auctionRepository.DeregisterBidCount();
            
            Messaging.Instance.Unsubscribe<string>(Messages.ReplicationChangeStatus, nameof(InformationViewModel));
            Messaging.Instance.Unsubscribe<Exception>(Messages.ReplicationError, nameof(InformationViewModel));
        }
        
        private void UpdateReplicationError(Exception ex)
        {
            //bugfix - only show error once
            //otherwise you can get bombed with errors
            //very unpleasant
            if (!_didShowReplicationError)
            {
                Task.Run(async () =>
                {
                    _didShowReplicationError = true;
                    await Page.DisplayAlert("Replication Error", ex.Message, "Ok");
                });
            }
        }

        private void UpdateReplicationStatus(string status)
        {
            ReplicationStatus = status;
        }

        private void SetupFields()
        {
            DatacenterLocation = _databaseService.DatacenterLocation;
            SyncGatewayUri = _databaseService.SyncGatewayUri;
            RestApiUri = _databaseService.RestApiUri;
            DatabaseName = _databaseService.DatabaseName;
            IndexCount = _databaseService.IndexCount;
            ReplicationStatus = _databaseService.LastReplicatorStatus;
            DeviceId = _databaseService.DeviceId;
        }

        private void OnUpdateBidCount(int count)
        {
            BidCount = count;
        }
        
        private void OnUpdateAuctionCount(int count)
        {
            AuctionCount = count;
        } 
        
        private async void OnDeleteDatabase()
        {
            await Task.CompletedTask;
        }
    }
}
