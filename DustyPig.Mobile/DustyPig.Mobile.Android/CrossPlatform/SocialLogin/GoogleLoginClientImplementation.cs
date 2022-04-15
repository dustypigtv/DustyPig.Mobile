/*
 Based on https://github.com/CrossGeeks/GoogleClientPlugin
*/

using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common.Apis;
using Android.Gms.Tasks;
using Android.Runtime;
using DustyPig.Mobile.CrossPlatform.SocialLogin;
using DustyPig.Mobile.Droid.CrossPlatform.SocialLogin;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(GoogleLoginClientImplementation))]
namespace DustyPig.Mobile.Droid.CrossPlatform.SocialLogin
{
    public class GoogleLoginClientImplementation : Java.Lang.Object, IGoogleLoginClient, IOnCompleteListener
    {
        private const int AUTH_ACTIVITY_ID = 9637;
        private const string DUSTY_PIG_CLIENT_ID = "233400762141-fibb1sigs61v1tsf66s1u6aqdh25ddd4.apps.googleusercontent.com";

        private static Activity _activity;
        private static GoogleSignInClient _googleClient;
        private static GoogleLoginClientImplementation _instance;

        private TaskCompletionSource<string> _taskCompletionSource;

        public static void Init(Activity activity)
        {
            _activity = activity;

            var gopBuilder = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestEmail()
                .RequestIdToken(DUSTY_PIG_CLIENT_ID);

            GoogleSignInOptions googleSignInOptions = gopBuilder.Build();

            _googleClient = GoogleSignIn.GetClient(activity, googleSignInOptions);
        }

        public GoogleLoginClientImplementation() => _instance = this;

        public Task<string> LoginAsync()
        {
            _taskCompletionSource = new TaskCompletionSource<string>();

            _googleClient.SignOut();
            _activity.StartActivityForResult(_googleClient.SignInIntent, AUTH_ACTIVITY_ID);

            return _taskCompletionSource.Task;
        }

        public void OnComplete(Android.Gms.Tasks.Task task)
        {

            if (task.IsSuccessful)
            {
                _taskCompletionSource?.TrySetResult(task.Result.JavaCast<GoogleSignInAccount>().IdToken);
            }
            else if (task.IsCanceled)
            {
                _taskCompletionSource?.TrySetCanceled();
            }
            else
            {
                var apiEx = task.Exception.JavaCast<ApiException>();

                switch (apiEx.StatusCode)
                {
                    case 12500:
                        _taskCompletionSource?.TrySetException(new Exception("Unkown Error"));
                        break;

                    case 12501:
                        _taskCompletionSource?.TrySetCanceled();
                        break;

                    case 12502:
                        _taskCompletionSource.TrySetException(new Exception("Another sign in is in progress"));
                        break;

                    default:
                        _taskCompletionSource?.TrySetException(new Exception(apiEx.LocalizedMessage));
                        break;
                }
            }
        }

        public static void OnAuthCompleted(int requestCode, Intent intent)
        {
            if (requestCode != AUTH_ACTIVITY_ID)
                return;

            GoogleSignIn.GetSignedInAccountFromIntent(intent).AddOnCompleteListener(_instance as IOnCompleteListener);
        }
    }
}