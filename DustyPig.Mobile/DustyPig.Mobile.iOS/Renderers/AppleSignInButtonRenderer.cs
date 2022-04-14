using AuthenticationServices;
using DustyPig.Mobile.Controls;
using DustyPig.Mobile.iOS.Renderers;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportRenderer(typeof(AppleSignInButton), typeof(AppleSignInButtonRenderer))]
namespace DustyPig.Mobile.iOS.Renderers
{
    public class AppleSignInButtonRenderer : ViewRenderer<AppleSignInButton, ASAuthorizationAppleIdButton>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<AppleSignInButton> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                if (Control != null)
                    Control.TouchUpInside -= This_TouchUpInside;
            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var button = new ASAuthorizationAppleIdButton(ASAuthorizationAppleIdButtonType.Default, ASAuthorizationAppleIdButtonStyle.White);
                    button.TouchUpInside += This_TouchUpInside;

                    SetNativeControl(button);
                }
            }
        }

        void This_TouchUpInside(object sender, EventArgs e) => Element?.InvokeSignIn(sender, e);

    }
}