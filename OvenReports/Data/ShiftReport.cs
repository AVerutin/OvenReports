using System;

namespace OvenReports.Data
{
    public class ShiftReport
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int CoilsCount { get; set; }
        public int CoilsWeight { get; set; }

        public ShiftReport()
        {
            PeriodStart = new DateTime();
            PeriodEnd = new DateTime();
            CoilsCount = default;
            CoilsWeight = default;
        }
    }
}