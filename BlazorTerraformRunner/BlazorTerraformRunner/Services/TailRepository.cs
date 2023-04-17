using BlazorTerraformRunner.Models;
using k8s;
using k8s.Models;
using Microsoft.Extensions.FileSystemGlobbing;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlazorTerraformRunner.Services
{
    public class TailRepository : INotifyPropertyChanged
    {
        JobRun? _currentJobRun;

        public event PropertyChangedEventHandler? PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public JobRun? CurrentJobRun
        {
            get
            {
                return _currentJobRun;
            }

            set
            {
                if (value != _currentJobRun)
                {
                    _currentJobRun = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void SetPodStatus(string podStatus)
        {
            if (_currentJobRun != null && _currentJobRun?.PodStatus != podStatus)
            {
                _currentJobRun.PodStatus = podStatus;
                NotifyPropertyChanged(nameof(_currentJobRun.PodStatus));
            }
        }
    }
}
