using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common.Apis;
using Android.Gms.Tasks;
using Android.Runtime;
using System;
/*
 Based on https://github.com/CrossGeeks/GoogleClientPlugin
*/
using System.Threading.Tasks;

namespace DustyPig.Mobile.CrossPlatform.SocialLogin
{
    public class GoogleClientManager : Java.Lang.Object, ISocialLoginClient, IOnCompleteListener
    {
        private const int AUTH_ACTIVITY_ID = 9637;
        private const string DUSTY_PIG_CLIENT_ID = "233400762141-fibb1sigs61v1tsf66s1u6aqdh25ddd4.apps.googleusercontent.com";

        private readonly Activity _activity;
        readonly GoogleSignInClient _googleClient;
        private TaskCompletionSource<string> _taskCompletionSource;

        public GoogleClientManager(Activity activity)
        {
            _activity = activity;

            var gopBuilder = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestEmail()
                .RequestIdToken(DUSTY_PIG_CLIENT_ID);

            GoogleSignInOptions googleSignInOptions = gopBuilder.Build();

            _googleClient = GoogleSignIn.GetClient(activity, googleSignInOptions);
        }

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
                _taskCompletionSource.TrySetResult(task.Result.JavaCast<GoogleSignInAccount>().IdToken);
            else if (task.IsCanceled)
                _taskCompletionSource.TrySetCanceled();
            else
                _taskCompletionSource.TrySetException(new Exception(task.Exception.JavaCast<ApiException>().Message));
        }

        public static void OnAuthCompleted(int requestCode, Intent intent)
        {
            if (requestCode != AUTH_ACTIVITY_ID)
                return;
            
            GoogleSignIn.GetSignedInAccountFromIntent(intent).AddOnCompleteListener(GoogleClient.Current as IOnCompleteListener);
        }
    }
}