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
            LoginButtonCommand = new Command(async () => await OnLoginButtonCommand(), ValidateCredentialInput);
            SignupCommand = new Command(async () => await OnSignupCommand());
            ForgotPasswordCommand = new Command(async () => await OnForgotPasswordCommand());
            FacebookLoginCommand = new Command(async () => await OnFacebookLoginCommand());
            GoogleLoginCommand = new Command(async () => await OnGoogleLoginCommand());
        }



        private bool _showSpinner;
        public bool ShowSpinner
        {
            get => _showSpinner;
            set => SetProperty(ref _showSpinner, value);
        }


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
            ShowSpinner = true;
            
            try
            {
                var ret = await App.API.Auth.PasswordLoginAsync(new PasswordCredentials
                {
                    Email = _email,
                    Password = _password
                });


            }
            catch (Exception ex)
            {
                ShowSpinner = false;
                ShowError = true;
            }

        }

        public Command SignupCommand { get; }
        private async Task OnSignupCommand()
        {
            throw new NotImplementedException();
        }


        public Command ForgotPasswordCommand { get; }
        private async Task OnForgotPasswordCommand()
        {
            throw new NotImplementedException();
        }



        public Command FacebookLoginCommand { get; }
        private async Task OnFacebookLoginCommand()
        {
            try
            {
                ShowSpinner = true;
                var token = await DependencyService.Get<IFacebookLoginClient>().LoginAsync();
                await OAuthLogin(OAuthCredentialProviders.Facebook, token);     
            }
            catch (OperationCanceledException)
            {
                ShowSpinner = false;
                //await Shell.Current.DisplayAlert("Facebook Login", "Cancelled", "OK");
            }
            catch (Exception ex)
            {
                ShowSpinner = false;
                await Shell.Current.DisplayAlert("Facebook Login", ex.Message, "OK");
            }
        }


        public Command GoogleLoginCommand { get; }
        private async Task OnGoogleLoginCommand()
        {
            try
            {
                ShowSpinner = true;
                var token = await DependencyService.Get<IGoogleLoginClient>().LoginAsync();
                await OAuthLogin(OAuthCredentialProviders.Google, token);
            }
            catch (OperationCanceledException)
            {
                ShowSpinner = false;
                //await Shell.Current.DisplayAlert("Google Login", "Cancelled", "OK");
            }
            catch (Exception ex)
            {
                ShowSpinner = false;
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

            ShowSpinner = false;
        }


        
    }
}
