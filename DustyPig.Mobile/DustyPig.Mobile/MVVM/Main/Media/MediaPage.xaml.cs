using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.MVVM.Main.Media;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Media
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MediaPage : ContentPage
    {
        public MediaPage(int idx)
        {
            InitializeComponent();

            var mode = idx == 0 ? MediaViewModel.Mode.Movies : MediaViewModel.Mode.TV;
            BindingContext = VM = new MediaViewModel(mode);
        }

        public MediaViewModel VM { get; }
       
        protected override void OnAppearing()
        {
            base.OnAppearing();
            VM.OnAppearing();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }
    }
}