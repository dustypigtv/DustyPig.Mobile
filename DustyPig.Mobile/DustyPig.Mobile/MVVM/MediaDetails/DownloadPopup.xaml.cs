using DustyPig.API.v3.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
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
    public partial class DownloadPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        readonly List<View> _labels = new List<View>();
        readonly TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();

        int _selectedIndex = -1;
        
        public DownloadPopup(MediaTypes mediaType, int currentCount)
        {
            InitializeComponent();

            CloseWhenBackgroundIsClicked = false;

            switch (mediaType)
            {
                case MediaTypes.Series:
                    Header = "Episodes to Download:";
                    break;

                case MediaTypes.Playlist:
                    Header = "Items to Download:";
                    break;
            }

            ShowDelete = currentCount > 0;
            LabelTouchedCommand = new Command<string>(OnLabelTouched);
            CancelCommand = new AsyncCommand(OnCancel, allowsMultipleExecutions: false);
            SaveCommand = new AsyncCommand(OnSave, allowsMultipleExecutions: false);

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

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            PanelWidth = Math.Min(340, width);
            PanelHeight = Math.Min(552, height);
        }


        private double _panelWidth;
        public double PanelWidth
        {
            get => _panelWidth;
            set
            {
                if(_panelWidth != value)
                {
                    _panelWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _panelHeight;
        public double PanelHeight
        {
            get => _panelHeight;
            set
            {
                if(_panelHeight != value)
                {
                    _panelHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        public Task<int> GetResult() => _taskCompletionSource.Task;

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

        public AsyncCommand CancelCommand { get; }
        private Task OnCancel()
        {
            _selectedIndex = -1;
            return OnSave();
        }

        public AsyncCommand SaveCommand { get; }
        private async Task OnSave()
        {
            if (_selectedIndex > 0)
                Services.Settings.LastDownloadCount = _selectedIndex;
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);
            _taskCompletionSource.SetResult(_selectedIndex);
        }
    }
}