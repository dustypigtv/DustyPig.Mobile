using DustyPig.API.v3.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Home
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePageSectionView : ContentView
    {
        public HomePageSectionView(HomeScreenList hsl)
        {
            InitializeComponent();
            BindingContext = VM = new HomePageSectionViewModel(hsl, Navigation);
        }

        public HomePageSectionViewModel VM { get; set; }

        public void ResetScrollPosition()
        {
            MyCV.ScrollTo(0, -1, ScrollToPosition.Start, true);
        }
    }
}