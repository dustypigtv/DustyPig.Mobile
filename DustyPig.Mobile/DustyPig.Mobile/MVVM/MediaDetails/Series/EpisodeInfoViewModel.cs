using DustyPig.API.v3.Models;
using DustyPig.Mobile.Services.Download;
using System;
using System.ComponentModel;

namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    public class EpisodeInfoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int EpisodeNumber { get; set; }

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

        public static EpisodeInfoViewModel FromEpisode(DetailedEpisode ep)
        {
            var ret = new EpisodeInfoViewModel
            {
                ArtworkUrl = _DetailsBaseViewModel.GetPath(DownloadService.CheckForLocalScreenshot(ep.Id), ep.ArtworkUrl),
                Description = StringUtils.Coalesce(ep.Description, "No episode synopsis"),
                EpisodeNumber = ep.EpisodeNumber,
                Id = ep.Id,
                Length = ep.Length,
                Title = ep.Title,
                UpNext = ep.UpNext
            };

            return ret;
        }
    }
}