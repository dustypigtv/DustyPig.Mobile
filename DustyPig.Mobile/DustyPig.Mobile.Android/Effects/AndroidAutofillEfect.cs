//https://mikalaidaronin.info/blog/posts/xamarin-forms-password-autofill/

using Android.OS;
using Android.Views;
using Android.Widget;
using DustyPig.Mobile.Droid.Effects;
using DustyPig.Mobile.Effects;
using System.Linq;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.ExportEffect(typeof(AndroidAutofillEffect), nameof(AutofillEffect))]
namespace DustyPig.Mobile.Droid.Effects
{
    public class AndroidAutofillEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var effect = (AutofillEffect)Element.Effects.FirstOrDefault(e => e is AutofillEffect);

            if (effect != null && Build.VERSION.SdkInt >= BuildVersionCodes.O && Control is EditText editText)
            {
                editText.ImportantForAutofill = Android.Views.ImportantForAutofill.Yes;
                switch (effect.Type)
                {
                    case AutofillContentType.None:
                        editText.SetAutofillHints(autofillHints: null);
                        break;
                    case AutofillContentType.Username:
                        editText.SetAutofillHints(View.AutofillHintUsername);
                        break;
                    case AutofillContentType.Password:
                        editText.SetAutofillHints(View.AutofillHintPassword);
                        break;
                }
            }
        }

        protected override void OnDetached()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O && Control is EditText editText)
            {
                editText.ImportantForAutofill = ImportantForAutofill.Auto;
                editText.SetAutofillHints(autofillHints: null);
            }
        }
    }
}