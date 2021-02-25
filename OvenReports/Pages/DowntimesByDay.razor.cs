using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public partial class DowntimesByDay
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
            
            _downTimes = _reports.GetDowntimesByDay(_meltsPeriod.PeriodStart, _meltsPeriod.PeriodFinish);
            
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Сформировать список простоев за текущий день
        /// </summary>
        private async void GetToday()
        {
            _setLoading(true);
            _downTimes = new List<DownTime>();
            await Task.Delay(100);
            
            DateTime today = DateTime.Now;
            string dateStart = $"{today.Day}-{today.Month}-{today.Year} 00:00:00.000";
            DateTime start = DateTime.Parse(dateStart);
            string dateFinish = $"{today.Day}-{today.Month}-{today.Year} 23:59:59.999";
            DateTime finish = DateTime.Parse(dateFinish);

            _downTimes = _reports.GetDowntimesByDay(start, finish);
            
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Сформировать список простоев за предыдущий день 
        /// </summary>
        private async void GetYesterday()
        {
            _setLoading(true);
            _downTimes = new List<DownTime>();
            await Task.Delay(100);
            
            DateTime yesterday = DateTime.Now.AddDays(-1);
            string dateStart = $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 00:00:00.000";
            DateTime start = DateTime.Parse(dateStart);
            string dateFinish = $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 23:59:59.999";
            DateTime finish = DateTime.Parse(dateFinish);

            _downTimes = _reports.GetDowntimesByDay(start, finish);

            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }
    }
}