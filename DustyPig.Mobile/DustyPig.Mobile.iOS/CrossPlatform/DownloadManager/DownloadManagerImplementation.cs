using DustyPig.Mobile.CrossPlatform.DownloadManager;
using Foundation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DustyPig.Mobile.iOS.CrossPlatform.DownloadManager
{
    public class DownloadManagerImplementation : NSObject, INSUrlSessionDownloadDelegate, IDownloadManager
    {
        private string _identifier => NSBundle.MainBundle.BundleIdentifier + ".BackgroundTransferSession";
        private readonly NSUrlSession _backgroundSession;

        internal DownloadManagerImplementation()
        {
            using (var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(_identifier))
                _backgroundSession = NSUrlSession.FromConfiguration(configuration, this, new NSOperationQueue());
        }

        public static Action BackgroundSessionCompletionHandler { get; set; }

        public string GetLocalPath(IDownload download) => Path.Combine(DownloadDirectory, download.Filename);

        public bool MoveDownloadedFile(DownloadImplementation download, NSUrl location, string destinationPathName)
        {
            var fileManager = NSFileManager.DefaultManager;

            var destinationUrl = new NSUrl(destinationPathName, false);
            NSError removeCopy;
            NSError errorCopy;

            fileManager.Remove(destinationUrl, out removeCopy);
            return fileManager.Move(location, destinationUrl, out errorCopy);
        }

        /// <summary>
        /// Wrap this in a locker
        /// </summary>
        private List<DownloadImplementation> ScanDownloads()
        {
            var ret = new List<DownloadImplementation>();

            var mre = new ManualResetEvent(false);
            _backgroundSession.GetTasks2((dataTasks, uploadTasks, downloadTasks) =>
            {
                foreach (var task in downloadTasks)
                    ret.Add(new DownloadImplementation(task));
                mre.Set();
            });
            mre.WaitOne();

            return ret;
        }

        #region IDownloadManager

        public IEnumerable<IDownload> GetDownloads() => ScanDownloads();

        public void Start(int mediaId, string url, string suffix, bool mobileNetworkAllowed)
        {
            var file = ScanDownloads()
                .Where(item => item.Url == url)
                .FirstOrDefault();

            if (file != null)
                return;

            using (var downloadUrl = NSUrl.FromString(url))
            using (var request = new NSMutableUrlRequest(downloadUrl))
            {
                request.AllowsCellularAccess = mobileNetworkAllowed;
                var task = _backgroundSession.CreateDownloadTask(request);
                task.TaskDescription = $"{mediaId}.{suffix}";
                task.Resume();
            }
        }


        public void Abort(IDownload download)
        {
            if (download == null)
                return;

            if (string.IsNullOrWhiteSpace(download.Url))
                return;


            var file = download as DownloadImplementation;
            if (file == null)
            {
                file = ScanDownloads()
                    .Where(item => item.Url == download.Url)
                    .FirstOrDefault();
            }

            if (file == null)
                return;

            file.Task?.Cancel();
        }


        public void AbortAll()
        {
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

        //public string GetLocalPath(int mediaId, string suffix) => Path.Combine(DownloadDirectory, $"{mediaId}.{suffix}");

        #endregion


        #region INSUrlSessionDownloadDelegate

        //[Export("URLSession:downloadTask:didResumeAtOffset:expectedTotalBytes:")]
        //public void DidResume(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long resumeFileOffset, long expectedTotalBytes)
        //{
        //    var download = AddOrGetDownload(downloadTask);
        //    download.SetStatus(DownloadStatus.RUNNING);
        //}


        [Export("URLSession:task:didCompleteWithError:")]
        public void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            if (error == null)
                return;

            task.Cancel();
            //var download = AddOrGetDownload(task);
            //download.SetStatus(DownloadStatus.FAILED, error.LocalizedDescription);
        }


        //[Export("URLSession:downloadTask:didWriteData:totalBytesWritten:totalBytesExpectedToWrite:")]
        //public void DidWriteData(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
        //{
        //    var download = AddOrGetDownload(downloadTask);
        //    download.SetStatus(DownloadStatus.RUNNING);
        //    download.CalcPercent(totalBytesExpectedToWrite, totalBytesWritten);
        //}

        public void DidFinishDownloading(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
        {
            var response = downloadTask.Response as NSHttpUrlResponse;
            if (response != null && response.StatusCode >= 400)
            {
                downloadTask.Cancel();
                return;
            }

            var download = new DownloadImplementation(downloadTask);
            var destinationPathName = GetLocalPath(download);
            if (!MoveDownloadedFile(download, location, destinationPathName))
                downloadTask.Cancel();
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