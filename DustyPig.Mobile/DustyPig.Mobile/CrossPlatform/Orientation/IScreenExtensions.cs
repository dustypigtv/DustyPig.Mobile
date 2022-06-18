using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.CrossPlatform.Orientation
{
    public static class IScreenExtensions
    {
        public static void SetOrientation(this IScreen @this, bool video)
        {
            switch (Device.Idiom)
            {
                case TargetIdiom.Desktop:
                    @this.AllowAnyOrientation();
                    break;

                case TargetIdiom.Tablet:
                    if (video)
                        @this.ForceLandscape();
                    else
                        @this.AllowAnyOrientation();
                    break;

                case TargetIdiom.TV:
                    @this.ForceLandscape();
                    break;

                default: //Phone
                    if (video)
                        @this.ForceLandscape();
                    else
                        @this.ForcePortrait();
                    break;
            }
        }

        public static void LockCurrentOrientation(this IScreen @this)
        {
            switch (DeviceDisplay.MainDisplayInfo.Orientation)
            {
                case DisplayOrientation.Portrait:
                    @this.ForcePortrait();
                    break;

                case DisplayOrientation.Landscape:
                    @this.ForceLandscape();
                    break;

                default:
                    @this.ForcePortrait();
                    break;
            }
        }
    }
}
