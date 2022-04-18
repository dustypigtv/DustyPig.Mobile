/*
    Based largely on https://github.com/CrossGeeks/FacebookClientPlugin 
*/
using Android.App;
using Android.Content;
using DustyPig.Mobile.CrossPlatform.SocialLogin;
using DustyPig.Mobile.Droid.CrossPlatform.SocialLogin;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Forms;

[assembly: Dependency(typeof(FacebookLoginClientImplementation))]
namespace DustyPig.Mobile.Droid.CrossPlatform.SocialLogin
{
    public class FacebookLoginClientImplementation : IFacebookLoginClient
    {
        private static readonly ICallbackManager _callbackManager = CallbackManagerFactory.Create();

        private static Activity _activity;

        private readonly LoginCallback _loginCallback;
        private TaskCompletionSource<string> _loginTaskCompletionSource;

        public static void Init(Activity activity) => _activity = activity;

        public FacebookLoginClientImplementation()
        {
            _loginCallback = new LoginCallback()
            {
                CancelAction = () => _loginTaskCompletionSource.TrySetCanceled(),
                ErrorAction = error => _loginTaskCompletionSource.TrySetException(error.InnerException),
                SuccessAction = success =>_loginTaskCompletionSource.TrySetResult(success.AccessToken.Token)
            };

            LoginManager.Instance.RegisterCallback(_callbackManager, _loginCallback);
        }

        public static void OnActivityResult(int requestCode, Result resultCode, Intent intent) =>
            _callbackManager?.OnActivityResult(requestCode, (int)resultCode, intent);

        public Task<string> LoginAsync()
        {
            _loginTaskCompletionSource = new TaskCompletionSource<string>();

            //There is a bug in the current Xamarin bindings that makes this crash when 
            //Attempting to use the native fb app.  Until Xamarin.Facebook.Login.Android 
            //is updated to SDK v12.2, I have to force WebOnly
            LoginManager.Instance.SetLoginBehavior(LoginBehavior.WebOnly);

            LoginManager.Instance.LogInWithReadPermissions(_activity, new string[] { "email" }.ToList());
            return _loginTaskCompletionSource.Task;
        }
    }

    public class LoginCallback : Java.Lang.Object, IFacebookCallback
    {
        public Action CancelAction { get; set; }

        public Action<FacebookException> ErrorAction { get; set; }

        public Action<LoginResult> SuccessAction { get; set; }

        public void OnCancel() => CancelAction?.Invoke();

        public void OnError(FacebookException error) => ErrorAction?.Invoke(error);

        public void OnSuccess(Java.Lang.Object result) => SuccessAction.Invoke(result as LoginResult);
    }
}