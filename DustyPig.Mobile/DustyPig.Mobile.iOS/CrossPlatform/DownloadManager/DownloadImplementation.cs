using DustyPig.Mobile.CrossPlatform.DownloadManager;
using Foundation;
using System;

namespace DustyPig.Mobile.iOS.CrossPlatform.DownloadManager
{
    public class DownloadImplementation : IDownload
    {
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



        public DownloadStatus Status { get; private set; }

        public string StatusDetails { get; private set; }

        public long TotalBytesExpected { get; private set; }

        public long TotalBytesWritten { get;  private set; }

        public int Percent { get; set; }

        public bool SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus status, string details = null)
        {
            if (Status == status && StatusDetails == details)
                return false;

            Status = status;
            StatusDetails = details;
            return true;
        }

        public bool CalcPercent(long expected, long written)
        {
            if (TotalBytesExpected == expected && TotalBytesWritten == written)
                return false;

            TotalBytesExpected = expected;
            TotalBytesWritten = written;

            if (TotalBytesExpected <= 0 || TotalBytesWritten <= 0)
            {
                Percent = 0;
            }
            else
            {
                double w = (double)TotalBytesWritten;
                double e = (double)TotalBytesExpected;
                Percent = (int)Math.Floor(w / e * 100);
            }

            return true;
        }

        public NSUrlSessionTask Task { get; set; }
    }
}