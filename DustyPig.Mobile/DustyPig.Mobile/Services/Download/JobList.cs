using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DustyPig.Mobile.Services.Download
{
    public class JobList 
    {
        private readonly object _locker = new object();
        private readonly List<Job> _jobs = new List<Job>();

        static readonly string _filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "downloads.json");

        public JobList()
        {
            try { Jobs = JsonConvert.DeserializeObject<List<Job>>(File.ReadAllText(_filename)); }
            catch { }
        }


        
        /// <summary>
        /// This returns a safe COPY of the job list
        /// </summary>
        public IReadOnlyList<Job> Jobs
        {
            get
            {
                lock (_locker)
                {
                    return _jobs.ToList();
                }
            }
            set
            {
                lock (_locker)
                {
                    _jobs.Clear();
                    if (value != null)
                        _jobs.AddRange(value);
                }
            }
        }


        public void AddJob(Job job)
        {
            lock (_locker)
            {
                _jobs.Add(job);
            }
        }

        public void RemoveJob(Job job)
        {
            lock (_locker)
            {
                _jobs.Remove(job);
            }
        }

        public void RemoveJob(int mediaId)
        {
            lock (_locker)
            {
                _jobs.RemoveAll(item => item.MediaId == mediaId);
            }
        }

        public void ClearJobs()
        {
            lock (_locker)
            {
                _jobs.Clear();
            }
        }



        public void Save()
        {
            File.WriteAllText(_filename, JsonConvert.SerializeObject(Jobs));
        }
    }
}
