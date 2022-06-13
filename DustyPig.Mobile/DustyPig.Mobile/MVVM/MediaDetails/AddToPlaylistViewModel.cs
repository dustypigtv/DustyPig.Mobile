using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.MVVM.Main.Home;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    public class AddToPlaylistViewModel : _BaseViewModel
    {
        private readonly BasicMedia _basicMedia;

        public AddToPlaylistViewModel(INavigation navigation, BasicMedia basicMedia, Action dismiss) : base(navigation)
        {
            _basicMedia = basicMedia;

            _newPlaylistText = _basicMedia.Title;
            if (_basicMedia.MediaType == MediaTypes.Movie)
                _newPlaylistText = _newPlaylistText.Substring(0, _newPlaylistText.Length - 7);

            CancelCommand = new Command(dismiss);
            NewPlaylistCommand = new AsyncCommand(OnNewPlaylist, canExecute: CanTapNewPlaylist, allowsMultipleExecutions: false);
            PlaylistTappedCommand = new AsyncCommand<int>(OnPlaylistTapped, allowsMultipleExecutions: false);

            IsBusy = true;
            LoadPlaylists();            
        }

        public Command CancelCommand { get; }


        private bool CanTapNewPlaylist() => !string.IsNullOrWhiteSpace(_newPlaylistText);

        private string _newPlaylistText;
        public string NewPlaylistText
        {
            get => _newPlaylistText;
            set
            {
                SetProperty(ref _newPlaylistText, value);
                NewPlaylistCommand.ChangeCanExecute();
            }
        }



        public AsyncCommand NewPlaylistCommand { get; }
        private async Task OnNewPlaylist()
        {
            IsBusy = true;

            var playlist = new CreatePlaylist { Name = NewPlaylistText };

            var createResponse = await App.API.Playlists.CreateAsync(playlist);
            if (!createResponse.Success)
            {
                await ShowAlertAsync("Error", createResponse.Error.FormatMessage());
                IsBusy = false;
                return;
            }



            var addResponse = await App.API.Playlists.AddItemAsync(createResponse.Data, _basicMedia.Id);
            if (!addResponse.Success)
            {
                await ShowAlertAsync("Error", addResponse.Error.FormatMessage());
                IsBusy = false;
                return;
            }

            HomeViewModel.InvokeAddedToPlaylists(new BasicMedia
            {
                ArtworkUrl = _basicMedia.ArtworkUrl,
                Id = createResponse.Data,
                MediaType = MediaTypes.Playlist,
                Title = playlist.Name
            });

            CancelCommand.Execute(null);
        }



        public AsyncCommand<int> PlaylistTappedCommand { get; }
        private async Task OnPlaylistTapped(int id)
        {
            IsBusy = true;

            try
            {
                var playlistDetailsResponse = await App.API.Playlists.GetDetailsAsync(id);
                playlistDetailsResponse.ThrowIfError();

                if (_basicMedia.MediaType == MediaTypes.Movie)
                {
                    if (playlistDetailsResponse.Data.Items.Any(item => item.MediaId == _basicMedia.Id))
                    {
                        var ret = await DependencyService.Get<IPopup>().YesNoAsync("Duplicate", $"The movie '{_basicMedia.Title}' already exists in the playlist '{playlistDetailsResponse.Data.Name}'. Do you want to add it again?");
                        if(!ret)
                        {
                            IsBusy = false;
                            return;
                        }
                    }

                    var addResponse = await App.API.Playlists.AddItemAsync(id, _basicMedia.Id);
                    addResponse.ThrowIfError();
                }
                else
                {
                    var seriesDetailsResponse = await App.API.Series.GetDetailsAsync(_basicMedia.Id);
                    seriesDetailsResponse.ThrowIfError();

                    if (playlistDetailsResponse.Data.Items.Any(item => seriesDetailsResponse.Data.Episodes.Select(ep => ep.Id).Contains(item.MediaId)))
                    {
                        var ret = await DependencyService.Get<IPopup>().YesNoAsync("Duplicate", $"Episodes from the series '{_basicMedia.Title}' already exists in the playlist '{playlistDetailsResponse.Data.Name}'. Do you want to add them again?");
                        if (!ret)
                        {
                            IsBusy = false;
                            return;
                        }
                    }

                    var addResponse = await App.API.Playlists.AddSeriesAsync(id, _basicMedia.Id);
                    addResponse.ThrowIfError();
                }

                playlistDetailsResponse = await App.API.Playlists.GetDetailsAsync(id);
                playlistDetailsResponse.ThrowIfError();

                HomeViewModel.InvokePlaylistArtworkUpdated(new BasicMedia
                {
                    Id = id,
                    ArtworkUrl = playlistDetailsResponse.Data.ArtworkUrl1,
                    ArtworkUrl2 = playlistDetailsResponse.Data.ArtworkUrl2,
                    ArtworkUrl3 = playlistDetailsResponse.Data.ArtworkUrl3,
                    ArtworkUrl4 = playlistDetailsResponse.Data.ArtworkUrl4,
                    MediaType = MediaTypes.Playlist,
                    Title = playlistDetailsResponse.Data.Name
                });

                CancelCommand.Execute(null);
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", ex.Message);
            }

            IsBusy = false;
        }

        

        private bool _showExistingPlaylists;
        public bool ShowExistingPlaylists
        {
            get => _showExistingPlaylists;
            set => SetProperty(ref _showExistingPlaylists, value);
        }

        public ObservableRangeCollection<BasicPlaylist> Playlists { get; } = new ObservableRangeCollection<BasicPlaylist>();

        private async void LoadPlaylists()
        {
            var response = await App.API.Playlists.ListAsync();
            if (response.Success)
            {
                Playlists.ReplaceRange(response.Data);
                ShowExistingPlaylists = response.Data.Count > 0;
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.FormatMessage());
            }

            IsBusy = false;
        }
    }
}
