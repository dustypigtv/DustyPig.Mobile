using System;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;

namespace DustyPig.Mobile.SocialLogin.FB
{
    public class LoginCallback : Java.Lang.Object, IFacebookCallback
    {
        public Action CancelAction { get; set; }

        public Action<FacebookException> ErrorAction { get; set; }

        public Action<LoginResult> SuccessAction { get; set; }

        public void OnCancel() => CancelAction?.Invoke();

        public void OnError(FacebookException error) => ErrorAction?.Invoke(error);

        public void OnSuccess(Java.Lang.Object result) => SuccessAction.Invoke(result as LoginResult);
    }
}