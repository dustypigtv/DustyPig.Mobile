using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DustyPig.Mobile.Helpers
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            _suppressNotification = true;
            bool hadItems = Count > 0;
            bool added = false;
            foreach (T item in list)
            {
                Add(item);
                added = true;
            }

            _suppressNotification = false;

            if (added)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                if (!hadItems)
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("HasItems"));
            }
        }

        public bool HasItems => Count > 0;
    }
}
