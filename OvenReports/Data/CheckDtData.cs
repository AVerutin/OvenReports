using System;

namespace OvenReports.Data
{
    public class CheckDtData
    {
        public int LandingId { get; set; }
        public string IngotMes { get; set; }
        public int CoilWeightMes { get; set; }
        public DateTime DateClose { get; set; }
        public string IngotDt { get; set; }
        public int CoilWeightDt { get; set; }
        public DateTime CoilDateParam { get; set; }
        public DateTime TimeBegin { get; set; }
        public DateTime TimeEnd { get; set; }
        public int BilletWeight { get; set; }
        public DateTime BilletDate { get; set; }

        public CheckDtData()
        {
            LandingId = default;
            IngotMes = default;
            CoilWeightMes = default;
            CoilWeightDt = default;
            DateClose = DateTime.MinValue;
            IngotDt = default;
            IngotMes = default;
            CoilDateParam = DateTime.MinValue;
            TimeBegin = DateTime.MinValue;
            TimeEnd = DateTime.MinValue;
            BilletWeight = default;
            BilletDate = DateTime.MinValue;
        }
    }
}