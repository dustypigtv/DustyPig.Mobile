using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform.SocialLogin;
using DustyPig.Mobile.Views;
using DustyPig.REST;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DustyPig.Mobile.ViewModels
{
    public class LoginViewModel : _BaseViewModel
    {
        public LoginViewModel()
        {
            LoginButtonCommand = new Command(async () => await OnLoginButtonCommand(), ValidateCredentialInput);
            SignupCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(SignupPage)));
            ForgotPasswordCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(ForgotPasswordPage)));
            FacebookLoginCommand = new Command(async () => await OnFacebookLoginCommand());
            GoogleLoginCommand = new Command(async () => await OnGoogleLoginCommand());
        }

        public bool ShowAppleButton => Device.RuntimePlatform == Device.iOS;

        

        private bool _showError;
        public bool ShowError
        {
            get => _showError;
            set => SetProperty(ref _showError, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                LoginButtonCommand.ChangeCanExecute();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                LoginButtonCommand.ChangeCanExecute();
            }
        }

        bool ValidateCredentialInput()
        {
            if (string.IsNullOrEmpty(_email))
                return false;

            if (string.IsNullOrEmpty(_password))
                return false;

            return true;
        }


        public Command LoginButtonCommand { get; }
        private async Task OnLoginButtonCommand()
        {
            ShowError = false;
            IsBusy = true;

            try
            {
                var dpToken = await App.API.Auth.PasswordLoginAsync(new PasswordCredentials
                {
                    Email = _email,
                    Password = _password
                });
                await ValidateTokenAndGoToProfiles(dpToken);
            }
            catch
            {
                ShowError = true;
            }

            IsBusy = false;
        }



        public Command SignupCommand { get; }

        public Command ForgotPasswordCommand { get; }



        public Command FacebookLoginCommand { get; }
        private async Task OnFacebookLoginCommand()
        {
            try
            {
                IsBusy = true;
                var token = await DependencyService.Get<IFacebookLoginClient>().LoginAsync();
                await OAuthLogin(OAuthCredentialProviders.Facebook, token);
            }
            catch (OperationCanceledException)
            {
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await Shell.Current.DisplayAlert("Facebook Login", ex.Message, "OK");
            }
        }


        public Command GoogleLoginCommand { get; }
        private async Task OnGoogleLoginCommand()
        {
            try
            {
                IsBusy = true;
                var token = await DependencyService.Get<IGoogleLoginClient>().LoginAsync();
                await OAuthLogin(OAuthCredentialProviders.Google, token);
            }
            catch (OperationCanceledException)
            {
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await Shell.Current.DisplayAlert("Google Login", ex.Message, "OK");
            }
        }



        private async Task OAuthLogin(OAuthCredentialProviders provider, string token)
        {
            try
            {
                var dpToken = await App.API.Auth.OAuthLoginAsync(new OAuthCredentials { Provider = provider, Token = token });
                await ValidateTokenAndGoToProfiles(dpToken);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Dusty Pig Error", ex.Message, "OK");
            }

            IsBusy = false;
        }


        private async Task ValidateTokenAndGoToProfiles(Response<string> dpToken)
        {
            dpToken.ThrowIfError();
            App.API.Token = dpToken.Data;
            await Shell.Current.GoToAsync(nameof(SelectProfilePage));
        }
    }
}
