using DustyPig.API.v3.Models;
using DustyPig.API.v3.MPAA;
using System;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.Movie
{
    public class MovieDetailsViewModel : _DetailsBaseViewModel
    {
        public MovieDetailsViewModel(BasicMedia basicMedia, INavigation navigation) : base(navigation)
        {
            Id = basicMedia.Id;
        }

        private string _year;
        public string Year
        {
            get => _year;
            set => SetProperty(ref _year, value);
        }

        
        
        public async void OnAppearing()
        {
            IsBusy = true;

            var response = await App.API.Movies.GetDetailsAsync(Id);
            if (response.Success)
            {
                BackdropUrl = string.IsNullOrWhiteSpace(response.Data.BackdropUrl) ?
                    response.Data.ArtworkUrl :
                    response.Data.BackdropUrl;
                Title = response.Data.Title;
                Year = response.Data.Date.Year.ToString();
                Description = response.Data.Description;
                Played = response.Data.Played ?? 0;
                ShowPlayedBar = Played > 0;
                Duration = response.Data.Length;
                Owner = response.Data.Owner;


                switch(response.Data.Rated)
                {
                    case API.v3.MPAA.Ratings.None:
                    case API.v3.MPAA.Ratings.NR:
                        Rating = "Not Rated";
                        break;

                    default:
                        Rating = response.Data.Rated.ToString().Replace('_', '-');
                        break;
                }

                Progress = Math.Min(Math.Max(Played / Duration, 0), 1);
                PlayButtonText = Played > 0 ? "Resume" : "Play";
               
                var dur = TimeSpan.FromSeconds(response.Data.Length);
                if (dur.Hours > 0)
                    DurationString = $"{dur.Hours}h {dur.Minutes}m";
                else
                    DurationString = $"{Math.Max(dur.Minutes, 1)}m";


                dur = TimeSpan.FromMinutes(response.Data.Length - (response.Data.Played ?? 0));
                if (dur.Hours > 0)
                    RemainingString = $"{dur.Hours}h {dur.Minutes}m remaining";
                else
                    RemainingString = $"{Math.Max(dur.Minutes, 1)}m remaining";


                WatchlistIcon = response.Data.InWatchlist ?
                    Helpers.FontAwesome.Check :
                    Helpers.FontAwesome.Plus;

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

              

            }
            else
            {
                await ShowAlertAsync("Error", "Unable to retrieve movie info");
                await Navigation.PopAsync();
            }

            IsBusy = false;
        }
    }
}
