using DustyPig.API.v3.Models;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DustyPig.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
            FacebookSuccessCommand = new Command<string>(async (s) => await OnFacebookSuccess(s));
            FacebookErrorCommand = new Command<string>(async (s) => await OnFacebookError(s));
            LoginCancelledCommand = new Command(OnLoginCancelled);
        }

        public Command<string> FacebookSuccessCommand { get; }

        private async Task OnFacebookSuccess(string oAuthToken)
        {
            var client = new API.v3.Client();
            var ret = await client.Auth.OAuthLoginAsync(new OAuthCredentials { Provider = OAuthCredentialProviders.Facebook, Token = oAuthToken });
            
            
        }


        public Command<string> FacebookErrorCommand { get; }

        private async Task OnFacebookError(string s)
        {

        }


        public Command LoginCancelledCommand { get; }

        private void OnLoginCancelled()
        {

        }
       
    }
}
