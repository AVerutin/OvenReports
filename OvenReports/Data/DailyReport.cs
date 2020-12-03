using System;

namespace OvenReports.Data
{
    public class DailyReport
    {
        public DateTime Date { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int CoilsCount { get; set; }
        public int CoilsWeight { get; set; }
        public int TotalCoils { get; set; }
        public int TotalWeight { get; set; }

        public DailyReport()
        {
            Date = new DateTime();
            PeriodStart = new DateTime();
            PeriodEnd = new DateTime();
            CoilsCount = default;
            CoilsWeight = default;
            TotalCoils = default;
            TotalWeight = default;
        }
    }
}