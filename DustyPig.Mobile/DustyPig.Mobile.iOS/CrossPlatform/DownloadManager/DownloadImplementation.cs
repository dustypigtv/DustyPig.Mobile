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

            CalcPercent(task.BytesExpectedToReceive, task.BytesReceived);

            Task = task;
        }

        public int MediaId { get; set; }

        public string Url { get; }

        public string Suffix { get; set; }

        public string Filename => $"{MediaId}.{Suffix}";

        public DownloadStatus Status { get; private set; }

        public string StatusDetails { get; private set; }

        public int Percent { get; set; }

        public void SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus status, string details = null)
        {
            Status = status;
            StatusDetails = details;
        }

        public void CalcPercent(long size, long downloaded)
        {
            if (size <= 0 || downloaded <= 0)
                Percent = 0;
            else
                Percent = (int)Math.Floor((double)downloaded / (double)size * 100);
        }

        public NSUrlSessionTask Task { get; set; }
    }
}