using DustyPig.Mobile.CrossPlatform;
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
        public Task Alert(string title, string message)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            UIAlertController alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            var firstAttributes = new UIStringAttributes { ForegroundColor = UIColor.White };
            var secondAttributes = new UIStringAttributes { ForegroundColor = UIColor.FromRGB(Theme.DarkGrey.R, Theme.DarkGrey.G, Theme.DarkGrey.B) };

            alertController.SetValueForKey(new NSAttributedString(title, firstAttributes), new NSString("attributedTitle"));
            alertController.SetValueForKey(new NSAttributedString(message, secondAttributes), new NSString("attributedMessage"));

            UIAlertAction okAction = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (sender) =>
            {
                taskCompletionSource.TrySetResult(true);
            });


            okAction.SetValueForKey(UIColor.White, new NSString("_titleTextColor"));

            alertController.AddAction(okAction);

            var currentViewController = GetTopViewControllerWithRootViewController(UIApplication.SharedApplication.Delegate.GetWindow().RootViewController);
            currentViewController.PresentViewController(alertController, true, null);

            return taskCompletionSource.Task;
        }

        public Task<bool> OkCancel(string title, string message)
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

            var currentViewController = GetTopViewControllerWithRootViewController(UIApplication.SharedApplication.Delegate.GetWindow().RootViewController);
            currentViewController.PresentViewController(alertController, true, null);

            return taskCompletionSource.Task;
        }



        UIViewController GetTopViewControllerWithRootViewController(UIViewController rootViewController)
        {
            if (rootViewController is UITabBarController)
            {
                UITabBarController tabBarController = (UITabBarController)rootViewController;
                return GetTopViewControllerWithRootViewController(tabBarController.SelectedViewController);
            }
            else if (rootViewController is UINavigationController)
            {
                UINavigationController navigationController = (UINavigationController)rootViewController;
                return GetTopViewControllerWithRootViewController(navigationController.VisibleViewController);
            }
            else if (rootViewController.PresentedViewController != null)
            {
                UIViewController presentedViewController = rootViewController.PresentedViewController;
                return GetTopViewControllerWithRootViewController(presentedViewController);
            }
            else
            {
                return rootViewController;
            }
        }
    }
}