using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Search
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage
    {
        public SearchPage()
        {
            InitializeComponent();
            BindingContext = VM = new SearchViewModel(AvailableCV, OtherCV);
        }

        public SearchViewModel VM { get; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            VM.OnSizeAllocated(width, height);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (string.IsNullOrWhiteSpace(TheSearchBar.Text))
                Device.BeginInvokeOnMainThread(async () =>
                {
                    while (!TheSearchBar.Focus())
                    {
                        await Task.Delay(100);
                    }
                });
        }

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e) => await VM.DoSearch(e.NewTextValue);


    }
}