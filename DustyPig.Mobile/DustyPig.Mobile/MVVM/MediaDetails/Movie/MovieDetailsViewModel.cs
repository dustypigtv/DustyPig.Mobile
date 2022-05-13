using DustyPig.API.v3.Models;
using System;

namespace DustyPig.Mobile.MVVM.MediaDetails.Movie
{
    public class MovieDetailsViewModel : _DetailsBaseViewModel
    {
        public MovieDetailsViewModel(BasicMedia basicMedia)
        {
            Basic = basicMedia;
         
        }

        public BasicMedia Basic { get; }

        private string _year;
        public string Year
        {
            get => _year;
            set => SetProperty(ref _year, value);
        }

        
        
        public async void OnAppearing()
        {
            IsBusy = true;

            var response = await App.API.Movies.GetDetailsAsync(Basic.Id);
            if (response.Success)
            {
                BackdropUrl = string.IsNullOrWhiteSpace(response.Data.BackdropUrl) ?
                    response.Data.ArtworkUrl :
                    response.Data.BackdropUrl;
                Title = response.Data.Title;
                Year = response.Data.Date.Year.ToString();
                Rating = response.Data.Rated.ToString().Replace('_', '-');
                Description = response.Data.Description;
                Played = response.Data.Played ?? 0;
                ShowPlayedBar = Played > 0;
                CastString = string.Join(", ", response.Data.Cast);
                DirectorsString = string.Join(", ", response.Data.Directors);
                Duration = response.Data.Length;


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
