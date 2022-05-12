using Android.App;
using Android.Runtime;
using Firebase.Messaging;

namespace DustyPig.Mobile.Droid.CrossPlatform.FCM
{
    [Service(Exported = true)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    [Preserve(AllMembers = true)]
    public class FCMService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            //DependencyService.Get<IFCM>().OnNotificationReceived();
        }

        public override async void OnNewToken(string token)
        {
            //await DependencyService.Get<IFCM>().OnTokenRefreshAsync();
        }
    }
}