using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public partial class DowntimesByShift
    {
        List<DownTime> _downTimes = new List<DownTime>();
        private readonly MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private readonly Reports _reports = new Reports();
        private string _showReport = "none";
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

        /// <summary>
        /// Сформировать список простоев за период
        /// </summary>
        private async void GetDowntimes()
        {
            _setLoading(true);
            _downTimes = new List<DownTime>();
            await Task.Delay(100);
            
            DateTime startShift = _meltsPeriod.PeriodStart.AddDays(-1);
            string dateStart =
                $"{startShift.Day}-{startShift.Month}-{startShift.Year} 20:00:00.000";
            DateTime start = DateTime.Parse(dateStart);
            string dateFinish =
                $"{_meltsPeriod.PeriodFinish.Day}-{_meltsPeriod.PeriodFinish.Month}-{_meltsPeriod.PeriodFinish.Year} 19:59:59.999";
            DateTime finish = DateTime.Parse(dateFinish);
            
            _downTimes = _reports.GetDowntimesByShift(start, finish);
            
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Сформировать список простоев за текущую смену
        /// </summary>
        private async void GetCurrentShift()
        {
            _setLoading(true);
            _downTimes = new List<DownTime>();
            await Task.Delay(100);
            
            Shift shift = new Shift();
            ShiftData current = shift.GetCurrentShift();

            _downTimes = _reports.GetDowntimesByShift(current.StartTime, current.FinishTime);
            
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Сформировать список простоев за предыдущую смену
        /// </summary>
        private async void GetPrevShift()
        {
            _setLoading(true);
            _downTimes = new List<DownTime>();
            await Task.Delay(100);
            
            Shift shift = new Shift();
            ShiftData previous = shift.GetPreviousShift();

            _downTimes = _reports.GetDowntimesByShift(previous.StartTime, previous.FinishTime);
            
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }
    }
}