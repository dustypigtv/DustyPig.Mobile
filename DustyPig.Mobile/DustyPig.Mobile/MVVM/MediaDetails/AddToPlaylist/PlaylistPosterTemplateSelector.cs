using DustyPig.API.v3.Models;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.MediaDetails.AddToPlaylist
{
    public class PlaylistPosterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Single { get; set; }
        public DataTemplate Multi { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var playlist = (BasicPlaylist)item;

            if (!string.IsNullOrWhiteSpace(playlist.ArtworkUrl2))
                if (!string.IsNullOrWhiteSpace(playlist.ArtworkUrl3))
                    if (!string.IsNullOrEmpty(playlist.ArtworkUrl4))
                        return Multi;

            return Single;
        }
    }
}
