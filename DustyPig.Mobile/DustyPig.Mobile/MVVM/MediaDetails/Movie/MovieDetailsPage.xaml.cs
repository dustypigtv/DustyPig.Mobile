using DustyPig.API.v3.Models;
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
        private bool _firstAppeared = false;

        public MovieDetailsPage(BasicMedia basicMedia, bool firstAppeared = false)
        {
            _firstAppeared = firstAppeared;

            InitializeComponent();

            SCButtons.CloseTapped += (sender, e) => BackgroundColor = Color.Transparent;

            On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            BindingContext = VM = new MovieDetailsViewModel(basicMedia, Navigation);
        }

        public MovieDetailsViewModel VM { get; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_firstAppeared)
                return;

            _firstAppeared = true;

            if (Device.Idiom != TargetIdiom.Phone)
            {
                //Default modal animation is 250 secs
                await Task.Delay(250);
                double alpha = 0;
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(50);
                    alpha += 0.1;
                    BackgroundColor = Color.FromRgba(0, 0, 0, alpha);
                }
            }
        }
    }
}