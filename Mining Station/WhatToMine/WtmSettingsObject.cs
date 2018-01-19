using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mining_Station
{
    public class WtmSettingsObject : NotifyObject
    {
        private bool _autoSwitch;
        public bool AutoSwitch
        {
            get { return _autoSwitch; }
            set { OnPropertyChanging("AutoSwitch"); _autoSwitch = value; OnPropertyChanged("AutoSwitch"); }
        }

        private string _displayCurrency;
        public string DisplayCurrency
        {
            get { return _displayCurrency; }
            set
            {

                OnPropertyChanging("DisplayCurrency");
                _displayCurrency = value;
                if(value != null)
                {
                    decimal rate;
                    DisplayCurrencyList.TryGetValue(value, out rate);
                    this.CurrencyRate = rate;
                }
                OnPropertyChanged("DisplayCurrency");
            }
        }

        private decimal _currencyRate;
        [ScriptIgnore]
        public decimal CurrencyRate
        {
            get { return _currencyRate; }
            set { _currencyRate = value; OnPropertyChanged("CurrencyRate"); }
        }

        private bool _useYahooRates;
        public bool UseYahooRates
        {
            get { return _useYahooRates; }
            set
            {
                OnPropertyChanging("UseYahooRates");
                _useYahooRates = value;
                OnPropertyChanged("UseYahooRates");
            }
        }


        private bool _average24;
        public bool Average24
        {
            get { return _average24; }
            set { OnPropertyChanging("Average24"); _average24 = value; OnPropertyChanged("Average24"); }
        }

        private bool _startWithWindows;
        public bool StartWithWindows
        {
            get { return _startWithWindows; }
            set { OnPropertyChanging("StartWithWindows"); _startWithWindows = value; OnPropertyChanged("StartWithWindows"); }
        }

        [ScriptIgnore]
        public List<string> SwitchPeriodList { get; set; } = new List<string> { "Hours", "Days" };

        [ScriptIgnore]
        public static Dictionary<string, decimal> DisplayCurrencyDefaultList { get; set; } = new Dictionary<string, decimal> { { "USD", 1 }, { "BTC", 0 } };
        [ScriptIgnore]
        public static Dictionary<string, decimal> DisplayCurrencyYahooList { get; set; }
        [ScriptIgnore]
        public Dictionary<string, decimal> DisplayCurrencyList { get; set; } = DisplayCurrencyDefaultList;
        //public static event EventHandler DisplayCurrencyListChanged;
        [ScriptIgnore]
        public static DateTime DisplayCurrencyListDate { get; set; }

        private string _switchPeriod;
        public string SwitchPeriod
        {
            get { return _switchPeriod; }
            set
            {

                OnPropertyChanging("SwitchPeriod");
                _switchPeriod = value;
                OnPropertyChanged("SwitchPeriod");
            }
        }

        private int _switchPeriodCount;
        public int SwitchPeriodCount
        {
            get { return _switchPeriodCount; }
            set
            {
                OnPropertyChanging("SwitchPeriodCount");
                _switchPeriodCount = value;
                OnPropertyChanged("SwitchPeriodCount");
            }
        }

        private ScheduleTime _switchTimeFrom;
        public ScheduleTime SwitchTimeFrom
        {
            get { return _switchTimeFrom; }
            set
            {
                OnPropertyChanging("SwitchTimeFrom");
                _switchTimeFrom = value;
                OnPropertyChanged("SwitchTimeFrom");
            }
        }

        private ScheduleTime _switchTimeTo;
        public ScheduleTime SwitchTimeTo
        {
            get { return _switchTimeTo; }
            set
            {
                OnPropertyChanging("SwitchTimeTo");
                _switchTimeTo = value;
                OnPropertyChanged("SwitchTimeTo");
            }
        }

        private bool _switchTimeEdit;
        [ScriptIgnore]
        public bool SwitchTimeEdit
        {
            get { return _switchTimeEdit; }
            set { OnPropertyChanging("SwitchTimeEdit"); _switchTimeEdit = value; OnPropertyChanged("SwitchTimeEdit"); }
        }

        private int _delayNextSwitchTime;
        public int DelayNextSwitchTime
        {
            get { return _delayNextSwitchTime; }
            set { OnPropertyChanging("DelayNextSwitchTime"); _delayNextSwitchTime = value; OnPropertyChanged("DelayNextSwitchTime"); }
        }


        private bool _useHistoricalAverage;
        public bool UseHistoricalAverage
        {
            get { return _useHistoricalAverage; }
            set
            {
                OnPropertyChanging("UseHistoricalAverage");
                _useHistoricalAverage = value;
                OnPropertyChanged("UseHistoricalAverage");
                if ((value == true) && (!BackupHistoricalPrices))
                {
                    _backupHistoricalPrices = true;
                    OnPropertyChanged("BackupHistoricalPrices");
                }
            }
        }

        private int _historicalAveragePeriod = 1;
        public int HistoricalAveragePeriod
        {
            get { return _historicalAveragePeriod; }
            set { OnPropertyChanging("HistoricalAveragePeriod"); _historicalAveragePeriod = value; OnPropertyChanged("HistoricalAveragePeriod"); }
        }

        private bool _restartMiner;
        public bool RestartMiner
        {
            get { return _restartMiner; }
            set
            {
                if (!restartMinerBypass)
                    OnPropertyChanging("RestartMiner");
                _restartMiner = value;
                OnPropertyChanged("RestartMiner");
            }
        }

        public ObservableCollection<string> KillList { get; set; } = new ObservableCollection<string>();

        bool restartMinerBypass = false;

        private bool _restartComputer;
        public bool RestartComputer
        {
            get { return _restartComputer; }
            set
            {
                OnPropertyChanging("RestartComputer");
                _restartComputer = value;
                if (value)
                {
                    restartMinerBypass = true;
                    this.RestartMiner = false;
                    restartMinerBypass = false;
                }
                    
                OnPropertyChanged("RestartComputer");
            }
        }

        private decimal _priceMargin;
        public decimal PriceMargin
        {
            get { return _priceMargin; }
            set { OnPropertyChanging("PriceMargin"); _priceMargin = value; OnPropertyChanged("PriceMargin"); }
        }

        private bool _backupHistoricalPrices;
        public bool BackupHistoricalPrices
        {
            get { return _backupHistoricalPrices; }
            set
            {
                if ((value == false) && (UseHistoricalAverage)) return;
                OnPropertyChanging("BackupHistoricalPrices");
                _backupHistoricalPrices = value;
                OnPropertyChanged("BackupHistoricalPrices");
            }
        }

        private ScheduleTime _historyTimeFrom;
        public ScheduleTime HistoryTimeFrom
        {
            get { return _historyTimeFrom; }
            set
            {
                OnPropertyChanging("HistoryTimeFrom");
                _historyTimeFrom = value;
                OnPropertyChanged("HistoryTimeFrom");
            }
        }

        private ScheduleTime _historyTimeTo;
        public ScheduleTime HistoryTimeTo
        {
            get { return _historyTimeTo; }
            set
            {
                OnPropertyChanging("HistoryTimeTo");
                _historyTimeTo = value;
                OnPropertyChanged("HistoryTimeTo");
            }
        }

        private bool _historyTimeEdit;
        [ScriptIgnore]
        public bool HistoryTimeEdit
        {
            get { return _historyTimeEdit; }
            set { OnPropertyChanging("HistoryTimeEdit"); _historyTimeEdit = value; OnPropertyChanged("HistoryTimeEdit"); }
        }

        private bool _saveAllCoins;
        public bool SaveAllCoins
        {
            get { return _saveAllCoins; }
            set { OnPropertyChanging("SaveAllCoins"); _saveAllCoins = value; OnPropertyChanged("SaveAllCoins"); }
        }

        private int _wtmRequestInterval;
        public int WtmRequestInterval
        {
            get { return _wtmRequestInterval; }
            set { OnPropertyChanging("WtmRequestInterval"); _wtmRequestInterval = value; OnPropertyChanged("WtmRequestInterval"); }
        }

        private int _defaultRequestInterval;
        public int DynamicRequestInterval
        {
            get { return _defaultRequestInterval; }
            set { OnPropertyChanging("DynamicRequestInterval"); _defaultRequestInterval = value; OnPropertyChanged("DynamicRequestInterval"); }
        }

        private int _defaultRequestTrigger;
        public int DynamicRequestTrigger
        {
            get { return _defaultRequestTrigger; }
            set { OnPropertyChanging("DynamicRequestTrigger"); _defaultRequestTrigger = value; OnPropertyChanged("DynamicRequestTrigger"); }
        }

        private bool _useProxy;
        public bool UseProxy
        {
            get { return _useProxy; }
            set { OnPropertyChanging("UseProxy"); _useProxy = value; OnPropertyChanged("UseProxy"); }
        }

        private string _proxy;
        public string Proxy
        {
            get { return _proxy; }
            set { OnPropertyChanging("Proxy"); _proxy = value; OnPropertyChanged("Proxy"); }
        }

        private bool _anonymousProxy;
        public bool AnonymousProxy
        {
            get { return _anonymousProxy; }
            set { OnPropertyChanging("AnonymousProxy"); _anonymousProxy = value; OnPropertyChanged("AnonymousProxy"); }
        }

        private string _proxyUserName;
        public string ProxyUserName
        {
            get { return _proxyUserName; }
            set { OnPropertyChanging("ProxyUserName"); _proxyUserName = value; OnPropertyChanged("ProxyUserName"); }
        }

        private SecureString _proxyPassword;
        [ScriptIgnore]
        public SecureString ProxyPassword   
        {
            get { return _proxyPassword; }
            set { OnPropertyChanging("ProxyPassword"); _proxyPassword = value; OnPropertyChanged("ProxyPassword"); }
        }

        private string _proxyPasswordEncrypted;
        public string ProxyPasswordEncrypted
        {
            get { return _proxyPasswordEncrypted; }
            set { _proxyPasswordEncrypted = value; OnPropertyChanged("ProxyPasswordEncrypted"); }
        }

        [ScriptIgnore]
        public List<string> ApplicationModes { get; set; } = new List<string> { "Standalone", "Server", "Client" };

        private string __applicationMode;
        public string ApplicationMode
        {
            get { return __applicationMode; }
            set
            {
                OnPropertyChanging("ApplicationMode");
                __applicationMode = value;
                if (value != "Standalone")
                    this.ServerSettingsAreUpdated = false;
                OnPropertyChanged("ApplicationMode");
            }
        }

        private string _serverName;
        public string ServerName
        {
            get { return _serverName; }
            set
            {
                OnPropertyChanging("ServerName");
                _serverName = value;
                this.ServerSettingsAreUpdated = false;
                OnPropertyChanged("ServerName");
            }
        }

        private int _tcpPort;
        public int TcpPort
        {
            get { return _tcpPort; }
            set
            {
                OnPropertyChanging("TcpPort");
                if (value < 0)
                    value = 0;
                if (value > 65535)
                    value = 65535;
                _tcpPort = value;
                OnPropertyChanged("TcpPort");
            }
        }

        private bool _updateWorkersFromServer;
        public bool UpdateWorkersFromServer
        {
            get { return _updateWorkersFromServer; }
            set { OnPropertyChanging("UpdateWorkersFromServer"); _updateWorkersFromServer = value; OnPropertyChanged("UpdateWorkersFromServer"); }
        }


        private bool _serverSettingsAreUpdated;
        [ScriptIgnore]
        public bool ServerSettingsAreUpdated
        {
            get { return _serverSettingsAreUpdated; }
            set { _serverSettingsAreUpdated = value; OnPropertyChanged("ServerSettingsAreUpdated"); }
        }

        private bool _queryWtmOnLocalServerFail;
        public bool QueryWtmOnLocalServerFail
        {
            get { return _queryWtmOnLocalServerFail; }
            set { OnPropertyChanging("QueryWtmOnLocalServerFail"); _queryWtmOnLocalServerFail = value; OnPropertyChanged("QueryWtmOnLocalServerFail"); }
        }

        private bool _dontSwitchServer;
        public bool DontSwitchServer
        {
            get { return _dontSwitchServer; }
            set { OnPropertyChanging("DontSwitchServer"); _dontSwitchServer = value; OnPropertyChanged("DontSwitchServer"); }
        }

        public WtmSettingsObject()
        {
            this.DisplayCurrency = "USD";
            this.UseYahooRates = true;
            this.SwitchPeriodCount = 1;
            this.SwitchPeriod = "Days";
            this.Average24 = true;
            this.AutoSwitch = false;
            this.ApplicationMode = "Standalone";
            this.ServerSettingsAreUpdated = true;
            this.ProxyPassword = new SecureString();
            this.DynamicRequestTrigger = 50;
            this.DynamicRequestInterval = 750;

            var defaultTime = new ScheduleTime { Hour = 0, Minute = 0 };
            this.SwitchTimeFrom = defaultTime;
            this.SwitchTimeTo = defaultTime;
            this.HistoryTimeFrom = defaultTime;
            this.HistoryTimeTo = defaultTime;

            this.TcpPort = 3535;
        }

        public WtmSettingsObject(bool createEmtpy)
        {

        }

        public void HookUpEvents()
        {
            this.SwitchTimeFrom.PropertyChanging += SwitchTimeFrom_PropertyChanging;
            this.SwitchTimeFrom.PropertyChanged += SwitchTimeFrom_PropertyChanged;
            this.SwitchTimeTo.PropertyChanging += SwitchTimeTo_PropertyChanging;
            this.SwitchTimeTo.PropertyChanged += SwitchTimeTo_PropertyChanged;
            this.HistoryTimeFrom.PropertyChanging += HistoryTimeFrom_PropertyChanging;
            this.HistoryTimeFrom.PropertyChanged += HistoryTimeFrom_PropertyChanged;
            this.HistoryTimeTo.PropertyChanging += HistoryTimeTo_PropertyChanging;
            this.HistoryTimeTo.PropertyChanged += HistoryTimeTo_PropertyChanged;
        }

        public void UnHookEvents()
        {
            this.SwitchTimeFrom.PropertyChanging -= SwitchTimeFrom_PropertyChanging;
            this.SwitchTimeFrom.PropertyChanged -= SwitchTimeFrom_PropertyChanged;
            this.SwitchTimeTo.PropertyChanging -= SwitchTimeTo_PropertyChanging;
            this.SwitchTimeTo.PropertyChanged -= SwitchTimeTo_PropertyChanged;
            this.HistoryTimeFrom.PropertyChanging -= HistoryTimeFrom_PropertyChanging;
            this.HistoryTimeFrom.PropertyChanged -= HistoryTimeFrom_PropertyChanged;
            this.HistoryTimeTo.PropertyChanging -= HistoryTimeTo_PropertyChanging;
            this.HistoryTimeTo.PropertyChanged -= HistoryTimeTo_PropertyChanged;
        }

        private void SwitchTimeFrom_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
        {
            OnPropertyChanging("SwitchTimeFrom." + e.PropertyName);
        }

        private void SwitchTimeFrom_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("SwitchTimeFrom." + e.PropertyName);
        }

        private void SwitchTimeTo_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
        {
            OnPropertyChanging("SwitchTimeTo." + e.PropertyName);
        }

        private void SwitchTimeTo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            OnPropertyChanged("SwitchTimeTo." + e.PropertyName);
        }

        private void HistoryTimeFrom_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
        {
            OnPropertyChanging("HistoryTimeFrom." + e.PropertyName);
        }

        private void HistoryTimeFrom_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("HistoryTimeFrom." + e.PropertyName);
        }

        private void HistoryTimeTo_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
        {
            OnPropertyChanging("HistoryTimeTo." + e.PropertyName);
        }

        private void HistoryTimeTo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("HistoryTimeTo." + e.PropertyName);
        }

        public WtmSettingsObject Clone()
        {
            WtmSettingsObject _new = new WtmSettingsObject(true);
            _new.DisplayCurrencyList = this.DisplayCurrencyList;
            _new.DisplayCurrency = this.DisplayCurrency;
            _new.UseYahooRates = this.UseYahooRates;
            _new.Average24 = this.Average24;
            _new.AutoSwitch = this.AutoSwitch;
            _new.RestartMiner = this.RestartMiner;
            _new.RestartComputer = this.RestartComputer;
            _new.KillList = new ObservableCollection<string>(this.KillList);
            _new.PriceMargin = this.PriceMargin;
            _new.DelayNextSwitchTime = this.DelayNextSwitchTime;
            _new.BackupHistoricalPrices = this.BackupHistoricalPrices;
            _new.SaveAllCoins = this.SaveAllCoins;
            _new.HistoricalAveragePeriod = this.HistoricalAveragePeriod;

            _new.StartWithWindows = this.StartWithWindows;
            _new.SwitchTimeFrom = this.SwitchTimeFrom.Clone();
            _new.SwitchTimeTo = this.SwitchTimeTo.Clone();

            _new.SwitchPeriod = this.SwitchPeriod;
            _new.SwitchPeriodCount = this.SwitchPeriodCount;

            _new.UseHistoricalAverage = this.UseHistoricalAverage;
            _new.HistoryTimeFrom = this.HistoryTimeFrom.Clone();
            _new.HistoryTimeTo = this.HistoryTimeTo.Clone();

            _new.WtmRequestInterval = this.WtmRequestInterval;
            _new.DynamicRequestTrigger = this.DynamicRequestTrigger;
            _new.DynamicRequestInterval = this.DynamicRequestInterval;

            _new.UseProxy = this.UseProxy;
            _new.Proxy = this.Proxy;
            _new.AnonymousProxy = this.AnonymousProxy;
            _new._proxyUserName = this.ProxyUserName;
            _new.ProxyPassword = this.ProxyPassword;
            _new.ProxyPasswordEncrypted = this.ProxyPasswordEncrypted;
            _new.ApplicationMode = this.ApplicationMode;
            _new.ServerName = this.ServerName;
            _new.TcpPort = this.TcpPort;

            _new.UpdateWorkersFromServer = this.UpdateWorkersFromServer;
            _new.QueryWtmOnLocalServerFail = this.QueryWtmOnLocalServerFail;
            _new.DontSwitchServer = this.DontSwitchServer;

            return _new;
        }

        public static WtmSettingsObject ReadWtmSettings(bool showError = true)
        {
            WtmSettingsObject settings = null;
            WtmSettingsObject convertedSettings = null;
            string settingsContent = string.Empty;
            if (File.Exists(Constants.WtmSettingsFile))
            {
                try { settingsContent = File.ReadAllText(Constants.WtmSettingsFile); }
                catch { return null; }
                convertedSettings = JsonConverter.ConvertFromJson<WtmSettingsObject>(settingsContent, showError);
                if (convertedSettings != null)
                {
                    settings = convertedSettings.Clone();
                }
            }
            return settings;
        }

        public static DateTime GetWtmSettingsLastUpdateTime()
        {
            if (!System.IO.File.Exists(Constants.WtmSettingsFile))
                return default(DateTime);
            var dateTime = System.IO.File.GetLastWriteTimeUtc(Constants.WtmSettingsFile);
            return dateTime;
        }

        public void DefaulDisplayCurrencyList(bool raisePropertyChanged = true)
        {
            this.DisplayCurrencyList = new Dictionary<string, decimal>(DisplayCurrencyDefaultList);
            this.DisplayCurrency = "USD";
            if (raisePropertyChanged)
                RaiseProperychanged("DisplayCurrencyList");
        }

        public async Task GetYahooRates(bool waitCursor = false)
        {
            if (waitCursor)
                Helpers.MouseCursorWait();
            var currencies = await Yahoo.GetAllCurrencies();
            if (currencies != null)
            {
                this.DisplayCurrencyList = new Dictionary<string, decimal>(DisplayCurrencyDefaultList);
                this.DisplayCurrencyList.Add("---", 0);
                foreach (var currency in currencies)
                {
                    this.DisplayCurrencyList.Add(currency.Key, currency.Value);
                }
                DisplayCurrencyListDate = DateTime.Now;
                RaiseProperychanged("DisplayCurrencyList");
            }
            else
            {
                DefaulDisplayCurrencyList();
            }
            
            if (waitCursor)
                Helpers.MouseCursorNormal();
        }
    }
}
