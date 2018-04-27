using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class Computer : NotifyObject
    {
        public enum OperationStatus
        {
            Indeterminate,
            OperationInProgress,
            Success,
            Failure,
            Pending,
            Possible,
            NotPossible,
            Necessary,
            NotNecessary
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                this.IsThisPc = string.Equals(value, Environment.MachineName, StringComparison.CurrentCultureIgnoreCase) ? true : false;
                OnPropertyChanged("Name");
            }
        }

        private bool _isThisPc;
        public bool IsThisPc
        {
            get { return _isThisPc; }
            set { _isThisPc = value; OnPropertyChanged("IsThisPc"); }
        }

        private string _applicationMode;
        public string ApplicationMode
        {
            get { return _applicationMode; }
            set { _applicationMode = value; OnPropertyChanged("ApplicationMode"); }
        }


        private OperationStatus _onlineStatus;
        public OperationStatus OnlineStatus
        {
            get { return _onlineStatus; }
            set { _onlineStatus = value; OnPropertyChanged("OnlineStatus"); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; OnPropertyChanged("IsExpanded"); }
        }

        private string _currentCoinName;
        public string CurrentCoinName
        {
            get { return _currentCoinName; }
            set { _currentCoinName = value; OnPropertyChanged("CurrentCoinName"); OnPropertyChanged("CurrentCoinNameAndSymbol"); }
        }

        private string _currentCoinSymbol;
        public string CurrentCoinSymbol
        {
            get { return _currentCoinSymbol; }
            set { _currentCoinSymbol = value; OnPropertyChanged("CurrentCoinSymbol"); OnPropertyChanged("CurrentCoinNameAndSymbol"); }
        }

        public string CurrentCoinNameAndSymbol
        {
            get
            {
                var str = $"{this.CurrentCoinName} ({this.CurrentCoinSymbol})";
                return str.Equals(" ()") ? string.Empty : str;
            }
        }

        private string _newCoinName;
        public string NewCoinName
        {
            get { return _newCoinName; }
            set { _newCoinName = value; OnPropertyChanged("NewCoinName"); }
        }

        private string _newCoinSymbol;
        public string NewCoinSymbol
        {
            get { return _newCoinSymbol; }
            set { _newCoinSymbol = value; OnPropertyChanged("NewCoinSymbol"); }
        }

        public string NewCoinNameAndSymbol { get { return $"{this.NewCoinName} ({this.NewCoinSymbol})"; } }

        private bool _switch;
        public bool Switch
        {
            get { return _switch; }
            set { _switch = value; OnPropertyChanged("Switch"); }
        }

        private OperationStatus _switchStatus;
        public OperationStatus SwitchStatus
        {
            get { return _switchStatus; }
            set { _switchStatus = value; OnPropertyChanged("SwitchStatus"); }
        }

        private bool _restart;
        public bool Restart
        {
            get { return _restart; }
            set { _restart = value; OnPropertyChanged("Restart"); }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set { _version = value; OnPropertyChanged("Version"); }
        }

        private DateTime _workersDate;
        public DateTime WorkersDate
        {
            get { return _workersDate; }
            set { _workersDate = value; OnPropertyChanged("WorkersDate"); }
        }

        private DateTime _wtmSettingsDate;
        public DateTime WtmSettingsDate
        {
            get { return _wtmSettingsDate; }
            set { _wtmSettingsDate = value; OnPropertyChanged("WtmSettingsDate"); }
        }

        private OperationStatus _restartStatus;
        public OperationStatus RestartStatus
        {
            get { return _restartStatus; }
            set { _restartStatus = value; OnPropertyChanged("RestartStatus"); }
        }

        private OperationStatus _updateStatus;
        public OperationStatus UpdateStatus
        {
            get { return _updateStatus; }
            set { _updateStatus = value; OnPropertyChanged("UpdateStatus"); }
        }

        public TaskCompletionSource<bool> UpdateSuccessfull { get; set; }

        public Computer() { }

        public Computer Clone()
        {
            var _new = new Computer()
            {
                Name = this.Name,
                OnlineStatus = this.OnlineStatus,
                CurrentCoinName = this.CurrentCoinName,
                CurrentCoinSymbol = this.CurrentCoinSymbol,
                NewCoinName = this.NewCoinName,
                NewCoinSymbol = this.NewCoinSymbol,
                Switch = this.Switch,
                SwitchStatus = this.SwitchStatus,
                Restart = this.Restart,
                RestartStatus = this.RestartStatus,
                Version = this.Version,
                WorkersDate = this.WorkersDate,
                WtmSettingsDate = this.WtmSettingsDate,
                UpdateStatus = this.UpdateStatus
            };
            return _new;
        }
    }
}
