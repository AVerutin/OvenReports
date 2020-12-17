using System;
using System.Collections.Generic;
using OvenReports.Data;
using NLog;

namespace OvenReports.Pages
{
    public partial class DowntimesByDay
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

        /// <summary>
        /// Сформировать список простоев за период
        /// </summary>
        private void GetDowntimes()
        {
            DateTime start = _meltsPeriod.PeriodStart;
            DateTime finish = _meltsPeriod.PeriodFinish;
            string dateStart = $"{start.Day}-{start.Month}-{start.Year} 00:00:00.000";
            start = DateTime.Parse(dateStart);
            string dateFinish = $"{finish.Day}-{finish.Month}-{finish.Year} 23:59:59.999";
            finish = DateTime.Parse(dateFinish);

            _downTimes = _db.GetDowntimes(start, finish);
            
            // Заполнение длительности простоев и расчет общей длительности простоев
            DateTime lastDowntime = DateTime.MinValue;
            TimeSpan totalDuration = new TimeSpan();
            
            foreach (DownTime item in _downTimes)
            {
                if (lastDowntime == DateTime.MinValue)
                {
                    lastDowntime = item.TimeStart;
                }

                if (item.TimeFinish != DateTime.MinValue)
                {
                    item.TimeDuration = item.TimeFinish.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                else
                {
                    item.TimeDuration = DateTime.Now.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                
                item.DurationTotal = totalDuration;
            }
            
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

            _downTimes = _db.GetDowntimes(start, finish);
            
            // Заполнение длительности простоев и расчет общей длительности простоев
            DateTime lastDowntime = DateTime.MinValue;
            TimeSpan totalDuration = new TimeSpan();
            
            foreach (DownTime item in _downTimes)
            {
                if (lastDowntime == DateTime.MinValue)
                {
                    lastDowntime = item.TimeStart;
                }

                if (item.TimeFinish != DateTime.MinValue)
                {
                    item.TimeDuration = item.TimeFinish.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                else
                {
                    item.TimeDuration = DateTime.Now.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                
                item.DurationTotal = totalDuration;
            }
            
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

            _downTimes = _db.GetDowntimes(start, finish);
            
            // Заполнение длительности простоев и расчет общей длительности простоев
            DateTime lastDowntime = DateTime.MinValue;
            TimeSpan totalDuration = new TimeSpan();
            
            foreach (DownTime item in _downTimes)
            {
                if (lastDowntime == DateTime.MinValue)
                {
                    lastDowntime = item.TimeStart;
                }

                if (item.TimeFinish != DateTime.MinValue)
                {
                    item.TimeDuration = item.TimeFinish.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                else
                {
                    item.TimeDuration = DateTime.Now.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                
                item.DurationTotal = totalDuration;
            }
            
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

            _downTimes = _db.GetDowntimes(current.StartTime, current.FinishTime);
            
            // Заполнение длительности простоев и расчет общей длительности простоев
            DateTime lastDowntime = DateTime.MinValue;
            TimeSpan totalDuration = new TimeSpan();
            
            foreach (DownTime item in _downTimes)
            {
                if (lastDowntime == DateTime.MinValue)
                {
                    lastDowntime = item.TimeStart;
                }

                if (item.TimeFinish != DateTime.MinValue)
                {
                    item.TimeDuration = item.TimeFinish.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                else
                {
                    item.TimeDuration = DateTime.Now.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                
                item.DurationTotal = totalDuration;
            }
            
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

            _downTimes = _db.GetDowntimes(previous.StartTime, previous.FinishTime);
            
            // Заполнение длительности простоев и расчет общей длительности простоев
            DateTime lastDowntime = DateTime.MinValue;
            TimeSpan totalDuration = new TimeSpan();
            
            foreach (DownTime item in _downTimes)
            {
                if (lastDowntime == DateTime.MinValue)
                {
                    lastDowntime = item.TimeStart;
                }

                if (item.TimeFinish != DateTime.MinValue)
                {
                    item.TimeDuration = item.TimeFinish.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                else
                {
                    item.TimeDuration = DateTime.Now.Subtract(item.TimeStart);
                    totalDuration += item.TimeDuration;
                }
                
                item.DurationTotal = totalDuration;
            }
            
            _showReport = "block";
            StateHasChanged();
        }
    }
}