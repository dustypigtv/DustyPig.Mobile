using SQLite;

namespace DustyPig.Mobile.Droid.CrossPlatform.DownloadManager
{
    public class DownloadInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int MediaId { get; set; }
        public string Url { get; set; }
        public string Suffix { get; set; }
    }
}