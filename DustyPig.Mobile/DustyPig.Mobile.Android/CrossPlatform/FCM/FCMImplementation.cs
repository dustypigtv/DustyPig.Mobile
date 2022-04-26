using Android.Content;
using DustyPig.Mobile.CrossPlatform.FCM;
using Firebase.Messaging;
using System.Threading.Tasks;

namespace DustyPig.Mobile.Droid.CrossPlatform.FCM
{
    internal class FCMImplementation : IFCM
    {
        public Task ResetTokenAsync()
        {
            try { return FirebaseMessaging.Instance.DeleteToken().ToAwaitableTask(); }
            catch { return Task.CompletedTask; }
        }

        public async Task<string> GetTokenAsync()
        {
            try { return (await FirebaseMessaging.Instance.GetToken().ToAwaitableTask()).ToString(); }
            catch { return null; }
        }
    }
}