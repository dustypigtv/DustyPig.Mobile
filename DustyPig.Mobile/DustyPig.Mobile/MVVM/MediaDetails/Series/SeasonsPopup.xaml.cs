using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SeasonsPopup : PopupPage
    {
        private readonly int _seasonsCnt = 0;
        private readonly TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();

        public SeasonsPopup(List<ushort> seasons, ushort current)
        {
            InitializeComponent();

            _seasonsCnt = seasons.Count;
            seasons.Sort();

            foreach (var season in seasons)
                Seasons.Add(new SeasonInfo(season));

            SeasonTappedCommand = new AsyncCommand<int>(SetResult, allowsMultipleExecutions: false);

            //All seasons are >= 0, so use -1 for cancel
            CancelCommand = new AsyncCommand(() => SetResult(-1), allowsMultipleExecutions: false);

            BindingContext = this;


            Device.BeginInvokeOnMainThread(async () =>
            {
                while (true)
                {
                    try
                    {
                        TheCV.ScrollTo(current, position: ScrollToPosition.Start, animate: false);
                        return;
                    }
                    catch { }
                    await Task.Delay(100);
                }
            });
        }

        private double _panelWidth;
        public double PanelWidth
        {
            get => _panelWidth;
            set
            {
                if(!Equals(_panelWidth, value))
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
                if(!Equals(_panelHeight, value))
                {
                    _panelHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<SeasonInfo> Seasons { get; } = new ObservableCollection<SeasonInfo>();

        public AsyncCommand CancelCommand { get; }
      
        public AsyncCommand<int> SeasonTappedCommand { get; }


        private async Task SetResult(int result)
        {
            await PopupNavigation.Instance.PopAsync(true);
            _taskCompletionSource.SetResult(result);           
        }

        public Task<int> GetResultAsync() => _taskCompletionSource.Task;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            PanelHeight = Math.Min(Helpers.Screen.Height * 0.75, 100 + (_seasonsCnt * 42));
            PanelWidth = Math.Min(200, width);
        }
    }


}