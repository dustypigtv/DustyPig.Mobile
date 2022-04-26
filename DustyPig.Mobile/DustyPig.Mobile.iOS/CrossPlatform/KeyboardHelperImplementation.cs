using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.iOS.CrossPlatform;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(KeyboardHelperImplementation))]
namespace DustyPig.Mobile.iOS.CrossPlatform
{
    public class KeyboardHelperImplementation : IKeyboardHelper
    {
        public void HideKeyboard()
        {
            UIApplication.SharedApplication.KeyWindow.EndEditing(true);
        }
    }
}