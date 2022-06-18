/*
    I *could* go to the trouble of implementing a sqlite database, but since this is is so lightweight...
    I'm just gonna have a serialized json file and a lock

    Also, trying to mix events and polling was... anyway setup for polling only
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
        static DateTime _lastDetails = DateTime.Now.AddMinutes(-2);

        public static void Init()
        {
            //_jobList.ClearJobs();
            //_jobList.Save();
            //_manager.AbortAll();

            _monitorTimer.Change(1000, Timeout.Infinite);
        }


        public static IReadOnlyList<Job> GetJobs() => _jobList.Jobs;



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

            if (_lastDetails.AddMinutes(1) < DateTime.Now)
            {
                UpdateDetails();
                _lastDetails = DateTime.Now;
            }

            //Delete any downlods in the native manager that aren't in this manager - do it the slow way!!!
            foreach (var download in _manager.GetDownloads())
            {
                var valid = FindFile(download);
                if (valid == null)
                {
                    _manager.Abort(download);
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("*** {0}.{1}: {2} ***", download.MediaId, download.Suffix, download.Status);
                    if (!string.IsNullOrWhiteSpace(download.StatusDetails))
                        System.Diagnostics.Debug.WriteLine(download.StatusDetails);
                    System.Diagnostics.Debug.WriteLine(download.Percent.ToString() + "%");
                    System.Diagnostics.Debug.WriteLine(String.Empty);
#endif

                    valid.Percent = download.Percent;
                    FindJob(download.MediaId)?.Update();
                }
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
                foreach (var jobFile in job.Files.Where(item => item.Suffix != "json").OrderBy(item => item.IsVideo))
                    if (!File.Exists(jobFile.LocalFile()))
                        _manager.Start(jobFile.MediaId, jobFile.Url, jobFile.Suffix, Settings.DownloadOverCellular);
        }

        static void UpdateDetails()
        {
            //Logic:
            //  Run this method once a minute
            //  Update date the most stale job
            //  Update any jobs over 5 min old


            var jobFileTimes = new List<KeyValuePair<Job, DateTime>>();
            foreach (var job in _jobList.Jobs)
            {
                var jobFile = job.Files.FirstOrDefault(item => item.Suffix == "json");
                if (jobFile == null)
                {
                    jobFileTimes.Add(new KeyValuePair<Job, DateTime>(job, DateTime.Now.AddDays(-1)));
                }
                else
                {
                    var jsonFile = new FileInfo(job.Files.First(item => item.Suffix == "json").LocalFile());
                    if (jsonFile.Exists)
                        jobFileTimes.Add(new KeyValuePair<Job, DateTime>(job, jsonFile.LastWriteTime));
                }
            }
            if (jobFileTimes.Count == 0)
                return;

            jobFileTimes.Sort((x, y) => x.Value.CompareTo(y.Value));

            var jobsToUpdate = new List<Job>();
            jobsToUpdate.Add(jobFileTimes[0].Key);
            for (int i = 1; i < jobFileTimes.Count; i++)
                if (jobFileTimes[i].Value.AddMinutes(5) < DateTime.Now)
                    jobsToUpdate.Add(jobFileTimes[i].Key);

            foreach (var job in jobsToUpdate)
            {
                try
                {
                    switch (job.MediaType)
                    {
                        case MediaTypes.Movie:
                            var movieResponse = App.API.Movies.GetDetailsAsync(job.MediaId).Result;
                            InternalAddOrUpdateMovie(movieResponse.Data);
                            break;

                        case MediaTypes.Series:
                            var seriesResponse = App.API.Series.GetDetailsAsync(job.MediaId).Result;
                            InternalAddOrUpdateSeries(seriesResponse.Data, job.ItemsCount);
                            break;

                        case MediaTypes.Playlist:
                            var playlistResponse = App.API.Playlists.GetDetailsAsync(job.MediaId).Result;
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
            lock (_locker)
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

                if (changed)
                    _jobList.Save();
            }
        }

        static void InternalAddOrUpdateSeries(DetailedSeries series, int itemCount)
        {
            lock (_locker)
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
        }

        static void InternalAddOrUpdatePlaylist(DetailedPlaylist playlist, int itemCount)
        {
            lock (_locker)
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
            if (upNext != null)
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
            foreach (var item in playlist.Items)
            {
                if (item.Index >= playlist.CurrentIndex)
                    toDownlod.Add(item);
                if (toDownlod.Count == job.ItemsCount)
                    break;
            }

            var validIds = toDownlod.Select(item => item.MediaId).ToList();
            validIds.Add(job.MediaId);
            bool ret = job.RemoveAllFiles(item => !validIds.Contains(item.MediaId));

            foreach (var pli in toDownlod)
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
            if (string.IsNullOrWhiteSpace(url))
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
                    MediaId = mediaId,
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
                lock (_locker)
                {
                    var job = _jobList.Jobs.FirstOrDefault(item => item.MediaId == mediaId);
                    if (job != null)
                    {
                        _jobList.RemoveJob(job);
                        _jobList.Save();
                    }
                    if (_jobList.Jobs.Count == 0)
                        _manager.AbortAll();
                }
            });
        }




        /// <summary>
        /// Top level
        /// </summary>
        static Job FindJob(int mediaId)
        {
            var ret = _jobList.Jobs.FirstOrDefault(item => item.MediaId == mediaId);
            if (ret != null)
                return ret;

            foreach (var job in _jobList.Jobs)
                foreach (var jobFile in job.Files)
                    if (jobFile.MediaId == mediaId)
                        return job;

            return null;
        }


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

                File.Delete(file);
                return true;
            }
            catch { }

            return false;
        }


    }
}
