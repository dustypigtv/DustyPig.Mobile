﻿using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Helpers;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth.ViewModels
{
    public class SelectProfileViewModel : _BaseViewModel
    {
        public RangeObservableCollection<BasicProfile> Profiles { get; } = new RangeObservableCollection<BasicProfile>();

        public async void OnAppearing()
        {
            IsBusy = true;

            var response = await App.API.Profiles.ListAsync();
            if (response.Success)
            {
                Profiles.AddRange(response.Data);
                IsBusy = false;
            }
            else
            {
                await DependencyService.Get<IPopup>().Alert("Error", response.Error.Message);
                await Shell.Current.GoToAsync("..");
            }
        }

        public async void OnItemTapped(BasicProfile bp)
        {
            var creds = new ProfileCredentials
            {
                Id = bp.Id,
                //DeviceToken = Plugin.FirebasePushNotification.CrossFirebasePushNotification.Current.Token
            };

            var response = await App.API.Auth.ProfileLoginAsync(creds);

        }
    }
}
