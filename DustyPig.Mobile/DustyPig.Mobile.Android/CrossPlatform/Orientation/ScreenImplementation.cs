using Android.App;
using Android.Content.PM;
using DustyPig.Mobile.CrossPlatform.Orientation;
using DustyPig.Mobile.Droid.CrossPlatform.Orientation;
using Xamarin.Forms;

[assembly: Dependency(typeof(ScreenImplementation))]
namespace DustyPig.Mobile.Droid.CrossPlatform.Orientation
{
    class ScreenImplementation : IScreen
    {
        public void AllowAnyOrientation() => MainActivity.Instance.RequestedOrientation = ScreenOrientation.Unspecified;

        public void ForceLandscape() => MainActivity.Instance.RequestedOrientation = ScreenOrientation.SensorLandscape;

        public void ForcePortrait() => MainActivity.Instance.RequestedOrientation = ScreenOrientation.SensorPortrait;
    }
}