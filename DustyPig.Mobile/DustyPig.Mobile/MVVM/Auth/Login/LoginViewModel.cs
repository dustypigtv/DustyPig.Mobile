using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform.FCM;
using DustyPig.Mobile.CrossPlatform.SocialLogin;
using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.MVVM.Auth.PasswordLogin;
using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth.Login
{
    public class LoginViewModel : _BaseLoginViewModel
    {
        public LoginViewModel(INavigation navigation) : base(navigation)
        {
            AppleLoginCommand = new AsyncCommand(() => SocialProviderLogin(OAuthCredentialProviders.Apple, DependencyService.Get<IAppleLoginClient>()));
            GoogleLoginCommand = new AsyncCommand(() => SocialProviderLogin(OAuthCredentialProviders.Google, DependencyService.Get<IGoogleLoginClient>()));
            FacebookLoginCommand = new AsyncCommand(() => SocialProviderLogin(OAuthCredentialProviders.Facebook, DependencyService.Get<IFacebookLoginClient>()));
            PasswordLoginCommand = new AsyncCommand(() => Navigation.PushAsync(new NavigationPage(new PasswordLoginPage())));
        }

        public bool ShowAppleButton => Device.RuntimePlatform == Device.iOS;

        public AsyncCommand PasswordLoginCommand { get; }

        public AsyncCommand AppleLoginCommand { get; }

        public AsyncCommand GoogleLoginCommand { get; }

        public AsyncCommand FacebookLoginCommand { get; }


        private async Task SocialProviderLogin(OAuthCredentialProviders provider, ISocialLoginClient client)
        {
            string token;
            try
            {
                IsBusy = true;
                token = await client.LoginAsync();
                if (string.IsNullOrEmpty(token))
                    throw new Exception("Could not get sign in token from provider");
            }
            catch (OperationCanceledException)
            {
                IsBusy = false;
                return;
            }
            catch (Exception ex)
            {
                await ShowAlertAsync(provider.ToString() + " Login", ex.Message);
                IsBusy = false;
                return;
            }


            try
            {
                //Reset token when logging in
                var fcm = DependencyService.Get<IFCM>();
                await fcm.ResetTokenAsync();

                var dpToken = await App.API.Auth.OAuthLoginAsync(new OAuthCredentials
                {
                    Provider = provider,
                    Token = token,
                    DeviceToken = await DependencyService.Get<IFCM>().GetTokenAsync()
                });
                await ValidateTokenAndGoToProfiles(dpToken);
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Dusty Pig Error", ex.FormatMessage());
            }

            IsBusy = false;
        }

    }
}
