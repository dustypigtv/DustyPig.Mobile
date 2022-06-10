﻿using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform;
using DustyPig.Mobile.Services.Download;
using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails
{
    public abstract class _DetailsBaseViewModel : _BaseViewModel
    {
        public _DetailsBaseViewModel(BasicMedia basicMedia, INavigation navigation) : base(navigation)
        {
            Basic_Media = basicMedia;

            DownloadCommand = new AsyncCommand(OnDownload, allowsMultipleExecutions: false);
            PlaylistCommand = new Command(AddToPlaylist);
            RequestPermissionCommand = new AsyncCommand(OnRequestPermission, allowsMultipleExecutions: false);
            ToggleWatchlistCommand = new AsyncCommand<int>(OnToggleWatchlist, allowsMultipleExecutions: false);
            ManageParentalControlsCommand = new Command(ManageParentalControls);

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                SetDownloadStatus();
                return true;
            });
        }

        public _DetailsBaseViewModel(BasicTMDB basicTMDB, INavigation navigation) : base(navigation)
        {
            Basic_TMDB = basicTMDB;
        }




        public BasicMedia Basic_Media { get; }

        public BasicTMDB Basic_TMDB { get; }

        public DetailedMovie Detailed_Movie { get; set; }

        public DetailedSeries Detailed_Series { get; set; }

        public int LibraryId { get; set; }

        public AsyncCommand RequestPermissionCommand { get; }
        private async Task OnRequestPermission()
        {
            IsBusy2 = true;

            var response = await App.API.Media.RequestAccessOverrideAsync(Basic_Media.Id);
            if (response.Success)
                await ShowAlertAsync("Success", "Request sent");
            else
                await ShowAlertAsync("Error", response.Error.Message);

            IsBusy2 = false;
        }



        public AsyncCommand<int> ToggleWatchlistCommand { get; }
        private async Task OnToggleWatchlist(int id)
        {
            IsBusy2 = true;

            REST.Response response = null;
            if (InWatchlist)
                response = await App.API.Media.DeleteFromWatchlistAsync(id);
            else
                response = await App.API.Media.AddToWatchlistAsync(Id);

            IsBusy2 = false;

            if (response.Success)
            {
                if (InWatchlist)
                    Main.Home.HomeViewModel.InvokeRemovedFromWatchlist(Basic_Media);
                else
                    Main.Home.HomeViewModel.InvokeAddedToWatchlist(Basic_Media);

                InWatchlist = !InWatchlist;
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.Message);
            }
        }

        
        public Command PlaylistCommand { get; }
        public void AddToPlaylist()
        {
            Navigation.ShowPopup(new AddToPlaylistPopup(Basic_Media));
        }


        public Command ManageParentalControlsCommand { get; }
        public void ManageParentalControls()
        {
            Navigation.ShowPopup(new ParentalControlsPopup(Basic_Media.Id, LibraryId));
        }


        public AsyncCommand DownloadCommand { get; }
        private async Task OnDownload()
        {
            var popup = DependencyService.Get<IPopup>();
            string detailType = Basic_Media.MediaType.ToString().ToLower();         
            var status = DownloadService.GetStatus(Id);

            switch (status.Status)
            {
                case JobStatus.Downloaded:

                    var confirmDelete = await popup.YesNoAsync("Confirm", $"Are you sure you want to remove the downloaded {detailType}?");
                    if (!confirmDelete)
                        return;
                    DownloadService.Delete(Id);

                    //Possible to get here from Downloaded info while offline. If deleting, close the page
                    if (NoInternet)
                    {
                        await Navigation.PopModalAsync();
                        return;
                    }
                    break;

                case JobStatus.Downloading:
                    var confirmCancel = await popup.YesNoAsync("Confirm", $"Are you sure you want to cancel downloading {detailType}?");
                    if (!confirmCancel)
                        return;
                    DownloadService.Delete(Id);

                    //Possible to get here from Downloaded info while offline. If deleting, close the page
                    if (NoInternet)
                    {
                        await Navigation.PopModalAsync();
                        return;
                    }
                    break;

                case JobStatus.NotDownloaded:
                    switch(Basic_Media.MediaType)
                    {
                        case MediaTypes.Movie:
                            DownloadService.AddOrUpdateMovie(Detailed_Movie);
                            break;

                        case MediaTypes.Series:
                            //DownloadService.AddOrUpdateSeries(Detailed_Series, count);
                            break;

                        case MediaTypes.Playlist:
                            //DownloadService.AddOrUpdatePlaylist(Detailed_Playlist, count);
                            break;
                    }                    
                    break;
            }

            SetDownloadStatus();
        }

        public void SetDownloadStatus()
        {
            var status = DownloadService.GetStatus(Id);
            switch (status.Status)
            {
                case JobStatus.Downloaded:
                    DownloadButtonText = "Downloaded - 100%";
                    break;

                case JobStatus.Downloading:
                    DownloadButtonText = $"Downloading - {status.Percent}%";
                    break;

                default:
                    DownloadButtonText = "Download";
                    break;
            }
        }

        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private bool _isBusy2;
        public bool IsBusy2
        {
            get => _isBusy2;
            set => SetProperty(ref _isBusy2, value);
        }

        private bool _inWatchlist;
        public bool InWatchlist
        {
            get => _inWatchlist;
            set
            {
                _inWatchlist = value;
                WatchlistIcon = value ?
                   Helpers.FontAwesome.Check :
                   Helpers.FontAwesome.Plus;
            }
        }

        private string _owner;
        public string Owner
        {
            get => _owner;
            set => SetProperty(ref _owner, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _imageHeight;
        public double ImageHeight
        {
            get => _imageHeight;
            set => SetProperty(ref _imageHeight, value);
        }

        private string _backdropUrl;
        public string BackdropUrl
        {
            get => _backdropUrl;
            set => SetProperty(ref _backdropUrl, value);
        }

        private string _rating;
        public string Rating
        {
            get => _rating;
            set => SetProperty(ref _rating, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private double _duration;
        public double Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        private string _durationString;
        public string DurationString
        {
            get => _durationString;
            set => SetProperty(ref _durationString, value);
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private string _remainingString;
        public string RemainingString
        {
            get => _remainingString;
            set => SetProperty(ref _remainingString, value);
        }

        private double _played;
        public double Played
        {
            get => _played;
            set => SetProperty(ref _played, value);
        }

        private string _genres;
        public string Genres
        {
            get => _genres;
            set => SetProperty(ref _genres, value);
        }

        private bool _showGenres;
        public bool ShowGenres
        {
            get => _showGenres;
            set => SetProperty(ref _showGenres, value);
        }

        private string _cast;
        public string Cast
        {
            get => _cast;
            set => SetProperty(ref _cast, value);
        }

        private bool _showCast;
        public bool ShowCast
        {
            get => _showCast;
            set => SetProperty(ref _showCast, value);
        }

        private string _directors;
        public string Directors
        {
            get => _directors;
            set => SetProperty(ref _directors, value);
        }

        private bool _showDirectors;
        public bool ShowDirectors
        {
            get => _showDirectors;
            set => SetProperty(ref _showDirectors, value);
        }

        private string _producers;
        public string Producers
        {
            get => _producers;
            set => SetProperty(ref _producers, value);
        }

        private bool _showProducers;
        public bool ShowProducers
        {
            get => _showProducers;
            set => SetProperty(ref _showProducers, value);
        }

        private string _writers;
        public string Writers
        {
            get => _writers;
            set => SetProperty(ref _writers, value);
        }

        private bool _showWriters;
        public bool ShowWriters
        {
            get => _showWriters;
            set => SetProperty(ref _showWriters, value);
        }

        private bool _showPlayedBar = false;
        public bool ShowPlayedBar
        {
            get => _showPlayedBar;
            set => SetProperty(ref _showPlayedBar, value);
        }

        private string _playButtonText;
        public string PlayButtonText
        {
            get => _playButtonText;
            set => SetProperty(ref _playButtonText, value);
        }

        private bool _canPlay;
        public bool CanPlay
        {
            get => _canPlay;
            set => SetProperty(ref _canPlay, value);
        }

        private string _watchlistIcon;
        public string WatchlistIcon
        {
            get => _watchlistIcon;
            set => SetProperty(ref _watchlistIcon, value);
        }

        private string _downloadButtonText = "Download";
        public string DownloadButtonText
        {
            get => _downloadButtonText;
            set => SetProperty(ref _downloadButtonText, value);
        }

        
        private string _year;
        public string Year
        {
            get => _year;
            set => SetProperty(ref _year, value);
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
       
            ImageHeight = (int)(Width * 0.5625);
        }

        


        public static string GetPath(string local, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (string.IsNullOrWhiteSpace(local))
                return url;

            if (!System.IO.File.Exists(local))
                return url;

            return local;
        }
    }
}
