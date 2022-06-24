using DustyPig.API.v3.Models;
using DustyPig.Mobile.Helpers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Movie
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MovieDetailsPage : ContentPage
    {
        public MovieDetailsPage(BasicMedia basicMedia, StackLayout slDimmer)
        {
            InitializeComponent();

            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            BindingContext = VM = new MovieDetailsViewModel(basicMedia, Navigation);

            SCButtons.CloseTapped += (sender, e) => slDimmer?.BrightenSL();
            slDimmer.DimSL();
        }

        public MovieDetailsViewModel VM { get; }

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