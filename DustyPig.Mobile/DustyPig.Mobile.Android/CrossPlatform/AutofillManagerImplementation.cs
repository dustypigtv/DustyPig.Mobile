//https://mikalaidaronin.info/blog/posts/xamarin-forms-password-autofill/

using Android.App;
using Android.OS;
using Android.Views.Autofill;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Droid.CrossPlatform;

[assembly: Xamarin.Forms.Dependency(typeof(AutofillManagerImplementation))]
namespace DustyPig.Mobile.Droid.CrossPlatform
{
    public class AutofillManagerImplementation : IAutofillManager
    {
        public void Commit()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var manager = (AutofillManager)Application.Context.GetSystemService(Java.Lang.Class.FromType(typeof(AutofillManager)));
                manager.Commit();
            }
        }
    }
}