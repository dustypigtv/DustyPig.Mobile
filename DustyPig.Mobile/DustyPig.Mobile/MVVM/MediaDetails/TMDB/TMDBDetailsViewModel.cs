using DustyPig.API.v3.Models;
using DustyPig.Mobile.Helpers;
using DustyPig.Mobile.MVVM.MediaDetails.Movie;
using DustyPig.Mobile.MVVM.MediaDetails.Series;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.TMDB
{
    public class TMDBDetailsViewModel : _DetailsBaseViewModel
    {
        private readonly BasicTMDB _basicTMDB;
        private TitleRequestPermissions _permission = TitleRequestPermissions.Disabled;

        public TMDBDetailsViewModel(BasicTMDB basicTMDB, INavigation navigation) : base(basicTMDB, navigation)
        {
            Id = basicTMDB.TMDB_ID;
            _basicTMDB = basicTMDB;
            IsBusy = true;
            RequestCommand = new AsyncCommand(OnRequest, allowsMultipleExecutions: false);
            LoadData();
        }

        private ObservableBasicMediaCollection _available = new ObservableBasicMediaCollection();
        public ObservableBasicMediaCollection Available
        {
            get => _available;
            set => SetProperty(ref _available, value);
        }

        private bool _showAvailable;
        public bool ShowAvailable
        {
            get => _showAvailable;
            set => SetProperty(ref _showAvailable, value);
        }

        private bool _showYear;
        public bool ShowYear
        {
            get => _showYear;
            set => SetProperty(ref _showYear, value);
        }

        private bool _showRequest;
        public bool ShowRequest
        {
            get => _showRequest;
            set => SetProperty(ref _showRequest, value);
        }

        private string _requestText;
        public string RequestText
        {
            get => _requestText;
            set => SetProperty(ref _requestText, value);
        }

        private string _requestStatus;
        public string RequestStatus
        {
            get => _requestStatus;
            set => SetProperty(ref _requestStatus, value);
        }
        
        public AsyncCommand RequestCommand { get; }
        private async Task OnRequest()
        {
            var request = new TitleRequest
            {
                MediaType = _basicTMDB.MediaType,
                TMDB_Id = _basicTMDB.TMDB_ID
            };

            if (_permission == TitleRequestPermissions.Enabled)
            {
                var page = new FriendPickerPage();
                await Navigation.PushModalAsync(page);
                request.FriendId = await page.GetResultAsync();
                if (request.FriendId <= 0)
                    return;
            }

            var response = await App.API.TMDB.RequestTitleAsync(request);
            if (response.Success)
            {
                await ShowAlertAsync("Success", "Request Sent");
                ShowRequest = false;
                if (RequestStatus == "Not Requested")
                    RequestStatus = "Requested";
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.Message);
            }
        }

        private async void LoadData()
        {
            var response = _basicTMDB.MediaType == TMDB_MediaTypes.Movie
                ? await App.API.TMDB.GetMovieAsync(Id)
                : await App.API.TMDB.GetSeriesAsync(Id);

            if (response.Success)
            {
                _permission = response.Data.RequestPermission;
                
                ShowRequest = _permission != TitleRequestPermissions.Disabled;
                switch (response.Data.RequestStatus)
                {
                    case API.v3.Models.RequestStatus.NotRequested:
                        RequestStatus = "Not Requested";
                        RequestText = "Request";
                        break;

                    default:
                        RequestStatus = response.Data.RequestStatus.ToString();
                        RequestText = "Notify When Available";
                        break;
                }


                BackdropUrl = string.IsNullOrWhiteSpace(response.Data.BackdropUrl) ?
                    response.Data.ArtworkUrl :
                    response.Data.BackdropUrl;
                Title = response.Data.Title;
                Year = response.Data.Year.ToString();
                ShowYear = response.Data.Year > 0;
                Description = response.Data.Description;

                switch (API.v3.MPAA.RatingsUtils.ToRatings(response.Data.Rated))
                {
                    case API.v3.MPAA.Ratings.None:
                    case API.v3.MPAA.Ratings.NR:
                        Rating = "Not Rated";
                        break;

                    default:
                        Rating = response.Data.Rated.ToString().Replace('_', '-');
                        break;
                }


                var genres = response.Data.Genres.ToString();
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

                if (response.Data.Available != null && response.Data.Available.Count > 0)
                {
                    Available.ReplaceRange(response.Data.Available);
                    ShowAvailable = true;
                }

                IsBusy = false;
            }
            else
            {
                await ShowAlertAsync("Error", response.Error.Message);
                await Navigation.PopModalAsync();
            }
        }


    }
}
