using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DustyPig.Mobile.iOS.CrossPlatform.DownloadManager
{
    public class UrlToIdMap
    {
        public List<UrlToIdMapItem> Items { get; set; } = new List<UrlToIdMapItem>();

        static readonly object _locker = new object();

        static string Filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "dustypig_dl_map.xml");
        
        static UrlToIdMap LoadMap()
        {
            try
            {
                using (var fs = File.OpenRead(Filename))
                {
                    var xs = new XmlSerializer(typeof(UrlToIdMap));
                    return (UrlToIdMap)xs.Deserialize(fs);
                }
            }
            catch
            {
                return new UrlToIdMap();
            }
        }

        static void SaveMap(UrlToIdMap map)
        {
            using (var fs = File.Create(Filename))
            {
                var xs = new XmlSerializer(typeof(UrlToIdMap));
                xs.Serialize(fs, map);
            }
        }

        public static void AddId(string url, int mediaEntryId)
        {
            lock (_locker)
            {
                var map = LoadMap();
                var existing = map.Items.FirstOrDefault(x => x.Url == url);
                if(existing == null)
                {
                    map.Items.Add(new UrlToIdMapItem { Url = url, MediaEntryId = mediaEntryId });
                    SaveMap(map);
                }
            }       
        }

        public static int GetId(string url)
        {
            lock (_locker)
            {
                try { return LoadMap().Items.Single(x => x.Url == url).MediaEntryId; }
                catch { return -1; }
            }
        }

        public static void DeleteId(int mediaEntryId)
        {
            lock (_locker)
            {
                var map = LoadMap();
                var existing = map.Items.FirstOrDefault(x => x.MediaEntryId == mediaEntryId);
                if (existing != null)
                {
                    map.Items.Remove(existing);
                    SaveMap(map);
                }
            }
        }
    }
}