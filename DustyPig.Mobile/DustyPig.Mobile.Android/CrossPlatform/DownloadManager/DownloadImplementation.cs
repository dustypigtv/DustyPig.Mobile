using Android.App;
using Android.Database;
using DustyPig.Mobile.CrossPlatform.DownloadManager;
using System;
using System.ComponentModel;

namespace DustyPig.Mobile.Droid.CrossPlatform.DownloadManager
{
    public class DownloadImplementation : IDownload
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DownloadImplementation(string url, int mediaEntryId)
        {
            Url = url;
            MediaEntryId = mediaEntryId;
            Status = Mobile.CrossPlatform.DownloadManager.DownloadStatus.INITIALIZED;
        }

        /**
         * Reinitializing an object after the app restarted
         */
        public DownloadImplementation(ICursor cursor)
        {
            Id = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnId));
            Url = cursor.GetString(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnUri));

            string localUri = cursor.GetString(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnLocalUri));
            MediaEntryId = int.Parse(System.IO.Path.GetFileNameWithoutExtension(localUri));
            
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


        public int MediaEntryId { get; set; }

        public long Id { get; set; }

        public string Url { get; set; }

        private Mobile.CrossPlatform.DownloadManager.DownloadStatus _status;
        public Mobile.CrossPlatform.DownloadManager.DownloadStatus Status
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
    }
}