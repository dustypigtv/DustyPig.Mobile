using DustyPig.Mobile.MVVM;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DustyPig.Mobile.Services
{
    static class LogoutService
    {
        public static void Logout()
        {            
            App.API.Token = null;

            Settings.DeleteProfileToken();
            Settings.AutoPlayNext = false;
            Settings.AutoSkipIntro = false;
            Settings.DownloadOverCellular = false;

            Progress.ProgressService.Reset();

            Download.DownloadService.Reset();

            HomePageCache.Reset();

            Application.Current.MainPage = new StartupPage();
        }
    }
}
