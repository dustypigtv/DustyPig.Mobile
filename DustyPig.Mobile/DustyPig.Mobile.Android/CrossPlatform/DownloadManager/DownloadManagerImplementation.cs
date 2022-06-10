using Android.App;
using Android.Content;
using Android.Database;
using DustyPig.Mobile.CrossPlatform.DownloadManager;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Xamarin.Essentials;

namespace DustyPig.Mobile.Droid.CrossPlatform.DownloadManager
{
    public class DownloadManagerImplementation : IDownloadManager
    {
        public static readonly DownloadManagerImplementation Current = new DownloadManagerImplementation();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler<IDownload> DownloadUpdated;

        private Android.OS.Handler _downloadWatcherHandler;
        private Java.Lang.Runnable _downloadWatcherHandlerRunnable;
        private readonly Android.App.DownloadManager _downloadManager = (Android.App.DownloadManager)Application.Context.GetSystemService(Context.DownloadService);

        private DownloadManagerImplementation()
        {
            //Skip media scanner in this app
            using (var fs = File.Create(Path.Combine(Root, ".nomedia"))) { }

            // Add all items to the Queue that are pending, paused or running
            ReinitializeAllFiles(new Action<ICursor>(cursor => ReinitializeFile(cursor)));

            // Check sequentially if parameters for any of the registered downloads changed
            StartDownloadWatcher();
        }


        private readonly IList<IDownload> _queue = new List<IDownload>();
        public IEnumerable<IDownload> GetQueue()
        {
            lock (_queue)
            {
                return _queue.ToList();
            }
        }

        public IDownload CreateDownload(string url, int id, string suffix) => new DownloadImplementation(url, id, suffix);

        public void Start(IDownload download, bool mobileNetworkAllowed)
        {
            var dl = (DownloadImplementation)download;
            dl.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.INITIALIZED);
            dl.CalcPercent(0, 0);
            AddFile(dl);
            DownloadUpdated?.Invoke(this, dl);

            string destinationPathName = GetTempPath(download);

            using var downloadUrl = Android.Net.Uri.Parse(download.Url);
            using var request = new Android.App.DownloadManager.Request(downloadUrl);

            var jfile = new Java.IO.File(destinationPathName);
            if (jfile.Exists())
                jfile.Delete();

            request.SetDestinationUri(Android.Net.Uri.FromFile(jfile));
            request.SetAllowedOverMetered(mobileNetworkAllowed);
            request.SetNotificationVisibility(DownloadVisibility.Hidden);
           
            dl.AndroidId = _downloadManager.Enqueue(request);       
        }

