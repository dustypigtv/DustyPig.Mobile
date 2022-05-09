using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.VewModels
{
    public class MediaViewModel : _BaseViewModel
    {
        public enum Mode
        {
            Movies,
            TV
        }

        private readonly Mode _mode;

        private bool _listFullyLoaded = false;

        public MediaViewModel(Mode mode)
        {
            var tabBar = Shell.Current.CurrentItem as TabBar;

            _mode = mode;
            RefreshCommand = new AsyncCommand(LoadInitial, allowsMultipleExecutions: false);
            LoadMoreCommand= new AsyncCommand(LoadMore, canExecute: () => !_listFullyLoaded, allowsMultipleExecutions: false);
            ItemTappedCommand = new AsyncCommand<BasicMedia>(OnItemTapped, allowsMultipleExecutions: false);
        }

        public AsyncCommand RefreshCommand { get; }

        public AsyncCommand LoadMoreCommand { get; }

        public AsyncCommand<BasicMedia> ItemTappedCommand { get; }
        private async Task OnItemTapped(BasicMedia item)
        {
            await DependencyService.Get<IPopup>().AlertAsync("Tapped", item.Title);
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

            REST.Response<List<BasicMedia>> response;

            //TO DO: Add Sorting
            if (_mode == Mode.Movies)
                response = await App.API.Movies.ListAsync(start);
            else
                response = await App.API.Series.ListAsync(start);

            if (response.Success)
            {
                
                _listFullyLoaded = response.Data.Count < 100;

                if (initial)
                    Items.UpdateList(response.Data);
                else
                    Items.AddNewItems(response.Data);
            }
            else
            {
                await DependencyService.Get<IPopup>().AlertAsync("Error", response.Error.FormatMessage());
            }

            IsBusy = false;
        }

    }
}
