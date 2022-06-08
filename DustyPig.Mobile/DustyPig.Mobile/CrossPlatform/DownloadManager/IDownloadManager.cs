using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DustyPig.Mobile.CrossPlatform.DownloadManager
{
    /// <summary>
    /// Download manager.
    /// </summary>
    public interface IDownloadManager
    {
        /// <summary>
        /// Gets the queue holding all the pending and downloading files.
        /// </summary>
        /// <value>The queue.</value>
        IEnumerable<IDownload> Queue { get; }

        /// <summary>
        /// Occurs when the queue changed.
        /// </summary>
        event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Creates a download file.
        /// </summary>
        /// <returns>The download file.</returns>
        /// <param name="url">URL to download.</param>
        /// <param name="id">MediaEntry id</param>
        IDownload CreateDownload(string url, int id);

        /// <summary>
        /// Start downloading the file. Most of the systems will put this file into a queue first.
        /// </summary>
        /// <param name="download">Download.</param>
        /// <param name="mobileNetworkAllowed">If mobile network is allowed.</param>
        void Start(IDownload download, bool mobileNetworkAllowed = false);

        /// <summary>
        /// Abort downloading the file.
        /// </summary>
        /// <param name="download">Download.</param>
        void Abort(IDownload download);

        /// <summary>
        /// Abort all.
        /// </summary>
        /// <returns>void</returns>
        void AbortAll();

        /// <summary>
        /// Directory  where downloads are stored
        /// </summary>
        string DownloadDirectory { get; }

        /// <summary>
        /// Directory where a url will be saved
        /// </summary>
        string GetLocalPath(int id);

    }
}
