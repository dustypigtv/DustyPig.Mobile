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
        private static Activity _mainActivity;

        public static void Init(Activity mainActivity) => _mainActivity = mainActivity;

        public void AllowAnyOrientation() => _mainActivity.RequestedOrientation = ScreenOrientation.Unspecified;

        public void ForceLandscape() => _mainActivity.RequestedOrientation = ScreenOrientation.SensorLandscape;

        public void ForcePortrait() => _mainActivity.RequestedOrientation = ScreenOrientation.SensorPortrait;
    }
}