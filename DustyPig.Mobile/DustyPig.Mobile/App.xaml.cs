using Xamarin.Forms;

namespace DustyPig.Mobile
{
    public partial class App : Application
    {
        private static readonly API.v3.Client _client = new API.v3.Client();

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





        public static API.v3.Client API => _client;



        
    }
}
