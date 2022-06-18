using DustyPig.API.v3.Models;
using DustyPig.API.v3.MPAA;
using DustyPig.Mobile.MVVM.Main.Home;
using DustyPig.Mobile.Services.Download;
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
        private int _upNextId;
        
        public SeriesDetailsViewModel(BasicMedia basicMedia, INavigation navigation) : base(basicMedia, navigation)
        {
            IsBusy = true;

            Id = basicMedia.Id;

            PlayCommand = new AsyncCommand(() => OnPlayEpisode(_upNextId), allowsMultipleExecutions: false);
            PlayEpisodeCommand = new AsyncCommand<int>(OnPlayEpisode, allowsMultipleExecutions: false);
            MarkWatchedCommand = new AsyncCommand(OnMarkWatched, allowsMultipleExecutions: false);
            ChangeSeasonCommand = new AsyncCommand(OnChangeSeason, allowsMultipleExecutions: false);
            ShowSynopsisCommand = new AsyncCommand<EpisodeInfoViewModel>(OnShowSynopsis, allowsMultipleExecutions: false);
           
            ShowSynopsis = Services.Settings.ShowEpisodeSynopsis;

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
                    Services.Settings.ShowEpisodeSynopsis = value;
                    EpisodePosterRowSpan = value ? 2 : 3;
                    double bottom = value ? 24 : 0;
                    EpisodeItemMargin = new Thickness(0, 0, 0, bottom);
                }
            }
        }

        private int _episodePosterRowSpan = 3;
        public int EpisodePosterRowSpan
        {
            get => _episodePosterRowSpan;
            set => SetProperty(ref _episodePosterRowSpan, value);
        }

        private Thickness _episodeItemMargin = new Thickness(0, 0, 0, 0);
        public Thickness EpisodeItemMargin
        {
            get => _episodeItemMargin;
            set => SetProperty(ref _episodeItemMargin, value);
        }

        public AsyncCommand<EpisodeInfoViewModel> ShowSynopsisCommand { get; }
        public async Task OnShowSynopsis(EpisodeInfoViewModel episode)
        {
            await ShowAlertAsync("Synopsis", $"{episode.EpisodeNumber}: {episode.Title}\n\n{episode.Synopsis}");
        }

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
            var ret = await Navigation.ShowPopupAsync(new MarkWatchedPopup());
            if (ret == MarkWatchedPopupResponse.NoAction)
                return;

            IsBusy2 = true;

            switch (ret)
            {
                case MarkWatchedPopupResponse.MarkSeriesWatched:
                    await AfterMarkingWatched(await App.API.Series.MarkSeriesWatchedAsync(Id), false);
                    break;

                case MarkWatchedPopupResponse.StopWatching:
                    await AfterMarkingWatched(await App.API.Series.RemoveFromContinueWatchingAsync(Id), true);
                    break;
            }

            IsBusy2 = false;
        }

        private async Task AfterMarkingWatched(REST.Response response, bool stoppedWatching)
        {
            if (response.Success)
            {
                HomeViewModel.InvokeMarkWatched(Id);

                var upnext = Detailed_Series.Episodes.FirstOrDefault(item => item.UpNext);
                if (upnext != null)
                {
                    upnext.UpNext = false;
                    upnext.Played = null;
                }

                if (!stoppedWatching)
                {
                    var ep = Detailed_Series.Episodes.LastOrDefault();
                    if (ep != null)
                    {
                        ep.UpNext = true;
                        ep.Played = ep.Length;
                    }
                }

                UpdateElements();
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.Message);
            }
        }


        public AsyncCommand PlayCommand { get; }
        public AsyncCommand<int> PlayEpisodeCommand { get; }
        private async Task OnPlayEpisode(int id)
        {
            await ShowAlertAsync("TO DO:", $"Play {id}");
        }

        public AsyncCommand ChangeSeasonCommand { get; }
        private async Task OnChangeSeason()
        {
            if (!MultipleSeasons)
                return;

            var ret = await Navigation.ShowPopupAsync(new SeasonsPopup(Detailed_Series.Episodes.Select(item => item.SeasonNumber).Distinct().ToList(), CurrentSeason));
            if (ret < 0)
                return;

            CurrentSeason = (ushort)ret;
            Episodes.Clear();
            Episodes.AddRange(Detailed_Series.Episodes.Where(item => item.SeasonNumber == ret).Select(item => EpisodeInfoViewModel.FromEpisode(item)));
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


        public ObservableRangeCollection<string> Seasons { get; } = new ObservableRangeCollection<string>();

        private ObservableRangeCollection<EpisodeInfoViewModel> _episodes = new ObservableRangeCollection<EpisodeInfoViewModel>();
        public ObservableRangeCollection<EpisodeInfoViewModel> Episodes
        {
            get => _episodes;
            set => SetProperty(ref _episodes, value);
        }

        private async void LoadData()
        {
            IsBusy = true;

            var response = await GetSeriesDetailsAsync(Id);
            if (response.Success)
            {
                Detailed_Series = response.Data;
                LibraryId = Detailed_Series.LibraryId;
                Seasons.AddRange(response.Data.Episodes.Select(item => item.SeasonNumber).Distinct().Select(item => $"Season {item}"));
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
            int cnt = Detailed_Series.Episodes.Select(item => item.SeasonNumber).Distinct().Count();
            SeasonCount = cnt.ToString() + " Season";
            if (cnt != 1)
            {
                SeasonCount += "s";
                MultipleSeasons = true;
            }

            string bdu = string.IsNullOrWhiteSpace(Detailed_Series.BackdropUrl) ?
                Detailed_Series.ArtworkUrl :
                Detailed_Series.BackdropUrl;
            BackdropUrl = GetPath(DownloadService.CheckForLocalBackdrop(Id), bdu);

            Title = Detailed_Series.Title;
            Owner = Detailed_Series.Owner;

            switch (Detailed_Series.Rated)
            {
                case API.v3.MPAA.Ratings.None:
                case API.v3.MPAA.Ratings.NR:
                    Rating = "Not Rated";
                    break;

                default:
                    Rating = Detailed_Series.Rated.ToString().Replace('_', '-');
                    break;
            }

            CanManage = Detailed_Series.CanManage;


            // These all are based on whether the user CAN play content, or needs permission

            CanPlay = Detailed_Series.CanPlay;
            InWatchlist = Detailed_Series.InWatchlist;

            var upNext = Detailed_Series.Episodes.FirstOrDefault(item => item.UpNext);
            if (upNext == null)
            {
                var ep = Detailed_Series.Episodes.First();
                _upNextId = ep.Id;
                Episodes.Clear();
                Episodes.AddRange(Detailed_Series.Episodes.Where(item => item.SeasonNumber == ep.SeasonNumber).Select(item => EpisodeInfoViewModel.FromEpisode(item)));
                CurrentSeason = ep.SeasonNumber;
                CurrentEpisode = $"S{ep.SeasonNumber}:E{ep.EpisodeNumber} {ep.Title}";
                Description = StringUtils.Coalesce(Detailed_Series.Description, ep.Description);
                PlayButtonText = "Play";
            }
            else
            {
                _upNextId = upNext.Id;
                Duration = upNext.Length;
                Played = upNext.Played ?? 0;
                PlayButtonText = Played > 0 ? "Resume" : "Play";
                Progress = Math.Min(Math.Max(Played / Duration, 0), 1);
                ShowPlayedBar = CanPlay && Played > 0;
                CurrentSeason = upNext.SeasonNumber;
                Episodes.Clear();
                Episodes.AddRange(Detailed_Series.Episodes.Where(item => item.SeasonNumber == upNext.SeasonNumber).Select(item => EpisodeInfoViewModel.FromEpisode(item)));
                CurrentEpisode = $"S{upNext.SeasonNumber}:E{upNext.EpisodeNumber} {upNext.Title}";
                Description = StringUtils.Coalesce(upNext.Description, "No episode description");

                var dur = TimeSpan.FromSeconds(upNext.Length - Played);
                if (dur.Hours > 0)
                    RemainingString = $"{dur.Hours}h {dur.Minutes}m remaining";
                else
                    RemainingString = $"{Math.Max(dur.Minutes, 0)}m remaining";
            }

            ShowWatchButton = CanPlay && upNext != null;



            var genres = Detailed_Series.Genres.AsString();
            if (!string.IsNullOrWhiteSpace(genres))
            {
                Genres = genres.Replace(",", ", ");
                ShowGenres = true;
            }

            if (Detailed_Series.Cast != null && Detailed_Series.Cast.Count > 0)
            {
                Cast = string.Join(", ", Detailed_Series.Cast);
                ShowCast = true;
            }

            if (Detailed_Series.Directors != null && Detailed_Series.Directors.Count > 0)
            {
                Directors = string.Join(", ", Detailed_Series.Directors);
                ShowDirectors = true;
            }

            if (Detailed_Series.Producers != null && Detailed_Series.Producers.Count > 0)
            {
                Producers = string.Join(", ", Detailed_Series.Producers);
                ShowProducers = true;
            }

            if (Detailed_Series.Writers != null && Detailed_Series.Writers.Count > 0)
            {
                Writers = string.Join(", ", Detailed_Series.Writers);
                ShowWriters = true;
            }
        }

        static async Task<REST.Response<DetailedSeries>> GetSeriesDetailsAsync(int id)
        {
            var data = await DownloadService.TryLoadDetailsAsync<DetailedSeries>(id);
            if (data != null)
                return new REST.Response<DetailedSeries> { Success = true, Data = data };

            return await App.API.Series.GetDetailsAsync(id);
        }

    }
}
