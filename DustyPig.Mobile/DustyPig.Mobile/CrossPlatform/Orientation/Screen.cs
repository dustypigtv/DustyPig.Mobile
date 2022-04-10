using Xamarin.Forms;

namespace DustyPig.Mobile.CrossPlatform.Orientation
{
    public static class Screen
    {
        public static IScreen Current { get; set; }

        public static void SetOrientation(bool video)
        {
            switch (Device.Idiom)
            {
                case TargetIdiom.Desktop:
                    Current.AllowAnyOrientation();
                    break;

                case TargetIdiom.Phone:
                    if (video)
                        Current.ForceLandscape();
                    else
                        Current.ForcePortrait();
                    break;

                case TargetIdiom.Tablet:
                    if (video)
                        Current.ForceLandscape();
                    else
                        Current.AllowAnyOrientation();
                    break;

                case TargetIdiom.TV:
                    Current.ForceLandscape();
                    break;

                default:
                    if (video)
                        Current.ForceLandscape();
                    else
                        Current.ForcePortrait();
                    break;
            }
        }
    }
}
