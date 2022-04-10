using Facebook.CoreKit;
using Facebook.LoginKit;
using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;

namespace DustyPig.Mobile.SocialLogin.FB
{
    public class FacebookClientManager : IFacebookClient
    {
        private static readonly LoginManager _loginManager = new LoginManager();

        public static void Init(UIApplication app, NSDictionary options)
        {
            ApplicationDelegate.SharedInstance.FinishedLaunching(app, options);
        }

        public static void OnActivated()
        {
            AppEvents.ActivateApp();
        }
        public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            return ApplicationDelegate.SharedInstance.OpenUrl(app, url, $"{options["UIApplicationOpenURLOptionsSourceApplicationKey"]}", options["UIApplicationOpenURLOptionsAnnotationKey"]);
        }

        public static bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }

        public Task<string> LoginAsync()
        {
            TaskCompletionSource<string> loginTask = new TaskCompletionSource<string>();

            var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while(vc.PresentedViewController != null)
            {
                vc = vc.PresentedViewController;
            }

            bool reask = AccessToken.CurrentAccessToken != null && !AccessToken.CurrentAccessToken.Permissions.Contains("email");
            _loginManager.AuthType = reask ? LoginAuthType.Rerequest : LoginAuthType.Reauthorize;

            _loginManager.LogIn(new string[] { "email" }, vc, (result, error) =>
            {
                if (error != null)
                    loginTask.TrySetException(new Exception(error.Description));
                else if (result.IsCancelled)
                    loginTask.TrySetCanceled();
                else
                    loginTask.TrySetResult(result.Token.TokenString);
            });

            return loginTask.Task;
        }
    }
}