        public void Abort(IDownload download)
        {
            var dl = (DownloadImplementation)download;
            _downloadManager.Remove(dl.AndroidId);
            if (dl.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.CANCELED))
                DownloadUpdated?.Invoke(this, download);
            RemoveFile(dl);
        }

        public void AbortAll()
        {
            foreach (var file in GetQueue())
                Abort(file);
        }

        void ReinitializeAllFiles(Action<ICursor> runnable)
        {
            // Reinitialize downloads that were started before the app was terminated or suspended
            var query = new Android.App.DownloadManager.Query();
            query.SetFilterByStatus(
                Android.App.DownloadStatus.Paused |
                Android.App.DownloadStatus.Pending |
                Android.App.DownloadStatus.Running
            );


            using var cursor = _downloadManager.InvokeQuery(query);
            while (cursor != null && cursor.MoveToNext())
            {
                runnable.Invoke(cursor);
            }
            cursor?.Close();
        }

        void ReinitializeFile(ICursor cursor)
        {
            try
            {
                var download = new DownloadImplementation(cursor);
                AddFile(download);
                UpdateDownloadProperties(cursor, download);
            }
            catch { }
        }

        void StartDownloadWatcher()
        {
            // Create an instance for a runnable-handler
            _downloadWatcherHandler = new Android.OS.Handler(Android.OS.Looper.MainLooper);

            // Create a runnable, restarting itself to update every file in the queue
            _downloadWatcherHandlerRunnable = new Java.Lang.Runnable(() =>
            {
                var downloads = GetQueue().Cast<DownloadImplementation>();

                foreach (var dl in downloads)
                {
                    var query = new Android.App.DownloadManager.Query();
                    query.SetFilterById(dl.AndroidId);

                    using var cursor = _downloadManager.InvokeQuery(query);
                    if (cursor != null && cursor.MoveToNext())
                        UpdateDownloadProperties(cursor, dl);
                    else
                        //This file is not listed in the native manager
                        RemoveFile(dl);
                    
                    cursor?.Close();
                }

                _downloadWatcherHandler.PostDelayed(_downloadWatcherHandlerRunnable, 250);
            });

            // Start this handler immediately
            _downloadWatcherHandler.PostDelayed(_downloadWatcherHandlerRunnable, 0);
        }

        /**
         * Update the properties for a file by it's cursor.
         * This method should be called in an interval and on reinitialization.
         */
        public void UpdateDownloadProperties(ICursor cursor, DownloadImplementation download)
        {
            var expected = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnTotalSizeBytes));
            var written = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnBytesDownloadedSoFar));
            bool changed = download.CalcPercent(expected, written);

            switch ((Android.App.DownloadStatus)cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnStatus)))
            {
                case Android.App.DownloadStatus.Successful:
                    if (MoveFile(download))
                        changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.COMPLETED);
                    //else
                    //    changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.Temp file missing");
                    break;

                case Android.App.DownloadStatus.Failed:
                    var reasonFailed = cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnReason));
                    if (reasonFailed < 600)
                    {
                        changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.HttpCode: " + reasonFailed);
                    }
                    else
                    {
                        switch ((DownloadError)reasonFailed)
                        {
                            case DownloadError.CannotResume:
                                changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.CannotResume");
                                break;
                            case DownloadError.DeviceNotFound:
                                changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.DeviceNotFound");
                                break;
                            case DownloadError.FileAlreadyExists:
                                changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.FileAlreadyExists");
                                break;
                            case DownloadError.FileError:
                                changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.FileError");
                                break;
                            case DownloadError.HttpDataError:
                                changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.HttpDataError");
                                break;
                            case DownloadError.InsufficientSpace:
                                changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.InsufficientSpace");
                                break;
                            case DownloadError.TooManyRedirects:
                                changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.TooManyRedirects");
                                break;
                            case DownloadError.UnhandledHttpCode:
                                changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.UnhandledHttpCode");
                                break;
                            case DownloadError.Unknown:
                                changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.Unknown");
                                break;
                            default:
                                changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.Unregistered: " + reasonFailed);
                                break;
                        }
                    }
                    break;

                case Android.App.DownloadStatus.Paused:
                    var reasonPaused = cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnReason));
                    switch ((DownloadPausedReason)reasonPaused)
                    {
                        case DownloadPausedReason.QueuedForWifi:
                            changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED, "Paused.QueuedForWifi");
                            break;
                        case DownloadPausedReason.WaitingToRetry:
                            changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED, "Paused.WaitingToRetry");
                            break;
                        case DownloadPausedReason.WaitingForNetwork:
                            changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED, "Paused.WaitingForNetwork");
                            break;
                        case DownloadPausedReason.Unknown:
                            changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED, "Paused.Unknown");
                            break;
                        default:
                            changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED, "Paused.Unregistered: " + reasonPaused);
                            break;
                    }
                    break;

                case Android.App.DownloadStatus.Pending:
                    changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PENDING);
                    break;

                case Android.App.DownloadStatus.Running:
                    changed |= download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.RUNNING);
                    break;
            }

            if (changed)
                DownloadUpdated?.Invoke(this, download);
        }

        protected internal void AddFile(IDownload download)
        {
            lock (_queue)
            {
                _queue.Add(download);
            }
            CollectionChanged?.Invoke(GetQueue(), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, download));
        }

        protected internal void RemoveFile(IDownload download)
        {
            lock (_queue)
            {
                _queue.Remove(download);
                CollectionChanged?.Invoke(GetQueue(), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, download));
            }
        }

        private string Root
        {
            get
            {
                if (Android.OS.Environment.MediaMounted.Equals(Android.OS.Environment.ExternalStorageState))
                    return Directory.CreateDirectory(Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "offline")).FullName;

                return Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "offline")).FullName;
            }
        }

        public string DownloadDirectory => Directory.CreateDirectory(Path.Combine(Root, "files")).FullName;

        public string TempDirectory => Directory.CreateDirectory(Path.Combine(Root, "tmp")).FullName;

        public string GetLocalPath(IDownload download) => Path.Combine(DownloadDirectory, download.Filename);

        public string GetLocalPath(int mediaId, string suffix) => Path.Combine(DownloadDirectory, $"{mediaId}.{suffix}");
        
        public string GetTempPath(IDownload download) => Path.Combine(TempDirectory, download.Filename);

        public bool MoveFile(IDownload download)
        {
            string temp = GetTempPath(download);
            if (!File.Exists(temp))
                return false;

            string local = GetLocalPath(download);
            if (File.Exists(local))
                File.Delete(local);

            File.Move(temp, local);
            return true;
        }
    }
}