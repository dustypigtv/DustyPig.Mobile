using Android.Content;
using Android.Views.InputMethods;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Droid.CrossPlatform;
using Xamarin.Forms;

[assembly: Dependency(typeof(KeyboardHelperImplementation))]
namespace DustyPig.Mobile.Droid.CrossPlatform
{
    public class KeyboardHelperImplementation : IKeyboardHelper
    {
        public void HideKeyboard()
        {
            var inputMethodManager = MainActivity.Instance.GetSystemService(Context.InputMethodService) as InputMethodManager;
            if (inputMethodManager != null)
            {
                var token = MainActivity.Instance.CurrentFocus?.WindowToken;
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.NotAlways);
            }
        }
    }
}