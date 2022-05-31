using DustyPig.API.v3.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DustyPig.Mobile.MVVM.Main.Home
{
    public class BasicMediaPosterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Default { get; set; }
        public DataTemplate Playlist { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var basicMedia = (BasicMedia)item;

            if (basicMedia.MediaType == MediaTypes.Playlist)
            {
                if (!string.IsNullOrWhiteSpace(basicMedia.ArtworkUrl2))
                    if (!string.IsNullOrWhiteSpace(basicMedia.ArtworkUrl3))
                        if (!string.IsNullOrEmpty(basicMedia.ArtworkUrl4))
                            return Playlist;

                return Default;
            }
            else
            {
                return Default;
            }
        }
    }
}
