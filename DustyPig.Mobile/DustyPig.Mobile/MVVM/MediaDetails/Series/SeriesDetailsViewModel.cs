using DustyPig.API.v3.Models;
using DustyPig.API.v3.MPAA;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    public class SeriesDetailsViewModel : _DetailsBaseViewModel
    {
        public SeriesDetailsViewModel(BasicMedia basicMedia, INavigation navigation) : base(basicMedia, navigation)
        {
            IsBusy = true;

            Id = basicMedia.Id;

            PlayCommand = new AsyncCommand(OnPlay, allowsMultipleExecutions: false);
            DownloadCommand = new AsyncCommand(OnDownload, allowsMultipleExecutions: false);
            RequestPermissionCommand = new AsyncCommand(OnRequestPermission, allowsMultipleExecutions: false);
            MarkWatchedCommand = new AsyncCommand(OnMarkWatched, allowsMultipleExecutions: false);
            OptionsCommand = new AsyncCommand(OnOptionsCommand, allowsMultipleExecutions: false);
            PlaylistCommand = new AsyncCommand(AddToPlaylist, allowsMultipleExecutions: false);
        }

        public DetailedSeries Series { get; set; }

        private bool _canManage;
        public bool CanManage
        {
            get => _canManage;
            set => SetProperty(ref _canManage, value);
        }

        public AsyncCommand MarkWatchedCommand { get; }
        private async Task OnMarkWatched()
        {
            
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
        

        public AsyncCommand OptionsCommand { get; }
        private async Task OnOptionsCommand()
        {
            
        }

        public AsyncCommand PlaylistCommand { get; }

        private string _seasonCount;
        public string SeasonCount
        {
            get => _seasonCount;
            set => SetProperty(ref _seasonCount, value);
        }

        private string _currentEpisode;
        public string CurrentEpisode
        {
            get => _currentEpisode;
            set => SetProperty(ref _currentEpisode, value);
        }


        private string _currentSeason;
        public string CurrentSeason
        {
            get => _currentSeason;
            set => SetProperty(ref _currentSeason, value);
        }

        private bool _multipleSeasons;
        public bool MultipleSeasons
        {
            get => _multipleSeasons;
            set => SetProperty(ref _multipleSeasons, value);
        }

        private ObservableRangeCollection<EpisodeInfoViewModel> _episodes = new ObservableRangeCollection<EpisodeInfoViewModel>();
        public ObservableRangeCollection<EpisodeInfoViewModel> Episodes
        {
            get => _episodes;
            set => SetProperty(ref _episodes, value);
        }

        public async void OnAppearing()
        {
            IsBusy = true;

            var response = await App.API.Series.GetDetailsAsync(Id);
            if (response.Success)
            {
                Series = response.Data;

                int cnt = response.Data.Episodes.Select(item => item.SeasonNumber).Distinct().Count();
                SeasonCount = cnt.ToString() + " Season";
                if (cnt != 1)
                {
                    SeasonCount += "s";
                    MultipleSeasons = true;
                }

                BackdropUrl = string.IsNullOrWhiteSpace(response.Data.BackdropUrl) ?
                    response.Data.ArtworkUrl :
                    response.Data.BackdropUrl;
                Title = response.Data.Title;
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

                CanManage = response.Data.CanManage;


                // These all are based on whether the user CAN play content, or needs permission

                CanPlay = response.Data.CanPlay;
                InWatchlist = response.Data.InWatchlist;

                var upNext = response.Data.Episodes.FirstOrDefault(item => item.UpNext);
                if(upNext == null)
                {
                    var ep = response.Data.Episodes.First();
                    Episodes.AddRange(response.Data.Episodes.Where(item => item.SeasonNumber == ep.SeasonNumber).Select(item => EpisodeInfoViewModel.FromEpisode(item)));
                    CurrentSeason = $"Season {ep.SeasonNumber}";
                    CurrentEpisode = $"S{ep.SeasonNumber}:E{ep.EpisodeNumber} {ep.Title}";
                    Description = StringUtils.Coalesce(ep.Description, response.Data.Description);
                    PlayButtonText = "Play";
                }
                else
                {
                    Duration = upNext.Length;
                    Played = upNext.Played ?? 0;
                    PlayButtonText = Played > 0 ? "Resume" : "Play";
                    Progress = Math.Min(Math.Max(Played / Duration, 0), 1);
                    ShowPlayedBar = CanPlay && Played > 0;
                    CurrentSeason = $"Season {upNext.SeasonNumber}";
                    Episodes.AddRange(response.Data.Episodes.Where(item => item.SeasonNumber == upNext.SeasonNumber).Select(item => EpisodeInfoViewModel.FromEpisode(item)));
                    CurrentEpisode = $"S{upNext.SeasonNumber}:E{upNext.EpisodeNumber} {upNext.Title}";
                    Description = StringUtils.Coalesce(upNext.Description, "No episode description");

                    var dur = TimeSpan.FromSeconds(upNext.Length - Played);
                    if (dur.Hours > 0)
                        RemainingString = $"{dur.Hours}h {dur.Minutes}m remaining";
                    else
                        RemainingString = $"{Math.Max(dur.Minutes, 1)}m remaining";
                }


                
               



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




                IsBusy = false;
            }
            else
            {
                await ShowAlertAsync("Error", "Unable to retrieve series info");
                await Navigation.PopModalAsync();
            }
        }
    }
}
