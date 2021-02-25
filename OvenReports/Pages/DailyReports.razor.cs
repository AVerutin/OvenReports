using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        
        private readonly MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private readonly Reports _reports = new Reports();
        private List<DailyReport> _reportList = new List<DailyReport>();
        private List<MeltsForPeriod> _meltsList = new List<MeltsForPeriod>();
        private readonly DbConnection _db = new DbConnection();
        private string _showReport = "none";
        private MeltInfo _meltInfo;
        private Logger _logger;
        private string _loading = "hidden;";
        
        protected override void OnInitialized()
        {
            _logger = LogManager.GetCurrentClassLogger();
            Initialize();
        }

        private void Initialize()
        {
        }
        
        private void _setLoading(bool visible)
        {
            _loading = visible ? "visible;" : "hidden;";
        }

        /// <summary>
        /// Сформировать отчет за период
        /// </summary>
        private async void GetReportByPeriod()
        {
            _setLoading(true);
            _reportList = new List<DailyReport>();
            List<DailyReport> result = new List<DailyReport>();
            await Task.Delay(100);
            
            DateTime periodStart = DateTime.Parse($"{_meltsPeriod.PeriodStart:d} 00:00:00.000");
            DateTime periodFinish = DateTime.Parse($"{_meltsPeriod.PeriodFinish:d} 23:59:59.999");
            
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
            _setLoading(false);
            StateHasChanged();
        }

        private async void GetReportByToday()
        {
            _setLoading(true);
            _reportList = new List<DailyReport>();
            await Task.Delay(100);
            
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
            _setLoading(false);
            StateHasChanged();
        }

        private async void GetReportByYesterday()
        {
            _setLoading(true);
            _reportList = new List<DailyReport>();
            await Task.Delay(100);
            
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
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Получить данные по бунтам за выбранный период
        /// </summary>
        /// <param name="start">Начало периода</param>
        /// <param name="finish">Конец периода</param>
        private async void GetPrepareCoils(DateTime start, DateTime finish)
        {
            _setLoading(true);
            _meltsList = new List<MeltsForPeriod>();
            await Task.Delay(100);
            
            DateTime periodStart = DateTime.Parse($"{start:d} 00:00:00.000");
            DateTime periodFinish = DateTime.Parse($"{finish:d} 23:59:59.999");
            _meltInfo.StartDate = periodStart;
            _meltInfo.FinishDate = periodFinish;

            _meltsList = _reports.GetDailyReport(periodStart, periodFinish);
            _setLoading(false);
            StateHasChanged();
        }
    }
}