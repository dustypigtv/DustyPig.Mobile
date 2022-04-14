using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform.SocialLogin;
using DustyPig.Mobile.Views;
using DustyPig.REST;
using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.ViewModels
{
    public class LoginViewModel : _BaseViewModel
    {
        public LoginViewModel()
        {
            LoginButtonCommand = new AsyncCommand(OnLoginButtonCommand, allowsMultipleExecutions: false);

            AppleLoginCommand = new AsyncCommand(() => SocialProviderLogin(OAuthCredentialProviders.Apple, DependencyService.Get<IAppleLoginClient>()), allowsMultipleExecutions: false);
            GoogleLoginCommand = new AsyncCommand(() => SocialProviderLogin(OAuthCredentialProviders.Google, DependencyService.Get<IGoogleLoginClient>()), allowsMultipleExecutions: false);
            FacebookLoginCommand = new AsyncCommand(() => SocialProviderLogin(OAuthCredentialProviders.Facebook, DependencyService.Get<IFacebookLoginClient>()), allowsMultipleExecutions: false);
        }

        public bool ShowAppleButton => Device.RuntimePlatform == Device.iOS;

        public AsyncCommand SignupCommand { get; } = new AsyncCommand(() => Shell.Current.GoToAsync(nameof(SignupPage)), allowsMultipleExecutions: false);

        public AsyncCommand ForgotPasswordCommand { get; } = new AsyncCommand(() => Shell.Current.GoToAsync(nameof(ForgotPasswordPage)), allowsMultipleExecutions: false);


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


        public AsyncCommand LoginButtonCommand { get; }
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
                IsBusy = false;
                await Shell.Current.DisplayAlert(provider.ToString() + " Login", ex.Message, "OK");
                return;
            }


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
