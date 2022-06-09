using DustyPig.Mobile.CrossPlatform.DownloadManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DustyPig.Mobile.iOS.CrossPlatform.DownloadManager
{
    public class UrlToIdMap
    {
        public List<UrlToIdMapItem> Items { get; set; } = new List<UrlToIdMapItem>();

        static readonly object _locker = new object();

        static string Filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "ios_download_map.json");
        
        static UrlToIdMap LoadMap()
        {
            try
            {
                return JsonConvert.DeserializeObject<UrlToIdMap>(File.ReadAllText(Filename));
            }
            catch
            {
                return new UrlToIdMap();
            }
        }

        static void SaveMap(UrlToIdMap map) => File.WriteAllText(Filename, JsonConvert.SerializeObject(map));
        

        public static void Add(IDownload download)
        {
            lock (_locker)
            {
                var map = LoadMap();
                var existing = map.Items.FirstOrDefault(x => x.Url == download.Url);
                if(existing == null)
                {
                    map.Items.Add(new UrlToIdMapItem { Url = download.Url, MediaId = download.MediaId, Suffix = download.Suffix });
                    SaveMap(map);
                }
            }       
        }

        public static UrlToIdMapItem Get(string url)
        {
            lock (_locker)
            {
                return LoadMap().Items.FirstOrDefault(x => x.Url == url);
            }
        }

        public static void Delete(IDownload download)
        {
            lock (_locker)
            {
                var map = LoadMap();
                var existing = map.Items.FirstOrDefault(x => x.Url == download.Url);
                if (existing != null)
                {
                    map.Items.Remove(existing);
                    SaveMap(map);
                }
            }
        }
    }
}