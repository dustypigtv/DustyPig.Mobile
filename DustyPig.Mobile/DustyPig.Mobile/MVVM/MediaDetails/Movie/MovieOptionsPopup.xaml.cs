using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Movie
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MovieOptionsPopup : Popup<MovieOptions>
    {
        public MovieOptionsPopup()
        {
            InitializeComponent();

            PlaylistCommand = new Command(() => Dismiss(MovieOptions.AddToPlaylist));
            ParentalControlsCommand = new Command(() => Dismiss(MovieOptions.ParentalControls));
            CancelCommand = new Command(() => Dismiss(MovieOptions.None));

            BindingContext = this;
        }

      
        public Command PlaylistCommand { get; }

        public Command ParentalControlsCommand { get; }

        public Command CancelCommand { get; }

    }
}