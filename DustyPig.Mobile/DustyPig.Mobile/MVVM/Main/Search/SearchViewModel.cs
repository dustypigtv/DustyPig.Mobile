using DustyPig.API.v3.Models;
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
        private int _lastIndex = 0;
        private double _width = 0;

        public SearchViewModel(CollectionView availableCV, CollectionView otherCV, INavigation navigation) : base(navigation)
        {
            _availableCV = availableCV;
            _otherCV = otherCV;
            _otherCV.TranslationX = 0;

            TabHeaderTapped = new Command<int>(OnTabHeaderTapped);
        }

        public Command<int> TabHeaderTapped { get; }
        private void OnTabHeaderTapped(int index)
        {
            if (index == _lastIndex)
            {
                if (index == 1)
                {
                    _otherCV.ScrollTo(0, -1, ScrollToPosition.Start, true);
                }
                else
                {
                    _availableCV.ScrollTo(0, -1, ScrollToPosition.Start, true);
                }
            }
            else
            {
                SlideItems(index, true);
            }
        }


        private void SlideItems(int index, bool animated)
        {
            if (!_showTabs)
                return;

            //Default is 250
            uint duration = animated ? 250u : 0u;


            if (index == 1)
            {
                AvailableCVColor = Theme.Grey;
                OtherCVColor = Color.White;

                _availableCV.TranslateTo(-_width - 100, 0, duration);
                _otherCV.TranslateTo(0, 0, duration);
            }
            else
            {
                AvailableCVColor = Color.White;
                OtherCVColor = Theme.Grey;

                _availableCV.TranslateTo(0, 0, duration);
                _otherCV.TranslateTo(_width + 100, 0, duration);
            }
            
            _lastIndex = index;
        }

        private Color _availableCVColor = Color.White;
        public Color AvailableCVColor
        {
            get => _availableCVColor;
            set => SetProperty(ref _availableCVColor, value);
        }

        private Color _otherCVColor = Theme.Grey;
        public Color OtherCVColor
        {
            get => _otherCVColor;
            set => SetProperty(ref _otherCVColor, value);
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
                    OnTabHeaderTapped(0);
            }
        }


        public void OnSizeAllocated(double width, double height)
        {
            _width = width;
            SlideItems(_lastIndex, false);            

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
            if(delay)
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


                MediaEmptyString = "Error searching. Please try again";
                AvailableItems.Clear();
                OtherItems.Clear();
            }

            IsBusy = false;
        }
    }
}
