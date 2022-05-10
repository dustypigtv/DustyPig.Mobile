using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Search
{
    public class SearchViewModel : _BaseViewModel
    {
        private string _lastQuery = null;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public SearchViewModel()
        {
            AvailableItemTappedCommand = new AsyncCommand<BasicMedia>(OnAvailableItemTapped, allowsMultipleExecutions: false);
            OtherItemTappedCommand = new AsyncCommand<BasicTMDB>(OnOtherItemTapped, allowsMultipleExecutions: false);
        }

        public AsyncCommand<BasicMedia> AvailableItemTappedCommand { get; }
        private async Task OnAvailableItemTapped(BasicMedia item)
        {
            await DependencyService.Get<IPopup>().AlertAsync("Tapped", item.Title);
        }

        public AsyncCommand<BasicTMDB> OtherItemTappedCommand { get; }
        private async Task OnOtherItemTapped(BasicTMDB item)
        {
            await DependencyService.Get<IPopup>().AlertAsync("Tapped", item.Title);
        }

        private int _span = 1;
        public int Span
        {
            get => _span;
            set => SetProperty(ref _span, Math.Max(value, 1));
        }

       
        private string _mediaEmptyString = string.Empty;
        public string MediaEmptyString
        {
            get => _mediaEmptyString;
            set => SetProperty(ref _mediaEmptyString, value);
        }

       
        private ObservableRangeCollection<BasicMedia> _availableItems = new ObservableRangeCollection<BasicMedia>();
        public ObservableRangeCollection<BasicMedia> AvailableItems
        {
            get => _availableItems;
            set => SetProperty(ref _availableItems, value);
        }

        private ObservableRangeCollection<BasicTMDB> _otherItems = new ObservableRangeCollection<BasicTMDB>();
        public ObservableRangeCollection<BasicTMDB> OtherItems
        {
            get => _otherItems;
            set => SetProperty(ref _otherItems, value);
        }

        private bool _showTabs = false;
        public bool ShowTabs
        {
            get => _showTabs;
            set
            {
                SetProperty(ref _showTabs, value);
                if (!_showTabs)
                    ShowAvailable = true;
            }
        }

        private bool _showAvailable = true;
        public bool ShowAvailable
        {
            get => _showAvailable;
            set => SetProperty(ref _showAvailable, value);
        }

        private int _selectedTab = 0;
        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                SetProperty(ref _selectedTab, value);
                if (ShowTabs)
                    ShowAvailable = value < 1;
                else
                    ShowAvailable = true;
            }
        }

        public void OnSizeAllocated(double width, double height)
        {

            //Poster width = 100
            //Spacing = 12
            //Total = 112
            //Remove spacing from each side of screen
            width -= 24;
            Span = Convert.ToInt32(Math.Floor(width / 112));
        }

        
        public async Task DoSearch(string query)
        {
            query += string.Empty;

            var terms = (StringUtils.NormalizedQueryString(query) + string.Empty).Tokenize();
            if (terms == null)
                terms = new List<string>();
            string formattedQuery = string.Join(" ", terms);

            if (formattedQuery == _lastQuery)
                return;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            if (query == string.Empty)
            {
                MediaEmptyString = string.Empty;
                IsBusy = false;
                AvailableItems.Clear();
                OtherItems.Clear();
                return;
            }

            if (query.Trim().Length < 3 || terms.Count < 1)
            {
                MediaEmptyString = "Enter more letters or words";
                IsBusy = false;
                AvailableItems.Clear();
                OtherItems.Clear();
               return;
            }

            
            IsBusy = true;
            MediaEmptyString = string.Empty;
            _lastQuery = formattedQuery;
            var response = await App.API.Media.SearchAsync(query, true, token);
            if (response.Success)
            {
                ShowTabs = response.Data.OtherTitlesAllowed;
                if (!ShowTabs)
                    ShowAvailable = true;

                MediaEmptyString = "No matches";
                if (response.Data.Available == null)
                    response.Data.Available = new List<BasicMedia>();
                AvailableItems.ReplaceRange(response.Data.Available);

                if (response.Data.OtherTitles == null)
                    response.Data.OtherTitles = new List<BasicTMDB>();
                OtherItems.ReplaceRange(response.Data.OtherTitles);
            }
            else
            {
                if (token.IsCancellationRequested)
                    return;
         

                MediaEmptyString = "Error searching. Please try again";
                AvailableItems.Clear();
                OtherItems.Clear();
            }

            IsBusy = false;
        }
    }
}
