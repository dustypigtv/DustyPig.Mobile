using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.Home
{
    public class HomePageSectionViewModel : _BaseViewModel
    {
        private bool _listFullyLoaded = false;


        public HomePageSectionViewModel(List<BasicMedia> lst)
        {
            _listFullyLoaded = lst.Count < 100;
            LoadMoreItemsCommand = new AsyncCommand(OnLoadMoreItems, canExecute: (obj) => !_listFullyLoaded, allowsMultipleExecutions: false);
            Items.AddRange(lst);
        }

        public AsyncCommand LoadMoreItemsCommand { get; }
        private async Task OnLoadMoreItems()
        {
            if (_listFullyLoaded)
                return;

            var response = await App.API.Media.LoadMoreHomeScreenItemsAsync(ListId, Items.Count);
            if (response.Success)
            {
                _listFullyLoaded = response.Data.Count < 100;
                Items.AddNewItems(response.Data);
            }
            else
            {
                _listFullyLoaded = true;
                await DependencyService.Get<IPopup>().AlertAsync("Error loading media", response.Error.FormatMessage());
            }
        }


        private long _listId;
        public long ListId
        {
            get => _listId;
            set => SetProperty(ref _listId, value);
        }

        private ObservableBasicMediaCollection _items = new ObservableBasicMediaCollection();
        public ObservableBasicMediaCollection Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }
    }
}