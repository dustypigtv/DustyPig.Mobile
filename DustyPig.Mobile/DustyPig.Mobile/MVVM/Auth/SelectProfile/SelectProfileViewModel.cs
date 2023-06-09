﻿using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform.FCM;
using DustyPig.Mobile.Helpers;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth.SelectProfile
{
    public class SelectProfileViewModel : _BaseViewModel
    {
        private readonly Entry _pinEntry;

        public SelectProfileViewModel(Entry PinEntry, INavigation navigation) : base(navigation)
        {
            _pinEntry = PinEntry;

            SubmitPinCommand = new AsyncCommand(() => Login(_selectedProfile.Id, short.Parse(_pin)), canExecute: CanSubmit);

            CancelCommand = new Command(() =>
            {
                ShowPin = false;
            });
        }

        public ObservableRangeCollection<BasicProfile> Profiles { get; } = new ObservableRangeCollection<BasicProfile>();

        private int _characterSpacing = 0;
        public int CharacterSpacing
        {
            get => _characterSpacing;
            set => SetProperty(ref _characterSpacing, value);
        }

        private string _pin = string.Empty;
        public string Pin
        {
            get => _pin;
            set
            {
                SetProperty(ref _pin, value);
                CharacterSpacing = string.IsNullOrWhiteSpace(_pin) ? 0 : 16;
                SubmitPinCommand.ChangeCanExecute();
            }
        }

        private bool _showPin;
        public bool ShowPin
        {
            get => _showPin;
            set => SetProperty(ref _showPin, value);
        }

        private BasicProfile _selectedProfile;
        public BasicProfile SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty(ref _selectedProfile, value);
        }

        public AsyncCommand SubmitPinCommand { get; }

        public Command CancelCommand { get; }

        private bool CanSubmit()
        {
            if ((_pin + string.Empty).Length == 4)
                if (short.TryParse(_pin, out short p))
                    if (p > 999)
                        return true;

            return false;
        }

        public async void OnAppearing()
        {
            IsBusy = true;

            var response = await App.API.Profiles.ListAsync();
            if (response.Success)
            {
                Profiles.AddRange(response.Data);
                IsBusy = false;
                if (response.Data.Count == 1)
                {
                    SelectedProfile = response.Data[0];
                    ShowPin = true;
                    while (!_pinEntry.Focus())
                    {
                        await Task.Delay(100);
                    }
                }
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.Message);
                await Navigation.PopAsync();
            }
        }

        public async void OnItemTapped(BasicProfile bp)
        {
            if (bp.HasPin)
            {
                Pin = null;
                SelectedProfile = bp;
                ShowPin = true;
                while (!_pinEntry.Focus())
                {
                    await Task.Delay(100);
                }
            }
            else
            {
                await Login(bp.Id, null);
            }
        }

        private async Task Login(int id, short? pin)
        {
            IsBusy = true;

            //Reset token when logging in
            var fcm = DependencyService.Get<IFCM>();
            await fcm.ResetTokenAsync();

            var creds = new ProfileCredentials
            {
                Id = id,
                Pin = pin,
                DeviceToken = await fcm.GetTokenAsync()
            };

            var response = await App.API.Auth.ProfileLoginAsync(creds);
            if (response.Success)
            {
                App.API.Token = response.Data.Token;
                await Services.Settings.SaveProfileTokenAsync(response.Data.Token);
                Application.Current.MainPage = new StartupPage();
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.FormatMessage());
                ShowPin = false;
                IsBusy = false;
            }

        }
    }
}
