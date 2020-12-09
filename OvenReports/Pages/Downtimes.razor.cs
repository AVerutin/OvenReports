using System;
using System.Collections.Generic;
using OvenReports.Data;
using NLog;

namespace OvenReports.Pages
{
    public partial class Downtimes
    {
        List<DownTime> _downTimes = new List<DownTime>();
        private readonly MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private readonly DBConnection _db = new DBConnection();

        private string _showReport = "none";
        private Logger _logger;
        
        protected override void OnInitialized()
        {
            _logger = LogManager.GetCurrentClassLogger();
            Initialize();
        }

        private void Initialize()
        {

        }

        private void GetDowntimes()
        {
            DateTime start = _meltsPeriod.PeriodStart;
            DateTime finish = _meltsPeriod.PeriodFinish;
            string dateStart = $"{start.Day}-{start.Month}-{start.Year} 00:00:00.000";
            start = DateTime.Parse(dateStart);
            string dateFinish = $"{finish.Day}-{finish.Month}-{finish.Year} 23:59:59.999";
            finish = DateTime.Parse(dateFinish);

            _downTimes = _db.GetDowntimes(start, finish);
            
            _showReport = "block";
            StateHasChanged();
        }
    }
}