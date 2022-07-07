using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.Search
{
    public class SearchViewModel : _BaseViewModel
    {
        private string _lastQuery = null;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly CollectionView _availableCV;
        private readonly CollectionView _otherCV;
        
        public SearchViewModel(CollectionView availableCV, CollectionView otherCV, StackLayout slDimmer, INavigation navigation) : base(slDimmer, navigation)
        {
            _availableCV = availableCV;
            _otherCV = otherCV;
        }

        public Command HideKeyboard { get; } = new Command(() => DependencyService.Get<IKeyboardHelper>().HideKeyboard());
        
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
                    TabIndex = 0;
                OnPropertyChanged(nameof(CVMargin));
            }
        }

        private int _tabIndex = 0;
        public int TabIndex
        {
            get => _tabIndex;
            set => SetProperty(ref _tabIndex, value);
        }

        public Thickness CVMargin
        {
            get
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    if (ShowTabs)
                        return new Thickness(0, 36, 0, 0);
                    else
                        return new Thickness(0, 0, 0, 0);
                }
                else
                {
                    return new Thickness(0, 12, 0, 0);
                }
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


        public async Task DoSearch(string query, bool delay)
        {
            var formattedQuery = StringUtils.NormalizedQueryString(query) + string.Empty;
            if (formattedQuery == _lastQuery)
                return;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            if (formattedQuery == string.Empty)
            {
                ShowTabs = false;
                MediaEmptyString = string.Empty;
                IsBusy = false;
                AvailableItems.Clear();
                OtherItems.Clear();
                return;
            }


            IsBusy = true;
            MediaEmptyString = string.Empty;
            _lastQuery = formattedQuery;

            //Less load on the server
            if (delay)
                try { await Task.Delay(500, token); }
                catch { return; }

            var response = await App.API.Media.SearchAsync(query, true, token);
            if (response.Success)
            {
                ShowTabs = response.Data.OtherTitlesAllowed;

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

                if (await response.Error.HandleUnauthorizedException())
                    return;

                ShowTabs = false;

                MediaEmptyString = "Error searching. Please try again";
                AvailableItems.Clear();
                OtherItems.Clear();
            }

            IsBusy = false;
        }
    }
}
