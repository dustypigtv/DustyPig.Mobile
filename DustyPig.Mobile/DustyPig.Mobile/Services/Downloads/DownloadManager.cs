using System;
using System.Collections.Generic;
using System.Text;

namespace DustyPig.Mobile.Services.Downloads
{
    internal class DownloadManager
    {
        public static DownloadStatus GetStatus(int id)
        {
            return DownloadStatus.NotDownloaded;
        }
    }
}
