using DustyPig.Mobile.MVVM;
using Xamarin.Forms;

namespace DustyPig.Mobile
{
    public partial class App : Application
    {
        internal static readonly API.v3.Client API = new API.v3.Client();

        internal static bool HomePageNeedsRefresh { get; set; }


        public App()
        {
            InitializeComponent();
            //MainPage = new AppShell();
            MainPage = new StartupPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
