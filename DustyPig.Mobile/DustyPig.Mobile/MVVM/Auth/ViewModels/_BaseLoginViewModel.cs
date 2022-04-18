using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.MVVM.Auth.Views;
using DustyPig.Mobile.Services;
using DustyPig.REST;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth.ViewModels
{
    public class _BaseLoginViewModel : _BaseViewModel
    {
        public async Task ValidateTokenAndGoToProfiles(Response<LoginResponse> dpToken)
        {
            dpToken.ThrowIfError();
            App.API.Token = dpToken.Data.Token;
            if (dpToken.Data.LoginType == LoginResponseType.Account)
            {
                await Shell.Current.GoToAsync(nameof(SelectProfilePage));
            }
            else
            {
                await Settings.SaveProfileTokenAsync(dpToken.Data.Token);
                Shell.Current.CurrentItem = new StartupPage();
            }
        }

        public Task ShowError(string title, string msg) => DependencyService.Get<IPopup>().AlertAsync(title, msg);

    }
}
