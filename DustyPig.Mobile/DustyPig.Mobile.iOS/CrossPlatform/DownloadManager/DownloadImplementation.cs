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

        public DownloadImplementation(string url, int mediaEntryId, string suffix)
        {
            Url = url;
            MediaId = mediaEntryId;
            Suffix = suffix;
            Status = DownloadStatus.INITIALIZED;
        }


        public DownloadImplementation(NSUrlSessionTask task)
        {
            Url = task.OriginalRequest.Url.AbsoluteString;
            MediaId = int.Parse(task.TaskDescription.Substring(0, task.TaskDescription.IndexOf('.')));
            Suffix = task.TaskDescription.Substring(task.TaskDescription.IndexOf('.') + 1);

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

        public int MediaId { get; set; }

        public string Url { get; }

        public string Suffix { get; set; }

        public string Filename => $"{MediaId}.{Suffix}";



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
                CalcPercent();
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
                CalcPercent();
            }
        }

        private int _percent;
        public int Percent
        {
            get => _percent;
            set
            {
                if (Equals(_percent, value))
                    return;
                _percent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Percent)));
            }
        }

        private void CalcPercent()
        {
            if(TotalBytesExpected <= 0 || TotalBytesWritten <= 0)
            {
                Percent = 0;
                return;
            }

            double w = (double)TotalBytesWritten;
            double e = (double)TotalBytesExpected;
            Percent = (int)Math.Floor(w / e * 100);
        }

        public NSUrlSessionTask Task { get; set; }


    }
}