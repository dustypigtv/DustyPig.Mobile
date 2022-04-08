using Android.Content;
using Android.Util;
using DustyPig.Mobile.Droid.LoginButtonRenderers;
using DustyPig.Mobile.LoginButtons;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(XamFacebook), typeof(XamFacebookRenderer))]
namespace DustyPig.Mobile.Droid.LoginButtonRenderers
{
    public class XamFacebookRenderer : ViewRenderer, IFacebookCallback
    {
        private bool _disposed = false;

        public XamFacebookRenderer(Context context) : base(context)
        {

        }

        private XamFacebook XamElement => (XamFacebook)Element;

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (Control != null)
                {
                    (Control as LoginButton).UnregisterCallback(MainActivity.CallbackManager);
                    Control.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }



        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (Control == null)
            {
                //When this button is shown, it means nobody is logged in. Make sure that facebook is in the same state
                LoginManager.Instance.LogOut();

                var fbButton = new LoginButton(Context) { LoginBehavior = LoginBehavior.NativeWithFallback };                
                fbButton.SetPermissions(new string[] { "public_profile", "email" });
                fbButton.RegisterCallback(MainActivity.CallbackManager, this);

                // *** TO DO ***
                //This is hacky, there must be a better way...
                //Based on button height of 40
                fbButton.SetTextSize(ComplexUnitType.Dip, 15);
                int padding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
                fbButton.SetPadding(fbButton.PaddingLeft, padding, fbButton.PaddingRight, padding);

                SetNativeControl(fbButton);
            }
        }



        public void OnCancel() => XamElement.OnCancel?.Execute(null);

        public void OnError(FacebookException fbException) => XamElement.OnError?.Execute(fbException.Message);

        public void OnSuccess(Java.Lang.Object result) => XamElement.OnSuccess?.Execute(((LoginResult)result).AccessToken.Token);

    }
}