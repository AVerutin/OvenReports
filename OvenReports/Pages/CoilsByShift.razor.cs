using System;
using System.Collections.Generic;
using OvenReports.Data;
using NLog;

namespace OvenReports.Pages
{
    public partial class CoilsByShift
    {
        private struct MeltInfo
        {
            public string MeltNumber;
            public double Diameter;
        }
        
        private MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private List<ShiftReport> _reportList = new List<ShiftReport>();
        private DBConnection _db = new DBConnection();
        private Shift _shift = new Shift();
        private List<CoilData> _selectedMelt = new List<CoilData>();
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
            try
            {
                _reportList = _db.GetShiftReport(_meltsPeriod.PeriodStart, _meltsPeriod.PeriodFinish);
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить список плавок за период с [{_meltsPeriod.PeriodStart}] по [{_meltsPeriod.PeriodFinish}] [{ex.Message}]");
            }
            
            _showReport = "block";
            StateHasChanged();
        }

        private void GetReportByToday()
        {
            ShiftData current = _shift.GetCurrentShift();
            // DateTime now = DateTime.Now;
            // string todayStart = $"{now.Day}-{now.Month}-{now.Year} 00:00:00.000";
            // string todayFinish = $"{now.Day}-{now.Month}-{now.Year} 23:59:59.999";
            try
            {
                _reportList = _db.GetShiftReport(current.StartTime, current.FinishTime);
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить список плавок за период с [{current.StartTime}] по [{current.FinishTime}] [{ex.Message}]");
            }
            
            _showReport = "block";
            StateHasChanged();
        }

        private void GetReportByYesterday()
        {
            ShiftData previous = _shift.GetPreviousShift();
            // DateTime yesterday = DateTime.Now.AddDays(-1);
            // string yesterdayStart = $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 00:00:00.000";
            // string yesterdayFinish = $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 23:59:59.999";
            try
            {
                _reportList = _db.GetShiftReport(previous.StartTime, previous.FinishTime);
            }
            catch (Exception ex)
            {
                _logger.Error($"Не удалось получить список плавок за период с [{previous.StartTime}] по [{previous.FinishTime}] [{ex.Message}]");
            }
            
            _showReport = "block";
            StateHasChanged();
        }

        private void PrepareCoils(int meltId)
        {
            // foreach (MeltsForPeriod melt in _meltsList)
            // {
            //     if (melt.MeltId == meltId)
            //     {
            //         _selectedMelt = melt.CoilsList;
            //         _meltInfo.MeltNumber = melt.MeltNumber;
            //         _meltInfo.Diameter = melt.Diameter;
            //     }
            // }
        }
    }
}