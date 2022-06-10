using DustyPig.API.v3.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DustyPig.Mobile.Services.Download
{
    internal class Job : INotifyPropertyChanged
    {
        /// <summary>
        /// Movie / Series
        /// </summary>
        public int MediaId { get; set; }

        public MediaTypes MediaType { get; set; }

        private List<JobFile> _jobFiles = new List<JobFile>();
        public List<JobFile> Files
        {
            get
            {
                if (_jobFiles == null)
                    _jobFiles = new List<JobFile>();
                return _jobFiles;
            }
            set
            {
                if (value == null)
                    _jobFiles = new List<JobFile>();
                else
                    _jobFiles = value;
            }
        }

        /// <summary>
        /// Only if Series/Playlist
        /// </summary>
        public int ItemsCount { get; set; }

        /// <summary>
        /// Whether this file SHOULD be downloaded, if this is a sub job. EG: for a series that only downloads the next 5 unwatched, only 5 of the jobs will have this flag set at any given time
        /// </summary>
        public bool Active { get; set; }

        public ushort Season { get; set; }

        public ushort Episode { get; set; }

        /// <summary>
        /// Playlist index
        /// </summary>
        public int Index { get; set; }

        private List<Job> _subJobs = new List<Job>();
        public List<Job> SubJobs
        {
            get
            {
                if (_subJobs == null)
                    _subJobs = new List<Job>();
                return _subJobs;
            }
            set
            {
                if (value == null)
                    _subJobs = new List<Job>();
                else
                    _subJobs = value;
            }
        }



        [JsonIgnore]
        public JobStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        private JobStatus _status = JobStatus.NotDownloaded;


        [JsonIgnore]
        public int Percent
        {
            get => _percent;
            set => SetProperty(ref _percent, value);
        }
        private int _percent;


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

            foreach (var subJob in SubJobs.Where(item => item.Active))
                foreach (var file in subJob.Files)
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
