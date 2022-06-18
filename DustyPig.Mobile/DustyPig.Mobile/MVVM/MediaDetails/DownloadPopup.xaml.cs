using DustyPig.API.v3.Models;
using System.Collections.Generic;
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
        readonly List<StackLayout> _labels = new List<StackLayout>();
        int _selectedIndex = -1;

        public DownloadPopup(MediaTypes mediaType, int currentCount)
        {
            InitializeComponent();


            switch (mediaType)
            {
                case MediaTypes.Series:
                    Header = "Number of episodes to download:";
                    break;

                case MediaTypes.Playlist:
                    Header = "Number of items to download:";
                    break;
            }

            ShowDelete = currentCount > 0;
            LabelTouchedCommand = new Command<string>(OnLabelTouched);
            CancelCommand = new Command(() => Dismiss(-1));
            SaveCommand = new Command(OnSave);

            _labels.Add(lbl0);
            _labels.Add(lbl1);
            _labels.Add(lbl2);
            _labels.Add(lbl3);
            _labels.Add(lbl4);
            _labels.Add(lbl5);
            _labels.Add(lbl6);
            _labels.Add(lbl7);
            _labels.Add(lbl8);
            _labels.Add(lbl9);
            _labels.Add(lbl10);

            if (currentCount < 1 || currentCount > 10)
                currentCount = Services.Settings.LastDownloadCount;
            OnLabelTouched(currentCount.ToString());

            BindingContext = this;
        }

        public string Header { get; set; }

        public bool ShowDelete { get; set; }


        public Command<string> LabelTouchedCommand { get; }
        private void OnLabelTouched(string s)
        {
            _selectedIndex = int.Parse(s);
            for (int i = 0; i < _labels.Count; i++)
                if (i == _selectedIndex)
                    _labels[i].BackgroundColor = Helpers.Theme.Grey;
                else
                    _labels[i].BackgroundColor = Color.Transparent;
        }

        public Command DeleteCommand { get; }

        public Command CancelCommand { get; }

        public Command SaveCommand { get; }
        private void OnSave()
        {
            if (_selectedIndex > 0)
                Services.Settings.LastDownloadCount = _selectedIndex;
            Dismiss(_selectedIndex);
        }
    }
}