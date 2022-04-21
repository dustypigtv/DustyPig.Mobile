using DustyPig.API.v3.Models;
using System.Collections.Generic;
using System.Linq;

namespace DustyPig.Mobile.Helpers
{
    public class ObservableBasicMediaCollection : RangeObservableCollection<BasicMedia>
    {
        public ObservableBasicMediaCollection() { }

        public ObservableBasicMediaCollection(IEnumerable<BasicMedia> lst) => AddRange(lst);

        public void UpdateList(List<BasicMedia> newLst)
        {
            if (Count == 0)
            {
                AddRange(newLst);
                return;
            }

            //Remove all from here that aren't in newLst
            var newIds = newLst.Select(item => item.Id);
            var toRemove = this
                .Where(item => !newIds.Contains(item.Id))
                .ToList();
            toRemove.ForEach(item => Remove(item));

            //Add/Move all new items
            for (int newIdx = 0; newIdx < newLst.Count; newIdx++)
            {
                var newItem = newLst[newIdx];

                var existingItem = this.FirstOrDefault(item => item.Id == newItem.Id);
                if (existingItem == null)
                {
                    Insert(newIdx, newItem);
                }
                else
                {
                    int oldIdx = IndexOf(existingItem);
                    if (oldIdx != newIdx)
                        Move(oldIdx, newIdx);
                }
            }

        }
    
        public void AddNewItems(List<BasicMedia> newLst)
        {
            var existingIds = this.Select(item => item.Id);
            newLst.RemoveAll(item => existingIds.Contains(item.Id));
            AddRange(newLst);
        }
    }
}