using Android.Content.PM;

namespace DustyPig.Mobile.Droid
{
    class PlatformDep : IPlatformDep
    {
        private readonly MainActivity _mainActivity;

        public PlatformDep(MainActivity mainActivity)
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