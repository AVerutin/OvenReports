using System;
using System.Collections.Generic;

// using NLog;

namespace OvenReports.Data
{
    public class MeltsForPeriod
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodFinish { get; set; }
        public DateTime FirstWeighting { get; set; }
        public DateTime LastWeighting { get; set; }
        public int MeltId { get; set; }
        public string MeltNumber { get; set; }
        public DateTime WeightingData { get; set; }
        public int WeightingHourStart { get; set; }
        public int WeightingHourFinish { get; set; }
        public int IngotsCount { get; set; }
        public int CoilsCount { get; set; }
        public int TotalCoilsCount { get; set; }
        public int WeightFact { get; set; }
        public int TotalWeightFact { get; set; }
        public string SteelMark { get; set; }
        public string IngotProfile { get; set; }
        public string Standart { get; set; }
        public double Diameter { get; set; }
        public string Customer { get; set; }
        public int ShiftNumber { get; set; }

        public List<CoilData> CoilsList { get; set; }
        
        // private Logger _logger = LogManager.GetCurrentClassLogger();

        public MeltsForPeriod()
        {
            PeriodFinish = DateTime.Now;
            FirstWeighting = DateTime.Now;
            LastWeighting = DateTime.Now;
            PeriodStart = GetStartOfMonth(PeriodFinish);
            CoilsList = new List<CoilData>();
            TotalCoilsCount = default;
            MeltId = default;
            MeltNumber = default;
            WeightingData = default;
            WeightingHourStart = default;
            WeightingHourFinish = default;
            WeightFact = default;
            TotalWeightFact = default;
            IngotsCount = default;
            CoilsCount = default;
            SteelMark = default;
            IngotProfile = default;
            Standart = default;
            Diameter = default;
            Customer = default;
            ShiftNumber = default;
        }

        public DateTime GetStartOfMonth(DateTime date)
        {
            DateTime result = new DateTime(date.Year, date.Month, 1);

            return result;
        }

        public DateTime GetLastDayOfMonth(DateTime date)
        {
            DateTime startDate = GetStartOfMonth(date); 
            DateTime finishDate = startDate.AddMonths(1).AddDays(-1);

            return finishDate;
        }
    }
}