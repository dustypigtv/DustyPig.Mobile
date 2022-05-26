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
            PlayEpisodeCommand = new AsyncCommand<int>(OnPlayEpisode, allowsMultipleExecutions: false);
            DownloadCommand = new AsyncCommand(OnDownload, allowsMultipleExecutions: false);
            MarkWatchedCommand = new AsyncCommand(OnMarkWatched, allowsMultipleExecutions: false);
            ChangeSeasonCommand = new AsyncCommand(OnChangeSeason, allowsMultipleExecutions: false);
        }

        public DetailedSeries Series { get; set; }

        private bool _canManage;
        public bool CanManage
        {
            get => _canManage;
            set => SetProperty(ref _canManage, value);
        }

        private bool _showWatchButton;
        public bool ShowWatchButton
        {
            get => _showWatchButton;
            set => SetProperty(ref _showWatchButton, value);
        }

        public AsyncCommand MarkWatchedCommand { get; }
        private async Task OnMarkWatched()
        {
            var ret = await Navigation.ShowPopupAsync(new MarkWatchedDialog());
            if (ret == MarkWatchedOptions.Cancel)
                return;

            IsBusy2 = true;

            REST.Response response;
            if (ret == MarkWatchedOptions.MarkSeriesWatched)
                response = await App.API.Series.MarkSeriesWatchedAsync(Id);
            else
                response = await App.API.Series.RemoveFromContinueWatchingAsync(Id);
                

            if (response.Success)
            {
                Main.Home.HomeViewModel.InvokeMarkWatched(Id);

                var upnext = Series.Episodes.FirstOrDefault(item => item.UpNext);
                if(upnext != null)
                {
                    upnext.UpNext = false;
                    upnext.Played = null;
                }

                var last = Series.Episodes.LastOrDefault();
                if(last != null)
                {
                    last.UpNext = true;
                    last.Played = last.Length;
                }

                UpdateElements();
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.Message);
            }

            IsBusy2 = false;
        }

        public AsyncCommand PlayCommand { get; }
        private async Task OnPlay()
        {
            await ShowAlertAsync("TO DO:", "Play Next");
        }

        public AsyncCommand<int> PlayEpisodeCommand { get; }
        private async Task OnPlayEpisode(int id)
        {
            await ShowAlertAsync("TO DO:", $"Play {id}");
        }

        public AsyncCommand DownloadCommand { get; }
        private async Task OnDownload()
        {
            await ShowAlertAsync("TO DO:", "Download");
        }

        
        public AsyncCommand ChangeSeasonCommand { get; }
        private async Task OnChangeSeason()
        {
            if (!MultipleSeasons)
                return;

            var ret = await Navigation.ShowPopupAsync(new SeasonsDialog(Series.Episodes.Select(item => item.SeasonNumber).Distinct().ToList(), CurrentSeason));
            if (ret < 0)
                return;

            CurrentSeason = (ushort)ret;
            Episodes.Clear();
            Episodes.AddRange(Series.Episodes.Where(item => item.SeasonNumber == ret).Select(item => EpisodeInfoViewModel.FromEpisode(item)));
        }


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


        private ushort _currentSeason;
        public ushort CurrentSeason
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
                UpdateElements();
                IsBusy = false;
            }
            else
            {
                await ShowAlertAsync("Error", "Unable to retrieve series info");
                await Navigation.PopModalAsync();
            }
        }
    
        private void UpdateElements()
        {
            int cnt = Series.Episodes.Select(item => item.SeasonNumber).Distinct().Count();
            SeasonCount = cnt.ToString() + " Season";
            if (cnt != 1)
            {
                SeasonCount += "s";
                MultipleSeasons = true;
            }

            BackdropUrl = string.IsNullOrWhiteSpace(Series.BackdropUrl) ?
                Series.ArtworkUrl :
                Series.BackdropUrl;
            Title = Series.Title;
            Owner = Series.Owner;

            switch (Series.Rated)
            {
                case API.v3.MPAA.Ratings.None:
                case API.v3.MPAA.Ratings.NR:
                    Rating = "Not Rated";
                    break;

                default:
                    Rating = Series.Rated.ToString().Replace('_', '-');
                    break;
            }

            CanManage = Series.CanManage;


            // These all are based on whether the user CAN play content, or needs permission

            CanPlay = Series.CanPlay;
            InWatchlist = Series.InWatchlist;

            var upNext = Series.Episodes.FirstOrDefault(item => item.UpNext);
            if (upNext == null)
            {
                var ep = Series.Episodes.First();
                Episodes.Clear();
                Episodes.AddRange(Series.Episodes.Where(item => item.SeasonNumber == ep.SeasonNumber).Select(item => EpisodeInfoViewModel.FromEpisode(item)));
                CurrentSeason = ep.SeasonNumber;
                CurrentEpisode = $"S{ep.SeasonNumber}:E{ep.EpisodeNumber} {ep.Title}";
                Description = StringUtils.Coalesce(Series.Description, ep.Description);
                PlayButtonText = "Play";
            }
            else
            {
                Duration = upNext.Length;
                Played = upNext.Played ?? 0;
                PlayButtonText = Played > 0 ? "Resume" : "Play";
                Progress = Math.Min(Math.Max(Played / Duration, 0), 1);
                ShowPlayedBar = CanPlay && Played > 0;
                CurrentSeason = upNext.SeasonNumber;
                Episodes.Clear();
                Episodes.AddRange(Series.Episodes.Where(item => item.SeasonNumber == upNext.SeasonNumber).Select(item => EpisodeInfoViewModel.FromEpisode(item)));
                CurrentEpisode = $"S{upNext.SeasonNumber}:E{upNext.EpisodeNumber} {upNext.Title}";
                Description = StringUtils.Coalesce(upNext.Description, "No episode description");

                var dur = TimeSpan.FromSeconds(upNext.Length - Played);
                if (dur.Hours > 0)
                    RemainingString = $"{dur.Hours}h {dur.Minutes}m remaining";
                else
                    RemainingString = $"{Math.Max(dur.Minutes, 0)}m remaining";
            }

            ShowWatchButton = CanPlay && upNext != null;



            var genres = Series.Genres.AsString();
            if (!string.IsNullOrWhiteSpace(genres))
            {
                Genres = genres.Replace(",", ", ");
                ShowGenres = true;
            }

            if (Series.Cast != null && Series.Cast.Count > 0)
            {
                Cast = string.Join(", ", Series.Cast);
                ShowCast = true;
            }

            if (Series.Directors != null && Series.Directors.Count > 0)
            {
                Directors = string.Join(", ", Series.Directors);
                ShowDirectors = true;
            }

            if (Series.Producers != null && Series.Producers.Count > 0)
            {
                Producers = string.Join(", ", Series.Producers);
                ShowProducers = true;
            }

            if (Series.Writers != null && Series.Writers.Count > 0)
            {
                Writers = string.Join(", ", Series.Writers);
                ShowWriters = true;
            }
        }

    }
}
