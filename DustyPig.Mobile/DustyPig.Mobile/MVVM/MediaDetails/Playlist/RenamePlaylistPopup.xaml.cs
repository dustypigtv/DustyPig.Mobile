using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Playlist
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RenamePlaylistPopup : Popup
    {
        readonly BasicMedia _basicMedia;

        public RenamePlaylistPopup(BasicMedia basicMedia)
        {
            InitializeComponent();

            _basicMedia = basicMedia;

            CancelCommand = new Command(() => Dismiss(null));
            SaveCommand = new AsyncCommand(OnSave, canExecute: CanSave, allowsMultipleExecutions: false);
            PlaylistTitle = _basicMedia.Title;

            BindingContext = this;

            Device.BeginInvokeOnMainThread(async () =>
            {
                while (!TB.Focus())
                {
                    await Task.Delay(100);
                }
                TB.CursorPosition = PlaylistTitle.Length;
            });
        }

        private bool CanSave() => !string.IsNullOrWhiteSpace(PlaylistTitle);

        private string _playlistTitle;
        public string PlaylistTitle
        {
            get => _playlistTitle;
            set
            {
                if (_playlistTitle != value)
                {
                    _playlistTitle = value;
                    OnPropertyChanged(nameof(PlaylistTitle));
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        public Command CancelCommand { get; }


        public AsyncCommand SaveCommand { get; }
        private async Task OnSave()
        {
            if (PlaylistTitle == _basicMedia.Title)
            {
                Dismiss(null);
                return;
            }

            IsBusy = true;

            var data = new UpdatePlaylist
            {
                Id = _basicMedia.Id,
                Name = PlaylistTitle
            };

            var response = await App.API.Playlists.UpdateAsync(data);
            if (response.Success)
            {
                _basicMedia.Title = PlaylistTitle;
                Dismiss(null);
            }
            else
            {
                await DependencyService.Get<IPopup>().AlertAsync("Error", response.Error.Message);
                IsBusy = false;
            }
        }
    }
}