using DustyPig.Mobile.CrossPlatform;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Search
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage, IPageShown
    {
        public SearchPage()
        {
            InitializeComponent();
            BindingContext = VM = new SearchViewModel(AvailableCV, OtherCV, Navigation);
        }

        public SearchViewModel VM { get; }
        
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

       
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
        }

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e) => await VM.DoSearch(e.NewTextValue, true);

        private async void TheSearchBar_SearchButtonPressed(object sender, EventArgs e) => await VM.DoSearch(TheSearchBar.Text, false);

        public void PageShown(bool reselected)
        {
            if (reselected)
            {
                AvailableCV.ScrollTo(0, -1, ScrollToPosition.Start, true);
                OtherCV.ScrollTo(0, -1, ScrollToPosition.Start, true);
            }

            if (reselected || string.IsNullOrWhiteSpace(TheSearchBar.Text))
                Device.BeginInvokeOnMainThread(async () =>
                {
                    while (!TheSearchBar.Focus())
                    {
                        await Task.Delay(100);
                    }
                });
        }

        
    }
}