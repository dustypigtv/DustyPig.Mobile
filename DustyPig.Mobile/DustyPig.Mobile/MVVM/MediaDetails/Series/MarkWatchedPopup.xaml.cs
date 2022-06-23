using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MarkWatchedPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        private readonly TaskCompletionSource<MarkWatchedPopupResponse> _taskCompletionSource = new TaskCompletionSource<MarkWatchedPopupResponse>();

        public MarkWatchedPopup()
        {
            InitializeComponent();

            CloseWhenBackgroundIsClicked = false;

            MarkWatchedCommnd = new AsyncCommand(OnMarkWatched, allowsMultipleExecutions: false);
            StopWatchingCommand = new AsyncCommand(OnStopWatching, allowsMultipleExecutions: false);
            CancelCommand = new AsyncCommand(OnCancel, allowsMultipleExecutions: false);

            BindingContext = this;
        }

        public AsyncCommand MarkWatchedCommnd { get; }
        private async Task OnMarkWatched()
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);
            _taskCompletionSource.SetResult(MarkWatchedPopupResponse.MarkSeriesWatched);
        }

        public AsyncCommand StopWatchingCommand { get; }
        private async Task OnStopWatching()
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);
            _taskCompletionSource.SetResult(MarkWatchedPopupResponse.StopWatching);
        }

        public AsyncCommand CancelCommand { get; }
        private async Task OnCancel()
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);
            _taskCompletionSource.SetResult(MarkWatchedPopupResponse.NoAction);
        }

        protected override bool OnBackButtonPressed()
        {
            _taskCompletionSource.SetResult(MarkWatchedPopupResponse.NoAction);
            return false;
        }


        public Task<MarkWatchedPopupResponse> GetResultAsync() => _taskCompletionSource.Task;
    }
}