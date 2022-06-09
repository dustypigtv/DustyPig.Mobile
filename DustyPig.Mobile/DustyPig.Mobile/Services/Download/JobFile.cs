using DustyPig.Mobile.CrossPlatform.DownloadManager;
using Newtonsoft.Json;
using System.IO;
using Xamarin.Forms;

namespace DustyPig.Mobile.Services.Download
{
    internal class JobFile
    {
        public int MediaId { get; set; }
        public string Suffix { get; set; }
        public string Url { get; set; }
        public bool IsVideo { get; set; }

        public string LocalFile() => Path.Combine(DependencyService.Get<IDownloadManager>().DownloadDirectory, $"{MediaId}.{Suffix}");


        public string TempFile() => Path.Combine(DependencyService.Get<IDownloadManager>().TempDirectory, $"{MediaId}.{Suffix}");
    }
}
