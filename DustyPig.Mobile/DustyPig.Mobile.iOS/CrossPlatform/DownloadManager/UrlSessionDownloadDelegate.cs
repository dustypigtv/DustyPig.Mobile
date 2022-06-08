using System.Linq;
using DustyPig.Mobile.CrossPlatform.DownloadManager;
using Foundation;

namespace DustyPig.Mobile.iOS.CrossPlatform.DownloadManager
{
    public class UrlSessionDownloadDelegate : NSObject, INSUrlSessionDownloadDelegate
    {
        public static readonly UrlSessionDownloadDelegate Current = new UrlSessionDownloadDelegate();

        private UrlSessionDownloadDelegate() { }

        protected DownloadImplementation GetDownloadFileByTask(NSUrlSessionTask downloadTask)
        {
            return DownloadManagerImplementation.Current.Queue
                .Cast<DownloadImplementation>()
                .FirstOrDefault(
                    i => i.Task != null &&
                    (int)i.Task.TaskIdentifier == (int)downloadTask.TaskIdentifier
                );
        }

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
            if (download == null)
                return;

            download.StatusDetails = error.LocalizedDescription;
            download.Status = DownloadStatus.FAILED;
            DownloadManagerImplementation.Current.RemoveDownload(download);
        }


        [Export("URLSession:downloadTask:didWriteData:totalBytesWritten:totalBytesExpectedToWrite:")]
        public void DidWriteData(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
        {
            var download = GetDownloadFileByTask(downloadTask);
            if (download == null)
            {
                downloadTask.Cancel();
                return;
            }

            download.Status = DownloadStatus.RUNNING;
            download.TotalBytesExpected = totalBytesExpectedToWrite;
            download.TotalBytesWritten = totalBytesWritten;
        }

        public void DidFinishDownloading(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
        {
            var download = GetDownloadFileByTask(downloadTask);
            if (download == null)
            {
                downloadTask.Cancel();
                return;
            }

            var response = downloadTask.Response as NSHttpUrlResponse;
            if (response != null && response.StatusCode >= 400)
            {
                download.StatusDetails = "Error.HttpCode: " + response.StatusCode;
                download.Status = DownloadStatus.FAILED;
                DownloadManagerImplementation.Current.RemoveDownload(download);
                return;
            }

            var destinationPathName = DownloadManagerImplementation.Current.GetLocalPath(download.MediaEntryId);
            var success = MoveDownloadedFile(download, location, destinationPathName);

            if (success)
                download.Status = DownloadStatus.COMPLETED;

            DownloadManagerImplementation.Current.RemoveDownload(download);
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

        [Export("URLSessionDidFinishEventsForBackgroundURLSession:")]
        public void DidFinishEventsForBackgroundSession(NSUrlSession session)
        {
            var handler = DownloadManagerImplementation.BackgroundSessionCompletionHandler;
            if (handler != null)
            {
                DownloadManagerImplementation.BackgroundSessionCompletionHandler = null;
                handler();
            }
        }
    }
}