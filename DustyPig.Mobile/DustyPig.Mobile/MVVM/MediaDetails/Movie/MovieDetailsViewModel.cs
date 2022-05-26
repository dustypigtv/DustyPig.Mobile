using DustyPig.API.v3.Models;
using DustyPig.API.v3.MPAA;
using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.Movie
{
    public class MovieDetailsViewModel : _DetailsBaseViewModel
    {
        public MovieDetailsViewModel(BasicMedia basicMedia, INavigation navigation) : base(basicMedia, navigation)
        {
            IsBusy = true;

            Id = basicMedia.Id;

            PlayCommand = new AsyncCommand(OnPlay, allowsMultipleExecutions: false);
            DownloadCommand = new AsyncCommand(OnDownload, allowsMultipleExecutions: false);
            RequestPermissionCommand = new AsyncCommand(OnRequestPermission, allowsMultipleExecutions: false);
            MarkWatchedCommand = new AsyncCommand(OnMarkWatched, allowsMultipleExecutions: false);
        }


        private DetailedMovie Movie { get; set; }

        private bool _canManage;
        public bool CanManage
        {
            get => _canManage;
            set => SetProperty(ref _canManage, value);
        }

        public AsyncCommand PlayCommand { get; }
        private async Task OnPlay()
        {
            await ShowAlertAsync("TO DO:", "Play");
        }

        public AsyncCommand DownloadCommand { get; }
        private async Task OnDownload()
        {
            await ShowAlertAsync("TO DO:", "Download");
        }

        public AsyncCommand RequestPermissionCommand { get; }
        private async Task OnRequestPermission()
        {
            await ShowAlertAsync("TO DO:", "Request Permission");
        }


        public AsyncCommand MarkWatchedCommand { get; }
        private async Task OnMarkWatched()
        {
            IsBusy2 = true;

            var response = await App.API.Media.UpdatePlaybackProgressAsync(Id, 0);
            if (response.Success)
            {
                Main.Home.HomeViewModel.InvokeMarkWatched(Id);
                ShowPlayedBar = false;
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.Message);
            }

            IsBusy2 = false;
        }


        
        
        
        public async void OnAppearing()
        {
            IsBusy = true;

            var response = await App.API.Movies.GetDetailsAsync(Id);
            if (response.Success)
            {
                Movie = response.Data;

                BackdropUrl = string.IsNullOrWhiteSpace(response.Data.BackdropUrl) ?
                    response.Data.ArtworkUrl :
                    response.Data.BackdropUrl;
                Title = response.Data.Title;
                Year = response.Data.Date.Year.ToString();
                Description = response.Data.Description;
                Duration = response.Data.Length;
                Owner = response.Data.Owner;

                switch (response.Data.Rated)
                {
                    case API.v3.MPAA.Ratings.None:
                    case API.v3.MPAA.Ratings.NR:
                        Rating = "Not Rated";
                        break;

                    default:
                        Rating = response.Data.Rated.ToString().Replace('_', '-');
                        break;
                }

                var dur = TimeSpan.FromSeconds(response.Data.Length);
                if (dur.Hours > 0)
                    DurationString = $"{dur.Hours}h {dur.Minutes}m";
                else
                    DurationString = $"{Math.Max(dur.Minutes, 0)}m";



                var genres = response.Data.Genres.AsString();
                if (!string.IsNullOrWhiteSpace(genres))
                {
                    Genres = genres.Replace(",", ", ");
                    ShowGenres = true;
                }

                if (response.Data.Cast != null && response.Data.Cast.Count > 0)
                {
                    Cast = string.Join(", ", response.Data.Cast);
                    ShowCast = true;
                }

                if (response.Data.Directors != null && response.Data.Directors.Count > 0)
                {
                    Directors = string.Join(", ", response.Data.Directors);
                    ShowDirectors = true;
                }

                if (response.Data.Producers != null && response.Data.Producers.Count > 0)
                {
                    Producers = string.Join(", ", response.Data.Producers);
                    ShowProducers = true;
                }

                if (response.Data.Writers != null && response.Data.Writers.Count > 0)
                {
                    Writers = string.Join(", ", response.Data.Writers);
                    ShowWriters = true;
                }





                // These all are based on whether the user CAN play content, or needs permission

                CanPlay = response.Data.CanPlay;
                InWatchlist = response.Data.InWatchlist;
                Played = response.Data.Played ?? 0;
                ShowPlayedBar = CanPlay && Played > 0;
                Progress = Math.Min(Math.Max(Played / Duration, 0), 1);
                PlayButtonText = Played > 0 ? "Resume" : "Play";
                
                dur = TimeSpan.FromSeconds(response.Data.Length - (response.Data.Played ?? 0));
                if (dur.Hours > 0)
                    RemainingString = $"{dur.Hours}h {dur.Minutes}m remaining";
                else
                    RemainingString = $"{Math.Max(dur.Minutes, 0)}m remaining";



                CanManage = response.Data.CanManage;


                IsBusy = false;
            }
            else
            {
                await ShowAlertAsync("Error", "Unable to retrieve movie info");
                await Navigation.PopModalAsync();
            }
        }
    }
}
