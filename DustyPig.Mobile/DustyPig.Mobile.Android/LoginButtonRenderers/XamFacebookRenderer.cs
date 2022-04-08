using Android.Content;
using Android.Util;
using DustyPig.Mobile.Droid.LoginButtonRenderers;
using DustyPig.Mobile.LoginButtons;
using System.ComponentModel;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(XamFacebook), typeof(XamFacebookRenderer))]
namespace DustyPig.Mobile.Droid.LoginButtonRenderers
{
    public class XamFacebookRenderer : ViewRenderer<XamFacebook, LoginButton>, IFacebookCallback
    {
        private bool _disposed = false;

        public XamFacebookRenderer(Context context) : base(context)
        {
            
        }

        private LoginButton NativeButton => (LoginButton)Control;

        private XamFacebook XamButton => (XamFacebook)Element;
        

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



        protected override void OnElementChanged(ElementChangedEventArgs<XamFacebook> e)
        {
            if (Control == null)
            {
                //When this button is shown, it means nobody is logged in. Make sure that facebook is in the same state
                LoginManager.Instance.LogOut();

                var fbButton = new LoginButton(Context) { LoginBehavior = LoginBehavior.WebOnly };
                fbButton.SetTextSize(ComplexUnitType.Dip, e.NewElement.TextSize);

                int paddingTop = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, e.NewElement.PaddingTop, Context.Resources.DisplayMetrics);
                int paddingBottom = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, e.NewElement.PaddingBottom, Context.Resources.DisplayMetrics);
                fbButton.SetPadding(fbButton.PaddingLeft, paddingTop, fbButton.PaddingRight, paddingBottom);
                
                fbButton.SetPermissions(new string[] { "public_profile", "email" });
                fbButton.RegisterCallback(MainActivity.CallbackManager, this);

                SetNativeControl(fbButton);
            }
        }


        public void OnCancel() => XamButton.OnCancel?.Execute(null);

        public void OnError(FacebookException fbException) => XamButton.OnError?.Execute(fbException.Message);

        public void OnSuccess(Java.Lang.Object result) => XamButton.OnSuccess?.Execute(((LoginResult)result).AccessToken.Token);

    }
}