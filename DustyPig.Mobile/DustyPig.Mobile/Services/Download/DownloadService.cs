/*
    I *could* go to the trouble of implementing a sqlite database, but since this is is so lightweight...
    I'm just gonna have a serialized json file and a lock
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using DustyPig.Mobile.CrossPlatform.DownloadManager;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Threading;
using DustyPig.API.v3.Models;
using DustyPig.Mobile.Helpers;

namespace DustyPig.Mobile.Services.Download
{
    static class DownloadService
    {
        static List<Job> _jobs;
        static readonly IDownloadManager _manager = DependencyService.Get<IDownloadManager>();
        static readonly string _filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "downloads.json");
        static readonly object _locker = new object();
        static readonly Timer _monitorTimer = new Timer(new TimerCallback(MonitorJobs), null, 60000, Timeout.Infinite);
        static readonly Timer _detailsTimer = new Timer(new TimerCallback(UpdateDetails), null, 120000, Timeout.Infinite);
        static bool _detailsFirstRun = true;

        public static void Init()
        {
            Load();
            CleanDirectories();
        }

        static void MonitorJobs(object dummy)
        {


            _monitorTimer.Change(1000, Timeout.Infinite);
        }

        static async void UpdateDetails(object dummy)
        {
            //Logic:
            //  Run this method once a minute
            //  For any info files that haven't been checked in 5 minutes, update
            //  If first run, stagger touch times for future updates

            int stagger = 0;

            foreach(var job in Jobs.Where(item => item.MediaType == MediaTypes.Movie))
            {
                try
                {
                    var jsonFile = new FileInfo(job.Files.First(item => item.Suffix == "json").LocalFile);
                    if(jsonFile.LastWriteTime.AddMinutes(5) < DateTime.Now)
                    {
                        string jsonData1 = File.ReadAllText(jsonFile.FullName);
                        var response = await App.API.Movies.GetDetailsAsync(job.MediaId);
                        if (response.Success)
                        {
                            string jsonData2 = JsonConvert.SerializeObject(response.Data);
                            if (jsonData1 != jsonData2)
                                AddOrUpdateMovie(response.Data);
                        }
                        if (_detailsFirstRun)
                            jsonFile.LastWriteTime = DateTime.Now.AddMinutes(++stagger);
                        else
                            jsonFile.LastWriteTime = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                if (stagger == 5)
                    stagger = 0;
            }

            foreach (var job in Jobs.Where(item => item.MediaType == MediaTypes.Series))
            {
                try
                {
                    var jsonFile = new FileInfo(job.Files.First(item => item.Suffix == "json").LocalFile);
                    if (jsonFile.LastWriteTime.AddMinutes(5) < DateTime.Now)
                    {
                        string jsonData1 = File.ReadAllText(jsonFile.FullName);
                        var response = await App.API.Series.GetDetailsAsync(job.MediaId);
                        if (response.Success)
                        {
                            string jsonData2 = JsonConvert.SerializeObject(response.Data);
                            if (jsonData1 != jsonData2)
                                AddOrUpdateSeries(response.Data, job.ItemsCount);
                        }
                        if (_detailsFirstRun)
                            jsonFile.LastWriteTime = DateTime.Now.AddMinutes(++stagger);
                        else
                            jsonFile.LastWriteTime = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                if (stagger == 5)
                    stagger = 0;
            }


            foreach (var job in Jobs.Where(item => item.MediaType == MediaTypes.Playlist))
            {
                try
                {
                    var jsonFile = new FileInfo(job.Files.First(item => item.Suffix == "json").LocalFile);
                    if (jsonFile.LastWriteTime.AddMinutes(5) < DateTime.Now)
                    {
                        string jsonData1 = File.ReadAllText(jsonFile.FullName);
                        var response = await App.API.Playlists.GetDetailsAsync(job.MediaId);
                        if (response.Success)
                        {
                            string jsonData2 = JsonConvert.SerializeObject(response.Data);
                            if (jsonData1 != jsonData2)
                                AddOrUpdatePlaylist(response.Data, job.ItemsCount);
                        }
                        if (_detailsFirstRun)
                            jsonFile.LastWriteTime = DateTime.Now.AddMinutes(++stagger);
                        else
                            jsonFile.LastWriteTime = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                if (stagger == 5)
                    stagger = 0;
            }

            _detailsFirstRun = false;
            _detailsTimer.Change(60000, Timeout.Infinite);
        }

        public static IReadOnlyList<Job> Jobs
        {
            get
            {
                lock (_locker)
                {
                    return _jobs.ToList();
                }
            }
        }

        public static void AddOrUpdateMovie(DetailedMovie movie)
        {
            bool isNew = false;
            var job = Jobs.FirstOrDefault(item => item.MediaId == movie.Id);
            if (job == null)
            {
                isNew = true;
                job = new Job
                {
                    MediaType = MediaTypes.Movie,
                    MediaId = movie.Id
                };
            }

            //Manually save the .json file
            UpdateJobFile(job, "json", null, false);
            File.WriteAllText(job.Files[0].LocalFile, JsonConvert.SerializeObject(movie));

            UpdateJobFile(job, "poster.jpg", movie.ArtworkUrl, false);
            UpdateJobFile(job, "mp4", movie.VideoUrl, true);
            UpdateOptionalJobFile(job, "backdrop.jpg", movie.BackdropUrl, false);
            UpdateOptionalJobFile(job, "bif", movie.BifUrl, false);
            UpdateExternalSubtitles(job, movie.ExternalSubtitles);

            if(isNew)
                lock (_locker) { _jobs.Add(job); }

            Save();
        }

        public static void AddOrUpdateSeries(DetailedSeries series, int itemCount)
        {
            bool isNew = false;
            var job = Jobs.FirstOrDefault(item => item.MediaId == series.Id);
            if (job == null)
            {
                isNew = true;
                job = new Job
                {
                    MediaType = MediaTypes.Series,
                    MediaId = series.Id,
                };
            }

            job.ItemsCount = itemCount;

            //Manually save the .json file
            UpdateJobFile(job, "json", null, false);
            File.WriteAllText(job.Files[0].LocalFile, JsonConvert.SerializeObject(series));

            UpdateJobFile(job, "poster.jpg", series.ArtworkUrl, false);
            UpdateOptionalJobFile(job, "backdrop.jgp", series.BackdropUrl, false);
            UpdateEpisodes(job, series);

            if(isNew)
                lock (_locker) { _jobs.Add(job); }

            Save();
        }

        static void UpdateEpisodes(Job job, DetailedSeries series)
        {
            job.SubJobs.RemoveAll(jobItem => !series.Episodes.Select(episodeItem => episodeItem.Id).Contains(jobItem.MediaId));
            foreach (var ep in series.Episodes)
            {                
                var subJob = job.SubJobs.FirstOrDefault(item => item.MediaId == ep.Id);
                if(subJob == null)
                {
                    subJob = new Job
                    {
                        MediaId = ep.Id,
                        MediaType = MediaTypes.Episode
                    };
                    job.SubJobs.Add(subJob);
                }

                subJob.Season = ep.SeasonNumber;
                subJob.Episode = ep.EpisodeNumber;

                UpdateJobFile(subJob, "mp4", ep.VideoUrl, true);
                UpdateJobFile(subJob, "jpg", ep.ArtworkUrl, false);
                UpdateOptionalJobFile(subJob, "bif", ep.BifUrl, false);
                UpdateExternalSubtitles(subJob, ep.ExternalSubtitles);
            }

            job.SubJobs.Sort((x, y) =>
            {
                int ret = x.Season.CompareTo(y.Season);
                if (x.Season == 0)
                    ret = -ret;
                if (ret == 0)
                    ret = x.Episode.CompareTo(y.Episode);
                return ret;
            });


            var upNext = series.Episodes.FirstOrDefault(item => item.UpNext);
            int activeCnt = 0;
            bool upNextFound = false;
            foreach (var ep in series.Episodes)
            {
                var subJob = job.SubJobs.First(item => item.MediaId == ep.Id);
                var active = false;

                if (upNext == null)
                {
                    if (activeCnt < job.ItemsCount)
                    {
                        activeCnt++;
                        active = true;
                    }
                }
                else
                {
                    if (upNext.Id == ep.Id)
                        upNextFound = true;
                    if (upNextFound)
                    {
                        if (activeCnt < job.ItemsCount)
                        {
                            activeCnt++;
                            active = true;
                        }
                    }
                }

                subJob.Active = active;
            }
        }

        public static void AddOrUpdatePlaylist(DetailedPlaylist playlist, int itemCount)
        {
            bool isNew = false;
            var job = Jobs.FirstOrDefault(item => item.MediaId == playlist.Id);
            if (job == null)
            {
                isNew = true;
                job = new Job
                {
                    MediaType = MediaTypes.Playlist,
                    MediaId = playlist.Id,
                };
            }

            job.ItemsCount = itemCount;

            //Manually save the .json file
            UpdateJobFile(job, "json", null, false);
            File.WriteAllText(job.Files[0].LocalFile, JsonConvert.SerializeObject(playlist));

            UpdateJobFile(job, "poster1.jpg", playlist.ArtworkUrl1, false);
            UpdateOptionalJobFile(job, "poster2.jpg", playlist.ArtworkUrl2, false);
            UpdateOptionalJobFile(job, "poster3.jpg", playlist.ArtworkUrl3, false);
            UpdateOptionalJobFile(job, "poster4.jpg", playlist.ArtworkUrl4, false);
            UpdatePlaylistItems(job, playlist);

            if(isNew)
                lock (_locker) { _jobs.Add(job); }

            Save();
        }

        static void UpdatePlaylistItems(Job job, DetailedPlaylist playlist)
        {
            job.SubJobs.RemoveAll(jobItem => !playlist.Items.Select(playlistItem => playlistItem.MediaId).Contains(jobItem.MediaId));
            foreach (var pli in playlist.Items)
            {
                var subJob = job.SubJobs.FirstOrDefault(item => item.MediaId == pli.Id);
                if (subJob == null)
                {
                    subJob = new Job
                    {
                        MediaId = pli.MediaId,
                        MediaType = pli.MediaType
                    };
                    job.SubJobs.Add(subJob);
                }

                subJob.Index = pli.Index;

                UpdateJobFile(subJob, "mp4", pli.VideoUrl, true);
                UpdateJobFile(subJob, "jpg", pli.ArtworkUrl, false);
                UpdateOptionalJobFile(subJob, "bif", pli.BifUrl, false);
                UpdateExternalSubtitles(subJob, pli.ExternalSubtitles);
            }

            job.SubJobs.Sort((x, y) => x.Index.CompareTo(y.Index));

            int activeCnt = 0;
            foreach(var subJob in job.SubJobs)
            {
                bool active = false;
                if (subJob.Index >= playlist.CurrentIndex && activeCnt < job.ItemsCount)
                {
                    active = true;
                    activeCnt++;
                }
                subJob.Active = active;
            }            
        }

        static void UpdateExternalSubtitles(Job job, IEnumerable<ExternalSubtitle> subs)
        {
            if (subs == null)
            {
                job.Files.RemoveAll(item => item.Suffix.EndsWith(".srt"));
            }
            else
            {
                job.Files.RemoveAll(fileItem => !subs.Select(subtitleItem => subtitleItem.SafeFilename()).Contains(fileItem.Suffix));
                foreach (var sub in subs)
                    UpdateOptionalJobFile(job, sub.SafeFilename(), sub.Url, false);
            }
        }

        static void UpdateOptionalJobFile(Job job, string suffix, string url, bool isVideo)
        {
            if (string.IsNullOrWhiteSpace(url))
                job.Files.RemoveAll(item => item.Suffix == suffix);
            else
                UpdateJobFile(job, suffix, url, isVideo);
        }

        static void UpdateJobFile(Job job, string suffix, string url, bool isVideo)
        {
            var jobFile = job.Files.FirstOrDefault(item => item.Suffix == suffix);
            if(jobFile == null)
            {
                jobFile = new JobFile
                {
                    MediaId = job.MediaId,
                    Suffix = suffix,
                    Url = url,
                    IsVideo = isVideo
                };
                job.Files.Add(jobFile);
            }
            else
            {
                jobFile.Url = url;
                jobFile.IsVideo = isVideo;
            }
        }



        public static void Delete(int mediaId)
        {
            var job = Jobs.FirstOrDefault(item => item.MediaId == mediaId);
            if (job != null)
            {
                foreach (var jobFile in job.Files)
                {
                    var downloads = _manager.Queue.Where(item => item.MediaId == mediaId).ToList();
                    downloads.ForEach(item => _manager.Abort(item));
                    TryDeleteFile(jobFile.TempFile);
                    TryDeleteFile(jobFile.LocalFile);
                }

                foreach (var jobFile in job.SubJobs.SelectMany(item => item.Files))
                {
                    var downloads = _manager.Queue.Where(item => item.MediaId == mediaId).ToList();
                    downloads.ForEach(item => _manager.Abort(item));
                    TryDeleteFile(jobFile.TempFile);
                    TryDeleteFile(jobFile.LocalFile);
                }
            }

            lock (_locker)
            {
                try { _jobs.RemoveAll(item => item.MediaId == mediaId); }
                catch { }
            }

            Save();
            CleanDirectories();
        }





        static void CleanDirectories()
        {
            var jobs = Jobs;
            var jobFiles = jobs.SelectMany(item => item.Files).ToList();
            jobFiles.AddRange(jobs.SelectMany(item => item.SubJobs).SelectMany(item => item.Files));
            var safeFiles = jobFiles.Select(item => item.TempFile).ToList();
            safeFiles.AddRange(jobFiles.Select(item => item.LocalFile).ToList());

            foreach (var file in Directory.GetFiles(_manager.TempDirectory))
                if (!safeFiles.Contains(file))
                    TryDeleteFile(file);

            foreach (var file in Directory.GetFiles(_manager.DownloadDirectory))
                if (!safeFiles.Contains(file))
                    TryDeleteFile(file);

        }

        static bool TryDeleteFile(string file)
        {
            if (!File.Exists(file))
                return true;

            try
            {
                File.Delete(file);
                return true;
            }
            catch { }

            return false;
        }

        static void Load()
        {
            lock (_locker)
            {
                try { _jobs = JsonConvert.DeserializeObject<List<Job>>(File.ReadAllText(_filename)); }
                catch { _jobs = new List<Job>(); }
            }
        }

        static void Save()
        {
            File.WriteAllText(_filename, JsonConvert.SerializeObject(Jobs));
        }
    }
}
