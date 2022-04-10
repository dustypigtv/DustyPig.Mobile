using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using DustyPig.Mobile.SocialLogin.FB;
using FFImageLoading.Forms.Platform;
using Java.Security;
using System;

namespace DustyPig.Mobile.Droid
{
    [Activity(Label = "Dusty Pig", LaunchMode = LaunchMode.SingleTop, Icon = "@mipmap/icon", Theme = "@style/SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {

#if DEBUG
            PrintHashKey();
#endif

            base.OnCreate(savedInstanceState);

            FacebookClient.Current = new FacebookClientManager(this);
            Xamarin.Facebook.AppEvents.AppEventsLogger.ActivateApp(Application, Resources.GetString(Resource.String.facebook_app_id));

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);


            //Make sure to set PlatformDep before creating a new App()
            App.PlatformDep = new PlatformDep(this);

            //FFImageLoading
            CachedImageRenderer.Init(true);


            SetTheme(Resource.Style.MainTheme);

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);
            FacebookClientManager.OnActivityResult(requestCode, resultCode, intent);
        }

#if DEBUG
        private static void PrintHashKey()
        {
            try
            {
                PackageInfo info = Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, PackageInfoFlags.SigningCertificates);
                foreach (var signature in info.SigningInfo.GetApkContentsSigners())
                {
                    MessageDigest md = MessageDigest.GetInstance("SHA");
                    md.Update(signature.ToByteArray());

                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine("***** SIGNING HASH *****");
                    System.Diagnostics.Debug.WriteLine(BitConverter.ToString(md.Digest()).Replace("-", ":"));
                    System.Diagnostics.Debug.WriteLine("");

                }
            }
            catch (NoSuchAlgorithmException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }
#endif

    }
}