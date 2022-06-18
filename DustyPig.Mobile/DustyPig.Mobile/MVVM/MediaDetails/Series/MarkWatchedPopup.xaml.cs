using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MarkWatchedPopup : Popup<MarkWatchedPopupResponse>
    {
        public MarkWatchedPopup()
        {
            InitializeComponent();
            
            MarkWatchedCommnd = new Command(() => Dismiss(MarkWatchedPopupResponse.MarkSeriesWatched));
            StopWatchingCommand = new Command(() => Dismiss(MarkWatchedPopupResponse.StopWatching));
            CancelCommand = new Command(() => Dismiss(MarkWatchedPopupResponse.NoAction));

            BindingContext = this;
        }

        public Command MarkWatchedCommnd { get; }

        public Command StopWatchingCommand { get; }

        public Command CancelCommand { get; }
    }
}