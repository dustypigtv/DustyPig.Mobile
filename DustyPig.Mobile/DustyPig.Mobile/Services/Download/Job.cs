using DustyPig.API.v3.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DustyPig.Mobile.Services.Download
{
    internal class Job
    {
        /// <summary>
        /// Movie / Series
        /// </summary>
        public int MediaId { get; set; }

        public MediaTypes MediaType { get; set; }

        public List<JobFile> Files { get; set; } = new List<JobFile>();

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


        public List<Job> SubJobs { get; set; }
                
    }
}
