using Xamarin.Essentials;

namespace DustyPig.Mobile.Helpers
{
    static class Screen
    {
        public static double CurrentWidth => DeviceDisplay.MainDisplayInfo.XamarinWidth();

        public static double CurrentHeight => DeviceDisplay.MainDisplayInfo.XamarinHeight();

        public static double XamarinWidth(this DisplayInfo di) => di.Width / di.Density;

        public static double XamarinHeight(this DisplayInfo di) => di.Height / di.Density;
    }
}
