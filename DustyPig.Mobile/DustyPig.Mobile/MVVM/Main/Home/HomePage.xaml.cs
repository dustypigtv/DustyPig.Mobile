using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Home
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage, IPageShown
    {
        public HomePage()
        {
            InitializeComponent();
            BindingContext = VM = new HomeViewModel(MainStack, EmptyLabel, SLDimmer, Navigation);
        }


        public HomeViewModel VM { get; }

        public void PageShown(bool reselected)
        {
            if (reselected)
                MainSV.ScrollToAsync(0, 0, true);

            foreach (var child in MainStack.Children)
                if (child is HomePageSectionView hpsv)
                    hpsv.ResetScrollPosition();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            VM.OnAppearing();
        }
    }
}