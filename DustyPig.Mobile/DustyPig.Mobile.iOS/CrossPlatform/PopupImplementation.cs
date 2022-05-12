using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.iOS.CrossPlatform;
using Foundation;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PopupImplementation))]
namespace DustyPig.Mobile.iOS.CrossPlatform
{
    public class PopupImplementation : IPopup
    {
        public Task AlertAsync(string title, string message)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            UIAlertController alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            var textColor = new UIStringAttributes { ForegroundColor = UIColor.White };
            alertController.SetValueForKey(new NSAttributedString(title, textColor), new NSString("attributedTitle"));
            alertController.SetValueForKey(new NSAttributedString(message, textColor), new NSString("attributedMessage"));

            UIAlertAction okAction = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (sender) =>
            {
                taskCompletionSource.TrySetResult(true);
            });
            okAction.SetValueForKey(UIColor.White, new NSString("_titleTextColor"));

            alertController.AddAction(okAction);

            SetBackgroundColor(alertController.View);

            var currentViewController = Utils.GetTopViewControllerWithRootViewController();
            currentViewController.PresentViewController(alertController, true, null);

            return taskCompletionSource.Task;
        }

        private static void SetBackgroundColor(UIView view)
        {
            if (view == null)
                return;
            view.BackgroundColor = UIColor.FromRGB(Theme.DarkGrey.R, Theme.DarkGrey.G, Theme.DarkGrey.B);
            if (view.Subviews != null)
                foreach (var subView in view.Subviews)
                    SetBackgroundColor(subView);
        }

        public Task<bool> OkCancelAsync(string title, string message)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            UIAlertController alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            var firstAttributes = new UIStringAttributes { ForegroundColor = UIColor.White };
            var secondAttributes = new UIStringAttributes { ForegroundColor = UIColor.FromRGB(Theme.DarkGrey.R, Theme.DarkGrey.G, Theme.DarkGrey.B) };

            alertController.SetValueForKey(new NSAttributedString(title, firstAttributes), new NSString("attributedTitle"));
            alertController.SetValueForKey(new NSAttributedString(message, secondAttributes), new NSString("attributedMessage"));

            UIAlertAction cancelAction = UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (sender) =>
            {
                taskCompletionSource.TrySetResult(false);
            });

            UIAlertAction okAction = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (sender) =>
            {
                taskCompletionSource.TrySetResult(true);
            });


            okAction.SetValueForKey(UIColor.White, new NSString("_titleTextColor"));

            alertController.AddAction(cancelAction);
            alertController.AddAction(okAction);

            var currentViewController = Utils.GetTopViewControllerWithRootViewController();
            currentViewController.PresentViewController(alertController, true, null);

            return taskCompletionSource.Task;
        }
    }
}