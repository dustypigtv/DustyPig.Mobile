﻿using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.MVVM.Main.Home;
using DustyPig.Mobile.Services.Download;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.Playlist
{
    public class PlaylistDetailsViewModel : _DetailsBaseViewModel
    {
        public PlaylistDetailsViewModel(BasicMedia basicMedia, INavigation navigation) : base(basicMedia, navigation)
        {
            Id = basicMedia.Id;

            PlayCommand = new AsyncCommand(() => OnPlayItem(Detailed_Playlist.Items[Detailed_Playlist.CurrentIndex].Id), allowsMultipleExecutions: false);
            PlayItemCommand = new AsyncCommand<int>(OnPlayItem, allowsMultipleExecutions: false);
            ShowSynopsisCommand = new AsyncCommand<string>(OnShowSynopsis, allowsMultipleExecutions: false);

            RenameCommand = new AsyncCommand(OnRename, allowsMultipleExecutions: false);
            DeleteCommand = new AsyncCommand(OnDelete, allowsMultipleExecutions: false);

            ShowSynopsis = Services.Settings.ShowPlaylistItemSynopsis;

            IsBusy = true;
            LoadData();
        }


        /// <summary>
        /// I know these 3 are anit-pattern, but nobody's perfect
        /// </summary>
        private bool _showSynopsis = false;
        public bool ShowSynopsis
        {
            get => _showSynopsis;
            set
            {
                if (SetProperty(ref _showSynopsis, value))
                {
                    Services.Settings.ShowPlaylistItemSynopsis = value;
                    PlaylistPosterRowSpan = value ? 2 : 3;
                    double bottom = value ? 24 : 0;
                    PlaylistItemMargin = new Thickness(0, 0, 0, bottom);
                }
            }
        }

        private int _playlistPosterRowSpan = 3;
        public int PlaylistPosterRowSpan
        {
            get => _playlistPosterRowSpan;
            set => SetProperty(ref _playlistPosterRowSpan, value);
        }

        private Thickness _playlistItemMargin = new Thickness(0, 0, 0, 0);
        public Thickness PlaylistItemMargin
        {
            get => _playlistItemMargin;
            set => SetProperty(ref _playlistItemMargin, value);
        }




        public AsyncCommand<string> ShowSynopsisCommand { get; }
        public async Task OnShowSynopsis(string synopsis)
        {
            await ShowAlertAsync("Synopsis", synopsis);
        }


        public AsyncCommand PlayCommand { get; }
        public AsyncCommand<int> PlayItemCommand { get; }
        private async Task OnPlayItem(int id)
        {
            await ShowAlertAsync("TO DO:", $"Play {id}");
        }



        public AsyncCommand RenameCommand { get; }
        private async Task OnRename()
        {
            await Navigation.ShowPopupAsync(new RenamePlaylistPopup(Basic_Media));
            Title = Basic_Media.Title;
            Detailed_Playlist.Name = Title;
        }


        public AsyncCommand DeleteCommand { get; }
        private async Task OnDelete()
        {
            var popup = DependencyService.Get<IPopup>();
            var ans = await popup.YesNoAsync("Confirm", "Are you sure you want to delete this playlist?");
            if (!ans)
                return;

            IsBusy2 = true;

            var response = await App.API.Playlists.DeleteAsync(Id);
            if (response.Success)
            {
                HomeViewModel.InvokeRmovedFromPlaylists(Basic_Media);
                await Navigation.PopModalAsync();
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.Message);
            }

            IsBusy2 = false;
        }

        private string _upNextTitle;
        public string UpNextTitle
        {
            get => _upNextTitle;
            set => SetProperty(ref _upNextTitle, value);
        }

        private ObservableRangeCollection<PlaylistItemViewModel> _items = new ObservableRangeCollection<PlaylistItemViewModel>();
        public ObservableRangeCollection<PlaylistItemViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private async void LoadData()
        {
            IsBusy = true;

            var response = await GetPlaylistDetailsAsync(Id);
            if (response.Success)
            {
                Detailed_Playlist = response.Data;
                Title = Detailed_Playlist.Name;


                if (Detailed_Playlist.CurrentIndex < 0)
                    Detailed_Playlist.CurrentIndex = 0;
                if (Detailed_Playlist.CurrentIndex > Detailed_Playlist.Items.Count - 1)
                    Detailed_Playlist.CurrentIndex = Detailed_Playlist.Items.Count - 1;


                var curItem = Detailed_Playlist.Items[Detailed_Playlist.CurrentIndex];
                UpNextTitle = curItem.Title;
                Duration = curItem.Length;
                Played = curItem.Played ?? 0;
                PlayButtonText = Played > 0 ? "Resume" : "Play";
                Progress = Math.Min(Math.Max(Played / Duration, 0), 1);
                ShowPlayedBar = Played > 0;
                Description = StringUtils.Coalesce(curItem.Description, "No description");
                Items.AddRange(Detailed_Playlist.Items.Select(item => PlaylistItemViewModel.FromItem(item, curItem.Index)));

                var dur = TimeSpan.FromSeconds(curItem.Length - Played);
                if (dur.Hours > 0)
                    RemainingString = $"{dur.Hours}h {dur.Minutes}m remaining";
                else
                    RemainingString = $"{Math.Max(dur.Minutes, 0)}m remaining";

                IsBusy = false;
            }
            else
            {
                await ShowAlertAsync("Error", "Unable to retrieve series info");
                await Navigation.PopModalAsync();
            }
        }

        static async Task<REST.Response<DetailedPlaylist>> GetPlaylistDetailsAsync(int id)
        {
            var data = await DownloadService.TryLoadDetailsAsync<DetailedPlaylist>(id);
            if (data != null)
                return new REST.Response<DetailedPlaylist> { Success = true, Data = data };

            return await App.API.Playlists.GetDetailsAsync(id);
        }
    }
}