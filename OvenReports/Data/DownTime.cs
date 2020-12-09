using System;

namespace OvenReports.Data
{
    public class DownTime
    {
        public DateTime TimeStart { get; set; }
        public DateTime TimeFinish { get; set; }
        public string Comment { get; set; }

        public DownTime()
        {
            TimeStart = new DateTime();
            TimeFinish = new DateTime();
            Comment = default;
        }
    }
}