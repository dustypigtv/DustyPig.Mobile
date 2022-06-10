using Android.App;
using Android.Database;
using DustyPig.Mobile.CrossPlatform.DownloadManager;
using System;
using System.ComponentModel;

namespace DustyPig.Mobile.Droid.CrossPlatform.DownloadManager
{
    public class DownloadImplementation : IDownload
    {
        public DownloadImplementation(string url, int mediaEntryId, string suffix)
        {
            Url = url;
            MediaId = mediaEntryId;
            Suffix = suffix;
            Status = Mobile.CrossPlatform.DownloadManager.DownloadStatus.INITIALIZED;
        }

        /**
         * Reinitializing an object after the app restarted
         */
        public DownloadImplementation(ICursor cursor)
        {
            AndroidId = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnId));
            Url = cursor.GetString(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnUri));

            string localUri = cursor.GetString(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnLocalUri));
            string filename = System.IO.Path.GetFileName(localUri);

            //Filename will be MediaId.Suffix
            MediaId = int.Parse(filename.Substring(0, filename.IndexOf('.')));
            Suffix = filename.Substring(filename.IndexOf('.') + 1);
            
            
            Status = (Android.App.DownloadStatus)cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnStatus)) switch
            {
                Android.App.DownloadStatus.Failed => Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED,
                Android.App.DownloadStatus.Paused => Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED,
                Android.App.DownloadStatus.Pending => Mobile.CrossPlatform.DownloadManager.DownloadStatus.PENDING,
                Android.App.DownloadStatus.Running => Mobile.CrossPlatform.DownloadManager.DownloadStatus.RUNNING,
                Android.App.DownloadStatus.Successful => Mobile.CrossPlatform.DownloadManager.DownloadStatus.COMPLETED,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }


        public int MediaId { get; set; }

        public string Url { get; set; }

        public string Suffix { get; set; }

        public string Filename => $"{MediaId}.{Suffix}";



        public long AndroidId { get; set; }

        
        public Mobile.CrossPlatform.DownloadManager.DownloadStatus Status { get; private set; }

        public string StatusDetails { get; private set; }

        public long TotalBytesExpected { get; private set; }

        public long TotalBytesWritten { get; private set; }

        public int Percent { get; private set; }

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
    }
}