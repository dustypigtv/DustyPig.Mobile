
using DustyPig.API.v3.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Movie
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MovieDetailsPage : ContentPage
    {
        public MovieDetailsPage(BasicMedia basicMedia)
        {
            InitializeComponent();
            BindingContext = VM = new MovieDetailsViewModel(basicMedia);
        }

        public MovieDetailsViewModel VM { get; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            VM.OnAppearing();
        }
    }
}