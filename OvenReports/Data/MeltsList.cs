using System;

namespace OvenReports.Data
{
    public class MeltsList
    {
        public string MeltNumber { get; set; }
        public string ProductProfile { get; set; }
        public double Diameter { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodFinish { get; set; }
        public string SteelMark { get; set; }
        public string IngotProfile { get; set; }
        public int IngotsCount { get; set; }
        public int IngotLength { get; set; }
        public string Standart { get; set; }
        public int ProductCode { get; set; }
        public string Customer { get; set; }
        public int CoilsCount { get; set; }
        public int TotalWeight { get; set; }

        public MeltsList()
        {
            MeltNumber = default;
            ProductProfile = default;
            Diameter = default;
            PeriodStart = default;
            PeriodFinish = default;
            SteelMark = default;
            IngotProfile = default;
            IngotsCount = default;
            IngotLength = default;
            Standart = default;
            ProductCode = default;
            Customer = default;
            CoilsCount = default;
            TotalWeight = default;
        }
    }
}