using DustyPig.API.v3.Models;
using DustyPig.API.v3.MPAA;
using System;
using System.Collections.Generic;
using System.Text;
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

        public FilterViewModel(ExploreRequest currentRequest, TaskCompletionSource<ExploreRequest> taskCompletionSource, INavigation navigation)
        {
            _currentRequest = currentRequest;
            _taskCompletionSource = taskCompletionSource;   
            _navigation = navigation;

            foreach (Genres genre in Enum.GetValues(typeof(Genres)))
                if(genre != Genres.Unknown)
                    GenreItems.Add(genre.AsString());
            GenreItems.Sort();
            GenreItems.Insert(0, ALL_GENRES);

            if (_currentRequest.FilterOnGenres == null)
                SelectedGenre = ALL_GENRES;
            else
                SelectedGenre = _currentRequest.FilterOnGenres.Value.AsString();


            foreach (string so in Enum.GetNames(typeof(SortOrder)))
                SortOrders.Add(so.Replace('_', ' '));
            SortOrders.Sort();
            SortOrder = _currentRequest.SortBy.ToString().Replace('_', ' ');

            ReturnMovies = _currentRequest.ReturnMovies;
            ReturnSeries = _currentRequest.ReturnSeries;

            CancelCommand = new AsyncCommand(OnCancel, allowsMultipleExecutions: false);
            SaveCommand = new AsyncCommand(OnSave, allowsMultipleExecutions: false);
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

            foreach(SortOrder so in Enum.GetValues(typeof(SortOrder)))
                if(so.ToString().Replace('_', ' ') == SortOrder)
                {
                    ret.SortBy = so;
                    break;
                }

            ret.ReturnMovies = ReturnMovies;
            ret.ReturnSeries = ReturnSeries;

            _taskCompletionSource.TrySetResult(ret);
            await _navigation.PopModalAsync();
        }

        public List<string> GenreItems { get; } = new List<string>();

        private string _selectedGenre;
        public string SelectedGenre
        {
            get => _selectedGenre;
            set => SetProperty(ref _selectedGenre, value);
        }

        public List<string> SortOrders { get; } = new List<string>();

        private string _sortOrder;
        public string SortOrder
        {
            get => _sortOrder;
            set => SetProperty(ref _sortOrder, value);
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
