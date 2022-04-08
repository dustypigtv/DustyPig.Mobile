using DustyPig.Mobile.iOS.LoginButtonRenderers;
using DustyPig.Mobile.LoginButtons;
using Facebook.LoginKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(XamFacebook), typeof(XamFacebookRenderer))]
namespace DustyPig.Mobile.iOS.LoginButtonRenderers
{
    public class XamFacebookRenderer : ViewRenderer<XamFacebook, LoginButton>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<XamFacebook> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
            }
        }
    }
}