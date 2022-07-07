using DustyPig.API.v3.Models;
using DustyPig.API.v3.MPAA;
using DustyPig.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.Explore
{
    public class ExploreViewModel : _BaseViewModel
    {
        private bool _listFullyLoaded = false;
        
        private readonly ExploreRequest _currentRequest = new ExploreRequest();
        private readonly StackLayout _slDimmer;

        public ExploreViewModel(StackLayout slDimmer, INavigation navigation) : base(slDimmer, navigation)
        {
            _slDimmer = slDimmer;
            RefreshCommand = new AsyncCommand(LoadInitial, allowsMultipleExecutions: false);
            LoadMoreCommand = new AsyncCommand(LoadMore, canExecute: () => !_listFullyLoaded, allowsMultipleExecutions: false);
            FilterCommand = new AsyncCommand(OnFilter, allowsMultipleExecutions: false);
            LoadInitial();
        }

       

        public AsyncCommand RefreshCommand { get; }

        public AsyncCommand LoadMoreCommand { get; }

        public AsyncCommand FilterCommand { get; }
        private async Task OnFilter()
        {
            _slDimmer?.DimSL(true);
            var page = new Filter.FilterPage(_currentRequest);
            await Navigation.PushModalAsync(page);
            var ret = await page.GetResultAsync();
            _slDimmer.BrightenSL(true);

            //Force compliance
            if (!(ret.ReturnMovies || ret.ReturnSeries))
                ret.ReturnMovies = true;

            //Check if filters changed
            bool changed = false;
            if (ret.FilterOnGenres != _currentRequest.FilterOnGenres)
            {
                _currentRequest.FilterOnGenres = ret.FilterOnGenres;
                _currentRequest.IncludeUnknownGenres = false;
                changed = true;
            }

            if (ret.ReturnSeries != _currentRequest.ReturnSeries)
            {
                _currentRequest.ReturnSeries = ret.ReturnSeries;
                changed = true;
            }

            if (ret.ReturnMovies != _currentRequest.ReturnMovies)
            {
                _currentRequest.ReturnMovies = ret.ReturnMovies;
                changed = true;
            }

            if (ret.SortBy != _currentRequest.SortBy)
            {
                _currentRequest.SortBy = ret.SortBy;
                changed = true;
            }

            if (changed)
                IsBusy = true;
        }

        private string _emptyMessage = "Loading";
        public string EmptyMessage
        {
            get => _emptyMessage;
            set => SetProperty(ref _emptyMessage, value);
        }

        private ObservableBasicMediaCollection _items = new ObservableBasicMediaCollection();
        public ObservableBasicMediaCollection Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private int _span = 1;
        public int Span
        {
            get => _span;
            set => SetProperty(ref _span, Math.Max(value, 1));
        }

        public void OnAppearing()
        {
            if (Items.Count == 0)
                IsBusy = true;
        }

        private bool _isBusy2;
        public bool IsBusy2
        {
            get => _isBusy2;
            set => SetProperty(ref _isBusy2, value);
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

        private Task LoadInitial() => LoadItems(true);

        private Task LoadMore() => LoadItems(false);

        private async Task LoadItems(bool initial)
        {
            IsBusy2 = !initial;
            EmptyMessage = "Loading";

            _currentRequest.Start = initial ? 0 : Items.Count;

            REST.Response<List<BasicMedia>> response = await App.API.Media.LoadExploreResultsAsync(_currentRequest);

            if (response.Success)
            {

                _listFullyLoaded = response.Data.Count == 0;

                if (initial)
                    Items.ReplaceRange(response.Data);
                else
                    Items.AddNewItems(response.Data);

                if (Items.Count == 0)
                    EmptyMessage = "No media found";
            }
            else
            {
                if (await response.Error.HandleUnauthorizedException())
                    return;
                await ShowAlertAsync("Error", response.Error.FormatMessage());
            }

            IsBusy = false;
            IsBusy2 = false;
        }

    }
}
