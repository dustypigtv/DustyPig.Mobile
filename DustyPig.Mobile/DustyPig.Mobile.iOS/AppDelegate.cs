using DustyPig.Mobile.iOS.CrossPlatform.Orientation;
using DustyPig.Mobile.iOS.CrossPlatform.SocialLogin;
using FFImageLoading.Forms.Platform;
using Foundation;
using ObjCRuntime;
using Plugin.FirebasePushNotification;
using System;
using UIKit;

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

            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);
            UITabBar.Appearance.SelectedImageTintColor = UIColor.White;

            CachedImageRenderer.Init();
            FacebookLoginClientImplementation.Init(app, options);
            GoogleLoginClientImplementation.Initialize();
            
            LoadApplication(new App());

            FirebasePushNotificationManager.Initialize(options, true);

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

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            FirebasePushNotificationManager.DidRegisterRemoteNotifications(deviceToken);
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            FirebasePushNotificationManager.RemoteNotificationRegistrationFailed(error);

        }
        // To receive notifications in foregroung on iOS 9 and below.
        // To receive notifications in background in any iOS version
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            // If you are receiving a notification message while your app is in the background,
            // this callback will not be fired 'till the user taps on the notification launching the application.

            // If you disable method swizzling, you'll need to call this method. 
            // This lets FCM track message delivery and analytics, which is performed
            // automatically with method swizzling enabled.
            FirebasePushNotificationManager.DidReceiveMessage(userInfo);
            // Do your magic to handle the notification data
            System.Console.WriteLine(userInfo);

            completionHandler(UIBackgroundFetchResult.NewData);
        }
    }
}
