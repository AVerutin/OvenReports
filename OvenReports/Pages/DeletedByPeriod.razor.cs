using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public partial class DeletedByPeriod
    {
        private readonly Reports _reports = new Reports();
        private List<DeletedIngots> _deletedIngots;
        private string _showReport = "none";
        private string _loading = "hidden;";
        private string _selectRow = "none;";
        
        private DateTime _timeBegin;
        private DateTime _timeEnd;
        
        protected override void OnInitialized()
        {
            Initialize();
        }

        private void Initialize()
        {
            _timeEnd = DateTime.Now;
            _timeBegin = DateTime.Parse($"{_timeEnd.Day}-{_timeEnd.Month}-{_timeEnd.Year} 00:00:00");
            _deletedIngots = new List<DeletedIngots>();
        }
        
        private void _setLoading(bool visible)
        {
            _loading = visible ? "visible;" : "hidden;";
        }

        /// <summary>
        /// Вывести данные за выбранный период
        /// </summary>
        private async void GetData()
        {
            _setLoading(true);
            _deletedIngots = new List<DeletedIngots>();
            await Task.Delay(100);

            _deletedIngots = _reports.GetDeletedIngotsByPeriod(_timeBegin, _timeEnd);
            _showReport = "block";
            
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Вывести данные за текущие сутки
        /// </summary>
        private async void GetCurrentDay()
        {
            _setLoading(true);
            _deletedIngots = new List<DeletedIngots>();
            await Task.Delay(100);

            DateTime today = DateTime.Now;
            DateTime start = DateTime.Parse(today.ToString("dd-MM-yyyy 00:00:00.000"));
            DateTime end = DateTime.Parse(today.ToString("dd-MM-yyyy 23:59:59.999"));
            _deletedIngots = _reports.GetDeletedIngotsByPeriod(start, end);
            _showReport = "block";
            
            _setLoading(false);
            StateHasChanged();
        }

        /// <summary>
        /// Вывести данные за предыдущие сутки
        /// </summary>
        private async void GetPrevDay()
        {
            _setLoading(true);
            _deletedIngots = new List<DeletedIngots>();
            await Task.Delay(100);
    
            DateTime yesterday = DateTime.Now.AddDays(-1);
            DateTime start = DateTime.Parse(yesterday.ToString("dd-MM-yyyy 00:00:00.000"));
            DateTime end = DateTime.Parse(yesterday.ToString("dd-MM-yyyy 23:59:59.999"));
            _deletedIngots = _reports.GetDeletedIngotsByPeriod(start, end);
            _showReport = "block";
            
            _setLoading(false);
            StateHasChanged();
        }
    }
}