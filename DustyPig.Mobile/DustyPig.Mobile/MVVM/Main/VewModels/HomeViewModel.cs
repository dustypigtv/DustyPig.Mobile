using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Helpers;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.VewModels
{
    public class HomeViewModel : _BaseViewModel
    {
        public HomeViewModel()
        {
            RefreshCommand = new AsyncCommand(Update);
            ItemTappedCommand = new AsyncCommand<BasicMedia>(OnItemTapped);
        }

        public AsyncCommand RefreshCommand { get; }

        public AsyncCommand<BasicMedia> ItemTappedCommand { get; }
        private async Task OnItemTapped(BasicMedia item)
        {
            await DependencyService.Get<IPopup>().AlertAsync("Tapped", item.Title);
        }

        private ObservableHomePageSectionCollection _sections = new ObservableHomePageSectionCollection();
        public ObservableHomePageSectionCollection Sections
        {
            get => _sections;
            set => SetProperty(ref _sections, value);
        }



        public void OnAppearing()
        {
            if (App.HomePageNeedsRefresh || Sections.Count == 0)
                IsBusy = true;
        }


        private async Task Update()
        {
            var response = await App.API.Media.GetHomeScreenAsync();
            if (response.Success)
            {
                Sections.UpdateList(response.Data.Sections);
                App.HomePageNeedsRefresh = false;
            }
            else
            {
                await DependencyService.Get<IPopup>().AlertAsync("Error", response.Error.FormatMessage());
            }

            App.HomePageNeedsRefresh = false;
            IsBusy = false;
        }
    }
}