using DustyPig.API.v3.Models;
using DustyPig.Mobile.Services.Download;
using System;
using System.ComponentModel;

namespace DustyPig.Mobile.MVVM.MediaDetails.Playlist
{
    public class PlaylistItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; set; }

        public int MediaId { get; set; }

        public string Title { get; set; }

        public string Synopsis { get; set; }

        public double Length { get; set; }

        public string ArtworkUrl { get; set; }

        private bool _upNext;
        public bool UpNext
        {
            get => _upNext;
            set
            {
                if (_upNext != value)
                {
                    _upNext = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpNext)));
                }
            }
        }

        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Index)));
                }
            }
        }

        public string Duration
        {
            get
            {
                var dur = TimeSpan.FromSeconds(Length);
                if (dur.Hours > 0)
                    return $"{dur.Hours}h {dur.Minutes}m";
                else
                    return $"{Math.Max(dur.Minutes, 1)}m";
            }
        }

        public static PlaylistItemViewModel FromItem(PlaylistItem item, int currentIndex)
        {
            var ret = new PlaylistItemViewModel
            {
                Synopsis = StringUtils.Coalesce(item.Description, "No synopsis"),
                Id = item.Id,
                Length = item.Length,
                MediaId = item.MediaId,
                Title = item.Title,
                Index = item.Index,
                UpNext = item.Index == currentIndex
            };

            switch (item.MediaType)
            {
                case MediaTypes.Movie:
                    ret.ArtworkUrl = _DetailsBaseViewModel.GetPath(DownloadService.CheckForLocalPoster(item.Id), item.ArtworkUrl);
                    break;

                case MediaTypes.Episode:
                    ret.ArtworkUrl = _DetailsBaseViewModel.GetPath(DownloadService.CheckForLocalScreenshot(item.Id), item.ArtworkUrl);
                    break;
            }

            return ret;
        }
    }
}
