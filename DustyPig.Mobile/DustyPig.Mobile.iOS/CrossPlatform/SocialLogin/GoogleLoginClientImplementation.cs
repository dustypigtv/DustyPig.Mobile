/*
 Based on https://github.com/CrossGeeks/GoogleClientPlugin
*/
using DustyPig.Mobile.CrossPlatform.SocialLogin;
using DustyPig.Mobile.iOS.CrossPlatform.SocialLogin;
using Foundation;
using Google.SignIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(GoogleLoginClientImplementation))]
namespace DustyPig.Mobile.iOS.CrossPlatform.SocialLogin
{
    public class GoogleLoginClientImplementation : NSObject, IGoogleLoginClient, ISignInDelegate
    {
        private TaskCompletionSource<string> _taskCompletionSource;

        public static void Initialize()
        {
            var googleServiceDictionary = NSDictionary.FromFile("GoogleService-Info.plist");
            SignIn.SharedInstance.ClientId = googleServiceDictionary["CLIENT_ID"].ToString();
        }


        public Task<string> LoginAsync()
        {
            _taskCompletionSource = new TaskCompletionSource<string>();

            SignIn.SharedInstance.Delegate = this;
            SignIn.SharedInstance.PresentingViewController = Utils.GetTopViewControllerWithRootViewController();
            SignIn.SharedInstance.SignInUser();

            return _taskCompletionSource.Task;
        }

        public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            return SignIn.SharedInstance.HandleUrl(url);
        }

        public void DidSignIn(SignIn signIn, GoogleUser user, NSError error)
        {
            var isSuccessful = user != null && error == null;

            if (isSuccessful)
            {
                user.Authentication.GetTokens((Authentication authentication, NSError tokenError) =>
                {
                    if (tokenError == null)
                    {

                        //_accessToken = authentication.AccessToken;
                        //_idToken = authentication.IdToken;
                        _taskCompletionSource.TrySetResult(authentication.IdToken);
                    }
                    else
                    {
                        _taskCompletionSource.TrySetException(new Exception(tokenError.Description));
                    }

                });
            }
            else
            {                
               _taskCompletionSource.TrySetException(new Exception(error.Description));
            }
        }
    }
}