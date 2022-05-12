using DustyPig.Mobile.CrossPlatform;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth.ForgotPassword
{
    public class ForgotPasswordViewModel : _BaseViewModel
    {
        public ForgotPasswordViewModel()
        {
            SubmitCommand = new AsyncCommand(OnSubmitCommand, canExecute: CanSubmit);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                SubmitCommand.ChangeCanExecute();
            }
        }

        private bool CanSubmit() => !string.IsNullOrWhiteSpace(Email);


        public AsyncCommand SubmitCommand { get; }
        private async Task OnSubmitCommand()
        {
            IsBusy = true;

            var popup = DependencyService.Get<IPopup>();

            var response = await App.API.Auth.SendPasswordResetEmailAsync(Email);
            if (response.Success)
            {
                await popup.AlertAsync("Success", "Please check your email for password reset instructions");
                await Navigation.PopAsync();
            }
            else
            {
                await popup.AlertAsync("Error", response.Error.Message);
            }


            IsBusy = false;
        }
    }
}
