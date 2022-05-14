using DustyPig.API.v3.Models;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.CommunityToolkit.ObjectModel;

namespace DustyPig.Mobile.Helpers
{
    public class ObservableBasicMediaCollection : ObservableRangeCollection<BasicMedia>
    {
        public ObservableBasicMediaCollection() { }

        public ObservableBasicMediaCollection(IEnumerable<BasicMedia> lst) => AddRange(lst);
        
        public void AddNewItems(List<BasicMedia> newLst)
        {
            var existingIds = this.Select(item => item.Id);
            newLst.RemoveAll(item => existingIds.Contains(item.Id));
            AddRange(newLst);
        }
    }
}