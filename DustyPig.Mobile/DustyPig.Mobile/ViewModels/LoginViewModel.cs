using DustyPig.API.v3.Models;
using DustyPig.Mobile.SocialLogin.FB;
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
        }

        public Command FacebookLoginCommand { get; }

        private async Task OnFacebookLoginCommand()
        {
            try
            {
                var fbtoken = await FacebookClient.Current.LoginAsync();

                //var client = new API.v3.Client();
                //var dpToken = await client.Auth.OAuthLoginAsync(new OAuthCredentials { Provider = OAuthCredentialProviders.Facebook, Token = fbtoken });
                await Shell.Current.DisplayAlert("Facebook Login", "Success!", "OK");
            }
            catch (OperationCanceledException ex)
            {
                await Shell.Current.DisplayAlert("Facebook Login", "Cancelled", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Facebook Login", ex.Message, "OK");
            }
        }

    }
}
