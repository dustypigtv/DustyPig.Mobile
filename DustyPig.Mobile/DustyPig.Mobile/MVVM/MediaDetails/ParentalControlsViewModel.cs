using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    public class ParentalControlsViewModel : _BaseViewModel
    {
        private readonly int _id;
        private readonly int _libraryId;

        public ParentalControlsViewModel(INavigation navigation, int id, int libraryId, Action dismiss) : base(navigation)
        {
            _id = id;
            _libraryId = libraryId;
            IsBusy = true;
            CloseCommand = new Command(dismiss);
            LoadPermissions();
        }

        public Command CloseCommand { get; }

        public ObservableCollection<ParentalControlsGroupViewModel> Profiles { get; } = new ObservableCollection<ParentalControlsGroupViewModel>();

        private bool _showNoLibraryAccessLabel;
        public bool ShowNoLibraryAccessLabel
        {
            get => _showNoLibraryAccessLabel;
            set => SetProperty(ref _showNoLibraryAccessLabel, value);
        }

        private string _noLibraryAccessText;
        public string NoLibraryAccessText
        {
            get => _noLibraryAccessText;
            set => SetProperty(ref _noLibraryAccessText, value);
        }

        private async void LoadPermissions()
        {
            var permissionResponse = await App.API.Media.GetTitlePermissionsAsync(_id, default);
            if (permissionResponse.Success)
            {
                permissionResponse.Data.Profiles[0].CanWatchByDefault = true;
                if (permissionResponse.Data.Profiles.Any(item => item.HasLibraryAccess))
                {
                    var grp = new ParentalControlsGroupViewModel { Header = "Profiles" };
                    foreach (var profile in permissionResponse.Data.Profiles.Where(item => item.HasLibraryAccess))
                        grp.Add(new ParentalControlsProfileViewModel(profile, _id));
                    Profiles.Add(grp);
                }

                if (permissionResponse.Data.Profiles.Any(item => !item.HasLibraryAccess))
                {
                    var grp = new ParentalControlsGroupViewModel { Header = "Profiles without library access *" };
                    foreach (var profile in permissionResponse.Data.Profiles.Where(item => !item.HasLibraryAccess))
                        grp.Add(new ParentalControlsProfileViewModel(profile, _id));
                    Profiles.Add(grp);

                    var libraryResponse = await App.API.Libraries.GetBasicAsync(_libraryId);
                    if (libraryResponse.Success)
                    {
                        if (string.IsNullOrWhiteSpace(libraryResponse.Data.Owner))
                            NoLibraryAccessText = $"* Give these profiles access to the '{libraryResponse.Data.Name}' library before setting parental controls on this title";
                        else
                        {
                            string possessive = libraryResponse.Data.Owner;
                            if (possessive.ICEndsWith("s"))
                                possessive += "'";
                            else
                                possessive += "'s";                                   
                            NoLibraryAccessText = $"* Give these profiles access to {possessive} '{libraryResponse.Data.Name}' library before setting parental controls on this title";
                        }
                    }
                    else
                    {
                        await ShowAlertAsync("Error", permissionResponse.Error.Message);
                        CloseCommand.Execute(null);
                    }
                    ShowNoLibraryAccessLabel = true;
                }

                IsBusy = false;
            }
            else
            {
                await ShowAlertAsync("Error", permissionResponse.Error.Message);
                CloseCommand.Execute(null);
            }
        }
    }
}
