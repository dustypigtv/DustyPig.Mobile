﻿using Android.App;
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
        public IEnumerable<IDownload> Queue
        {
            get
            {
                lock (_queue)
                {
                    return _queue.ToList();
                }
            }
        }

        public IDownload CreateDownload(string url, int id, string suffix) => new DownloadImplementation(url, id, suffix);

        public void Start(IDownload download, bool mobileNetworkAllowed)
        {
            var dl = (DownloadImplementation)download;
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
        
            AddFile(dl);
        }

        public void Abort(IDownload download)
        {
            var dl = (DownloadImplementation)download;
            dl.Status = Mobile.CrossPlatform.DownloadManager.DownloadStatus.CANCELED;
            _downloadManager.Remove(dl.AndroidId);
            RemoveFile(dl);
        }

        public void AbortAll()
        {
            foreach (var file in Queue)
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
            var download = new DownloadImplementation(cursor);
            AddFile(download);
            UpdateDownloadProperties(cursor, download);
        }

        void StartDownloadWatcher()
        {
            // Create an instance for a runnable-handler
            _downloadWatcherHandler = new Android.OS.Handler(Android.OS.Looper.MainLooper);

            // Create a runnable, restarting itself to update every file in the queue
            _downloadWatcherHandlerRunnable = new Java.Lang.Runnable(() =>
            {
                var downloads = Queue.Cast<DownloadImplementation>().ToList();

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

                _downloadWatcherHandler.PostDelayed(_downloadWatcherHandlerRunnable, 1000);
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
            download.TotalBytesWritten = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnBytesDownloadedSoFar));
            download.TotalBytesExpected = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnTotalSizeBytes));
            
            switch ((Android.App.DownloadStatus)cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnStatus)))
            {
                case Android.App.DownloadStatus.Successful:
                    download.StatusDetails = default(string);
                    download.Status = Mobile.CrossPlatform.DownloadManager.DownloadStatus.COMPLETED;
                    MoveFile(download);
                    break;

                case Android.App.DownloadStatus.Failed:
                    var reasonFailed = cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnReason));
                    if (reasonFailed < 600)
                    {
                        download.StatusDetails = "Error.HttpCode: " + reasonFailed;
                    }
                    else
                    {
                        switch ((DownloadError)reasonFailed)
                        {
                            case DownloadError.CannotResume:
                                download.StatusDetails = "Error.CannotResume";
                                break;
                            case DownloadError.DeviceNotFound:
                                download.StatusDetails = "Error.DeviceNotFound";
                                break;
                            case DownloadError.FileAlreadyExists:
                                download.StatusDetails = "Error.FileAlreadyExists";
                                break;
                            case DownloadError.FileError:
                                download.StatusDetails = "Error.FileError";
                                break;
                            case DownloadError.HttpDataError:
                                download.StatusDetails = "Error.HttpDataError";
                                break;
                            case DownloadError.InsufficientSpace:
                                download.StatusDetails = "Error.InsufficientSpace";
                                break;
                            case DownloadError.TooManyRedirects:
                                download.StatusDetails = "Error.TooManyRedirects";
                                break;
                            case DownloadError.UnhandledHttpCode:
                                download.StatusDetails = "Error.UnhandledHttpCode";
                                break;
                            case DownloadError.Unknown:
                                download.StatusDetails = "Error.Unknown";
                                break;
                            default:
                                download.StatusDetails = "Error.Unregistered: " + reasonFailed;
                                break;
                        }
                    }
                    download.Status = Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED;
                    break;

                case Android.App.DownloadStatus.Paused:
                    var reasonPaused = cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnReason));
                    switch ((DownloadPausedReason)reasonPaused)
                    {
                        case DownloadPausedReason.QueuedForWifi:
                            download.StatusDetails = "Paused.QueuedForWifi";
                            break;
                        case DownloadPausedReason.WaitingToRetry:
                            download.StatusDetails = "Paused.WaitingToRetry";
                            break;
                        case DownloadPausedReason.WaitingForNetwork:
                            download.StatusDetails = "Paused.WaitingForNetwork";
                            break;
                        case DownloadPausedReason.Unknown:
                            download.StatusDetails = "Paused.Unknown";
                            break;
                        default:
                            download.StatusDetails = "Paused.Unregistered: " + reasonPaused;
                            break;
                    }
                    download.Status = Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED;
                    break;

                case Android.App.DownloadStatus.Pending:
                    download.StatusDetails = default(string);
                    download.Status = Mobile.CrossPlatform.DownloadManager.DownloadStatus.PENDING;
                    break;

                case Android.App.DownloadStatus.Running:
                    download.StatusDetails = default(string);
                    download.Status = Mobile.CrossPlatform.DownloadManager.DownloadStatus.RUNNING;
                    break;
            }
        }

        protected internal void AddFile(IDownload download)
        {
            lock (_queue)
            {
                _queue.Add(download);
            }
            CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, download));
        }

        protected internal void RemoveFile(IDownload download)
        {
            lock (_queue)
            {
                _queue.Remove(download);
                CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, download));
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
        
        public string GetTempPath(IDownload download) => Path.Combine(TempDirectory, download.Filename);

        public void MoveFile(IDownload download)
        {
            string local = GetLocalPath(download);
            if (File.Exists(local))
                File.Delete(local);

            File.Move(GetTempPath(download), local);
        }
    }
}