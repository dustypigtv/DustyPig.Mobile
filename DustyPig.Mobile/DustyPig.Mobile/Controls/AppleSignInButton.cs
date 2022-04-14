using System;
using Xamarin.Forms;

namespace DustyPig.Mobile.Controls
{
    public class AppleSignInButton : Button
    {
        public event EventHandler SignIn;

        public void InvokeSignIn(object sender, EventArgs e) => SignIn?.Invoke(sender, e);
    }
}
