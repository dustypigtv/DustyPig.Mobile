using System;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : Xamarin.Forms.TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetIsSwipePagingEnabled(false);
        }

        
        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();

            var cp = CurrentPage as NavigationPage;
            if (cp == null)
                return;

            if (cp.RootPage is Search.SearchPage sp)
                sp.InvokePageShown();
        }

    }
}