using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Tree;
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
        private string _loading = "hidden;";
        
        protected override void OnInitialized()
        {
            Initialize();
        }

        private void Initialize()
        {

        }
        
        private void _setLoading(bool visible)
        {
            _loading = visible ? "visible;" : "hidden;";
        }

        private async void GetReportByPeriod()
        {
            _setLoading(true);
            _reportList = new List<ShiftReport>();
            await Task.Delay(100);
            
            DateTime rangeStart = _meltsPeriod.PeriodStart.AddDays(-1);
            DateTime rangeFinish = _meltsPeriod.PeriodFinish;
            string timeStart = $"{rangeStart.Day}-{rangeStart.Month}-{rangeStart.Year} 20:00:00.000";
            string timeFinish = $"{rangeFinish.Day}-{rangeFinish.Month}-{rangeFinish.Year} 20:00:00.000";
            DateTime periodStart = DateTime.Parse(timeStart);
            DateTime periodFinish = DateTime.Parse(timeFinish);

            _reportList = _reports.GetShiftsReport(periodStart, periodFinish);

            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Формирование отчета за текущую смены
        /// </summary>
        private async void GetReportByCurrentShift()
        {
            _setLoading(true);
            _reportList = new List<ShiftReport>();
            await Task.Delay(100);
            
            ShiftData current = _shift.GetCurrentShift();
            _meltInfo.StartDate = current.StartTime;
            _meltInfo.FinishDate = current.FinishTime;
            _reportList = _reports.GetShiftsReport(current.StartTime, current.FinishTime);
            
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Формирование отчета за предыдущую смену
        /// </summary>
        private async void GetReportByPreviousShift()
        {
            _setLoading(true);
            _reportList = new List<ShiftReport>();
            await Task.Delay(100);
            
            ShiftData previous = _shift.GetPreviousShift();
            _meltInfo.StartDate = previous.StartTime;
            _meltInfo.FinishDate = previous.FinishTime;
            _reportList = _reports.GetShiftsReport(previous.StartTime, previous.FinishTime);
            
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }
        
        /// <summary>
        /// Подготовка данных по часам за выбранную смену 
        /// </summary>
        /// <param name="startPeriod">Начало смены</param>
        /// <param name="finishPeriod">Окончание смены</param>
        private async void GetPrepareCoils(DateTime startPeriod, DateTime finishPeriod)
        {
            _setLoading(true);
            _meltsList = new List<MeltsForPeriod>();
            await Task.Delay(100);
            
            _meltInfo.StartDate = startPeriod;
            _meltInfo.FinishDate = finishPeriod;
            _meltsList = _reports.GetShiftReport(startPeriod, finishPeriod);
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }
    }
}