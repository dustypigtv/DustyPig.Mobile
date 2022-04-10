using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform.SocialLogin;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DustyPig.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
            FacebookLoginCommand = new Command(async () => await OnFacebookLoginCommand());
            GoogleLoginCommand = new Command(async () => await OnGoogleLoginCommand());
        }

        public Command FacebookLoginCommand { get; }

        private async Task OnFacebookLoginCommand()
        {
            try
            {
                var token = await FacebookClient.Current.LoginAsync();
                await OAuthLogin(OAuthCredentialProviders.Facebook, token);     
            }
            catch (OperationCanceledException)
            {
                await Shell.Current.DisplayAlert("Facebook Login", "Cancelled", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Facebook Login", ex.Message, "OK");
            }
        }


        public Command GoogleLoginCommand { get; }

        private async Task OnGoogleLoginCommand()
        {
            try
            {
                var token = await GoogleClient.Current.LoginAsync();
                await OAuthLogin(OAuthCredentialProviders.Google, token);
            }
            catch (OperationCanceledException)
            {
                await Shell.Current.DisplayAlert("Google Login", "Cancelled", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Google Login", ex.Message, "OK");
            }
        }


        private async Task OAuthLogin(OAuthCredentialProviders provider, string token)
        {
            try
            {
                var dpToken = await App.API.Auth.OAuthLoginAsync(new OAuthCredentials { Provider = provider, Token = token });
                dpToken.ThrowIfError();
                await Shell.Current.DisplayAlert("Login", "Success!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

    }
}
