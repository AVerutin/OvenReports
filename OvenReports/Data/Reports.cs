using System;
using System.Collections.Generic;
using NLog;

namespace OvenReports.Data
{
    public class Reports
    {
        private readonly Logger _logger;
        private readonly DBConnection _db = new DBConnection();
        
        public Reports()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public List<LandingData> GetMelt(string meltNumber)
        {
            List<LandingData> result = new List<LandingData>();
            List<LandingData> meltsList = new List<LandingData>();
                
            try
            {
                result = _db.GetMeltByNumber(meltNumber);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить плавки №{meltNumber} [{ex.Message}]");
            }

            foreach (LandingData melt in result)
            {
                if(melt.Weighted>0)
                    meltsList.Add(melt);
            }

            foreach (LandingData melt in result)
            {
                if(melt.Weighted>0)
                {
                    try
                    {
                        melt.CoilList = _db.GetCoilsByMelt(melt.MeltNumber, melt.Diameter, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(
                            $"Не удалось получить список бунтов для плавки №{melt.MeltNumber}, диаметр {melt.Diameter} [{ex.Message}]");
                    }

                    melt.FirstWeighting = melt.CoilList[0].DateWeight;
                    melt.LastWeighting = melt.CoilList[^1].DateWeight;

                    foreach (CoilData coil in melt.CoilList)
                    {
                        melt.WeightReal += coil.WeightFact;
                    }
                }
            }

            return meltsList;
        }

        public List<LandingData> GetMelts(DateTime start, DateTime finish)
        {
            string dateStart = $"{start.Day}-{start.Month}-{start.Year} 00:00:00.000";
            start = DateTime.Parse(dateStart);
            string dateFinish = $"{finish.Day}-{finish.Month}-{finish.Year} 23:59:59.999";
            finish = DateTime.Parse(dateFinish);
            
            List<LandingData> meltsList = new List<LandingData>();
            List<MeltsList> melts = new List<MeltsList>();
            
            try
            {
                melts = _db.GetMeltsListSummary(start, finish);
                
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Не удалось получить список плавок за период с [{start}] по [{finish}] [{ex.Message}]");
            }

            foreach (MeltsList melt in melts)
            {
                LandingData item = new LandingData();
                item.MeltNumber = melt.MeltNumber;
                item.ProductProfile = melt.ProductProfile;
                item.Diameter = melt.Diameter;
                item.FirstWeighting = melt.PeriodStart;
                item.LastWeighting = melt.PeriodFinish;
                item.SteelMark = melt.SteelMark;
                item.IngotProfile = melt.IngotProfile;
                item.IngotsCount = melt.IngotsCount;
                item.IngotLength = melt.IngotLength;
                item.Standart = melt.Standart;
                item.ProductCode = melt.ProductCode;
                item.Customer = melt.Customer;
                item.Weighted = melt.CoilsCount;
                item.WeightReal = melt.TotalWeight;
                
                meltsList.Add(item);
            }

            return meltsList;
        }
    }
}