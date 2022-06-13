using DustyPig.API.v3.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    /// <summary>
    /// LOGIC:
    ///     Return -1 when cancelled
    ///     Return 0 to delete a download job
    ///     Otherwise return 1-10 number of items to download
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadPopup : Popup<int>
    {
        public DownloadPopup(MediaTypes mediaType, int currentCount)
        {
            InitializeComponent();


            ShowDelete = currentCount > 0;

            _selectedItem = ((currentCount < 1 || currentCount > 10) ? 5 : currentCount).ToString();


            switch (mediaType)
            {
                case MediaTypes.Series:
                    Header = "Number of episodes to download:";
                    break;

                case MediaTypes.Playlist:
                    Header = "Number of items to download:";
                    break;
            }

            Items = new ObservableCollection<string>();
            Items.Add("None (Delete download)");
            for (int i = 1; i <= 10; i++)
                Items.Add(i.ToString());

            CancelCommand = new Command(() => Dismiss(-1));
            SaveCommand = new Command(OnSave);

            BindingContext = this;
        }

        public string Header { get; set; }

        public bool ShowDelete { get; set; }

        private string _selectedItem;
        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (Equals(_selectedItem, value))
                    return;
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ObservableCollection<string> Items { get; }

        public Command DeleteCommand { get; }        

        public Command CancelCommand { get; }       

        public Command SaveCommand { get; }
        private void OnSave()
        {
            if (int.TryParse(SelectedItem, out int cnt))
                Dismiss(cnt);
            else
                //Delete download
                Dismiss(0);
        }
    }
}