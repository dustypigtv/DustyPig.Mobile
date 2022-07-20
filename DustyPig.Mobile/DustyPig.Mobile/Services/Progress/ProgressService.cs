/*
    I could go to the trouble of setting up sqlite database, but this should be very lightweight, 
    so I'm just gonna use a json file
 */
using DustyPig.Mobile.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DustyPig.Mobile.Services.Progress
{
    internal static class ProgressService
    {
        static readonly string _filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "playback_progress.json");
        static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public static void Init()
        {
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            Task.Run(() => Connectivity_ConnectivityChanged(null, null));
        }

        private static void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            NoInternet = (int)Connectivity.NetworkAccess < 3;
            UpdateFromList();
        }

        static bool NoInternet { get; set; }

        public static async void UpdateProgress(int id, double seconds)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (NoInternet)
                {
                    AddToList(id, seconds);
                }
                else
                {
                    var response = await App.API.Media.UpdatePlaybackProgressAsync(id, seconds);
                    if (response.Success)
                    {
                        var lst = Load();
                        var rec = lst.FirstOrDefault(item => item.MediaId == id);
                        if (rec != null)
                        {
                            lst.Remove(rec);
                            Save(lst);
                        }
                    }
                    else
                    {
                        AddToList(id, seconds);
                        await response.HandleUnauthorizedException();                         
                    }
                }
            }
            catch { }

            _semaphore.Release();
        }

        static void AddToList(int id, double seconds)
        {
            var lst = Load();
            var rec = lst.FirstOrDefault(item => item.MediaId == id);
            if (rec == null)
            {
                rec = new ProgressRecord { MediaId = id };
                lst.Add(rec);
            }
            rec.Played = seconds;
            rec.TimestampUtc = DateTime.UtcNow;
            Save(lst);
        }

        static async void UpdateFromList()
        {
            if (NoInternet)
                return;

            await _semaphore.WaitAsync();

            try
            {
                var lst = Load();
                foreach (var item in lst.OrderBy(item => item.TimestampUtc).ToList())
                {
                    var response = await App.API.Media.UpdatePlaybackProgressAsync(item.MediaId, item.Played);
                    if (response.Success)
                        lst.Remove(item);
                    else if (await response.HandleUnauthorizedException())
                        break;
                       
                }
                Save(lst);
            }
            catch { }

            _semaphore.Release();
        }

        static List<ProgressRecord> Load()
        {
            try { return JsonConvert.DeserializeObject<List<ProgressRecord>>(File.ReadAllText(_filename)); }
            catch { return new List<ProgressRecord>(); }
        }

        static void Save(List<ProgressRecord> lst)
        {
            File.WriteAllText(_filename, JsonConvert.SerializeObject(lst));
        }

        public static async void Reset()
        {
            await _semaphore.WaitAsync();

            try
            {
                File.Delete(_filename);
            }
            catch { }

            _semaphore.Release();
        }
    }
}
