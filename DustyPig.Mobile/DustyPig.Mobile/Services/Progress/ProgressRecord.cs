using System;

namespace DustyPig.Mobile.Services.Progress
{
    internal class ProgressRecord
    {
        public int MediaId { get; set; }
        public double Played { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}
