using DustyPig.Mobile.CrossPlatform.FCM;
using Firebase.CloudMessaging;
using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;

namespace DustyPig.Mobile.iOS.CrossPlatform.FCM
{
    public class FCMImplementation : NSObject, IFCM, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        public FCMImplementation()
        {
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                (granted, error) => {
                    if (!granted)
                    {
                        //Error?.Invoke(this, new FCMErrorEventArgs("User permission for remote notifications is not granted"));
                    }
                });

            Firebase.Core.App.Configure();
            UNUserNotificationCenter.Current.Delegate = this;                        
            Messaging.SharedInstance.Delegate = this;
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
        }

        public async Task<string> GetTokenAsync()
        {
            try 
            {
                var ret = Messaging.SharedInstance.FcmToken;
                if (string.IsNullOrEmpty(ret))
                    ret = await Messaging.SharedInstance.RetrieveFcmTokenAsync(Firebase.Core.App.DefaultInstance.Options.GcmSenderId);
                return ret;
            }
            catch { return null; }
        }

        public Task ResetTokenAsync()
        {
            try { return Messaging.SharedInstance.DeleteFcmTokenAsync(Firebase.Core.App.DefaultInstance.Options.GcmSenderId); }
            catch { return Task.CompletedTask; }
        }


        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            //OnNotificationReceived(notification.ToFCMNotification());
            //completionHandler(UNNotificationPresentationOptions.Alert);
        }

        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            //if (_notificationTapped == null)
            //{
            //    _missedTappedNotification = response.Notification.ToFCMNotification();
            //}
            //else
            //{
            //    _notificationTapped.Invoke(this, new FCMNotificationTappedEventArgs(response.Notification.ToFCMNotification()));
            //}
        }

        
        //public event EventHandler<FCMNotificationReceivedEventArgs> NotificationReceived;
        //public event EventHandler<FCMErrorEventArgs> Error;

        //private event EventHandler<FCMNotificationTappedEventArgs> _notificationTapped;
        //public event EventHandler<FCMNotificationTappedEventArgs> NotificationTapped
        //{
        //    add
        //    {
        //        _notificationTapped += value;
        //        if (_missedTappedNotification != null)
        //        {
        //            _notificationTapped?.Invoke(this, new FCMNotificationTappedEventArgs(_missedTappedNotification));
        //            _missedTappedNotification = null;
        //        }
        //    }
        //    remove => _notificationTapped -= value;
        //}
    }
}