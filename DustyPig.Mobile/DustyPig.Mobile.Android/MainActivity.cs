﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using DustyPig.Mobile.CrossPlatform.DownloadManager;
using DustyPig.Mobile.CrossPlatform.FCM;
using DustyPig.Mobile.Droid.CrossPlatform;
using DustyPig.Mobile.Droid.CrossPlatform.FCM;
using DustyPig.Mobile.Droid.CrossPlatform.Orientation;
using DustyPig.Mobile.Droid.CrossPlatform.SocialLogin;
using FFImageLoading.Forms.Platform;
using Java.Security;
using System;

namespace DustyPig.Mobile.Droid
{
    [Activity(Label = "Dusty Pig", 
        LaunchMode = LaunchMode.SingleTop, 
        Icon = "@mipmap/icon", 
        Theme = "@style/SplashTheme", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    
    [IntentFilter(new[] { Intent.ActionView },
              Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
              DataScheme = "https",
              DataHost = "service.dustypig.tv",
              AutoVerify = true)]
    
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            
#if DEBUG
            //Print the signing hashes
            try
            {
                PackageInfo info = Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, PackageInfoFlags.SigningCertificates);
                foreach (var signature in info.SigningInfo.GetApkContentsSigners())
                {
                    MessageDigest md = MessageDigest.GetInstance("SHA");
                    md.Update(signature.ToByteArray());
                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine("***** SHA-1 HASH *****");
                    System.Diagnostics.Debug.WriteLine(BitConverter.ToString(md.Digest()).Replace("-", ":"));
                    System.Diagnostics.Debug.WriteLine("");

                    md = MessageDigest.GetInstance("SHA-256");
                    md.Update(signature.ToByteArray());
                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine("***** SHA-256 HASH *****");
                    System.Diagnostics.Debug.WriteLine(BitConverter.ToString(md.Digest()).Replace("-", ":"));
                    System.Diagnostics.Debug.WriteLine("");
                }
            }
            catch (NoSuchAlgorithmException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
#endif

            base.OnCreate(savedInstanceState);
            Instance = this;

            Rg.Plugins.Popup.Popup.Init(this);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            PopupImplemention.Init(Resource.Style.MainTheme_AlertDialog);
            CachedImageRenderer.Init(true);

            Firebase.FirebaseApp.InitializeApp(this);
            Xamarin.Forms.DependencyService.RegisterSingleton<IFCM>(new FCMImplementation());

            Xamarin.Forms.DependencyService.RegisterSingleton<IDownloadManager>(new CrossPlatform.DownloadManager.DownloadManagerImplementation());

            
            SetTheme(Resource.Style.MainTheme);

            LoadApplication(new App());

            Window.SetStatusBarColor(Android.Graphics.Color.Black);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);
            FacebookLoginClientImplementation.OnActivityResult(requestCode, resultCode, intent);
            GoogleLoginClientImplementation.OnAuthCompleted(requestCode, intent);
        }

        public override void OnBackPressed()
        {
            Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
        }
    }
}