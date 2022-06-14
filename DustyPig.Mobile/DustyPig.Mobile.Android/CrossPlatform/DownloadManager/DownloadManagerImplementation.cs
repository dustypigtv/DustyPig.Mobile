using Android.App;
using Android.Content;
using DustyPig.Mobile.CrossPlatform.DownloadManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DustyPig.Mobile.Droid.CrossPlatform.DownloadManager
{
    public class DownloadManagerImplementation : IDownloadManager
    {
        readonly Android.App.DownloadManager _downloadManager = (Android.App.DownloadManager)Application.Context.GetSystemService(Context.DownloadService);
        
        public DownloadManagerImplementation()
        {
            //Skip media scanner in this app
            using (var fs = File.Create(Path.Combine(Root, ".nomedia"))) { }
        }

        /// <summary>
        /// Wrap inside a lock
        /// </summary>
        List<DownloadImplementation> ScanDownloads()
        {
            var ret = new List<DownloadImplementation>();

            var query = new Android.App.DownloadManager.Query();
            using var cursor = _downloadManager.InvokeQuery(query);
            if (cursor != null)
            {
                while (cursor.MoveToNext())
                {
                    try
                    {
                        var download = new DownloadImplementation(cursor);
                        if (download.Status == Mobile.CrossPlatform.DownloadManager.DownloadStatus.COMPLETED)
                        {
                            if (MoveFile(download))
                                _downloadManager.Remove(download.AndroidId);
                            else
                                download.SetStatus(Mobile.CrossPlatform.DownloadManager.DownloadStatus.RUNNING);
                        }

                        if (download.StatusDetails == "Error.CannotResume")
                            _downloadManager.Remove(download.AndroidId);

                        ret.Add(download);
                    }
                    catch (Exception ex)
                    {                        
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
                cursor.Close();
            }

            return ret;
        }


        public IEnumerable<IDownload> GetDownloads() => ScanDownloads();


        public void Start(int mediaId, string url, string suffix, bool mobileNetworkAllowed)
        {
            var allDownloads = ScanDownloads();
            var download = allDownloads.FirstOrDefault(item => item.Url == url);
            if (download != null)
                return;

            string destinationPathName = GetTempPath(new DownloadImplementation { MediaId = mediaId, Suffix = suffix });
            using var downloadUrl = Android.Net.Uri.Parse(url);
            using var request = new Android.App.DownloadManager.Request(downloadUrl);

            using var jfile = new Java.IO.File(destinationPathName);
            if (jfile.Exists())
                jfile.Delete();

            request.SetTitle($"{mediaId}.{suffix}");
            request.SetDestinationUri(Android.Net.Uri.FromFile(jfile));
            request.SetAllowedOverMetered(mobileNetworkAllowed);
            request.SetNotificationVisibility(DownloadVisibility.Hidden);

            _downloadManager.Enqueue(request);
        }

        public void Abort(IDownload download)
        {
            if (download == null)
                return;

            if (string.IsNullOrWhiteSpace(download.Url))
                return;

            var dl = download as DownloadImplementation;

            if (dl == null)
                dl = ScanDownloads().FirstOrDefault(item => item.Url == download.Url);

            if (dl != null)
                _downloadManager.Remove(dl.AndroidId);
        }

        public void AbortAll()
        {
            var query = new Android.App.DownloadManager.Query();
            using var cursor = _downloadManager.InvokeQuery(query);
            if (cursor != null)
            {
                while (cursor.MoveToNext())
                {
                    var androidId = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnId));
                    _downloadManager.Remove(androidId);
                }
                cursor.Close();
            }
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

        string GetTempPath(IDownload download) => Path.Combine(TempDirectory, download.Filename);


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