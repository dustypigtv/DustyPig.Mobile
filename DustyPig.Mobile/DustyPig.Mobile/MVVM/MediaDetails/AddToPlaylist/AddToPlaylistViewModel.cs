using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.MVVM.Main.Home;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.AddToPlaylist
{
    public class AddToPlaylistViewModel : ObservableObject
    {
        private BasicMedia _basicMedia;
        private INavigation _navigation;

        public AddToPlaylistViewModel(BasicMedia basicMedia, INavigation navigation)
        {
            _basicMedia = basicMedia;
            _navigation = navigation;
            IsBusy = true;

            NewPlaylistCommand = new AsyncCommand(OnNewPlaylist, CanTapNewPlaylist, allowsMultipleExecutions: false);
            PlaylistTappedCommand = new AsyncCommand<int>(OnPlaylistTapped, allowsMultipleExecutions: false);
            CancelCommand = new AsyncCommand(_navigation.PopModalAsync, allowsMultipleExecutions: false);

            LoadData();
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

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

        public AsyncCommand CancelCommand { get; }
        
        public AsyncCommand NewPlaylistCommand { get; }
        private async Task OnNewPlaylist()
        {
            IsBusy = true;

            var playlist = new CreatePlaylist { Name = NewPlaylistText };

            var createResponse = await App.API.Playlists.CreateAsync(playlist);
            if (!createResponse.Success)
            {
                if (await createResponse.HandleUnauthorizedException())
                    return;
                
                await Helpers.Alerts.ShowAlertAsync("Error", createResponse.Error.Message);
                IsBusy = false;
                return;
            }



            var addResponse = _basicMedia.MediaType == MediaTypes.Movie
                ? await App.API.Playlists.AddItemAsync(createResponse.Data, _basicMedia.Id)
                : await App.API.Playlists.AddSeriesAsync(createResponse.Data, _basicMedia.Id);

            if (!addResponse.Success)
            {
                if (await addResponse.HandleUnauthorizedException())
                    return;
                await Helpers.Alerts.ShowAlertAsync("Error", addResponse.Error.Message);
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

            await _navigation.PopModalAsync();
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
                        if (!ret)
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

                await _navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await Helpers.Alerts.ShowAlertAsync("Error", ex.Message);
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

            Height = height - 36;
        }

        private async void LoadData()
        {
            var response = await App.API.Playlists.ListAsync();
            if (response.Success)
            {
                Playlists.ReplaceRange(response.Data);
                ShowExistingPlaylists = response.Data.Count > 0;
            }
            else
            {
                if (await response.HandleUnauthorizedException())
                    return;
                await Helpers.Alerts.ShowAlertAsync("Error", response.Error.Message);
            }

            IsBusy = false;
        }
    }
}
