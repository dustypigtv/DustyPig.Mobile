using DustyPig.Mobile.CrossPlatform.DownloadManager;
using DustyPig.Mobile.CrossPlatform.FCM;
using DustyPig.Mobile.iOS.CrossPlatform.DownloadManager;
using DustyPig.Mobile.iOS.CrossPlatform.FCM;
using DustyPig.Mobile.iOS.CrossPlatform.Orientation;
using DustyPig.Mobile.iOS.CrossPlatform.SocialLogin;
using FFImageLoading.Forms.Platform;
using Foundation;
using ObjCRuntime;
using System;
using UIKit;
using Xamarin.Forms;

namespace DustyPig.Mobile.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            CachedImageRenderer.Init();
            FacebookLoginClientImplementation.Init(app, options);
            GoogleLoginClientImplementation.Initialize();

            DependencyService.RegisterSingleton<IFCM>(new FCMImplementation());
            
            DependencyService.RegisterSingleton<IDownloadManager>(new CrossPlatform.DownloadManager.DownloadManagerImplementation());

            LoadApplication(new App());

            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);
            UITabBar.Appearance.SelectedImageTintColor = UIColor.White;

            return base.FinishedLaunching(app, options);
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, [Transient] UIWindow forWindow)
        {
            return ScreenImplementation.CurrentOrientation;
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);
            FacebookLoginClientImplementation.OnActivated();
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (FacebookLoginClientImplementation.OpenUrl(app, url, options))
                return true;

            if (GoogleLoginClientImplementation.OpenUrl(app, url, options))
                return true;

            return base.OpenUrl(app, url, options);
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            if (FacebookLoginClientImplementation.OpenUrl(application, url, sourceApplication, annotation))
                return true;

            return base.OpenUrl(application, url, sourceApplication, annotation);
        }

        public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
        {
            DownloadManagerImplementation.BackgroundSessionCompletionHandler = completionHandler;
        }
    }
}
