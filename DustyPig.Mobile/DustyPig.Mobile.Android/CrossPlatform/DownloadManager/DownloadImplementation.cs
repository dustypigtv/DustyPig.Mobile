using Android.App;
using Android.Database;
using DustyPig.Mobile.CrossPlatform.DownloadManager;
using System;
using System.Linq;

namespace DustyPig.Mobile.Droid.CrossPlatform.DownloadManager
{
    public class DownloadImplementation : IDownload
    {
        
        public DownloadImplementation(ICursor cursor, SQLite.SQLiteConnection conn)
        {
            AndroidId = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnId));           
            Url = cursor.GetString(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnUri));
            if (string.IsNullOrWhiteSpace(Url))
                throw new ArgumentNullException(nameof(Url));


            //Get info from my database
            var rec = conn.Table<DownloadInfo>()
                .Where(item => item.Url == Url)
                .First();
            MediaId = rec.MediaId;
            Suffix = rec.Suffix;


            //Rest of the info from the android native manager
            TotalBytesExpected = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnTotalSizeBytes));
            TotalBytesWritten = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnBytesDownloadedSoFar));
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



            switch ((Android.App.DownloadStatus)cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnStatus)))
            {
                case Android.App.DownloadStatus.Successful:
                    SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.COMPLETED);
                    break;

                case Android.App.DownloadStatus.Failed:
                    var reasonFailed = cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnReason));
                    if (reasonFailed < 600)
                    {
                        SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.HttpCode: " + reasonFailed);
                    }
                    else
                    {
                        switch ((DownloadError)reasonFailed)
                        {
                            case DownloadError.CannotResume:
                                SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.CannotResume");
                                break;
                            case DownloadError.DeviceNotFound:
                                SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.DeviceNotFound");
                                break;
                            case DownloadError.FileAlreadyExists:
                                SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.FileAlreadyExists");
                                break;
                            case DownloadError.FileError:
                                SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.FileError");
                                break;
                            case DownloadError.HttpDataError:
                                SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.HttpDataError");
                                break;
                            case DownloadError.InsufficientSpace:
                                SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.InsufficientSpace");
                                break;
                            case DownloadError.TooManyRedirects:
                                SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.TooManyRedirects");
                                break;
                            case DownloadError.UnhandledHttpCode:
                                SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.UnhandledHttpCode");
                                break;
                            case DownloadError.Unknown:
                                SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.Unknown");
                                break;
                            default:
                                SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.FAILED, "Error.Unregistered: " + reasonFailed);
                                break;
                        }
                    }
                    break;

                case Android.App.DownloadStatus.Paused:
                    var reasonPaused = cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnReason));
                    switch ((DownloadPausedReason)reasonPaused)
                    {
                        case DownloadPausedReason.QueuedForWifi:
                            SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED, "Paused.QueuedForWifi");
                            break;
                        case DownloadPausedReason.WaitingToRetry:
                            SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED, "Paused.WaitingToRetry");
                            break;
                        case DownloadPausedReason.WaitingForNetwork:
                            SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED, "Paused.WaitingForNetwork");
                            break;
                        case DownloadPausedReason.Unknown:
                            SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED, "Paused.Unknown");
                            break;
                        default:
                            SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PAUSED, "Paused.Unregistered: " + reasonPaused);
                            break;
                    }
                    break;

                case Android.App.DownloadStatus.Pending:
                    SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.PENDING);
                    break;

                case Android.App.DownloadStatus.Running:
                    SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.RUNNING);
                    break;
            }

        }

        public int MediaId { get; set; }

        public string Url { get; set; }

        public string Suffix { get; set; }

        public string Filename => $"{MediaId}.{Suffix}";



        /// <summary>
        /// Id in the native manager
        /// </summary>
        public long AndroidId { get; set; }

        
        
        public Mobile.CrossPlatform.DownloadManager.DownloadStatus Status { get; private set; }

        public string StatusDetails { get; private set; }

        public long TotalBytesExpected { get; private set; }

        public long TotalBytesWritten { get; private set; }

        public int Percent { get; private set; }

        public void SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus status, string details = null)
        {
            Status = status;
            StatusDetails = details;
        }

        
    }
}