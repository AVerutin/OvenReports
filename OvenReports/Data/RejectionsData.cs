using System;

namespace OvenReports.Data
{
    public class RejectionsData
    {
        public string Melt { get; set; }
        public int IngotsCount { get; set; }
        public DateTime TimeBegin { get; set; }
        public DateTime TimeEnd { get; set; }

        public RejectionsData()
        {
            Melt = default;
            IngotsCount = default;
            TimeBegin = DateTime.MinValue;
            TimeEnd = DateTime.MinValue;
        }
    }
}