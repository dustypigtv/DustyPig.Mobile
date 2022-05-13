using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth.ForgotPassword
{
    public class ForgotPasswordViewModel : _BaseViewModel
    {
        public ForgotPasswordViewModel(INavigation navigation) : base(navigation)
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

            var response = await App.API.Auth.SendPasswordResetEmailAsync(Email);
            if (response.Success)
            {
                await ShowAlertAsync("Success", "Please check your email for password reset instructions");
                await Navigation.PopAsync();
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.Message);
            }


            IsBusy = false;
        }
    }
}
