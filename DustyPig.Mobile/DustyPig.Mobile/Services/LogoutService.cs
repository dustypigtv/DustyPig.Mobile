using DustyPig.Mobile.MVVM;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DustyPig.Mobile.Services
{
    static class LogoutService
    {
        public static async Task LogoutAsync()
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAllAsync();
            await App.Current.MainPage.Navigation.PopToRootAsync();
            await App.API.Auth.SignoutAsync();

            SetGlobalProprties();

            Application.Current.MainPage = new StartupPage();
        }

        /// <summary>
        /// This should only be called from StartupPage or inside of LogoutAsync
        /// </summary>
        public static void SetGlobalProprties()
        {
            App.LoggedIn = false;

            App.API.Token = null;

            Settings.DeleteProfileToken();
            Settings.AutoPlayNext = false;
            Settings.AutoSkipIntro = false;
            Settings.DownloadOverCellular = false;

            Progress.ProgressService.Reset();

            Download.DownloadService.Reset();

            HomePageCache.Reset();
        }
    }
}
