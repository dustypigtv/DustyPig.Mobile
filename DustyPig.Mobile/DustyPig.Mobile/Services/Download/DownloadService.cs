/*
    I *could* go to the trouble of implementing a sqlite database, but since this is is so lightweight...
    I'm just gonna have a serialized json file and a lock
*/
using DustyPig.API.v3.Models;
using DustyPig.Mobile.CrossPlatform.DownloadManager;
using DustyPig.Mobile.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DustyPig.Mobile.Services.Download
{
    static class DownloadService
    {
        static readonly JobList _jobList = new JobList();
        static readonly IDownloadManager _manager = DependencyService.Get<IDownloadManager>();
        static readonly object _locker = new object();
        static readonly Timer _monitorTimer = new Timer(new TimerCallback(MonitorJobsCallback));
        static readonly Timer _detailsTimer = new Timer(new TimerCallback(UpdateDetailsCallback));
        static bool _firstUpdateRan = false;

        public static void Init()
        {            
            //Faster startup
            Task.Run(() =>
            {
                _manager.DownloadUpdated += NativeManager_DownloadUpdated;
                _detailsTimer.Change(1000, Timeout.Infinite);
                _monitorTimer.Change(1000, Timeout.Infinite);
            });
        }

        


        static void NativeManager_DownloadUpdated(object sender, IDownload e)
        {
            if (!App.LoggedIn)
                return;

            var file = FindFile(e);
            if (file != null)
                file.Percent = e.Percent;

            FindJob(e.MediaId)?.Update();

          
            switch (e.Status)
            {
                case DownloadStatus.CANCELED:
                case DownloadStatus.COMPLETED:
                case DownloadStatus.INITIALIZED:
                case DownloadStatus.PAUSED:
                case DownloadStatus.PENDING:
                    Console.WriteLine("*** DOWNLOAD {0} ***", e.Status);
                    break;


                case DownloadStatus.FAILED:
                    Console.WriteLine("*** DOWNLOAD FAILED ***");
                    Console.WriteLine(e.StatusDetails);

                    switch (e.StatusDetails)
                    {
                        case "Error.CannotResume":
                           _manager.Abort(e);
                            break;

                        default:
                            break;
                    }                                  
                    break;

                case DownloadStatus.RUNNING:

                    if (e.TotalBytesExpected > 0)
                        Console.WriteLine("*** {0} PERCENT: {1:0.00%} ***", e.MediaId, (double)e.TotalBytesWritten / (double)e.TotalBytesExpected);
                    break;
            }

        }





        static void MonitorJobsCallback(object dummy)
        {
            try { MonitorJobs(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            
            _monitorTimer.Change(1000, Timeout.Infinite);
        }

        static void MonitorJobs()
        {
            //This is the meat: It synchronizes what I WANT to be downloaded with the native download manager

            if (!App.LoggedIn)
                return;

            if (!_firstUpdateRan)
                return;

            //Remove completed downloads from native manager
            foreach (var download in _manager.GetDownloads())
                if (download.Status == DownloadStatus.COMPLETED)
                    if (!string.IsNullOrWhiteSpace(download.Suffix))
                        if (!string.IsNullOrWhiteSpace(CheckForLocalFile(download.MediaId, download.Suffix)))
                            _manager.Abort(download);

            //Delete any downlods in the native manager that aren't in this manager - do it the slow way!!!
            foreach (var download in _manager.GetDownloads())
            {
                var valid = _jobList.Jobs.SelectMany(item => item.Files)
                    .Where(item => item.Url == download.Url)
                    .Any();

                if (!valid)
                    _manager.Abort(download);
            }


            //Clean files - do it the slow way!!!
            foreach (var file in Directory.GetFiles(_manager.TempDirectory))
                if (!_jobList.Jobs.SelectMany(item => item.Files).Select(item => item.TempFile()).Contains(file))
                    TryDeleteFile(file);

            foreach (var file in Directory.GetFiles(_manager.DownloadDirectory))
                if (!_jobList.Jobs.SelectMany(item => item.Files).Select(item => item.LocalFile()).Contains(file))
                    TryDeleteFile(file);



            //Add any jobs that are missing from the native manager
            foreach (var job in _jobList.Jobs)
            {
                foreach (var jobFile in job.Files.Where(item => item.Suffix != "json"))
                {
                    if (!File.Exists(jobFile.LocalFile()))
                    {
                        var download = _manager.GetDownloads()
                            .Where(item => item.Url == jobFile.Url)
                            .FirstOrDefault();

                        if (download == null)
                            _manager.Start(jobFile.MediaId, jobFile.Url, jobFile.Suffix, Settings.DownloadOverCellular);
                    }
                }
            }
        }







        static async void UpdateDetailsCallback(object dummy)
        {
            try { await UpdateDetails(); }
            catch { }

            if (_firstUpdateRan)
                _detailsTimer.Change(60000, Timeout.Infinite);
            else
                _detailsTimer.Change(1000, Timeout.Infinite);
        }

        static async Task UpdateDetails()
        {
            //Logic:
            //  Run this method once a minute
            //  Update date the most stale job
            //  Update any jobs over 5 min old

            if (!App.LoggedIn)
                return;
           
            var jobFileTimes = new List<KeyValuePair<Job, DateTime>>();
            foreach (var job in _jobList.Jobs)
            {
                var jobFile = job.Files.FirstOrDefault(item => item.Suffix == "json");
                if(jobFile == null)
                {
                    jobFileTimes.Add(new KeyValuePair<Job, DateTime>(job, DateTime.Now.AddDays(-1)));
                }
                else
                {
                    var jsonFile = new FileInfo(job.Files.First(item => item.Suffix == "json").LocalFile());
                    if(jsonFile.Exists)
                        jobFileTimes.Add(new KeyValuePair<Job, DateTime>(job, jsonFile.LastWriteTime));
                }
            }
            if (jobFileTimes.Count == 0)
            {
                _firstUpdateRan = true;
                return;
            }

            jobFileTimes.Sort((x, y) => x.Value.CompareTo(y.Value));

            var jobsToUpdate = new List<Job>();
            jobsToUpdate.Add(jobFileTimes[0].Key);
            for (int i = 1; i < jobFileTimes.Count; i++)
                if (jobFileTimes[i].Value.AddMinutes(5) < DateTime.Now)
                    jobsToUpdate.Add(jobFileTimes[i].Key);

            foreach(var job in jobsToUpdate)
            {
                try
                {
                    switch (job.MediaType)
                    {
                        case MediaTypes.Movie:
                            var movieResponse = await App.API.Movies.GetDetailsAsync(job.MediaId);
                            InternalAddOrUpdateMovie(movieResponse.Data);
                            break;

                        case MediaTypes.Series:
                            var seriesResponse = await App.API.Series.GetDetailsAsync(job.MediaId);
                            InternalAddOrUpdateSeries(seriesResponse.Data, job.ItemsCount);
                            break;

                        case MediaTypes.Playlist:
                            var playlistResponse = await App.API.Playlists.GetDetailsAsync(job.MediaId);
                            InternalAddOrUpdatePlaylist(playlistResponse.Data, job.ItemsCount);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            
        }

        





        static string ReadFile(string filename)
        {
            lock (_locker)
            {
                try { return File.ReadAllText(filename); }
                catch { return null; }
            }
        }

        static void SaveFile(string filename, string data)
        {
            lock (_locker)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                File.WriteAllText(filename, data);
            }
        }



        public static void AddMovie(DetailedMovie movie)
        {
            //Fire & forget
            Task.Run(() => InternalAddOrUpdateMovie(movie));
        }

        public static void AddOrUpdateSeries(DetailedSeries series, int itemCount)
        {
            //Fire & forget
            Task.Run(() => InternalAddOrUpdateSeries(series, itemCount));
        }

        public static void AddOrUpdatePlaylist(DetailedPlaylist playlist, int itemCount)
        {
            //Fire & forget
            Task.Run(() => InternalAddOrUpdatePlaylist(playlist, itemCount));
        }




        static void InternalAddOrUpdateMovie(DetailedMovie movie)
        {
            bool changed = false;

            var job = _jobList.Jobs.FirstOrDefault(item => item.MediaId == movie.Id);
            if (job == null)
            {
                job = new Job
                {
                    MediaType = MediaTypes.Movie,
                    MediaId = movie.Id
                };
                _jobList.AddJob(job);
                changed = true;
            }

            //Manually save the .json file
            changed |= AddOrUpdateJobFile(job, job.MediaId, "json", null, false);
            File.WriteAllText(job.Files[0].LocalFile(), JsonConvert.SerializeObject(movie));

            changed |= AddOrUpdateJobFile(job, job.MediaId, "poster.jpg", movie.ArtworkUrl, false);
            changed |= AddOrUpdateJobFile(job, job.MediaId, "mp4", movie.VideoUrl, true);
            changed |= AddOrUpdateOptionalJobFile(job, job.MediaId, "backdrop.jpg", movie.BackdropUrl);
            changed |= AddOrUpdateOptionalJobFile(job, job.MediaId, "bif", movie.BifUrl);
            changed |= AddOrUpdateExternalSubtitles(job, job.MediaId, movie.ExternalSubtitles);

            if(changed)
                _jobList.Save();
        }

        static void InternalAddOrUpdateSeries(DetailedSeries series, int itemCount)
        {
            bool changed = false;

            var job = _jobList.Jobs.FirstOrDefault(item => item.MediaId == series.Id);
            if (job == null)
            {
                job = new Job
                {
                    MediaType = MediaTypes.Series,
                    MediaId = series.Id,
                };
                _jobList.AddJob(job);
                changed = true;
            }

            if (job.ItemsCount != itemCount)
                changed = true;
            job.ItemsCount = itemCount;

            //Manually save the .json file
            changed |= AddOrUpdateJobFile(job, job.MediaId, "json", null, false);
            File.WriteAllText(job.Files.First(item => item.Suffix == "json").LocalFile(), JsonConvert.SerializeObject(series));

            changed |= AddOrUpdateJobFile(job, job.MediaId, "poster.jpg", series.ArtworkUrl, false);
            changed |= AddOrUpdateOptionalJobFile(job, job.MediaId, "backdrop.jpg", series.BackdropUrl);
            changed |= AddOrUpdateEpisodes(job, series);

            if (changed)
                _jobList.Save();
        }

        static void InternalAddOrUpdatePlaylist(DetailedPlaylist playlist, int itemCount)
        {
            bool changed = false;

            var job = _jobList.Jobs.FirstOrDefault(item => item.MediaId == playlist.Id);
            if (job == null)
            {
                job = new Job
                {
                    MediaType = MediaTypes.Playlist,
                    MediaId = playlist.Id,
                };
                _jobList.AddJob(job);
                changed = true;
            }

            if (job.ItemsCount != itemCount)
                changed = true;
            job.ItemsCount = itemCount;

            //Manually save the .json file
            changed |= AddOrUpdateJobFile(job, job.MediaId, "json", null, false);
            File.WriteAllText(job.Files.First(item => item.Suffix == "json").LocalFile(), JsonConvert.SerializeObject(playlist));

            changed |= AddOrUpdateJobFile(job, job.MediaId, "poster1.jpg", playlist.ArtworkUrl1, false);
            changed |= AddOrUpdateOptionalJobFile(job, job.MediaId, "poster2.jpg", playlist.ArtworkUrl2);
            changed |= AddOrUpdateOptionalJobFile(job, job.MediaId, "poster3.jpg", playlist.ArtworkUrl3);
            changed |= AddOrUpdateOptionalJobFile(job, job.MediaId, "poster4.jpg", playlist.ArtworkUrl4);
            changed |= AddOrUpdatePlaylistItems(job, playlist);

            if (changed)
                _jobList.Save();
        }

        static bool AddOrUpdateEpisodes(Job job, DetailedSeries series)
        {
            series.Episodes.Sort((x, y) =>
            {
                int sortRet = x.SeasonNumber.CompareTo(y.SeasonNumber);
                if (sortRet == 0)
                    sortRet = x.EpisodeNumber.CompareTo(y.EpisodeNumber);
                return sortRet;
            });

            var toDownlod = new List<DetailedEpisode>();

            var upNext = series.Episodes.FirstOrDefault(item => item.UpNext);
            if (upNext == null)
                upNext = series.Episodes.FirstOrDefault();
            if(upNext != null)
            {
                bool upNextFound = false;
                foreach (var episode in series.Episodes)
                {
                    if (episode == upNext)
                        upNextFound = true;

                    if (upNextFound)
                        toDownlod.Add(episode);
                    if (toDownlod.Count == job.ItemsCount)
                        break;
                }
            }

            var validIds = toDownlod.Select(item => item.Id).ToList();
            validIds.Add(job.MediaId);
            bool ret = job.RemoveAllFiles(item => !validIds.Contains(item.MediaId));

            foreach (var episode in toDownlod)
            {
                ret |= AddOrUpdateJobFile(job, episode.Id, "mp4", episode.VideoUrl, true);
                ret |= AddOrUpdateJobFile(job, episode.Id, "jpg", episode.ArtworkUrl, false);
                ret |= AddOrUpdateOptionalJobFile(job, episode.Id, "bif", episode.BifUrl);
                ret |= AddOrUpdateExternalSubtitles(job, episode.Id, episode.ExternalSubtitles);
            }

            return ret;
        }

        static bool AddOrUpdatePlaylistItems(Job job, DetailedPlaylist playlist)
        {
            playlist.Items.Sort((x, y) => x.Index.CompareTo(y.Index));

            var toDownlod = new List<PlaylistItem>();
            foreach(var item in playlist.Items)
            {
                if (item.Index >= playlist.CurrentIndex)
                    toDownlod.Add(item);
                if (toDownlod.Count == job.ItemsCount)
                    break;
            }

            var validIds = toDownlod.Select(item => item.MediaId).ToList();
            validIds.Add(job.MediaId);
            bool ret = job.RemoveAllFiles(item => !validIds.Contains(item.MediaId));
            
            foreach(var pli in toDownlod)
            {
                ret |= AddOrUpdateJobFile(job, pli.MediaId, "mp4", pli.VideoUrl, true);
                ret |= AddOrUpdateJobFile(job, pli.MediaId, "jpg", pli.ArtworkUrl, false);
                ret |= AddOrUpdateOptionalJobFile(job, pli.MediaId, "bif", pli.BifUrl);
                ret |= AddOrUpdateExternalSubtitles(job, pli.MediaId, pli.ExternalSubtitles);
            }

            return ret;
        }

        static bool AddOrUpdateExternalSubtitles(Job job, int mediaId, IEnumerable<ExternalSubtitle> subs)
        {
            if (subs == null)
            {
                return job.RemoveAllFiles(item => item.Suffix.EndsWith(".srt"));
            }
            else
            {
                bool ret = job.RemoveAllFiles(fileItem => fileItem.Suffix.EndsWith(".srt") && !subs.Select(subtitleItem => subtitleItem.SafeFilename()).Contains(fileItem.Suffix));
                foreach (var sub in subs)
                    ret |= AddOrUpdateJobFile(job, mediaId, sub.SafeFilename(), sub.Url, false);
                return ret;
            }
        }

        static bool AddOrUpdateOptionalJobFile(Job job, int mediaId, string suffix, string url)
        {
            if(string.IsNullOrWhiteSpace(url))
            {
                var jobFile = job.Files
                     .Where(item => item.MediaId == mediaId)
                     .Where(item => item.Suffix == suffix)
                     .FirstOrDefault();

                if (jobFile == null)
                    return false;

                job.RemoveFile(jobFile);
                return true;
            }
            else
            {
                return AddOrUpdateJobFile(job, mediaId, suffix, url, false);
            }
        }

        static bool AddOrUpdateJobFile(Job job, int mediaId, string suffix, string url, bool isVideo)
        {
            var jobFile = job.Files
                .Where(item => item.MediaId == mediaId)
                .Where(item => item.Suffix == suffix)
                .FirstOrDefault();

            if (jobFile == null)
            {
                jobFile = new JobFile
                {
                    MediaId = job.MediaId,
                    Suffix = suffix,
                    Url = url,
                    IsVideo = isVideo
                };
                job.AddFile(jobFile);
                return true;
            }
            else
            {
                bool ret = jobFile.Url != url || jobFile.IsVideo != isVideo;
                jobFile.Url = url;
                jobFile.IsVideo = isVideo;
                return ret;
            }
        }



        public static void Delete(int mediaId)
        {
            //Fire & forget
            Task.Run(() =>
            {
                var job = _jobList.Jobs.FirstOrDefault(item => item.MediaId == mediaId);
                if (job != null)
                {
                    foreach (var jobFile in job.Files)
                    {
                        var downloads = _manager.GetDownloads().Where(item => item.MediaId == mediaId).ToList();
                        downloads.ForEach(item => _manager.Abort(item));
                        TryDeleteFile(jobFile.TempFile());
                        TryDeleteFile(jobFile.LocalFile());
                    }

                    _jobList.RemoveJob(job);
                    _jobList.Save();
                }
            });
        }




        /// <summary>
        /// Top level
        /// </summary>
        static Job FindJob(int mediaId) => _jobList.Jobs.FirstOrDefault(item => item.MediaId == mediaId);

        static JobFile FindFile(IDownload download)
        {
            return _jobList.Jobs.SelectMany(item => item.Files)
                .Where(item => item.Url == download.Url)
                .FirstOrDefault();
        }



        /// <summary>
        /// Top level check - Movie, Series or Playlist
        /// </summary>
        public static Task<(JobStatus Status, int Percent, int ItemCount)> GetStatusAsync(int mediaId)
        {
            return Task.Run(() =>
            {
                var job = FindJob(mediaId);
                if (job == null)
                    return (JobStatus.NotDownloaded, 0, 0);

                bool done = true;
                foreach (var jobFile in job.Files)
                    if (!File.Exists(jobFile.LocalFile()))
                    {
                        done = false;
                        break;
                    }
                
                if (done)
                    return (JobStatus.Downloaded, 100, job.ItemsCount);

                return (JobStatus.Downloading, job.Percent, job.ItemsCount);
            });
        }


        public static string CheckForLocalDetails(int mediaId) => CheckForLocalFile(mediaId, "json");

        public static string CheckForLocalPoster(int mediaId) => CheckForLocalFile(mediaId, "poster.jpg");

        public static string CheckForLocalBackdrop(int mediaId) => CheckForLocalFile(mediaId, "backdrop.jpg");

        public static string CheckForLocalVideo(int mediaId) => CheckForLocalFile(mediaId, "mp4");

        public static string CheckForLocalBif(int mediaId) => CheckForLocalFile(mediaId, "bif");

        public static string CheckForLocalSubtitle(int mediaId, ExternalSubtitle sub) => CheckForLocalFile(mediaId, sub.SafeFilename());

        public static string CheckForLocalScreenshot(int mediaId) => CheckForLocalFile(mediaId, "jpg");

        static string CheckForLocalFile(int mediaId, string suffix)
        {
            //Only return local file if there is a download job, because if not
            //Then the local file will soon be deleted            
            var job = _jobList.Jobs.FirstOrDefault(item => item.MediaId == mediaId);
            if (job == null)
                return null;

            string localFile = Path.Combine(_manager.DownloadDirectory, $"{mediaId}.{suffix}");
            if (File.Exists(localFile))
                return localFile;

            return null;
        }

        public static Task<T> TryLoadDetailsAsync<T>(int mediaId)
        {
            return Task<T>.Run(() =>
            {
                try
                {
                    string local = CheckForLocalDetails(mediaId);
                    if (local == null)
                        return default(T);
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(local));
                }
                catch
                {
                    return default(T);
                }
            });
        }

        public static void Reset()
        {
            //Fire & forget
            Task.Run(() =>
            {
                _jobList.ClearJobs();
                _jobList.Save();
            });
        }
                
        static bool TryDeleteFile(string file)
        {            
            try
            {
                if (!File.Exists(file))
                    return true;

                if (new FileInfo(file).LastWriteTime.AddMinutes(1) > DateTime.Now)
                    return false;

                File.Delete(file);
                return true;
            }
            catch { }

            return false;
        }


    }
}
