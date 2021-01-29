using System;
using System.Collections.Generic;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public partial class ShiftReports
    {
        private struct MeltInfo
        {
            public DateTime StartDate;
            public DateTime FinishDate;
        }
        
        private readonly MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private List<ShiftReport> _reportList = new List<ShiftReport>();
        private readonly Reports _reports = new Reports();
        private readonly Shift _shift = new Shift();
        private List<MeltsForPeriod> _meltsList = new List<MeltsForPeriod>();
        private string _showReport = "none";
        private MeltInfo _meltInfo;
        
        protected override void OnInitialized()
        {
            Initialize();
        }

        private void Initialize()
        {

        }

        private void GetReportByPeriod()
        {
            DateTime rangeStart = _meltsPeriod.PeriodStart.AddDays(-1);
            DateTime rangeFinish = _meltsPeriod.PeriodFinish;
            string timeStart = $"{rangeStart.Day}-{rangeStart.Month}-{rangeStart.Year} 20:00:00.000";
            string timeFinish = $"{rangeFinish.Day}-{rangeFinish.Month}-{rangeFinish.Year} 20:00:00.000";
            DateTime periodStart = DateTime.Parse(timeStart);
            DateTime periodFinish = DateTime.Parse(timeFinish);

            _reportList = _reports.GetShiftsReport(periodStart, periodFinish);

            _showReport = "block";
            StateHasChanged();
        }

        /// <summary>
        /// Формирование отчета за текущую смены
        /// </summary>
        private void GetReportByCurrentShift()
        {
            ShiftData current = _shift.GetCurrentShift();
            _meltInfo.StartDate = current.StartTime;
            _meltInfo.FinishDate = current.FinishTime;
            _reportList = _reports.GetShiftsReport(current.StartTime, current.FinishTime);
            
            _showReport = "block";
            StateHasChanged();
        }

        /// <summary>
        /// Формирование отчета за предыдущую смену
        /// </summary>
        private void GetReportByPreviousShift()
        {
            ShiftData previous = _shift.GetPreviousShift();
            _meltInfo.StartDate = previous.StartTime;
            _meltInfo.FinishDate = previous.FinishTime;
            _reportList = _reports.GetShiftsReport(previous.StartTime, previous.FinishTime);
            
            _showReport = "block";
            StateHasChanged();
        }
        
        /// <summary>
        /// Подготовка данных по часам за выбранную смену 
        /// </summary>
        /// <param name="startPeriod">Начало смены</param>
        /// <param name="finishPeriod">Окончание смены</param>
        private void GetPrepareCoils(DateTime startPeriod, DateTime finishPeriod)
        {
            _meltInfo.StartDate = startPeriod;
            _meltInfo.FinishDate = finishPeriod;
            _meltsList = _reports.GetShiftReport(startPeriod, finishPeriod);
            _showReport = "block";
            StateHasChanged();
        }
    }
}