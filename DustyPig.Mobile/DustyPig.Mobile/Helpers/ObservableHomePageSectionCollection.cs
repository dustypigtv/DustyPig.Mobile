using DustyPig.API.v3.Models;
using DustyPig.Mobile.MVVM.Main.Home;
using System.Collections.Generic;
using System.Linq;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.Helpers
{
    public class ObservableHomePageSectionCollection : ObservableRangeCollection<HomePageSectionViewModel>
    {
        public ObservableHomePageSectionCollection(INavigation navigation)
        {
            Navigation = navigation;
        }

        public ObservableHomePageSectionCollection(List<HomeScreenList> items, INavigation navigation)
        {
            Navigation = navigation;
            ReplaceRange(items);
        }

        private INavigation Navigation { get; }

        private HomePageSectionViewModel ConvertToViewModel(HomeScreenList item) =>
            new HomePageSectionViewModel(item.Items, Navigation)
            {
                ListId = item.ListId,
                Title = item.Title
            };

        public void ReplaceRange(List<HomeScreenList> newLst)
        {
            Clear();
            AddRange(newLst.Select(item => ConvertToViewModel(item)));
        }
    }
}