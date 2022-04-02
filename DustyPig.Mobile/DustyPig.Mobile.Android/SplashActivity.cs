using Android.App;
using Android.Content;
using Android.OS;

namespace DustyPig.Mobile.Droid
{
    [Activity(Label = "DustyPig Pig", Icon = "@mipmap/icon", Theme = "@style/SplashTheme", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        protected override void OnResume()
        {
            base.OnResume();
            //StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            //Finish();
        }

        public override void OnBackPressed() { }
    }
}