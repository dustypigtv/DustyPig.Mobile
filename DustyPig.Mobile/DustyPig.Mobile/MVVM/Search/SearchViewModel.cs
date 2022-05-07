using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace DustyPig.Mobile.MVVM.Search
{
    public class SearchViewModel : _BaseViewModel
    {
        private string _mediaEmptyString;
        public string MediaEmptyString
        {
            get => _mediaEmptyString;
            set => SetProperty(ref _mediaEmptyString, value);
        }

        private string _tmdbEmptyString;
        public string TMDBEmptyString
        {
            get => _tmdbEmptyString;
            set => SetProperty(ref _tmdbEmptyString, value);
        }

        //private ObservableRangeCollection

        public async Task OnDoQuery(string query)
        {
            query = (query + string.Empty).Trim();

            if (query == string.Empty)
            {
                return;
            }

            if(query.Length < 3)
            {
                return;
            }

            var response = await App.API.Media.SearchAsync(query);
            if (response.Success)
            {
                foreach (var item in response.Data.Available)
                    Console.WriteLine(item.Title);

                foreach (var item in response.Data.OtherTitles)
                    Console.WriteLine(item.Title);
            }
            else
            {

            }
        }
    }
}
