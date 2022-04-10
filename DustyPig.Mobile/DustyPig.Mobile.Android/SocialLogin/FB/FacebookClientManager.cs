using Android.App;
using Android.Content;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;

namespace DustyPig.Mobile.SocialLogin.FB
{
    public class FacebookClientManager : IFacebookClient
    {
        private static readonly ICallbackManager _callbackManager = CallbackManagerFactory.Create();
        
        private Activity _activity;
        private LoginCallback _loginCallback;
        private TaskCompletionSource<string> _loginTaskCompletionSource;

        public FacebookClientManager(Activity activity)
        {
            _activity = activity;
            _loginCallback = new LoginCallback()
            {
                CancelAction = () =>
                {
                    if (EmailDenied)
                        ThrowEmailException();
                    else
                        _loginTaskCompletionSource.TrySetCanceled();
                },

                ErrorAction = error => _loginTaskCompletionSource.TrySetException(new Exception(error.ToString())),

                SuccessAction = success =>
                {
                    if (success.RecentlyDeniedPermissions.Contains("email"))
                        ThrowEmailException();
                    else
                        _loginTaskCompletionSource.TrySetResult(success.AccessToken.Token);
                }
            };

            LoginManager.Instance.RegisterCallback(_callbackManager, _loginCallback);            
        }

        public static void OnActivityResult(int requestCode, Result resultCode, Intent intent) =>
            _callbackManager?.OnActivityResult(requestCode, (int)resultCode, intent);

        private static bool EmailDenied = AccessToken.CurrentAccessToken != null && !AccessToken.CurrentAccessToken.Permissions.Contains("email");

        private void ThrowEmailException() =>
            _loginTaskCompletionSource?.TrySetException(new Exception("Email is required to use Dusty Pig. Please try again and allow email access"));

        public Task<string> LoginAsync()
        {
            _loginTaskCompletionSource = new TaskCompletionSource<string>();

            LoginManager.Instance.SetAuthType(EmailDenied ? "rerequest" : null);
            LoginManager.Instance.SetLoginBehavior(LoginBehavior.WebOnly);
            LoginManager.Instance.LogInWithReadPermissions(_activity, new string[] { "email" }.ToList());
           
            return _loginTaskCompletionSource.Task;
        }
    }
}