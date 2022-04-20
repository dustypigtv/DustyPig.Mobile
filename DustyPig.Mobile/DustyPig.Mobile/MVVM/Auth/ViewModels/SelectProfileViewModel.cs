using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.CrossPlatform.FCM;
using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.Services;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Auth.ViewModels
{
    public class SelectProfileViewModel : _BaseViewModel
    {
        private readonly Entry _pinEntry;

        public SelectProfileViewModel(Entry PinEntry)
        {
            _pinEntry = PinEntry;

            SubmitPinCommand = new AsyncCommand((() => Login(_selectedProfile.Id, short.Parse(_pin))), canExecute: CanSubmit);

            CancelCommand = new Command(() =>
            {
                ShowPin = false;
            });
        }

        public RangeObservableCollection<BasicProfile> Profiles { get; } = new RangeObservableCollection<BasicProfile>();

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
            }
            else
            {
                await DependencyService.Get<IPopup>().AlertAsync("Error", response.Error.Message);
                await Shell.Current.GoToAsync("..");
            }
        }

        public async void OnItemTapped(BasicProfile bp)
        {
            if (bp.HasPin)
            {
                Pin = null;
                SelectedProfile = bp;
                ShowPin = true;
                while(!_pinEntry.Focus())
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
                Shell.Current.CurrentItem = new StartupPage();
            }
            else
            {
                await DependencyService.Get<IPopup>().AlertAsync("Error", response.Error.FormatMessage());
                ShowPin = false;
                IsBusy = false;
            }

        }
    }
}
