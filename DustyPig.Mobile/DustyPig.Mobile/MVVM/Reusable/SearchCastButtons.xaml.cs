using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.MVVM.Search;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Reusable
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchCastButtons : ContentView
    {
        public const double RIGHT_EDGE_OFFSET = -52;

        public SearchCastButtons()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void ImageButton_Pressed(object sender, System.EventArgs e)
        {
            var ib = sender as ImageButton;
            ib.BackgroundColor = Theme.LightGrey;
        }

        private void ImageButton_Released(object sender, System.EventArgs e)
        {
            var ib = sender as ImageButton;
            ib.BackgroundColor = Color.Transparent;

        }


        //Because want to hide the search button on the search page, but keep other buttons visible
        public static readonly BindableProperty SearchButtonVisibleProperty = BindableProperty.Create(
            nameof(SearchButtonVisible),
            typeof(bool),
            typeof(SearchCastButtons),
            true);

        public bool SearchButtonVisible
        {
            get => (bool)GetValue(SearchButtonVisibleProperty);
            set => SetValue(SearchButtonVisibleProperty, value);
        }


        public AsyncCommand SearchButtonTapped { get; } = new AsyncCommand(() => Shell.Current.GoToAsync(nameof(SearchPage)));
    }
}