using DustyPig.API.v3.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace DustyPig.Mobile.Services
{
    static class HomePageCache
    {
        static readonly string _filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "home_screen_cache.json");

        public static HomeScreen Load()
        {
            try { return JsonConvert.DeserializeObject<HomeScreen>(File.ReadAllText(_filename)); }
            catch { return new HomeScreen(); }
        }

        public static void Save(HomeScreen hs)
        {
            File.WriteAllText(_filename, JsonConvert.SerializeObject(hs));
        }
        
        public static void Reset()
        {
            try { File.Delete(_filename); }
            catch { }
        }
    }
}
