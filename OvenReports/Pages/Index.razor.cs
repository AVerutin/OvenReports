using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public partial class Index
    {
        private struct MeltInfo
        {
            public string MeltNumber;
            public double Diameter;
        }
        
        private readonly MeltsForPeriod _meltsPeriod = new MeltsForPeriod();
        private readonly Reports _reports = new Reports();
        private List<LandingData> _meltsList = new List<LandingData>();
        
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

        private async void GetMelt()
        {
            _setLoading(true);
            _meltsList = new List<LandingData>();
            await Task.Delay(100);
            
            string meltNumber = _meltsPeriod.MeltNumber;
            if (!string.IsNullOrEmpty(meltNumber))
            {
                _meltsList = _reports.GetMelt(meltNumber);
                _showReport = "block";
            }  
            
            _setLoading(false);
            StateHasChanged();
        }

        private async void GetMelts()
        {
            _setLoading(true);
            _meltsList = new List<LandingData>();
            await Task.Delay(100);
            
            DateTime start = _meltsPeriod.PeriodStart;
            DateTime finish = _meltsPeriod.PeriodFinish;
            
            string dateStart = $"{start.Day}-{start.Month}-{start.Year} 00:00:00.000";
            start = DateTime.Parse(dateStart);
            string dateFinish = $"{finish.Day}-{finish.Month}-{finish.Year} 23:59:59.999";
            finish = DateTime.Parse(dateFinish);

            _meltsList = _reports.GetMelts(start, finish, OrderTypes.OrderByMeltNumber);
            _showReport = "block";
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Получить список бунтов для выбранной плавки
        /// </summary>
        /// <param name="meltNumber">Номер плавки</param>
        /// <param name="diameter">Диаметр прокатываемого профиля</param>
        private void GetPrepareCoils(string meltNumber, double diameter)
        {
            // Получать список бунтов по запросу из БД
            _selectedMelt = _db.GetCoilsByMelt(meltNumber, diameter, false);
            _meltInfo.MeltNumber = _selectedMelt[0].MeltNumber;
            _meltInfo.Diameter = _selectedMelt[0].Diameter;
        }
    }
}