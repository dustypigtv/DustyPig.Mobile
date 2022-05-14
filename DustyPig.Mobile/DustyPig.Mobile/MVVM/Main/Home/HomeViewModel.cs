using DustyPig.Mobile.Helpers;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.Home
{
    public class HomeViewModel : _BaseViewModel
    {
        public HomeViewModel(INavigation navigation) : base(navigation)
        {
            RefreshCommand = new AsyncCommand(Update);
            Sections = new ObservableHomePageSectionCollection(Navigation);

            ////Only do this in the home tab - since this class doesn't get destroyed
            //InternetConnectivityChanged += (sender, e) =>
            //{
            //    var tabBar = Shell.Current.CurrentItem as TabBar;
            //    for (int i = 0; i < 3; i++)
            //        tabBar.Items[i].IsEnabled = e;

            //    if (!e && new string[] { "Home", "Movies", "TV" }.Contains(tabBar.CurrentItem.Title))
            //        tabBar.CurrentItem = tabBar.Items[3];
            //};
        }

        public AsyncCommand RefreshCommand { get; }

        private ObservableHomePageSectionCollection _sections;
        public ObservableHomePageSectionCollection Sections
        {
            get => _sections;
            set => SetProperty(ref _sections, value);
        }

        private string _emptyView = "Loading";
        public string EmptyView
        {
            get => _emptyView;
            set => SetProperty(ref _emptyView, value);
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
                Sections.ReplaceRange(response.Data.Sections);
                App.HomePageNeedsRefresh = false;
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.FormatMessage());
            }

            EmptyView = "No media found";
            App.HomePageNeedsRefresh = false;
            IsBusy = false;
        }
    }
}