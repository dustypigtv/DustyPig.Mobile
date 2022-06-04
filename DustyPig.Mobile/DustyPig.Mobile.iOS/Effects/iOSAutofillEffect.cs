// https://mikalaidaronin.info/blog/posts/xamarin-forms-password-autofill/

using DustyPig.Mobile.Effects;
using DustyPig.Mobile.iOS.Effects;
using Foundation;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportEffect(typeof(iOSAutofillEffect), nameof(AutofillEffect))]
namespace DustyPig.Mobile.iOS.Effects
{
    public class iOSAutofillEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var effect = (AutofillEffect)Element.Effects.FirstOrDefault(e => e is Mobile.Effects.AutofillEffect);
            if (effect != null  && UIDevice.CurrentDevice.CheckSystemVersion(11, 0) && Control is UITextField textField)
            {
                switch (effect.Type)
                {
                    case AutofillContentType.None:
                        textField.TextContentType = NSString.Empty;
                        break;
                    case AutofillContentType.Username:
                        textField.TextContentType = UITextContentType.Username;
                        break;
                    case AutofillContentType.Password:
                        textField.TextContentType = UITextContentType.Password;
                        break;
                }
            }
        }

        protected override void OnDetached()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0) && Control is UITextField textField)
            {
                textField.TextContentType = NSString.Empty;
            }
        }
    }
}