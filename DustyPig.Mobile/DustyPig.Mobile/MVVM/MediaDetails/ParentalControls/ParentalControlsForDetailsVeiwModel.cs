using DustyPig.API.v3.Models;
using DustyPig.Mobile.Helpers;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.ParentalControls
{
    public class ParentalControlsForDetailsVeiwModel : _DetailsBaseViewModel
    {
        private readonly int _id;
        private readonly int _libraryId;
        private readonly INavigation _navigation;

        public ParentalControlsForDetailsVeiwModel(int id, int libraryId, INavigation navigation) : base(navigation)
        {
            _id = id;
            _libraryId = libraryId;
            _navigation = navigation;
            IsBusy = true;
            CancelCommand = new AsyncCommand(Navigation.PopModalAsync, allowsMultipleExecutions: false);
            SaveCommand = new AsyncCommand(OnSave, allowsMultipleExecutions: false);
            LoadData();
        }

        public AsyncCommand CancelCommand { get; }

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

            if (data.Overrides.Count > 0)
            {
                var response = await App.API.Media.SetAccessOverrideAsync(data);
                if (response.Success)
                {
                    await Navigation.PopModalAsync();
                }
                else
                {
                    if (await response.Error.HandleUnauthorizedException())
                        return;
                    await ShowAlertAsync("Error", response.Error.Message);
                    foreach (var grp in Profiles)
                        foreach (var profile in grp.Where(item => item.HasLibraryAccess))
                            profile.CanWatch = profile.OrigCanWatch;
                }
            }
            else
            {
                await Navigation.PopModalAsync();
            }

            IsBusy = false;

        }

        
        public ObservableRangeCollection<ParentalControlsGroupViewModel> Profiles { get; } = new ObservableRangeCollection<ParentalControlsGroupViewModel>();

        
        private async void LoadData()
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
                        await ShowAlertAsync("Error", permissionResponse.Error.Message);
                    }
                }


                Profiles.AddRange(grps);

                IsBusy = false;
            }
            else
            {
                if (await permissionResponse.Error.HandleUnauthorizedException())
                    return;
                await Helpers.Alerts.ShowAlertAsync("Error", permissionResponse.Error.Message);
                await Navigation.PopModalAsync();
            }
        }


    }
}
