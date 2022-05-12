﻿using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.MVVM.Auth.SelectProfile;
using DustyPig.REST;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth
{
    public class _BaseLoginViewModel : _BaseViewModel
    {
        public async Task ValidateTokenAndGoToProfiles(Response<LoginResponse> dpToken)
        {
            dpToken.ThrowIfError();
            App.API.Token = dpToken.Data.Token;
            if (dpToken.Data.LoginType == LoginResponseType.Account)
            {
                //await Shell.Current.GoToAsync(nameof(SelectProfilePage));
                await Navigation.PushAsync(new SelectProfilePage());
            }
            else
            {
                await Services.Settings.SaveProfileTokenAsync(dpToken.Data.Token);
                //Shell.Current.CurrentItem = new StartupPage();
                Application.Current.MainPage = new StartupPage();
            }
        }

        public Task ShowError(string title, string msg) => DependencyService.Get<IPopup>().AlertAsync(title, msg);

    }
}