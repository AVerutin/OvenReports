using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using OvenReports.Annotations;

namespace OvenReports.Data
{
    public sealed class ReportingService : INotifyPropertyChanged
    {
        public Dictionary<string,double> MeltsList = new Dictionary<string, double>();
        
        public event PropertyChangedEventHandler PropertyChanged;
        public readonly IWebHostEnvironment WebHostEnvironment;

        public ReportingService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment= webHostEnvironment;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}