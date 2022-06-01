using System;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Reusable
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CastButtons : ContentView
    {
        public const double RIGHT_EDGE_OFFSET = -52;

        public CastButtons()
        {
            InitializeComponent();

            /*
                Important!!!
                
                Application.Current.MainPage.Navigation is somehow different from Navigation.
                Calling it from the root is how to hide the tab bar while the search page is shown
             */
            
            CloseButtonTapped = new AsyncCommand(async ()=>
            {
                CloseTapped?.Invoke(this, EventArgs.Empty);
                await Navigation.PopModalAsync();
            }, allowsMultipleExecutions: false);


            //Make sure this comes after any properties that are not INotifyProperty
            BindingContext = this;
        }

        public event EventHandler CloseTapped;


        public static readonly BindableProperty CloseButtonVisibleProperty = BindableProperty.Create(
            nameof(CloseButtonVisible),
            typeof(bool),
            typeof(CastButtons),
            false);

        public bool CloseButtonVisible
        {
            get => (bool)GetValue(CloseButtonVisibleProperty);
            set => SetValue(CloseButtonVisibleProperty, value);
        }

        public AsyncCommand CloseButtonTapped { get; }
    }
}