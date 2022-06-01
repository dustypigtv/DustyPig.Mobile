using DustyPig.Mobile.CrossPlatform;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main.Search
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage
    {
        public event EventHandler PageShown;

        public SearchPage()
        {
            InitializeComponent();
            PageShown += SearchPage_PageShown;
            BindingContext = VM = new SearchViewModel(AvailableCV, OtherCV, Navigation);
        }

        public SearchViewModel VM { get; }

        private void SearchPage_PageShown(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TheSearchBar.Text))
                Device.BeginInvokeOnMainThread(async () =>
                {
                    while (!TheSearchBar.Focus())
                    {
                        await Task.Delay(100);
                    }
                });
        }

        public void InvokePageShown() => PageShown?.Invoke(null, EventArgs.Empty);


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

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e) => await VM.DoSearch(e.NewTextValue);


    }
}