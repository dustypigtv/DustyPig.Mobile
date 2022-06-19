using DustyPig.Mobile.CrossPlatform.Orientation;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.Helpers
{
    static class Screen
    {
        public static IScreen IScreen => DependencyService.Get<IScreen>();

        public static double Width => DeviceDisplay.MainDisplayInfo.CurrentWidth();

        public static double Height => DeviceDisplay.MainDisplayInfo.CurrentHeight();

        public static double CurrentWidth(this DisplayInfo di) => di.Width / di.Density;

        public static double CurrentHeight(this DisplayInfo di) => di.Height / di.Density;
    }
}
