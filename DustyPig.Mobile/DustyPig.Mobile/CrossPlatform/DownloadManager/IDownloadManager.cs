using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DustyPig.Mobile.CrossPlatform.DownloadManager
{
   public interface IDownloadManager
    {
        IEnumerable<IDownload> GetQueue();

        event NotifyCollectionChangedEventHandler CollectionChanged;

        event EventHandler<IDownload> DownloadUpdated;

        IDownload CreateDownload(string url, int id, string suffix);

        void Start(IDownload download, bool mobileNetworkAllowed);

        void Abort(IDownload download);

        void AbortAll();

        string DownloadDirectory { get; }

        string TempDirectory { get; }

        string GetLocalPath(int mediaId, string suffix);
    }
}
