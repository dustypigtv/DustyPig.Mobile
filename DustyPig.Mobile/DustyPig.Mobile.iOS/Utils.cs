using UIKit;

namespace DustyPig.Mobile.iOS
{
    internal static class Utils
    {
        public static UIViewController GetTopViewControllerWithRootViewController()
        {
            return GetTopViewControllerWithRootViewController(UIApplication.SharedApplication.Delegate.GetWindow().RootViewController);
        }

        static UIViewController GetTopViewControllerWithRootViewController(UIViewController rootViewController)
        {
            if (rootViewController is UITabBarController tabBarController)
            {
                return GetTopViewControllerWithRootViewController(tabBarController.SelectedViewController);
            }
            else if (rootViewController is UINavigationController navigationController)
            {
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