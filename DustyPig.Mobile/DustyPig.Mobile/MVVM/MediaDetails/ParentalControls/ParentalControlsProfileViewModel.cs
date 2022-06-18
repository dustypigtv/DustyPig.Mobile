using DustyPig.API.v3.Models;
using Xamarin.CommunityToolkit.ObjectModel;

namespace DustyPig.Mobile.MVVM.MediaDetails.ParentalControls
{
    public class ParentalControlsProfileViewModel : ObservableObject
    {
        private readonly ProfileTitlePermissionInfo _info;
        private readonly int _titleId;

        public ParentalControlsProfileViewModel(ProfileTitlePermissionInfo info, int titleId)
        {
            _info = info;
            _titleId = titleId;
            Id = _info.Id;
            Name = _info.Name;
            HasLibraryAccess = _info.HasLibraryAccess;
            CanWatchByDefault = _info.CanWatchByDefault;

            CanWatch =
                (_info.CanWatchByDefault && _info.Override != OverrideState.Block)
                || (!_info.CanWatchByDefault && _info.Override == OverrideState.Allow);

            OrigCanWatch = CanWatch;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public bool CanWatchByDefault { get; set; }

        public bool OrigCanWatch { get; set; }

        private bool _canWatch;
        public bool CanWatch
        {
            get => _canWatch;
            set => SetProperty(ref _canWatch, value);
        }

        public bool HasLibraryAccess { get; set; }

    }
}
