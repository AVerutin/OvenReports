using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OvenReports.Data;

namespace OvenReports.Pages
{
    public class Period
    {
        public DateTime TimeBegin;
        public DateTime TimeEnd;
    }
    
    public partial class CheckDt
    {
        private readonly Reports _reports = new Reports();
        private List<CheckDtData> _checkDate;
        private string _showReport = "none";
        private string _loading = "hidden;";
        private Period _period = new Period();
        private string _selectRow = "none;";
        
        protected override void OnInitialized()
        {
            Initialize();
        }

        private void Initialize()
        {
            DateTime now = DateTime.Now;
            _period.TimeBegin = DateTime.Parse($"{now.Day}-{now.Month}-{now.Year} 00:00:00");
            _period.TimeEnd = now;
            _checkDate = new List<CheckDtData>();
        }
        
        private void _setLoading(bool visible)
        {
            _loading = visible ? "visible;" : "hidden;";
        }

        private async void GetData()
        {
            _setLoading(true);
            _checkDate = new List<CheckDtData>();
            await Task.Delay(100);

            DateTime begin = DateTime.Parse(_period.TimeBegin.ToString("dd-MM-yyyy 00:00:00.000"));
            DateTime end = DateTime.Parse(_period.TimeEnd.ToString("dd-MM-yyyy 23:59:59.999"));
            _checkDate = _reports.GetCheckDt(begin, end);
            _showReport = "block";
            
            _setLoading(false);
            StateHasChanged();
        }
    }
}