using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DustyPig.Mobile.MVVM.MediaDetails.Playlist
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RenamePlaylistPopup : PopupPage
    {
        readonly BasicMedia _basicMedia;
        readonly TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>();

        public RenamePlaylistPopup(BasicMedia basicMedia)
        {
            InitializeComponent();

            _basicMedia = basicMedia;

            CancelCommand = new AsyncCommand(OnCancel, allowsMultipleExecutions: false);
            SaveCommand = new AsyncCommand(OnSave, canExecute: CanSave, allowsMultipleExecutions: false);
            PlaylistTitle = _basicMedia.Title;

            BindingContext = this;
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

        private bool _isBusy2;
        public bool IsBusy2
        {
            get => _isBusy2;
            set
            {
                if (_isBusy2 != value)
                {
                    _isBusy2 = value;
                    OnPropertyChanged();
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Device.BeginInvokeOnMainThread(async () =>
            {
                while (!TB.Focus())
                {
                    await Task.Delay(100);
                }
                TB.CursorPosition = PlaylistTitle.Length;
            });
        }

        protected override bool OnBackButtonPressed() => true;

        public AsyncCommand CancelCommand { get; }
        private async Task OnCancel()
        {
            await PopupNavigation.Instance.PopAsync(true);
            _taskCompletionSource.SetResult(false);
        }

        public AsyncCommand SaveCommand { get; }
        private async Task OnSave()
        {
            if (PlaylistTitle == _basicMedia.Title)
            {
                await PopupNavigation.Instance.PopAsync(true);
                _taskCompletionSource.SetResult(false); 
                return;
            }

            IsBusy2 = true;

            var data = new UpdatePlaylist
            {
                Id = _basicMedia.Id,
                Name = PlaylistTitle
            };

            var response = await App.API.Playlists.UpdateAsync(data);
            if (response.Success)
            {
                _basicMedia.Title = PlaylistTitle;
                await PopupNavigation.Instance.PopAsync(true);
                _taskCompletionSource.SetResult(true);
            }
            else
            {
                await DependencyService.Get<IPopup>().AlertAsync("Error", response.Error.Message);
                IsBusy2 = false;
            }
        }

        public Task<bool> GetResultAsync() => _taskCompletionSource.Task;
    }
}