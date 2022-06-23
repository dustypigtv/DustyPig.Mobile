using DustyPig.API.v3.Models;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.TMDB
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TMDBDetailsPage : ContentPage
    {
        public TMDBDetailsPage(BasicTMDB basicTMDB)
        {
            InitializeComponent();
            SCButtons.CloseTapped += (sender, e) => BackgroundColor = Color.Transparent;

            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            BindingContext = VM = new TMDBDetailsViewModel(basicTMDB, Navigation);
        }

        public TMDBDetailsViewModel VM { get; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }
    }
}