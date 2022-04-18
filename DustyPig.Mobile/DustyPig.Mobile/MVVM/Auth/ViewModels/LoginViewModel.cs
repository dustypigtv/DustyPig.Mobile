using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.CrossPlatform.FCM;
using DustyPig.Mobile.CrossPlatform.SocialLogin;
using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.MVVM.Auth.Views;
using DustyPig.Mobile.Views;
using DustyPig.REST;
using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth.ViewModels
{
    public class LoginViewModel : _BaseViewModel
    {
        public LoginViewModel()
        {
            LoginButtonCommand = new AsyncCommand(OnLoginButtonCommand, canExecute: ValidateCredentialInput, allowsMultipleExecutions: false);

            AppleLoginCommand = new AsyncCommand(() => SocialProviderLogin(OAuthCredentialProviders.Apple, DependencyService.Get<IAppleLoginClient>()), allowsMultipleExecutions: false);
            GoogleLoginCommand = new AsyncCommand(() => SocialProviderLogin(OAuthCredentialProviders.Google, DependencyService.Get<IGoogleLoginClient>()), allowsMultipleExecutions: false);
            FacebookLoginCommand = new AsyncCommand(() => SocialProviderLogin(OAuthCredentialProviders.Facebook, DependencyService.Get<IFacebookLoginClient>()), allowsMultipleExecutions: false);
        }

        public bool ShowAppleButton => Device.RuntimePlatform == Device.iOS;

        public AsyncCommand SignupCommand { get; } = new AsyncCommand(() => Shell.Current.GoToAsync(nameof(SignupPage)), allowsMultipleExecutions: false);

        public AsyncCommand ForgotPasswordCommand { get; } = new AsyncCommand(() => Shell.Current.GoToAsync(nameof(ForgotPasswordPage)), allowsMultipleExecutions: false);

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
            if (string.IsNullOrWhiteSpace(_email))
                return false;

            if (string.IsNullOrWhiteSpace(_password))
                return false;

            return true;
        }


        public AsyncCommand LoginButtonCommand { get; }
        private async Task OnLoginButtonCommand()
        {
            IsBusy = true;

            try
            {
                //Reset token when logging in
                var fcm = DependencyService.Get<IFCM>();
                await fcm.ResetTokenAsync();

                var dpToken = await App.API.Auth.PasswordLoginAsync(new PasswordCredentials
                {
                    Email = _email,
                    Password = _password,
                    DeviceToken = await DependencyService.Get<IFCM>().GetTokenAsync()
                });
                await ValidateTokenAndGoToProfiles(dpToken);
            }
            catch (Exception ex)
            {
               await ShowError("Login", ex.FormatMessage());
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
                await ShowError(provider.ToString() + " Login", ex.Message);
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
                await ShowError("Dusty Pig Error", ex.FormatMessage());
            }

            IsBusy = false;
        }


        private async Task ValidateTokenAndGoToProfiles(Response<LoginResponse> dpToken)
        {
            dpToken.ThrowIfError();
            App.API.Token = dpToken.Data.Token;
            if (dpToken.Data.LoginType == LoginResponseType.Account)
                await Shell.Current.GoToAsync(nameof(SelectProfilePage));
            else
                await Shell.Current.GoToAsync(nameof(StartupPage));
        }

        private Task ShowError(string title, string msg) => DependencyService.Get<IPopup>().AlertAsync(title, msg);
        
    }
}
