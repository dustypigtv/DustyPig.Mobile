using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.CrossPlatform.FCM;
using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.MVVM.Auth.SelectProfile;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth.Signup
{
    public class SignupViewModel : _BaseViewModel
    {
        public SignupViewModel()
        {
            SignupButtonCommand = new AsyncCommand(OnSignupButtonCommand, canExecute: ValidateCredentialInput);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                SignupButtonCommand.ChangeCanExecute();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                SignupButtonCommand.ChangeCanExecute();
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


        public AsyncCommand SignupButtonCommand { get; }
        private async Task OnSignupButtonCommand()
        {
            IsBusy = true;
            var popup = DependencyService.Get<IPopup>();

            var ret = await App.API.Account.CreateAsync(new CreateAccount
            {
                DisplayName = _name,
                Email = _email,
                Password = _password,
                DeviceToken = await DependencyService.Get<IFCM>().GetTokenAsync()
            });

            if (ret.Success)
            {
                if (ret.Data.EmailVerificationRequired)
                {
                    await popup.AlertAsync("Success!", "Please check your email to verify your account");
                    await Navigation.PopAsync();
                }
                else
                {
                    App.API.Token = ret.Data.Token;
                    if (ret.Data.LoginType == LoginResponseType.Account)
                        await Navigation.PushAsync(new SelectProfilePage());
                    else
                        Application.Current.MainPage = new StartupPage();
                }
            }
            else
            {
                await popup.AlertAsync("Error", ret.Error.FormatMessage());
            }

            IsBusy = false;
        }
    }
}
