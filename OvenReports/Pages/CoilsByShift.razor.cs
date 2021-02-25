using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public partial class CoilsByShift
    {
        private struct MeltInfo
        {
            public DateTime StartPeriod;
            public DateTime FinishPeriod;
        }
        
        private readonly MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private readonly Reports _reports = new Reports();
        private List<MeltsForPeriod> _meltsList = new List<MeltsForPeriod>();
        private List<CoilData> _selectedMelt = new List<CoilData>();
        private readonly DbConnection _db = new DbConnection();
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
        /// Сформировать отчет за период
        /// </summary>
        private async void GetReportByPeriod()
        {
            _setLoading(true);
            _meltsList = new List<MeltsForPeriod>();
            await Task.Delay(100);
            
            DateTime rangeStart = _meltsPeriod.PeriodStart.AddDays(-1);
            DateTime rangeFinish = _meltsPeriod.PeriodFinish;
            string timeStart = $"{rangeStart.Day}-{rangeStart.Month}-{rangeStart.Year} 20:00:00.000";
            string timeFinish = $"{rangeFinish.Day}-{rangeFinish.Month}-{rangeFinish.Year} 20:00:00.000";
            DateTime periodStart = DateTime.Parse(timeStart);
            DateTime periodFinish = DateTime.Parse(timeFinish);
            
            // Получение данных по смене
            Shift shift = new Shift();
            ShiftData shiftData = shift.GetShiftByDate(periodStart); 
            _meltsList = new List<MeltsForPeriod>();

            while (shiftData.FinishTime <= periodFinish)
            {
                List<MeltsForPeriod> melts = _reports.GetMeltsByShift(shiftData.StartTime, shiftData.FinishTime);

                foreach (MeltsForPeriod melt in melts)
                {
                    _meltsList.Add(melt);
                }

                if (shiftData.FinishTime != periodFinish && melts.Count > 0)
                    _meltsList.Add(new MeltsForPeriod());

                shiftData = shift.GetShiftByDate(shiftData.StartTime.AddHours(12));
            }

            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Сформировать отчет по текущей смене
        /// </summary>
        private async void GetReportCurrentShift()
        {
            _setLoading(true);
            _meltsList = new List<MeltsForPeriod>();
            await Task.Delay(100);
            
            // Получить данные по текущей смене
            Shift shift = new Shift();
            ShiftData currentShift = shift.GetCurrentShift();

            _meltsList = _reports.GetMeltsByShift(currentShift.StartTime, currentShift.FinishTime);
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Сформировать отчет по предыдущей смене
        /// </summary>
        private async void GetReportByPreviousShift()
        {
            _setLoading(true);
            _meltsList = new List<MeltsForPeriod>();
            await Task.Delay(100);
            
            // Получить данные по предыдущей смене
            Shift shift = new Shift();
            ShiftData previousShift = shift.GetPreviousShift();

            _meltsList = _reports.GetMeltsByShift(previousShift.StartTime, previousShift.FinishTime);
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Получить список бунтов за указанный час
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="hour">Час</param>
        private async void GetPrepareCoils(DateTime date, int hour)
        {
            _setLoading(true);
            _selectedMelt = new List<CoilData>();
            await Task.Delay(100);
            
            string startTime = $"{date.Day}-{date.Month}-{date.Year} {hour}:00:00.000";
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
    }
}