using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public partial class CoilsByDay
    {
        private struct MeltInfo
        {
            public DateTime StartPeriod;
            public DateTime FinishPeriod;
        }
        
        private readonly MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private List<MeltsForPeriod> _meltsList = new List<MeltsForPeriod>();
        private readonly Reports _reports = new Reports();
        private readonly DbConnection _db = new DbConnection();
        private List<CoilData> _selectedMelt = new List<CoilData>();
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

        /// <summary>
        /// Получить список плавок за день
        /// </summary>
        private async void GetMeltsList()
        {
            _setLoading(true);
            _meltsList = new List<MeltsForPeriod>();
            await Task.Delay(100);
            
            string start =
                $"{_meltsPeriod.PeriodStart.Day}-{_meltsPeriod.PeriodStart.Month}-{_meltsPeriod.PeriodStart.Year} 00:00:00.000";
            string finish =
                $"{_meltsPeriod.PeriodFinish.Day}-{_meltsPeriod.PeriodFinish.Month}-{_meltsPeriod.PeriodFinish.Year} 23:59:59.999";
            
            _meltsList = _reports.GetMeltsByDay(DateTime.Parse(start), DateTime.Parse(finish));
            _showReport = "block";
            
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Получить список бунтов за час
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="hour"Час></param>
        private async void GetPrepareCoils(DateTime date, int hour)
        {
            _setLoading(true);
            _selectedMelt = new List<CoilData>();
            await Task.Delay(100);

            string startTime = $"{date.Day}-{date.Month}-{date.Year} {hour}:00:00";
            DateTime start = DateTime.Parse(startTime);
            DateTime finish = start.AddHours(1);

            _meltInfo.StartPeriod = start;
            _meltInfo.FinishPeriod = finish;
            string melt = "";
            double diam = 0;
            
            List<CoilData> coils = _db.GetHourlyCoilsByPeriodDetail(start, finish);
            foreach (CoilData coil in coils)
            {
                if (melt != coil.MeltNumber || Math.Abs(diam - coil.Diameter) > 0.1)
                {
                    if (melt != "" && diam != 0)
                    {
                        _selectedMelt.Add(new CoilData());
                    }
                    melt = coil.MeltNumber;
                    diam = coil.Diameter;
                }
                _selectedMelt.Add(coil);
            }
            
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Получить список плавок за текущие сутки
        /// </summary>
        private async void GetCurrentDay()
        {
            _setLoading(true);
            _meltsList = new List<MeltsForPeriod>();
            await Task.Delay(100);
            
            DateTime now = DateTime.Now;
            string todayStart = $"{now.Day}-{now.Month}-{now.Year} 00:00:00.000";
            string todayFinish = $"{now.Day}-{now.Month}-{now.Year} 23:59:59.999";
            
            _meltsList = _reports.GetMeltsByDay(DateTime.Parse(todayStart), DateTime.Parse(todayFinish));
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Получить список плавок за предыдущие сутки
        /// </summary>
        private async void GetPrevDay()
        {
            _setLoading(true);
            _meltsList = new List<MeltsForPeriod>();
            await Task.Delay(100);
            
            DateTime yesterday = DateTime.Now.AddDays(-1);
            string yesterdayStart = $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 00:00:00.000";
            string yesterdayFinish = $"{yesterday.Day}-{yesterday.Month}-{yesterday.Year} 23:59:59.999";
            
            _meltsList = _reports.GetMeltsByDay(DateTime.Parse(yesterdayStart), DateTime.Parse(yesterdayFinish));
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }
    }
}
