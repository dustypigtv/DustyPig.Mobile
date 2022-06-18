using DustyPig.API.v3.Models;
using DustyPig.Mobile.Helpers;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.Home
{
    public class HomePageSectionViewModel : _BaseViewModel
    {
        private bool _listFullyLoaded = false;

        public HomePageSectionViewModel(HomeScreenList hsl, INavigation navigation) : base(navigation)
        {
            ListId = hsl.ListId;
            Title = hsl.Title;
            _listFullyLoaded = hsl.Items.Count == 0;
            LoadMoreItemsCommand = new AsyncCommand(OnLoadMoreItems, canExecute: (obj) => !_listFullyLoaded, allowsMultipleExecutions: false);
            Items.AddRange(hsl.Items);
        }


        public AsyncCommand LoadMoreItemsCommand { get; }
        private async Task OnLoadMoreItems()
        {
            if (_listFullyLoaded)
                return;

            var response = await App.API.Media.LoadMoreHomeScreenItemsAsync(ListId, Items.Count);
            if (response.Success)
            {
                _listFullyLoaded = response.Data.Count == 0;
                Items.AddNewItems(response.Data);
            }
            else
            {
                _listFullyLoaded = true;
                await ShowAlertAsync("Error loading media", response.Error.FormatMessage());
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