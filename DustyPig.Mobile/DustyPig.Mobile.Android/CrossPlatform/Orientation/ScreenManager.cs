using Android.App;
using Android.Content.PM;

namespace DustyPig.Mobile.CrossPlatform.Orientation
{
    class ScreenManager : IScreen
    {
        private readonly Activity _mainActivity;

        public ScreenManager(Activity mainActivity)
        {
            _mainActivity = mainActivity;
        }

        public void AllowAnyOrientation()
        {
            _mainActivity.RequestedOrientation = ScreenOrientation.Unspecified;
        }

        public void ForceLandscape()
        {
            _mainActivity.RequestedOrientation = ScreenOrientation.SensorLandscape;
        }

        public void ForcePortrait()
        {
            _mainActivity.RequestedOrientation = ScreenOrientation.SensorPortrait;
        }
    }
}