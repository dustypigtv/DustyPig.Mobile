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
            CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, download));
        }

        protected internal void RemoveDownload(IDownload download)
        {
            lock (_queue)
            {
                _queue.Remove(download);                
            }
            CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, download));
        }

        public string GetLocalPath(IDownload download) => Path.Combine(DownloadDirectory, download.Filename);


        protected DownloadImplementation GetDownloadFileByTask(NSUrlSessionTask downloadTask)
        {
            var ret = Queue
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

        public bool MoveDownloadedFile(DownloadImplementation file, NSUrl location, string destinationPathName)
        {
            var fileManager = NSFileManager.DefaultManager;

            var destinationUrl = new NSUrl(destinationPathName, false);
            NSError removeCopy;
            NSError errorCopy;

            fileManager.Remove(destinationUrl, out removeCopy);
            var success = fileManager.Move(location, destinationUrl, out errorCopy);

            if (!success)
            {
                file.StatusDetails = errorCopy.LocalizedDescription;
                file.Status = DownloadStatus.FAILED;
            }

            return success;
        }

        #region IDownloadManager

        private readonly IList<IDownload> _queue;
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

        public void Start(IDownload download, bool mobileNetworkAllowed = false)
        {
            var dl = (DownloadImplementation)download;
            AddDownload(dl);

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
            file.Status = DownloadStatus.CANCELED;
            file.Task?.Cancel();
            RemoveDownload(file);
        }

        public void AbortAll()
        {
            foreach (var file in Queue)
                Abort(file);
            
            _backgroundSession.GetTasks2((dataTasks, uploadTasks, downloadTasks) =>
            {
                foreach (var task in downloadTasks)
                    task.Cancel();
            });
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


        #endregion


        #region INSUrlSessionDownloadDelegate

        [Export("URLSession:downloadTask:didResumeAtOffset:expectedTotalBytes:")]
        public void DidResume(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long resumeFileOffset, long expectedTotalBytes)
        {
            var download = GetDownloadFileByTask(downloadTask);
            if (download == null)
            {
                downloadTask.Cancel();
                return;
            }

            download.Status = DownloadStatus.RUNNING;
        }


        [Export("URLSession:task:didCompleteWithError:")]
        public void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            var download = GetDownloadFileByTask(task);
            download.StatusDetails = error.LocalizedDescription;
            download.Status = DownloadStatus.FAILED;
        }


        [Export("URLSession:downloadTask:didWriteData:totalBytesWritten:totalBytesExpectedToWrite:")]
        public void DidWriteData(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
        {
            var download = GetDownloadFileByTask(downloadTask);
            download.Status = DownloadStatus.RUNNING;
            download.TotalBytesExpected = totalBytesExpectedToWrite;
            download.TotalBytesWritten = totalBytesWritten;
        }

        public void DidFinishDownloading(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
        {
            var download = GetDownloadFileByTask(downloadTask);
           
            var response = downloadTask.Response as NSHttpUrlResponse;
            if (response != null && response.StatusCode >= 400)
            {
                download.StatusDetails = "Error.HttpCode: " + response.StatusCode;
                download.Status = DownloadStatus.FAILED;
                return;
            }

            var destinationPathName = GetLocalPath(download);
            var success = MoveDownloadedFile(download, location, destinationPathName);

            if (success)
                download.Status = DownloadStatus.COMPLETED;
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