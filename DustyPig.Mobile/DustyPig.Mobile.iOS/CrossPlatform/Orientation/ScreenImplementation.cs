using DustyPig.Mobile.CrossPlatform.Orientation;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ScreenImplementation))]
namespace DustyPig.Mobile.CrossPlatform.Orientation
{
    class ScreenImplementation : IScreen
    {
        public static UIInterfaceOrientationMask CurrentOrientation { get; set; }

        public void AllowAnyOrientation()
        {
            CurrentOrientation = UIInterfaceOrientationMask.All;
            UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)(UIInterfaceOrientation.Unknown)), new NSString("orientation"));
        }

        public void ForceLandscape()
        {
            CurrentOrientation = UIInterfaceOrientationMask.Landscape;
            UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)UIInterfaceOrientation.LandscapeLeft), new NSString("orientation"));
        }

        public void ForcePortrait()
        {
            CurrentOrientation = UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown;
            UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)UIInterfaceOrientation.Portrait), new NSString("orientation"));
        }
    }
}