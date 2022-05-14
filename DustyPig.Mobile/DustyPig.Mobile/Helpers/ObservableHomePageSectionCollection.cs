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
            AddRange(items);
        }

        private INavigation Navigation { get; }

        private HomePageSectionViewModel ConvertToViewModel(HomeScreenList item) =>
            new HomePageSectionViewModel(item.Items, Navigation)
            {
                ListId = item.ListId,
                Title = item.Title
            };

        public void AddRange(List<HomeScreenList> lst)
        {
            AddRange(lst.Select(item => ConvertToViewModel(item)));
        }

        public void UpdateList(List<HomeScreenList> newLst)
        {
            if (Count == 0)
            {
                AddRange(newLst);
                return;
            }

            //Remove all from here that aren't in newLst
            var newIds = newLst.Select(item => item.ListId);
            var toRemove = this
                .Where(item => !newIds.Contains(item.ListId))
                .ToList();
            toRemove.ForEach(item => Remove(item));

            //Add/Move all new items
            for (int newIdx = 0; newIdx < newLst.Count; newIdx++)
            {
                var newItem = newLst[newIdx];

                var existingItem = this.FirstOrDefault(item => item.ListId == newItem.ListId);
                if (existingItem == null)
                {
                    Insert(newIdx, ConvertToViewModel(newItem));
                }
                else
                {
                    int oldIdx = IndexOf(existingItem);
                    if (oldIdx != newIdx)
                        Move(oldIdx, newIdx);
                    existingItem.Items.UpdateList(newItem.Items);
                }
            }

        }
    }
}