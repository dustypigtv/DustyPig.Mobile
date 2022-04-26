using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.MVVM.Main.Views;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
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


            //Only do this in the home tab - since this class doesn't get destroyed
            InternetConnectivityChanged += (sender, e) =>
            {
                var tabBar = Shell.Current.CurrentItem as TabBar;
                for (int i = 0; i < 3; i++)
                    tabBar.Items[i].IsEnabled = e;

                if (!e && new string[] { "Home", "Movies", "TV" }.Contains(tabBar.CurrentItem.Title))
                    tabBar.CurrentItem = tabBar.Items[3];
            };
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
            bool firstAppearing = Sections.Count == 0;
            string cache_file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "home_screen_cache.json");

            if (firstAppearing)
            {
                try
                {
                    var cached = JsonConvert.DeserializeObject<REST.Response<HomeScreen>>(File.ReadAllText(cache_file));
                    Sections.UpdateList(cached.Data.Sections);
                    if (Sections.Count > 0)
                        IsBusy = false;
                }
                catch { }
            }

            var response = await App.API.Media.GetHomeScreenAsync();
            if (response.Success)
            {
                Sections.UpdateList(response.Data.Sections);
                App.HomePageNeedsRefresh = false;
                File.WriteAllText(cache_file, JsonConvert.SerializeObject(response));
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