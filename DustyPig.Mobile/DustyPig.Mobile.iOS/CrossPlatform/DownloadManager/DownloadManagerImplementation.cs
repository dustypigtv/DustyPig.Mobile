using DustyPig.Mobile.CrossPlatform.DownloadManager;
using Foundation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace DustyPig.Mobile.iOS.CrossPlatform.DownloadManager
{
    /// <summary>
    /// The iOS implementation of the download manager.
    /// </summary>
    public class DownloadManagerImplementation : IDownloadManager
    {
        public static readonly DownloadManagerImplementation Current = new DownloadManagerImplementation();

        private string _identifier => NSBundle.MainBundle.BundleIdentifier + ".BackgroundTransferSession";

        private readonly NSUrlSession _backgroundSession;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private DownloadManagerImplementation()
        {
            _queue = new List<IDownload>();

            using (var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(_identifier))
                _backgroundSession = NSUrlSession.FromConfiguration(configuration, UrlSessionDownloadDelegate.Current, new NSOperationQueue());

            // Reinitialize tasks that were started before the app was terminated or suspended
            _backgroundSession.GetTasks2((dataTasks, uploadTasks, downloadTasks) =>
            {
                foreach (var task in downloadTasks)
                    AddDownload(new DownloadImplementation(task));
            });
        }

        public static Action BackgroundSessionCompletionHandler { get; set; }

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

        public IDownload CreateDownload(string url, int id) => new DownloadImplementation(url, id);

        public void Start(IDownload download, bool mobileNetworkAllowed = false)
        {
            var dl = (DownloadImplementation)download;
            AddDownload(dl);

            using (var downloadUrl = NSUrl.FromString(download.Url))
            using (var request = new NSMutableUrlRequest(downloadUrl))
            {
                request.AllowsCellularAccess = mobileNetworkAllowed;
                dl.Task = _backgroundSession.CreateDownloadTask(request);
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
        }

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
                UrlToIdMap.DeleteId(download.MediaEntryId);
            }
            CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, download));
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

        public string GetLocalPath(int id) => Path.Combine(DownloadDirectory, $"{id}.mp4");

    }
}