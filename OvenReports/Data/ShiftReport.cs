using System;

namespace OvenReports.Data
{
    public class ShiftReport
    {
        public int ShiftNumber { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int CoilsCount { get; set; }
        public int CoilsWeight { get; set; }

        public ShiftReport()
        {
            ShiftNumber = default;
            PeriodStart = new DateTime();
            PeriodEnd = new DateTime();
            CoilsCount = default;
            CoilsWeight = default;
        }
    }
}