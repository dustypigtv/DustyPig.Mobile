using DustyPig.API.v3.Models;

namespace DustyPig.Mobile.MVVM.MediaDetails.Movie
{
    public class MovieDetailsViewModel : _DetailsBaseViewModel
    {
        public MovieDetailsViewModel(BasicMedia basicMedia)
        {
            Basic = basicMedia;
         
        }

        public BasicMedia Basic { get; }

        public async void OnAppearing()
        {
            var response = await App.API.Movies.GetDetailsAsync(Basic.Id);
            if (response.Success)
            {
                BackdropUrl = string.IsNullOrWhiteSpace(response.Data.BackdropUrl) ?
                    response.Data.ArtworkUrl :
                    response.Data.BackdropUrl;

            }
            else
            {
                await ShowAlertAsync("Error", "Unable to retrieve movie info");
                await Navigation.PopAsync();
            }
        }
    }
}
