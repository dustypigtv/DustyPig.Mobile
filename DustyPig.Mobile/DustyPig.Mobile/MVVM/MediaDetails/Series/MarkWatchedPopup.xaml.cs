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
            _taskCompletionSource.SetResult(MarkWatchedPopupResponse.MarkSeriesWatched);
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);
        }

        public AsyncCommand StopWatchingCommand { get; }
        private async Task OnStopWatching()
        {
            _taskCompletionSource.SetResult(MarkWatchedPopupResponse.StopWatching);
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);
        }

        public AsyncCommand CancelCommand { get; }
        private async Task OnCancel()
        {
            _taskCompletionSource.SetResult(MarkWatchedPopupResponse.NoAction);
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);
        }


        public Task<MarkWatchedPopupResponse> GetResult() => _taskCompletionSource.Task;
    }
}