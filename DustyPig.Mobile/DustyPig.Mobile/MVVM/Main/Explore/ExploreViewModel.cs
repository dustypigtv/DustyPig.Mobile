using DustyPig.API.v3.Models;
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

        public ExploreViewModel(INavigation navigation) : base(navigation)
        {
            RefreshCommand = new AsyncCommand(LoadInitial, allowsMultipleExecutions: false);
            LoadMoreCommand = new AsyncCommand(LoadMore, canExecute: () => !_listFullyLoaded, allowsMultipleExecutions: false);
        }

        public AsyncCommand RefreshCommand { get; }

        public AsyncCommand LoadMoreCommand { get; }

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
            int start = initial ? 0 : Items.Count;

            REST.Response<List<BasicMedia>> response = await App.API.Media.LoadExploreResultsAsync(new ExploreRequest
            {
                Start = start
            });

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
                await ShowAlertAsync("Error", response.Error.FormatMessage());
            }

            IsBusy = false;
        }

    }
}
