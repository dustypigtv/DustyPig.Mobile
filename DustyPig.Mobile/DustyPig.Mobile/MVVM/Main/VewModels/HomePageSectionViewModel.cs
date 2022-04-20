using DustyPig.API.v3.Models;
using DustyPig.Mobile.Helpers;
using System.Collections.Generic;

namespace DustyPig.Mobile.MVVM.Main.VewModels
{
    public class HomePageSectionViewModel : _BaseViewModel
    {
        public HomePageSectionViewModel() { }

        public HomePageSectionViewModel(IEnumerable<BasicMedia> lst) => Items.AddRange(lst);

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
