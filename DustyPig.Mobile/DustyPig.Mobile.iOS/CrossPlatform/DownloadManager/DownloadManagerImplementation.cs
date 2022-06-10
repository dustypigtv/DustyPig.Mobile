using DustyPig.Mobile.CrossPlatform.DownloadManager;
using Foundation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace DustyPig.Mobile.iOS.CrossPlatform.DownloadManager
{
    public class DownloadManagerImplementation : NSObject, INSUrlSessionDownloadDelegate, IDownloadManager
    {
        private string _identifier => NSBundle.MainBundle.BundleIdentifier + ".BackgroundTransferSession";

        private readonly NSUrlSession _backgroundSession;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler<IDownload> DownloadUpdated;

        internal DownloadManagerImplementation()
        {
            _queue = new List<IDownload>();

            using (var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(_identifier))
                _backgroundSession = NSUrlSession.FromConfiguration(configuration, this, new NSOperationQueue());

            // Reinitialize tasks that were started before the app was terminated or suspended
            _backgroundSession.GetTasks2((dataTasks, uploadTasks, downloadTasks) =>
            {
                foreach (var task in downloadTasks)
                    AddDownload(new DownloadImplementation(task));
            });
        }

        public static Action BackgroundSessionCompletionHandler { get; set; }

        protected internal void AddDownload(IDownload download)
        {
            lock (_queue)
            {
                _queue.Add(download);
            }
            CollectionChanged?.Invoke(GetQueue(), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, download));
        }

        protected internal void RemoveDownload(IDownload download)
        {
            lock (_queue)
            {
                _queue.Remove(download);                
            }
            CollectionChanged?.Invoke(GetQueue(), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, download));
        }

        public string GetLocalPath(IDownload download) => Path.Combine(DownloadDirectory, download.Filename);


        protected DownloadImplementation GetDownloadFileByTask(NSUrlSessionTask downloadTask)
        {
            var ret = GetQueue()
                .Cast<DownloadImplementation>()
                .FirstOrDefault(
                    i => i.Task != null &&
                    (int)i.Task.TaskIdentifier == (int)downloadTask.TaskIdentifier
                );

            if(ret == null)
            {
                ret = new DownloadImplementation(downloadTask);
                AddDownload(ret);
            }

            return ret;
        }

        public bool MoveDownloadedFile(DownloadImplementation download, NSUrl location, string destinationPathName)
        {
            var fileManager = NSFileManager.DefaultManager;

            var destinationUrl = new NSUrl(destinationPathName, false);
            NSError removeCopy;
            NSError errorCopy;

            fileManager.Remove(destinationUrl, out removeCopy);
            var success = fileManager.Move(location, destinationUrl, out errorCopy);

            if (!success)
            {
                if (download.SetStatus(DownloadStatus.FAILED, errorCopy.LocalizedDescription))
                    DownloadUpdated?.Invoke(this, download);
            }

            return success;
        }

        #region IDownloadManager

        private readonly IList<IDownload> _queue;
        public IEnumerable<IDownload> GetQueue()
        {
            lock (_queue)
            {
                return _queue.ToList();
            }
        }

        public IDownload CreateDownload(string url, int id, string suffix) => new DownloadImplementation(url, id, suffix);

        public void Start(IDownload download, bool mobileNetworkAllowed = false)
        {
            var dl = (DownloadImplementation)download;
            dl.SetStatus(DownloadStatus.INITIALIZED);
            AddDownload(dl);
            DownloadUpdated?.Invoke(this, download);

            using (var downloadUrl = NSUrl.FromString(download.Url))
            using (var request = new NSMutableUrlRequest(downloadUrl))
            {
                request.AllowsCellularAccess = mobileNetworkAllowed;                
                dl.Task = _backgroundSession.CreateDownloadTask(request);
                dl.Task.TaskDescription = download.Filename;
                dl.Task.Resume();
            }
        }

        public void Abort(IDownload download)
        {
            var file = (DownloadImplementation)download;
            if(file.SetStatus(DownloadStatus.CANCELED))
                DownloadUpdated?.Invoke(this, download);
            file.Task?.Cancel();
            RemoveDownload(file);
        }

        public void AbortAll()
        {            
            _backgroundSession.GetTasks2((dataTasks, uploadTasks, downloadTasks) =>
            {
                foreach (var task in downloadTasks)
                    task.Cancel();
            });

            foreach (var file in GetQueue())
                Abort(file);
        }

        public string DownloadDirectory
        {
            get
            {
                string ret = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library", "Caches", "Downloaded");
                Directory.CreateDirectory(ret);
                return ret;
            }
        }

        public string TempDirectory
        {
            get
            {
                string ret = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library", "Caches", "Temp");
                Directory.CreateDirectory(ret);
                return ret;
            }
        }

        public string GetLocalPath(int mediaId, string suffix) => Path.Combine(DownloadDirectory, $"{mediaId}.{suffix}");

        #endregion


        #region INSUrlSessionDownloadDelegate

        [Export("URLSession:downloadTask:didResumeAtOffset:expectedTotalBytes:")]
        public void DidResume(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long resumeFileOffset, long expectedTotalBytes)
        {
            var download = GetDownloadFileByTask(downloadTask);
            if(download.SetStatus(DownloadStatus.RUNNING))
                DownloadUpdated?.Invoke(this, download);
        }


        [Export("URLSession:task:didCompleteWithError:")]
        public void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            if (error == null)
                return;

            var download = GetDownloadFileByTask(task);
            if (download.SetStatus(DownloadStatus.FAILED, error.LocalizedDescription))
                DownloadUpdated?.Invoke(this, download);
        }


        [Export("URLSession:downloadTask:didWriteData:totalBytesWritten:totalBytesExpectedToWrite:")]
        public void DidWriteData(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
        {
            var download = GetDownloadFileByTask(downloadTask);
            if (download.SetStatus(DownloadStatus.RUNNING) || download.CalcPercent(totalBytesExpectedToWrite, totalBytesWritten))
                DownloadUpdated?.Invoke(this, download);
        }

        public void DidFinishDownloading(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
        {
            var download = GetDownloadFileByTask(downloadTask);

            var response = downloadTask.Response as NSHttpUrlResponse;
            if (response != null && response.StatusCode >= 400)
            {
                if (download.SetStatus(DownloadStatus.FAILED, "Error.HttpCode: " + response.StatusCode))
                    DownloadUpdated?.Invoke(this, download);
                return;
            }

            var destinationPathName = GetLocalPath(download);
            var success = MoveDownloadedFile(download, location, destinationPathName);

            if (success && download.SetStatus(DownloadStatus.COMPLETED))
                DownloadUpdated?.Invoke(this, download);
        }

        [Export("URLSessionDidFinishEventsForBackgroundURLSession:")]
        public void DidFinishEventsForBackgroundSession(NSUrlSession session)
        {
            var handler = BackgroundSessionCompletionHandler;
            if (handler != null)
            {
                BackgroundSessionCompletionHandler = null;
                handler();
            }
        }

        #endregion
    }
}