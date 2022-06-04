//https://mikalaidaronin.info/blog/posts/xamarin-forms-password-autofill/

using Xamarin.Forms;

namespace DustyPig.Mobile.Effects
{
    public enum AutofillContentType
    {
        None,
        Username,
        Password
    }

    public class AutofillEffect : RoutingEffect
    {
        public AutofillContentType Type { get; set; }

        public AutofillEffect() : base("AppEffects." + nameof(AutofillEffect))
        {
        }
    }
}
