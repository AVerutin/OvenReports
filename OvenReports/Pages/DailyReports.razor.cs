using System;
using System.Collections.Generic;
using OvenReports.Data;
using NLog;

namespace OvenReports.Pages
{
    public partial class DailyReports
    {
        private struct MeltInfo
        {
            public DateTime StartDate;
            public DateTime FinishDate;
        }
        
        private MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private readonly Reports _reports = new Reports();
        private List<DailyReport> _reportList = new List<DailyReport>();
        private List<MeltsForPeriod> _meltsList = new List<MeltsForPeriod>();
        private DBConnection _db = new DBConnection();
        private string _showReport = "none";
        private MeltInfo _meltInfo;
        private Logger _logger;
        
        protected override void OnInitialized()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Запущен отчет по бунтам");
            Initialize();
        }

        private void Initialize()
        {
            // _meltsPeriod.PeriodFinish = DateTime.Now; // GetCurrentTime();
            // _meltsPeriod.PeriodStart = _shift.GetShiftStart(_meltsPeriod.PeriodFinish);
        }

        private void GetReportByPeriod()
        {
            DateTime periodStart = DateTime.Parse($"{_meltsPeriod.PeriodStart:d} 00:00:00.000");
            DateTime periodFinish = DateTime.Parse($"{_meltsPeriod.PeriodFinish:d} 23:59:59.999");
            List<DailyReport> result = new List<DailyReport>();
            _reportList = new List<DailyReport>();
            
            try
            {
                result = _db.GetDailyReport(periodStart, periodFinish);
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить список плавок за период с [{_meltsPeriod.PeriodStart}] по [{_meltsPeriod.PeriodFinish}] [{ex.Message}]");
            }

            DateTime prev = periodStart.AddDays(-1);
            foreach (DailyReport day in result)
            {
                int diff = (int)(day.Date - prev.Date).TotalDays;
                if (diff > 1)
                {
                    for (int i = 0; i < diff - 1; i++)
                    {
                        prev = prev.AddDays(1);
                        DailyReport empty = new DailyReport
                        {
                            Date = prev,
                            PeriodStart = DateTime.Parse($"{prev:d} 00:00:00.000"),
                            PeriodEnd = DateTime.Parse($"{prev:d} 23:59:59.999")
                        };
                        _reportList.Add(empty);
                    }
                }
                
                _reportList.Add(day);
                prev = day.Date;
            }

            int coilsTotal = 0;
            int weightTotal = 0;
            foreach (DailyReport report in _reportList)
            {
                coilsTotal += report.CoilsCount;
                weightTotal += report.CoilsWeight;
                report.TotalCoils = coilsTotal;
                report.TotalWeight = weightTotal;
            }
            
            _showReport = "block";
            StateHasChanged();
        }

        private void GetReportByToday()
        {
            DateTime now = DateTime.Now;
            DateTime todayStart = DateTime.Parse($"{now:d} 00:00:00.000");
            DateTime todayFinish = DateTime.Parse($"{now:d} 23:59:59.999");
            
            try
            {
                _reportList = _db.GetDailyReport(todayStart, todayFinish);
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить список плавок за период с [{todayStart}] по [{todayFinish}] [{ex.Message}]");
            }
            
            int coilsTotal = 0;
            int weightTotal = 0;
            foreach (DailyReport report in _reportList)
            {
                coilsTotal += report.CoilsCount;
                weightTotal += report.CoilsWeight;
                report.TotalCoils = coilsTotal;
                report.TotalWeight = weightTotal;
            }
            
            _showReport = "block";
            StateHasChanged();
        }

        private void GetReportByYesterday()
        {
            DateTime yesterday = DateTime.Now.AddDays(-1);
            DateTime yesterdayStart = DateTime.Parse($"{yesterday:d} 00:00:00.000");
            DateTime yesterdayFinish = DateTime.Parse($"{yesterday:d} 23:59:59.999");
            
            try
            {
                _reportList = _db.GetDailyReport(yesterdayStart, yesterdayFinish);
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить список плавок за период с [{yesterdayStart}] по [{yesterdayFinish}] [{ex.Message}]");
            }
            
            int coilsTotal = 0;
            int weightTotal = 0;
            foreach (DailyReport report in _reportList)
            {
                coilsTotal += report.CoilsCount;
                weightTotal += report.CoilsWeight;
                report.TotalCoils = coilsTotal;
                report.TotalWeight = weightTotal;
            }
            
            _showReport = "block";
            StateHasChanged();
        }

        private void PrepareCoils(DateTime start, DateTime finish)
        {
            DateTime periodStart = DateTime.Parse($"{start:d} 00:00:00.000");
            DateTime periodFinish = DateTime.Parse($"{finish:d} 23:59:59.999");
            _meltInfo.StartDate = periodStart;
            _meltInfo.FinishDate = periodFinish;

            _meltsList = _reports.GetDailyReport(periodStart, periodFinish);
            StateHasChanged();
        }
    }
}