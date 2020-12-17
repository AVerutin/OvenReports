using System;
using System.Collections.Generic;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public partial class DowntimesByShift
    {
        List<DownTime> _downTimes = new List<DownTime>();
        private readonly MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private readonly Reports _reports = new Reports();
        private string _showReport = "none";

        protected override void OnInitialized()
        {
            Initialize();
        }

        private void Initialize()
        {

        }

        /// <summary>
        /// Сформировать список простоев за период
        /// </summary>
        private void GetDowntimes()
        {
            DateTime startShift = _meltsPeriod.PeriodStart.AddDays(-1);
            string dateStart =
                $"{startShift.Day}-{startShift.Month}-{startShift.Year} 20:00:00.000";
            DateTime start = DateTime.Parse(dateStart);
            string dateFinish =
                $"{_meltsPeriod.PeriodFinish.Day}-{_meltsPeriod.PeriodFinish.Month}-{_meltsPeriod.PeriodFinish.Year} 19:59:59.999";
            DateTime finish = DateTime.Parse(dateFinish);
            
            _downTimes = _reports.GetDowntimesByShift(start, finish);
            
            _showReport = "block";
            StateHasChanged();
        }

        /// <summary>
        /// Сформировать список простоев за текущую смену
        /// </summary>
        private void GetCurrentShift()
        {
            Shift shift = new Shift();
            ShiftData current = shift.GetCurrentShift();

            _downTimes = _reports.GetDowntimesByShift(current.StartTime, current.FinishTime);
            
            _showReport = "block";
            StateHasChanged();
        }

        /// <summary>
        /// Сформировать список простоев за предыдущую смену
        /// </summary>
        private void GetPrevShift()
        {
            Shift shift = new Shift();
            ShiftData previous = shift.GetPreviousShift();

            _downTimes = _reports.GetDowntimesByShift(previous.StartTime, previous.FinishTime);
            
            _showReport = "block";
            StateHasChanged();
        }
    }
}