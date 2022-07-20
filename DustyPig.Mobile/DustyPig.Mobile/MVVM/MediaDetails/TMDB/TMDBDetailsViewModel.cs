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
            IsBusy2 = true;

            var request = new TitleRequest
            {
                MediaType = _basicTMDB.MediaType,
                TMDB_Id = _basicTMDB.TMDB_ID
            };

            if (RequestText == "Cancel Request")
            {
                var response = await App.API.TMDB.CancelRequestAsync(request);
                if (response.Success)
                {
                    var permissionResponse = await App.API.TMDB.GetRequestTitlePermissionAsync();
                    if (permissionResponse.Success)
                    {
                        await ShowAlertAsync("Success", "Request Cancelled");
                        _permission = permissionResponse.Data;
                        ShowRequest = _permission != TitleRequestPermissions.Disabled;
                        RequestStatus = "Not Requested";
                        RequestText = "Requested";
                    }
                    else
                    {
                        if (await permissionResponse.HandleUnauthorizedException())
                            return;
                        await ShowAlertAsync("Error", permissionResponse.Error.Message);
                    }
                }
                else
                {
                    if (await response.HandleUnauthorizedException())
                        return;
                    await ShowAlertAsync("Error", response.Error.Message);
                }
            }
            else
            {
                if (_permission == TitleRequestPermissions.Enabled)
                {
                    var page = new FriendPickerPage();
                    await Navigation.PushModalAsync(page);
                    request.FriendId = await page.GetResultAsync();
                    if (request.FriendId <= 0)
                    {
                        IsBusy2 = false;
                        return;
                    }
                }

                var response = await App.API.TMDB.RequestTitleAsync(request);
                if (response.Success)
                {
                    await ShowAlertAsync("Success", "Request Sent");
                    RequestStatus = "Requested";
                    RequestText = "Cancel Request";
                }
                else
                {
                    if (await response.HandleUnauthorizedException())
                        return;
                    await ShowAlertAsync("Error", response.Error.Message);
                }
            }

            IsBusy2 = false;
        }



        private async void LoadData()
        {
            var response = _basicTMDB.MediaType == TMDB_MediaTypes.Movie
                ? await App.API.TMDB.GetMovieAsync(Id)
                : await App.API.TMDB.GetSeriesAsync(Id);

            if (response.Success)
            {
                _permission = response.Data.RequestPermission;
                
                switch (response.Data.RequestStatus)
                {
                    case API.v3.Models.RequestStatus.Denied:
                        RequestStatus = "Request Denied";
                        RequestText = "Cancel Request";
                        ShowRequest = true;
                        break;

                    case API.v3.Models.RequestStatus.Fulfilled:
                        RequestStatus = "Available";
                        RequestText = "Cancel Request";
                        ShowRequest = true;
                        break;

                    case API.v3.Models.RequestStatus.NotRequested:
                        RequestStatus = "Not Requested";
                        RequestText = "Request";
                        ShowRequest = _permission != TitleRequestPermissions.Disabled;
                        break;

                    case API.v3.Models.RequestStatus.Pending:
                        RequestStatus = "Pending";
                        RequestText = "Cancel Request";
                        ShowRequest = true;
                        break;

                    case API.v3.Models.RequestStatus.RequestSentToAccount:
                    case API.v3.Models.RequestStatus.RequestSentToMain:
                        RequestStatus = "Requested";
                        RequestText = "Cancel Request";
                        ShowRequest = true;
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
                if (await response.HandleUnauthorizedException())
                    return;
                await ShowAlertAsync("Error", response.Error.Message);
                await Navigation.PopModalAsync();
            }
        }


    }
}
