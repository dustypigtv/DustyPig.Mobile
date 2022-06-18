using DustyPig.API.v3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.ParentalControls
{
    public class ParentalControlsForDetailsVeiwModel : ObservableObject
    {
        private readonly int _id;
        private readonly int _libraryId;
        private readonly INavigation _navigation;

        public ParentalControlsForDetailsVeiwModel(int id, int libraryId, INavigation navigation)
        {
            _id = id;
            _libraryId = libraryId;
            _navigation = navigation;
            IsBusy = true;
            SaveCommand = new AsyncCommand(OnSave, allowsMultipleExecutions: false);
            LoadPermissions();
        }

        public AsyncCommand SaveCommand { get; }
        private async Task OnSave()
        {
            IsBusy = true;

            var data = new TitleOverride { MediaEntryId = _id };
            foreach (var grp in Profiles)
            {
                foreach (var profile in grp.Where(item => item.HasLibraryAccess && item.CanWatch != item.OrigCanWatch))
                {
                    var overrideInfo = new ProfileTitleOverrideInfo
                    {
                        ProfileId = profile.Id,
                        NewState = OverrideState.Default
                    };

                    if (profile.CanWatch)
                    {
                        if (!profile.CanWatchByDefault)
                            overrideInfo.NewState = OverrideState.Allow;
                    }
                    else
                    {
                        if (profile.CanWatchByDefault)
                            overrideInfo.NewState = OverrideState.Block;
                    }


                    data.Overrides.Add(overrideInfo);
                }
            }

            var response = await App.API.Media.SetAccessOverrideAsync(data);
            if (response.Success)
            {
                await _navigation.PopModalAsync();
            }
            else
            {
                await Helpers.Alerts.ShowAlertAsync("Error", response.Error.Message);
                foreach (var grp in Profiles)
                    foreach (var profile in grp.Where(item => item.HasLibraryAccess))
                        profile.CanWatch = profile.OrigCanWatch;
            }

            IsBusy = false;

        }

        public ObservableRangeCollection<ParentalControlsGroupViewModel> Profiles { get; } = new ObservableRangeCollection<ParentalControlsGroupViewModel>();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public void OnSizeAllocated(double width, double height)
        {
            if (Device.Idiom == TargetIdiom.Phone)
            {
                Width = width;
            }
            else
            {
                double newWidth = width;
                switch (DeviceDisplay.MainDisplayInfo.Orientation)
                {
                    case DisplayOrientation.Landscape:
                        newWidth = width * 0.33;
                        break;

                    case DisplayOrientation.Portrait:
                        newWidth = height * 0.33;
                        break;
                }
                newWidth = Math.Max(newWidth, 400);

                Width = (int)Math.Min(width, newWidth);
            }
        }

        private async void LoadPermissions()
        {
            var permissionResponse = await App.API.Media.GetTitlePermissionsAsync(_id, default);
            if (permissionResponse.Success)
            {
                var grps = new List<ParentalControlsGroupViewModel>();

                if (permissionResponse.Data.Profiles.Any(item => item.HasLibraryAccess))
                {
                    var grp = new ParentalControlsGroupViewModel { Header = "Profiles" };
                    grps.Add(grp);
                    foreach (var profile in permissionResponse.Data.Profiles.Where(item => item.HasLibraryAccess))
                        grp.Add(new ParentalControlsProfileViewModel(profile, _id));
                }


                if (permissionResponse.Data.Profiles.Any(item => !item.HasLibraryAccess))
                {
                    var grp = new ParentalControlsGroupViewModel
                    {
                        Header = "Profiles without library access",
                        ShowIcon = true
                    };
                    grps.Add(grp);
                    foreach (var profile in permissionResponse.Data.Profiles.Where(item => !item.HasLibraryAccess))
                        grp.Add(new ParentalControlsProfileViewModel(profile, _id));

                    var libraryResponse = await App.API.Libraries.GetBasicAsync(_libraryId);
                    if (libraryResponse.Success)
                    {
                        if (string.IsNullOrWhiteSpace(libraryResponse.Data.Owner))
                            grp.Info = $"Give these profiles access to the '{libraryResponse.Data.Name}' library before setting parental controls on this title";
                        else
                            grp.Info = $"Give these profiles access to {libraryResponse.Data.Owner}'s '{libraryResponse.Data.Name}' library before setting parental controls on this title";
                    }
                    else
                    {
                        await Helpers.Alerts.ShowAlertAsync("Error", permissionResponse.Error.Message);
                    }
                }


                Profiles.AddRange(grps);

                IsBusy = false;
            }
            else
            {
                await Helpers.Alerts.ShowAlertAsync("Error", permissionResponse.Error.Message);
            }
        }


    }
}
