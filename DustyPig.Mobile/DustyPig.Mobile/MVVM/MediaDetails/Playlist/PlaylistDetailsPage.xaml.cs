using DustyPig.API.v3.Models;
using DustyPig.Mobile.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Playlist
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlaylistDetailsPage : ContentPage
    {
        public PlaylistDetailsPage(BasicMedia basicMedia, StackLayout slDimmer)
        {
            InitializeComponent();

            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            BindingContext = VM = new PlaylistDetailsViewModel(basicMedia, Navigation);

            SCButtons.CloseTapped += (sender, e) => slDimmer?.BrightenSL();
            slDimmer.DimSL();
        }

        public PlaylistDetailsViewModel VM { get; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        protected override bool OnBackButtonPressed()
        {
            SCButtons.CloseButtonTapped.ExecuteAsync();
            return true;
        }
    }
}