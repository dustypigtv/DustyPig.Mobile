using Xamarin.Forms;

namespace DustyPig.Mobile.CrossPlatform.FCM
{
    public class CrossFCM
    {
        public static IFCM Instance => DependencyService.Get<IFCM>();
    }
}
