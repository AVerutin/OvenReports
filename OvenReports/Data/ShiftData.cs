using System;

namespace OvenReports.Data
{
    public class ShiftData
    {
        public int Number { get; set; }
        public int Count { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }

        public ShiftData()
        {
            Number = 0;
            Count = 0;
            StartTime = new DateTime();
            FinishTime = new DateTime();
        }
    }
}