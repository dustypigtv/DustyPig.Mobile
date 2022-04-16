using DustyPig.Mobile.CrossPlatform;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.ViewModels
{
    public class ForgotPasswordViewModel : _BaseViewModel
    {
        public ForgotPasswordViewModel()
        {
            SubmitCommand = new AsyncCommand(OnSubmitCommand, canExecute: CanSubmit, allowsMultipleExecutions: false);
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
            if(response.Success)
            {
                await popup.Alert("Success", "Please check your email for password reset instructions");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await popup.Alert("Error", response.Error.Message);
            }


            IsBusy = false;
        }
    }
}
