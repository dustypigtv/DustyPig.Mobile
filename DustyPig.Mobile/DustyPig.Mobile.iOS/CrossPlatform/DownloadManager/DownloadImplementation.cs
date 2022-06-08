using DustyPig.Mobile.CrossPlatform.DownloadManager;
using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DustyPig.Mobile.iOS.CrossPlatform.DownloadManager
{
    public class DownloadImplementation : IDownload
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int MediaEntryId { get; set; }

        public NSUrlSessionTask Task { get; set; }

        public string Url { get; }

        private DownloadStatus _status;
        public DownloadStatus Status
        {
            get => _status;
            set
            {
                if (Equals(_status, value))
                    return;
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        private string _statusDetails;
        public string StatusDetails
        {
            get => _statusDetails;
            set
            {
                if (Equals(_statusDetails, value))
                    return;
                _statusDetails = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusDetails)));
            }
        }

        private long _totalBytesExpected;
        public long TotalBytesExpected
        {
            get => _totalBytesExpected;
            set
            {
                if (Equals(_totalBytesExpected, value))
                    return;
                _totalBytesExpected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBytesExpected)));
            }
        }

        private long _totalBytesWritten;
        public long TotalBytesWritten
        {
            get => _totalBytesWritten;
            set
            {
                if (Equals(_totalBytesWritten, value))
                    return;
                _totalBytesWritten = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBytesWritten)));
            }
        }




        public DownloadImplementation(string url, int mediaEntryId)
        {
            Url = url;
            MediaEntryId = mediaEntryId;
            Status = DownloadStatus.INITIALIZED;

            UrlToIdMap.AddId(url, mediaEntryId);
        }


        public DownloadImplementation(NSUrlSessionTask task)
        {
            Url = task.OriginalRequest.Url.AbsoluteString;

            MediaEntryId = UrlToIdMap.GetId(Url);

            switch (task.State)
            {
                case NSUrlSessionTaskState.Running:
                    Status = DownloadStatus.RUNNING;
                    break;

                case NSUrlSessionTaskState.Completed:
                    Status = DownloadStatus.COMPLETED;
                    break;

                case NSUrlSessionTaskState.Canceling:
                    Status = DownloadStatus.RUNNING;
                    break;

                case NSUrlSessionTaskState.Suspended:
                    Status = DownloadStatus.PAUSED;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Task = task;
        }
    }
}