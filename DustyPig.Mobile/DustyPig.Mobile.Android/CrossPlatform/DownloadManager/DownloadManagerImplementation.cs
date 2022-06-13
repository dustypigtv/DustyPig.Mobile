using Android.App;
using Android.Content;
using DustyPig.Mobile.CrossPlatform.DownloadManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xamarin.Essentials;

namespace DustyPig.Mobile.Droid.CrossPlatform.DownloadManager
{
    public class DownloadManagerImplementation : IDownloadManager
    {
        const int TIMER_MILLISECONDS = 1000;

        public static readonly DownloadManagerImplementation Current = new DownloadManagerImplementation();

        public event EventHandler<IDownload> DownloadUpdated;
       
        readonly Android.App.DownloadManager _downloadManager = (Android.App.DownloadManager)Application.Context.GetSystemService(Context.DownloadService);
        readonly Timer _watcher;
        readonly SQLite.SQLiteConnection _connection;

        //private readonly object _locker = new object();

        private DownloadManagerImplementation()
        {
            //Skip media scanner in this app
            using (var fs = File.Create(Path.Combine(Root, ".nomedia"))) { }

            string dbfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "droid.downloads.sqlite");
            _connection = new SQLite.SQLiteConnection(dbfile, SQLite.SQLiteOpenFlags.Create | SQLite.SQLiteOpenFlags.ReadWrite | SQLite.SQLiteOpenFlags.FullMutex);
            _connection.CreateTable<DownloadInfo>(SQLite.CreateFlags.None);

            _watcher = new Timer(new TimerCallback(WatcherCallback), null, TIMER_MILLISECONDS, Timeout.Infinite);

        }

        void WatcherCallback(object dummy)
        {
            try { ScanDownloads(); }
            catch { }

            _watcher.Change(TIMER_MILLISECONDS, Timeout.Infinite);
        }

        List<DownloadImplementation> ScanDownloads()
        {
            var ret = new List<DownloadImplementation>();

            var dummy = TempDirectory;

            var query = new Android.App.DownloadManager.Query();
            using var cursor = _downloadManager.InvokeQuery(query);
            if (cursor != null)
            {
                while (cursor.MoveToNext())
                {
                    var androidId = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnId));
                    try
                    {
                        var download = new DownloadImplementation(cursor, _connection);
                        if (download.Status == Mobile.CrossPlatform.DownloadManager.DownloadStatus.COMPLETED)
                            if (MoveFile(download))
                                _downloadManager.Remove(download.AndroidId);
                        

                        ret.Add(download);
                        try { DownloadUpdated?.Invoke(this, download); }
                        catch { }
                    }
                    catch { }                    
                }
                cursor.Close();
            }

            return ret;
        }

        public IEnumerable<IDownload> GetDownloads() => ScanDownloads();


        DownloadImplementation FindDownload(IDownload dl) => FindDownload(dl.Url);

        DownloadImplementation FindDownload(string url)
        {
            return ScanDownloads()
                .Where(item => item.Url == url)
                .FirstOrDefault();
        }


        public void Start(int mediaId, string url, string suffix, bool mobileNetworkAllowed)
        {
            if (FindDownload(url) != null)
                return;

            var info = _connection.Table<DownloadInfo>()
                .Where(item => item.Url == url)
                .FirstOrDefault();

            if(info == null)
            {
                info = new DownloadInfo
                {
                    MediaId = mediaId,
                    Suffix = suffix,
                    Url = url
                };
                _connection.Insert(info);
            }
            
            string destinationPathName = GetTempPath(mediaId, suffix);

            using var downloadUrl = Android.Net.Uri.Parse(url);
            using var request = new Android.App.DownloadManager.Request(downloadUrl);

            using var jfile = new Java.IO.File(destinationPathName);
            if (jfile.Exists())
                jfile.Delete();
            
            request.SetDestinationUri(Android.Net.Uri.FromFile(jfile));
            request.SetAllowedOverMetered(mobileNetworkAllowed);
            request.SetNotificationVisibility(DownloadVisibility.Hidden);
           
            _downloadManager.Enqueue(request);       
        }

        public void Abort(IDownload download)
        {
            var dl = download as DownloadImplementation;
            
            if (dl == null)
                dl = FindDownload(dl);

            if (dl != null)
                _downloadManager.Remove(dl.AndroidId);

            var info = _connection.Table<DownloadInfo>()
               .Where(item => item.Url == download.Url)
               .FirstOrDefault();

            if (info != null)
                _connection.Delete(info);
        }

        public void AbortAll()
        {
            var query = new Android.App.DownloadManager.Query();
            using var cursor = _downloadManager.InvokeQuery(query);
            if(cursor != null)
            {
                while(cursor.MoveToNext())
                {
                    var androidId = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnId));
                    _downloadManager.Remove(androidId);
                }
                cursor.Close();
            }

            _connection.DeleteAll<DownloadInfo>();
        }
               
        string Root
        {
            get
            {
                if (Android.OS.Environment.MediaMounted.Equals(Android.OS.Environment.ExternalStorageState))
                    return Directory.CreateDirectory(Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "offline")).FullName;

                return Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "offline")).FullName;
            }
        }

        public string DownloadDirectory => Directory.CreateDirectory(Path.Combine(Root, "files")).FullName;

        public string TempDirectory => Directory.CreateDirectory(Path.Combine(Root, "tmp")).FullName;

        string GetLocalPath(IDownload download) => Path.Combine(DownloadDirectory, download.Filename);

        string GetLocalPath(int mediaId, string suffix) => Path.Combine(DownloadDirectory, $"{mediaId}.{suffix}");
        
        string GetTempPath(IDownload download) => Path.Combine(TempDirectory, download.Filename);
        
        string GetTempPath(int mediaId, string suffix) => Path.Combine(TempDirectory, $"{mediaId}.{suffix}");

        bool MoveFile(IDownload download)
        {
            if (string.IsNullOrWhiteSpace(download.Suffix))
                return false;

            string temp = GetTempPath(download);
            if (!File.Exists(temp))
                return false;

            string local = GetLocalPath(download);
            if (File.Exists(local))
                File.Delete(local);

            File.Move(temp, local);
            return true;
        }
    }
}