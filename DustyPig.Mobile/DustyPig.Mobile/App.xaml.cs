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



        public static IPlatformDep PlatformDep { get; set; }




        public static void SetOrientation(bool video)
        {
            switch (Device.Idiom)
            {
                case TargetIdiom.Desktop:
                    PlatformDep.AllowAnyOrientation();
                    break;

                case TargetIdiom.Phone:
                    if (video)
                        PlatformDep.ForceLandscape();
                    else
                        PlatformDep.ForcePortrait();
                    break;

                case TargetIdiom.Tablet:
                    if (video)
                        PlatformDep.ForceLandscape();
                    else
                        PlatformDep.AllowAnyOrientation();
                    break;

                case TargetIdiom.TV:
                    PlatformDep.ForceLandscape();
                    break;

                default:
                    if (video)
                        PlatformDep.ForceLandscape();
                    else
                        PlatformDep.ForcePortrait();
                    break;
            }
        }
    }
}
