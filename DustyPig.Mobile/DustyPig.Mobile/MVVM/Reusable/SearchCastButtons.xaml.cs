using DustyPig.Mobile.MVVM.Search;
using System;
using System.Threading.Tasks;
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

            /*
                Important!!!
                
                Application.Current.MainPage.Navigation is somehow different from Navigation.
                Calling it from the root is how to hide the tab bar while the search page is shown
             */
            SearchButtonTapped = new AsyncCommand(() => Application.Current.MainPage.Navigation.PushAsync(new SearchPage()));

            CloseButtonTapped = new AsyncCommand(async ()=>
            {
                CloseTapped?.Invoke(this, EventArgs.Empty);
                await Navigation.PopModalAsync();
            }, allowsMultipleExecutions: false);


            //Make sure this comes after any properties that are not INotifyProperty
            BindingContext = this;
        }

        public event EventHandler CloseTapped;


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

        public static readonly BindableProperty CloseButtonVisibleProperty = BindableProperty.Create(
            nameof(CloseButtonVisible),
            typeof(bool),
            typeof(SearchCastButtons),
            false);

        public bool CloseButtonVisible
        {
            get => (bool)GetValue(CloseButtonVisibleProperty);
            set => SetValue(CloseButtonVisibleProperty, value);
        }

        public AsyncCommand SearchButtonTapped { get; }

        public AsyncCommand CloseButtonTapped { get; }
    }
}