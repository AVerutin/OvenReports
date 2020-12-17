using System;
using System.Collections.Generic;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public partial class DowntimesByDay
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
            _downTimes = _reports.GetDowntimesByDay(_meltsPeriod.PeriodStart, _meltsPeriod.PeriodFinish);
            
            _showReport = "block";
            StateHasChanged();
        }

        /// <summary>
        /// Сформировать список простоев за текущий день
        /// </summary>
        private void GetToday()
        {
            DateTime today = DateTime.Now;
            string dateStart = $"{today.Day}-{today.Month}-{today.Year} 00:00:00.000";
            DateTime start = DateTime.Parse(dateStart);
            string dateFinish = $"{today.Day}-{today.Month}-{today.Year} 23:59:59.999";
            DateTime finish = DateTime.Parse(dateFinish);

            _downTimes = _reports.GetDowntimesByDay(start, finish);
            
            _showReport = "block";
            StateHasChanged();
        }

        /// <summary>
        /// Сформировать список простоев за предыдущий день 
        /// </summary>
        private void GetYesterday()
        {
            DateTime yesterday = DateTime.Now.AddDays(-1);
            string dateStart = $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 00:00:00.000";
            DateTime start = DateTime.Parse(dateStart);
            string dateFinish = $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 23:59:59.999";
            DateTime finish = DateTime.Parse(dateFinish);

            _downTimes = _reports.GetDowntimesByDay(start, finish);

            _showReport = "block";
            StateHasChanged();
        }
    }
}