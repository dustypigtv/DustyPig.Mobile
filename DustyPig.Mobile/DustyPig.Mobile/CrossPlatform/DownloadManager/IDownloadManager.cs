using System.Collections.Generic;

namespace DustyPig.Mobile.CrossPlatform.DownloadManager
{
    public interface IDownloadManager
    {
        IEnumerable<IDownload> GetDownloads();

        //event EventHandler<IDownload> DownloadUpdated;

        void Start(int mediaId, string url, string suffix, bool mobileNetworkAllowed);

        void Abort(IDownload download);

        void AbortAll();

        string DownloadDirectory { get; }

        string TempDirectory { get; }

        //string GetLocalPath(int mediaId, string suffix);
    }
}
