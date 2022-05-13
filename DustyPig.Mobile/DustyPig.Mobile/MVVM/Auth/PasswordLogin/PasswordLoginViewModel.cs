using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform.FCM;
using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.MVVM.Auth.ForgotPassword;
using DustyPig.Mobile.MVVM.Auth.Signup;
using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth.PasswordLogin
{
    public class PasswordLoginViewModel : _BaseLoginViewModel
    {
        public PasswordLoginViewModel(INavigation navigation) : base(navigation)
        {
            LoginButtonCommand = new AsyncCommand(OnLoginButtonCommand, canExecute: ValidateCredentialInput);
            SignupCommand = new AsyncCommand(() => Navigation.PushAsync(new NavigationPage(new SignupPage())));
            ForgotPasswordCommand = new AsyncCommand(() => Navigation.PushAsync(new NavigationPage(new ForgotPasswordPage())));
        }

        public AsyncCommand SignupCommand { get; }

        public AsyncCommand ForgotPasswordCommand { get; }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                LoginButtonCommand.ChangeCanExecute();
                if ((_email + string.Empty).ToLower().Trim() == API.v3.Clients.AuthClient.TEST_EMAIL)
                    Password = API.v3.Clients.AuthClient.TEST_PASSWORD;
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
                await ShowAlertAsync("Login", ex.FormatMessage());
            }

            IsBusy = false;
        }



    }
}
