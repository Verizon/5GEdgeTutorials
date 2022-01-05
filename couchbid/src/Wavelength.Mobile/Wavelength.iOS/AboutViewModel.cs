using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Wavelength.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private string _aboutDescriptionIntro;
        public string AboutDescriptionIntro
        {
            get => _aboutDescriptionIntro;
            set => SetProperty(ref _aboutDescriptionIntro, value);
	    }

        private string _aboutDescriptionDetails;
        public string AboutDescriptionDetails
        {
            get => _aboutDescriptionDetails;
            set => SetProperty(ref _aboutDescriptionDetails, value);
	    }

        private string _aboutDescriptionDisclaimer;
        public string AboutDescriptionDisclaimer
        {
            get => _aboutDescriptionDisclaimer;
            set => SetProperty(ref _aboutDescriptionDisclaimer, value);
        }

        private string _aboutCouchbaseLite;
        public string AboutCouchbaseLite
        {
            get => _aboutCouchbaseLite;
            set => SetProperty(ref _aboutCouchbaseLite, value);
	    }

        public ICommand OpenVerizonWebCommand { get; }
        public ICommand OpenCouchbaseWebCommand { get; }
        public ICommand OpenAWSWebCommand { get; }

        public AboutViewModel()
        {
            Title = "About Couchbid";

            SetFields();

            OpenVerizonWebCommand = new Command(async () => await Browser.OpenAsync("https://www.verizon.com/business/solutions/5g/edge-computing/developer-resources/"));
            OpenCouchbaseWebCommand = new Command(async () => await Browser.OpenAsync("https://developer.couchbase.com/mobile/"));
            OpenAWSWebCommand = new Command(async () => await Browser.OpenAsync("https://aws.amazon.com/wavelength/"));
        }

        private void SetFields() 
	    {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("Couchbid is a demo app implementation of Couchbase Lite, Sync Gateway, and Couchbase Server");
            sb.Append(" in AWS Wavelength using Verizon 5G Edge.");
            AboutDescriptionIntro = sb.ToString();

            sb.Clear();

		    sb.Append("This app shows items in a fictitious auction.  ");
            sb.Append("The winner is cacluated based on who bids with the lowest latency to the Post Bid API.  Users can bid as many times as they like ");
            sb.Append("before the auction expires.  Once the auction expires, a winner is picked and displayed on that device only.");
            AboutDescriptionDetails = sb.ToString();

            sb.Clear();

            sb.Append("This app generates a unique ID (UUID) that is used to track your bids which is used to calculate a winner.  ");
            sb.Append("This unique ID is NOT used to track any personal information about you or the device.");
            AboutDescriptionDisclaimer = sb.ToString();

            sb.Clear();
            sb.Append("Couchbase Mobile is the complete NoSQL database solution for all data storage, access, sync and security across the entire application stack.  ");
            AboutCouchbaseLite = sb.ToString();

        } 

	    
    }
}
