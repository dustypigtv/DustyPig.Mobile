using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Explore
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExplorePage : ContentPage, IPageShown
    {
        public ExplorePage()
        {
            InitializeComponent();
            BindingContext = VM = new ExploreViewModel(SLDimmer, Navigation);
        }

        public ExploreViewModel VM { get; }

        public void PageShown(bool reselected)
        {
            if(reselected)
                MainCV.ScrollTo(0, -1, ScrollToPosition.Start, true);
        }

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