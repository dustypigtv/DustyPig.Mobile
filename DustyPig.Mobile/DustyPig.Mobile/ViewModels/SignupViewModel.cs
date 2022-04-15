using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Views;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.ViewModels
{
    public class SignupViewModel : _BaseViewModel
    {
        public SignupViewModel()
        {
            SignupButtonCommand = new AsyncCommand(OnSignupButtonCommand, canExecute: ValidateCredentialInput, allowsMultipleExecutions: false);
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
                Password = _password
            });

            if (ret.Success)
            {
                if (ret.Data.EmailVerificationRequired)
                {
                    await popup.Alert("Success!", "Please check your email to verify your account");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    App.API.Token = ret.Data.Token;
                    await Shell.Current.GoToAsync(nameof(StartupPage));
                }
            }
            else
            {
                await popup.Alert("Error", ret.Error.FormatMessage());
            }

            IsBusy = false;
        }
    }
}
