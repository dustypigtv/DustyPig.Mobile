using DustyPig.Mobile.Views;
using Xamarin.Forms;

namespace DustyPig.Mobile
{
    public partial class App : Application
    {
        internal static readonly API.v3.Client API = new API.v3.Client();

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
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
