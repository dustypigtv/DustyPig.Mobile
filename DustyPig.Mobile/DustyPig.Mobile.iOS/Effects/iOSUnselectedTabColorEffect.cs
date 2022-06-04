using DustyPig.Mobile.Effects;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(DustyPig.Mobile.iOS.Effects.iOSUnselectedTabColorEffect), nameof(UnselectedTabColorEffect))]
namespace DustyPig.Mobile.iOS.Effects
{
    public class iOSUnselectedTabColorEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var tabBar = Container.Subviews.OfType<UITabBar>().FirstOrDefault();
            if (tabBar == null)
                return;
            

            tabBar.UnselectedItemTintColor = UIColor.White;
        }

        protected override void OnDetached()
        {
        }
    }
}