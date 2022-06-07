using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    public class ParentalControlsProfileViewModel
    {
        private readonly ProfileTilePermissionInfo _info;
        private readonly int _titleId;

        public ParentalControlsProfileViewModel(ProfileTilePermissionInfo info, int titleId)
        {
            _info = info;
            _titleId = titleId;
            Id = _info.Id;
            Name = _info.Name;
            HasLibraryAccess = _info.HasLibraryAccess;

            CanWatch =
                (_info.CanWatchByDefault && _info.Override != OverrideState.Block)
                || (!_info.CanWatchByDefault && _info.Override == OverrideState.Allow);

        }

        public int Id { get; set; }

        public string Name { get; set; }

        private bool _canWatch;
        public bool CanWatch
        {
            get => _canWatch;
            set
            {
                if (_canWatch != value)
                {
                    _canWatch = value;
                    OnSetCanWatch();
                }
            }
        }

        public bool HasLibraryAccess { get; set; }

        private async void OnSetCanWatch()
        {
            //_canWatch is the NEW state

            var state = OverrideState.Default;
            if (_canWatch)
            {
                if (!_info.CanWatchByDefault)
                    state = OverrideState.Allow;
            }
            else
            {
                if (_info.CanWatchByDefault)
                    state = OverrideState.Block;
            }

            var response = await App.API.Media.SetAccessOverrideAsync(new TitleOverride
            {
                MediaEntryId = _titleId,
                ProfileId = Id,
                State = state
            });

            if (!response.Success)
            {
                string msg = response.Error.Message + string.Empty;
                if (msg.StartsWith("\"") && msg.EndsWith("\""))
                    msg = msg.Trim('"');

                await DependencyService.Get<IPopup>().AlertAsync("Error", msg);
            }
        }
    }
}
