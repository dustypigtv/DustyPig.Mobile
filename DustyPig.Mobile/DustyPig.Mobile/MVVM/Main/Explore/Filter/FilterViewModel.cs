using DustyPig.API.v3.Models;
using DustyPig.API.v3.MPAA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.Explore.Filter
{
    public class FilterViewModel : ObservableObject
    {
        const string ALL_GENRES = "All Genres";

        readonly ExploreRequest _currentRequest;
        readonly TaskCompletionSource<ExploreRequest> _taskCompletionSource;
        readonly INavigation _navigation;

        readonly List<KeyValuePair<string, SortOrder>> _sortMap = new List<KeyValuePair<string, SortOrder>>();

        public FilterViewModel(ExploreRequest currentRequest, TaskCompletionSource<ExploreRequest> taskCompletionSource, INavigation navigation)
        {
            _currentRequest = currentRequest;
            _taskCompletionSource = taskCompletionSource;   
            _navigation = navigation;

            IsBusy = true;
            LoadGenres();

            if (_currentRequest.FilterOnGenres == null)
                SelectedGenre = ALL_GENRES;
            else
                SelectedGenre = _currentRequest.FilterOnGenres.Value.AsString();


            _sortMap.Add(new KeyValuePair<string, SortOrder>("Added", SortOrder.Added_Descending));
            _sortMap.Add(new KeyValuePair<string, SortOrder>("Alphabetical", SortOrder.Alphabetical));
            _sortMap.Add(new KeyValuePair<string, SortOrder>("Popularity", SortOrder.Popularity_Descending));
            _sortMap.Add(new KeyValuePair<string, SortOrder>("Released", SortOrder.Released_Descending));

            foreach (var kvp in _sortMap)
                SortOrders.Add(kvp.Key);
            SortOrders.Sort();
            SelectedSortOrder = _sortMap.First(item => item.Value ==  _currentRequest.SortBy).Key;

            ReturnMovies = _currentRequest.ReturnMovies;
            ReturnSeries = _currentRequest.ReturnSeries;

            CancelCommand = new AsyncCommand(OnCancel, allowsMultipleExecutions: false);
            SaveCommand = new AsyncCommand(OnSave, allowsMultipleExecutions: false);
        }

        private async void LoadGenres()
        {
            IsBusy = true;

            var lst = new List<string>();
            
            var response = await App.API.Media.GetAllAvailableGenresAsync();
            if (response.Success)
            {
                foreach (Genres genre in Enum.GetValues(typeof(Genres)))
                    if (genre != Genres.Unknown)
                        if (response.Data.HasFlag(genre))
                            lst.Add(genre.AsString());
            }
            else
            {
                //Ignore the error and just add all the genres
                foreach (Genres genre in Enum.GetValues(typeof(Genres)))
                    if (genre != Genres.Unknown)
                        lst.Add(genre.AsString());
            }

            lst.Sort();
            lst.Insert(0, ALL_GENRES);

            GenreItems.AddRange(lst);

            IsBusy = false;
        }

        public AsyncCommand CancelCommand { get; }
        private async Task OnCancel()
        {
            //Return the original state
            _taskCompletionSource.TrySetResult(_currentRequest);
            await _navigation.PopModalAsync();
        }

        public AsyncCommand SaveCommand { get; }
        private async Task OnSave()
        {
            var ret = new ExploreRequest();

            if (SelectedGenre != ALL_GENRES)
                ret.FilterOnGenres = SelectedGenre.ToGenres();

            ret.SortBy = _sortMap.First(item => item.Key == SelectedSortOrder).Value;

            ret.ReturnMovies = ReturnMovies;
            ret.ReturnSeries = ReturnSeries;

            if (!(ret.ReturnMovies || ret.ReturnSeries))
                ret.ReturnMovies = true;

            _taskCompletionSource.TrySetResult(ret);
            await _navigation.PopModalAsync();
        }

        public ObservableRangeCollection<string> GenreItems { get; } = new ObservableRangeCollection<string>();

        private string _selectedGenre;
        public string SelectedGenre
        {
            get => _selectedGenre;
            set => SetProperty(ref _selectedGenre, value);
        }

        public List<string> SortOrders { get; } = new List<string>();

        private string _selectedSortOrder;
        public string SelectedSortOrder
        {
            get => _selectedSortOrder;
            set => SetProperty(ref _selectedSortOrder, value);
        }

        private bool _returnMovies;
        public bool ReturnMovies
        {
            get => _returnMovies;
            set => SetProperty(ref _returnMovies, value);
        }

        private bool _returnSeries;
        public bool ReturnSeries
        {
            get => _returnSeries;
            set => SetProperty(ref _returnSeries, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public void OnSizeAllocated(double width, double height)
        {
            if (Device.Idiom == TargetIdiom.Phone)
            {
                Width = width;
            }
            else
            {
                double newWidth = width;
                switch (DeviceDisplay.MainDisplayInfo.Orientation)
                {
                    case DisplayOrientation.Landscape:
                        newWidth = width * 0.33;
                        break;

                    case DisplayOrientation.Portrait:
                        newWidth = height * 0.33;
                        break;
                }
                newWidth = Math.Max(newWidth, 400);

                Width = (int)Math.Min(width, newWidth);
            }
        }


    }
}
