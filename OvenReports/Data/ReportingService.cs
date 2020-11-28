using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OvenReports.Annotations;

namespace OvenReports.Data
{
    public class ReportingService : INotifyPropertyChanged
    {
        public Dictionary<string,double> MeltsList = new Dictionary<string, double>();
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}