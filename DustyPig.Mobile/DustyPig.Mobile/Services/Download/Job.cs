using DustyPig.API.v3.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DustyPig.Mobile.Services.Download
{
    public class Job : INotifyPropertyChanged
    {
        private readonly object _locker = new object();
        private readonly List<JobFile> _files = new List<JobFile>();



        /// <summary>
        /// Movie / Series / Playlist
        /// </summary>
        public int MediaId { get; set; }

        public MediaTypes MediaType { get; set; }

        /// <summary>
        /// Only if Series / Playlist
        /// </summary>
        public int ItemsCount { get; set; }


        /// <summary>
        /// This returns a safe COPY of the job list
        /// </summary>
        public IReadOnlyList<JobFile> Files
        {
            get
            {
                lock (_locker)
                {
                    return _files.ToList();
                }
            }
            set
            {
                lock (_locker)
                {
                    _files.Clear();
                    if (value != null)
                        _files.AddRange(value);
                }
            }
        }


        public void AddFile(JobFile jobFile)
        {
            lock (_locker)
            {
                _files.Add(jobFile);
            }
        }

        public void RemoveFile(JobFile jobFile)
        {
            lock (_locker)
            {
                _files.Remove(jobFile);
            }
        }

        public bool RemoveAllFiles(Predicate<JobFile> match)
        {
            lock (_locker)
            {
                int cnt = _files.Count;
                _files.RemoveAll(match);
                return cnt != _files.Count;
            }
        }

        public void ClearFiles()
        {
            lock (_locker)
            {
                _files.Clear();
            }
        }



        [JsonIgnore]
        private JobStatus _status = JobStatus.NotDownloaded;
        public JobStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }


        [JsonIgnore]
        private int _percent;
        public int Percent
        {
            get => _percent;
            set => SetProperty(ref _percent, value);
        }
        

        /// <summary>
        /// Only for top level jobs (Movie, Series, Playlist)
        /// </summary>
        public void Update()
        {
            //Total = 100% for each video + 1% for each other file

            double downloaded = 0;
            double total = 0;
            JobStatus status = JobStatus.Downloaded;
            foreach (var file in Files)
            {
                total += file.IsVideo ? 100 : 1;
                if (System.IO.File.Exists(file.LocalFile()))
                {
                    downloaded += file.IsVideo ? 100 : 1;
                }
                else
                {
                    if (file.IsVideo)
                        downloaded += file.Percent;
                    status = JobStatus.Downloading;
                }
            }


            Status = status;
            if (status == JobStatus.Downloaded)
                Percent = 100;
            else
                Percent = Math.Min(99, (int)Math.Floor(downloaded / total * 100));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;
            backingStore = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
