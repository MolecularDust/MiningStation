using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Mining_Station
{
    public partial class ViewModel : NotifyObject
    {
        public static ViewModel Instance { get; set; }

        private Workers _workers;
        public Workers Workers
        {
            get { return _workers; }
            set { _workers = value; OnPropertyChanged("Workers"); }
        }

        public ObservableCollection<AlgoCoin> WtmCoins { get; set; }

        public int CoinTableIndex { get; set; }
        public int CoinIndex { get; set; }

        private WtmSettingsObject _wtmSettings;
        public WtmSettingsObject WtmSettings
        {
            get { return _wtmSettings; }
            set { _wtmSettings = value; OnPropertyChanged("WtmSettings"); }
        }

        private ScheduleTime WtmSwitchTimeFromStored { get; set; }
        private ScheduleTime WtmSwitchTimeToStored { get; set; }
        private int WtmHistoricalAveragePeriodStored { get; set; }
        private ScheduleTime WtmHistoryTimeFromStored { get; set; }
        private ScheduleTime WtmHistoryTimeToStored { get; set; }

        private bool _switchTimeIsUpdated;
        public bool SwitchTimeIsUpdated
        {
            get { return _switchTimeIsUpdated; }
            set { _switchTimeIsUpdated = value; OnPropertyChanged("SwitchTimeIsUpdated"); }
        }

        private bool _historyTimeIsUpdated;
        public bool HistoryTimeIsUpdated
        {
            get { return _historyTimeIsUpdated; }
            set { _historyTimeIsUpdated = value; OnPropertyChanged("HistoryTimeIsUpdated"); }
        }

        private bool SwitchTimeUpdateIsInProgress;
        private bool HistoryTimeUpdateIsInProgress;

        public ServiceHost WtmProxyServerServiceHost { get; set; }
        public ServiceHost AccessPoinServiceHost { get; set; }
        public ServiceHost FileServerServiceHost { get; set; }

        public History History { get; set; }
        public bool SaveUndoIsEnabled { get; set; } = true;

        public ProfitTables ProfitTables { get; set; }

        private bool _profitTablesEnabled;
        public bool ProfitTablesEnabled
        {
            get { return _profitTablesEnabled; }
            set { _profitTablesEnabled = value; OnPropertyChanged("ProfitTablesEnabled"); }
        }

        public ObservableCollection<HistoricalChart> HistoricalCharts { get; set; }

        private bool _historicalChartsEnabled;
        public bool HistoricalChartsEnabled
        {
            get { return _historicalChartsEnabled; }
            set { _historicalChartsEnabled = value; OnPropertyChanged("HistoricalChartsEnabled"); }
        }

        private bool _isInitializingWtm;
        public bool IsInitializingWtm
        {
            get { return _isInitializingWtm; }
            set { _isInitializingWtm = value; OnPropertyChanged("IsInitializingWtm"); }
        }

        public bool DefaultWorkers = false;
        public bool DefaultWtmSettings = false;

        public bool SwitchIsInProgress = false;
        public bool SwitchManuallyIsInProgress = false;
        public bool UpdatePriceIsInProgress = false;
        private DateTime _switchSchedule;
        public DateTime SwitchSchedule
        {
            get { return _switchSchedule; }
            set { _switchSchedule = value; OnPropertyChanged("SwitchSchedule"); }
        }
        private DateTime _switchLastTime;
        public DateTime SwitchLastTime
        {
            get { return _switchLastTime; }
            set { _switchLastTime = value; OnPropertyChanged("SwitchLastTime"); }
        }

        private DateTime _historicalPricesSchedule;
        public DateTime HistoricalPricesSchedule
        {
            get { return _historicalPricesSchedule; }
            set { _historicalPricesSchedule = value; OnPropertyChanged("HistoricalPricesSchedule"); }
        }
        private DateTime _historicalPricesLastTime;
        public DateTime HistoricalPricesLastTime
        {
            get { return _historicalPricesLastTime; }
            set { _historicalPricesLastTime = value; OnPropertyChanged("HistoricalPricesLastTime"); }
        }

        private string _statusBarText;
        public string StatusBarText
        {
            get { return _statusBarText; }
            set { _statusBarText = value; OnPropertyChanged("StatusBarText"); }
        }

        private bool _scanLanIsInProgress;
        public bool ScanLanIsInProgress
        {
            get { return _scanLanIsInProgress; }
            set { _scanLanIsInProgress = value; OnPropertyChanged("ScanLanIsInProgress"); }
        }

        public CancellationTokenSource ScanLanCancelSource;

        public SemaphoreSlim SwitchSemaphore = new SemaphoreSlim(1);
        public SemaphoreSlim UpdatePriceSemaphore = new SemaphoreSlim(1);
        public CancellationTokenSource SwitchCancelSource { get; set; }
        public CancellationTokenSource UpdatePriceCancelSource { get; set; }

        private bool RestartComputerPending = false;

        public bool TestBool { get; set; }
        public string TestProperty { get; set; }

        public RelayCommandLight Initialize { get; private set; }
        public RelayCommandLight AddWorker { get; private set; }
        public RelayCommandLight DeleteWorker { get; private set; }
        public RelayCommandLight NewWorker { get; private set; }
        public RelayCommandLight ExportWorkers { get; private set; }
        public RelayCommandLight ImportWorkers { get; private set; }
        public RelayCommandLight AddCoinTable { get; private set; }
        public RelayCommandLight DeleteCoinTable { get; private set; }
        public RelayCommandLight MoveCoinTable { get; private set; }
        public RelayCommandLight CopyCoinTable { get; private set; }
        public RelayCommandLight MoveWorker { get; private set; }
        public RelayCommandLight CopyWorker { get; private set; }
        public RelayCommandLight MoveWorkerUp { get; private set; }
        public RelayCommandLight MoveWorkerDown { get; private set; }
        public RelayCommandLight AddCoin { get; private set; }
        public RelayCommandLight DeleteCoin { get; private set; }
        public RelayCommandLight Undo { get; private set; }
        public RelayCommandLight Redo { get; private set; }
        public RelayCommandLight Save { get; private set; }
        public RelayCommandLight CalculateProfit { get; private set; }
        public RelayCommandLight Export { get; private set; }
        public RelayCommandLight SwitchManually { get; private set; }
        public RelayCommandLight ApplyAutoSwitch { get; private set; }
        public RelayCommandLight ApplyHistoryBackup { get; private set; }
        public RelayCommandLight EditKillList { get; private set; }
        public RelayCommandLight GenerateRandomPort { get; private set; }
        public RelayCommandLight ApplyServerSettings { get; private set; }
        public RelayCommandLight TestConnection { get; private set; }
        public RelayCommandLight LoadHistoricalCharts { get; private set; }
        public RelayCommandLight ScanLan { get; private set; }
        public RelayCommandLight ScanLanStop { get; private set; }
        public RelayCommandLight ClearProfitTables { get; private set; }
        public RelayCommandLight MassUpdateApplication { get; private set; }
        public RelayCommandLight MassUpdateWorkers { get; private set; }
        public RelayCommandLight MassUpdateWtmSettings { get; private set; }
        public RelayCommandLight AddCoinsByAlgorithm { get; private set; }
        public RelayCommandLight AddComputers { get; private set; }
        public RelayCommandLight CancelWaiting { get; private set; }
        public RelayCommandLight EditPath { get; private set; }
        public RelayCommandLight OpenInExplorer { get; private set; }

        public RelayCommandLight WorkersExpandAll { get; private set; }
        public RelayCommandLight WorkersCollapseAll { get; private set; }
        public RelayCommandLight WorkerSelectAll { get; private set; }
        public RelayCommandLight WorkerSelectNone { get; private set; }
        public RelayCommandLight WorkerQueryAll { get; private set; }
        public RelayCommandLight WorkerQueryNone { get; private set; }
        public RelayCommandLight LineSeriesSelectAll { get; private set; }
        public RelayCommandLight LineSeriesSelectNone { get; private set; }

        public RelayCommandLight About { get; private set; }
        public RelayCommandLight Exit { get; private set; }
        public RelayCommandLight Test { get; private set; }

        public string ProfitTablesTempMessage { get; set; } =
            "1. Select workers to calculate profit for.\n" +
            "2. Click 'Calculate Profit' to download data from whattomine.com.";

        // ViewModel constructor
        public ViewModel()
        {
            TestProperty = "TEST";

            IsInitializingWtm = true;

            ProfitTables = new ProfitTables { Tables = new ObservableCollection<ProfitTable>() };
            ProfitTables.Tables.CollectionChanged += ProfitTables_CollectionChanged;

            HistoricalCharts = new ObservableCollection<HistoricalChart>();

            // Read workers from file or create default
            Workers = Workers.ReadWorkers(false);
            if (Workers == null)
            {
                Workers = new Workers(true).Clone();
                DefaultWorkers = true;
            }

            WorkersPropertyEventsAdd();

            // Read Wtm settings from file or create default settings
            WtmSettings = WtmSettingsObject.ReadWtmSettings(false);
            if (WtmSettings == null)
            {
                WtmSettings = new WtmSettingsObject();
                DefaultWtmSettings = true;
            }

            WhatToMine.RequestInterval = WtmSettings.WtmRequestInterval;
            WtmSettings.ServerSettingsAreUpdated = true;
            UpdateWtmHttpClient();

            WtmSwitchTimeFromStored = WtmSettings.SwitchTimeFrom.Clone();
            WtmSwitchTimeToStored = WtmSettings.SwitchTimeTo.Clone();
            WtmHistoricalAveragePeriodStored = WtmSettings.HistoricalAveragePeriod;
            WtmHistoryTimeFromStored = WtmSettings.HistoryTimeFrom.Clone();
            WtmHistoryTimeToStored = WtmSettings.HistoryTimeTo.Clone();

            HookUpWmtSettingsPropertyChangeEvents();

            SwitchTimeIsUpdated = true;
            HistoryTimeIsUpdated = true;

            History = new History();

            ProfitTablesEnabled = true;

            Initialize = new RelayCommandLight(InitializeCommand);
            AddWorker = new RelayCommandLight(AddWorkerCommand);
            DeleteWorker = new RelayCommandLight(DeleteWorkerCommand);
            NewWorker = new RelayCommandLight(NewWorkerCommand);
            ExportWorkers = new RelayCommandLight(ExportWorkersCommand);
            ImportWorkers = new RelayCommandLight(ImportWorkersCommand);
            MoveWorker = new RelayCommandLight(MoveWorkerCommand);
            CopyWorker = new RelayCommandLight(CopyWorkerCommand);
            MoveWorkerDown = new RelayCommandLight(MoveWorkerDownCommand);
            MoveWorkerUp = new RelayCommandLight(MoveWorkerUpCommand);
            AddCoinTable = new RelayCommandLight(AddCoinTableCommand);
            DeleteCoinTable = new RelayCommandLight(DeleteCoinTableCommand);
            MoveCoinTable = new RelayCommandLight(MoveCoinTableCommand);
            CopyCoinTable = new RelayCommandLight(CopyCoinTableCommand);
            AddCoin = new RelayCommandLight(AddCoinCommand);
            DeleteCoin = new RelayCommandLight(DeleteCoinCommand);
            Save = new RelayCommandLight(SaveCommand);
            Undo = new RelayCommandLight(UndoCommand);
            Redo = new RelayCommandLight(RedoCommand);
            CalculateProfit = new RelayCommandLight(CalculateProfitCommand, CalculateProfit_CanExecute);
            Export = new RelayCommandLight(ExportCommand, Export_CanExecute);
            SwitchManually = new RelayCommandLight(SwitchManuallyCommand, SwitchManually_CanExecute);
            ApplyAutoSwitch = new RelayCommandLight(ApplyAutoSwitchCommand);
            ApplyHistoryBackup = new RelayCommandLight(ApplyHistoryBackupCommand);
            EditKillList = new RelayCommandLight(EditKillListCommand);
            GenerateRandomPort = new RelayCommandLight(GenerateRandomPortCommand);
            ApplyServerSettings = new RelayCommandLight(ApplyServerSettingsCommand);
            TestConnection = new RelayCommandLight(TestConnectionCommand, TestConnection_CanExecute);
            LoadHistoricalCharts = new RelayCommandLight(LoadHistoricalChartsCommand, CalculateProfit_CanExecute);
            LineSeriesSelectAll = new RelayCommandLight(LineSeriesSelectAllCommand);
            LineSeriesSelectNone = new RelayCommandLight(LineSeriesSelectNoneCommand);
            ScanLan = new RelayCommandLight(ScanLanCommand);
            ScanLanStop = new RelayCommandLight(ScanLanStopCommand);
            ClearProfitTables = new RelayCommandLight(ClearProfitTablesCommand);
            MassUpdateApplication = new RelayCommandLight(MassUpdateApplicationCommand);
            MassUpdateWorkers = new RelayCommandLight(MassUpdateWorkersCommand);
            MassUpdateWtmSettings = new RelayCommandLight(MassUpdateWtmSettingsCommand);
            AddCoinsByAlgorithm = new RelayCommandLight(AddCoinsByAlgorithmCommand);
            AddComputers = new RelayCommandLight(AddComputersCommand);
            CancelWaiting = new RelayCommandLight(CancelWaitingCommand);
            EditPath = new RelayCommandLight(EditPathCommand);
            OpenInExplorer = new RelayCommandLight(OpenInExplorerCommand);

            WorkersExpandAll = new RelayCommandLight(WorkersExpandAllCommand);
            WorkersCollapseAll = new RelayCommandLight(WorkersCollapseAllCommand);
            WorkerSelectAll = new RelayCommandLight(WorkerSelectAllCommand);
            WorkerSelectNone = new RelayCommandLight(WorkerSelectNoneCommand);
            WorkerQueryAll = new RelayCommandLight(WorkerQueryAllCommand);
            WorkerQueryNone = new RelayCommandLight(WorkerQueryNoneCommand);

            About = new RelayCommandLight(AboutCommand);
            Exit = new RelayCommandLight(ExitCommand);
            Test = new RelayCommandLight(TestCommand);

            Instance = this;

        } // end ViewModel constructor

        public void WorkersPropertyEventsAdd()
        {
            Workers.PropertyChanging += Workers_PropertyChanging;
            Workers.PropertyChanged += Workers_PropertyChanged;
        }

        public void WorkersPropertyEventsRemove()
        {
            Workers.PropertyChanging -= Workers_PropertyChanging;
            Workers.PropertyChanged -= Workers_PropertyChanged;
        }

        private void CancelWaitingCommand(object obj)
        {
            ScanLanCancelSource.Cancel();
        }

        private void EditKillListCommand(object obj)
        {
            var editKillListDialog = new EditKillListDialog();
            editKillListDialog.DataContext = new EditKillListVM(WtmSettings.KillList, Workers.WorkerList);
            var dialogResult = editKillListDialog.ShowDialog();
            if (dialogResult != true)
                return;
            var vm = editKillListDialog.DataContext as EditKillListVM;
            if (vm == null)
                return;
            WtmSettings.KillList.Clear();
            if (vm.KillList == null || vm.KillList.Count == 0)
                return;
            foreach (var entry in vm.KillList)
                WtmSettings.KillList.Add(entry.Process);
        }

        private void HookUpWmtSettingsPropertyChangeEvents()
        {
            WtmSettings.HookUpEvents();
            WtmSettings.PropertyChanging += WtmSettings_PropertyChanging;
            WtmSettings.PropertyChanged += WtmSettings_PropertyChanged;
        }

        private void UnHookUpWmtSettingsPropertyChangeEvents()
        {
            WtmSettings.UnHookEvents();
            WtmSettings.PropertyChanging -= WtmSettings_PropertyChanging;
            WtmSettings.PropertyChanged -= WtmSettings_PropertyChanged;
        }

        private void ApplyAutoSwitchCommand(object obj)
        {
            ApplyWtmSettingsAndSave();
        }

        private void ApplyHistoryBackupCommand(object obj)
        {
            ApplyWtmSettingsAndSave();
        }

        private void ApplyWtmSettingsAndSave()
        {
            if (WtmSettings.AutoSwitch)
            {
                ClearJob(JobType.Switch);

                if (WtmSettings.SwitchPeriod == "Hours")
                {
                    WtmSettings.SwitchTimeFrom.Hour = 0;
                    WtmSettings.SwitchTimeTo.Hour = 0;
                }
                WtmSwitchTimeFromStored = WtmSettings.SwitchTimeFrom.Clone();
                WtmSwitchTimeToStored = WtmSettings.SwitchTimeTo.Clone();
                WtmHistoricalAveragePeriodStored = WtmSettings.HistoricalAveragePeriod;
                ResetScheduledJob(JobType.Switch);
                SwitchTimeIsUpdated = true;
            }
            if (WtmSettings.BackupHistoricalPrices)
            {
                ClearJob(JobType.UpdatePriceHistory);

                WtmHistoryTimeFromStored = WtmSettings.HistoryTimeFrom.Clone();
                WtmHistoryTimeToStored = WtmSettings.HistoryTimeTo.Clone();
                ResetScheduledJob(JobType.UpdatePriceHistory);
                HistoryTimeIsUpdated = true;
            }

            SaveWtmSettings();
        }

        private void WtmSettings_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            Debug.WriteLine("111 WtmSettings_PropertyChanging " + e.PropertyName + " " + WtmSettings.SwitchTimeFrom.Hour + ":" + WtmSettings.SwitchTimeFrom.Minute + " " + WtmSettings.SwitchTimeTo.Hour + ":" + WtmSettings.SwitchTimeTo.Minute);
            Debug.WriteLine("111 SwitchTimeEdit " + WtmSettings.SwitchTimeEdit + " HistoryTimeEdit " + WtmSettings.HistoryTimeEdit);

            if (e.PropertyName.Contains("SwitchTime") || e.PropertyName == "SwitchPeriodCount"
                || e.PropertyName.Contains("HistoryTime") || e.PropertyName == "HistoricalAveragePeriod")
                return;

            Debug.WriteLine("!!! SwitchTimeFrom_PropertyChanging SaveUndoRedo" + " " + WtmSettings.SwitchTimeEdit);
            Debug.WriteLine("SwitchTimeUpdateIsInProgress: " + SwitchTimeUpdateIsInProgress + " SwitchTimeIsUpdated: " + SwitchTimeIsUpdated);

            SaveUndoRedo("WtmSettings");

        }

        private async void WtmSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("222 WtmSettings_PropertyChanged " + e.PropertyName + " " + WtmSettings.SwitchTimeFrom.Hour + ":" + WtmSettings.SwitchTimeFrom.Minute + " " + +WtmSettings.SwitchTimeTo.Hour + ":" + WtmSettings.SwitchTimeTo.Minute);
            Debug.WriteLine("222 SwitchTimeEdit " + WtmSettings.SwitchTimeEdit);
            if (e.PropertyName == "SwitchPeriod")
                SwitchTimeIsUpdated = false;
            if (e.PropertyName == "SwitchTimeEdit")
            {
                if (WtmSettings.SwitchTimeEdit && SwitchTimeIsUpdated)
                    SwitchTimeIsUpdated = false;

                if (WtmSettings.SwitchTimeEdit == false)
                {
                    SwitchTimeUpdateIsInProgress = false;
                    return;
                }

                if (WtmSettings.SwitchTimeEdit && SwitchTimeUpdateIsInProgress)
                    return;

                if (WtmSettings.SwitchTimeEdit && SwitchTimeUpdateIsInProgress == false)
                {
                    Debug.WriteLine("!!! WtmSettings_PropertyChanged SaveUndoRedo" + " " + WtmSettings.SwitchTimeEdit + " Property: " + e.PropertyName);
                    Debug.WriteLine("SwitchTimeUpdateIsInProgress: " + SwitchTimeUpdateIsInProgress + " SwitchTimeIsUpdated: " + SwitchTimeIsUpdated);
                    SaveUndoRedo("WtmSettings");
                    SwitchTimeUpdateIsInProgress = true;
                    return;
                }
            }
            if (e.PropertyName == "HistoryTimeEdit")
            {
                if (WtmSettings.HistoryTimeEdit && HistoryTimeIsUpdated)
                    HistoryTimeIsUpdated = false;

                if (WtmSettings.HistoryTimeEdit == false)
                {
                    HistoryTimeUpdateIsInProgress = false;
                    return;
                }

                if (WtmSettings.HistoryTimeEdit && HistoryTimeUpdateIsInProgress)
                    return;

                if (WtmSettings.HistoryTimeEdit && HistoryTimeUpdateIsInProgress == false)
                {
                    Debug.WriteLine("!!! WtmSettings_PropertyChanged SaveUndoRedo" + " " + WtmSettings.HistoryTimeEdit);
                    Debug.WriteLine("HistoryTimeUpdateIsInProgress: " + HistoryTimeUpdateIsInProgress + " HistoryTimeIsUpdated: " + HistoryTimeIsUpdated);
                    SaveUndoRedo("WtmSettings");
                    HistoryTimeUpdateIsInProgress = true;
                    return;
                }
            }

            if (e.PropertyName == "StartWithWindows")
            {
                if (WtmSettings.StartWithWindows)
                {
                    string path = Helpers.ApplicationPath();
                    SetRegistryKeyValue(Constants.RunRegistryKey, Constants.AppName, path);
                }
                else DeleteRegistryKeyValue(Constants.RunRegistryKey, Constants.AppName);
            }
            if (e.PropertyName == "AutoSwitch")
            {
                if (WtmSettings.AutoSwitch == false)
                {
                    ClearJob(JobType.Switch);
                    SwitchSchedule = new DateTime();
                    UpdateNextJobStatus();
                    WtmSwitchTimeFromStored = WtmSettings.SwitchTimeFrom.Clone();
                    WtmSwitchTimeToStored = WtmSettings.SwitchTimeTo.Clone();
                    SwitchTimeIsUpdated = true;
                }
                else
                {
                    WtmSwitchTimeFromStored = new ScheduleTime(-1, -1);
                    WtmSwitchTimeToStored = new ScheduleTime(-1, -1);
                    SwitchTimeIsUpdated = true;
                }
            }
            if (e.PropertyName == "BackupHistoricalPrices")
            {
                if (WtmSettings.BackupHistoricalPrices == false)
                {
                    ClearJob(JobType.UpdatePriceHistory);
                    HistoricalPricesSchedule = new DateTime();
                    UpdateNextJobStatus();
                    WtmHistoryTimeFromStored = WtmSettings.HistoryTimeFrom.Clone();
                    WtmHistoryTimeFromStored = WtmSettings.HistoryTimeTo.Clone();
                    HistoryTimeIsUpdated = true;
                }
                else
                {
                    WtmHistoryTimeFromStored = new ScheduleTime(-1, -1);
                    WtmHistoryTimeFromStored = new ScheduleTime(-1, -1);
                    HistoryTimeIsUpdated = true;
                }
            }
            if (e.PropertyName == "ProxyPassword")
            {
                WtmSettings.ProxyPasswordEncrypted = WtmSettings.ProxyPassword.Encrypt();
            }
            if (e.PropertyName == "Proxy"
                || e.PropertyName == "UseProxy"
                || e.PropertyName == "ProxyUserName"
                || e.PropertyName == "ProxyPassword"
                || e.PropertyName == "AnonymousProxy")
            {
                UpdateWtmHttpClient();
            }
            if (e.PropertyName == "ApplicationMode")
            {
                TestConnection.RaiseCanExecuteChanged();
                if (WtmSettings.ApplicationMode == "Standalone")
                {
                    WtmSettings.ServerSettingsAreUpdated = true;
                    if (WtmProxyServerServiceHost != null)
                        WtmProxyServerServiceHost.Close();
                }
                ApplyWtmSettingsAndSave();
            }
            if (e.PropertyName == "WtmRequestInterval")
            {
                WhatToMine.RequestInterval = WtmSettings.WtmRequestInterval;
            }
            if (e.PropertyName == "UseYahooRates")
            {
                SaveUndoIsEnabled = false;
                if (WtmSettings.UseYahooRates)
                    await WtmSettings.GetYahooRates(true);
                else WtmSettings.DefaulDisplayCurrencyList();
                SaveUndoIsEnabled = true;
            }
        }

        private void Workers_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            Debug.WriteLine("!!!03 WORKERS_PropertyChanging: " + sender.GetType().ToString() + "Args: " + e.PropertyName);
            Debug.WriteLine("!!!03 Current property name " + " Current worker " + Workers.WorkerIndex);
            switch (e.PropertyName)
            {
                case "PowerCost":
                    SaveUndoRedo("WorkersPowerCost");
                    break;
                case "CoinType":
                    SaveUndoRedo("WorkersCoinType");
                    break;
                case "DisplayCoinAs":
                    SaveUndoRedo("WorkersDisplayCoinAs");
                    break;
                default:
                    SaveUndoRedo(e.PropertyName);
                    break;
            }
        }

        private async void Workers_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Query")
            {
                var somethingToDo = Workers.WorkerList.FirstOrDefault(item => item.Query == true);
                if (somethingToDo != null)
                {
                    CalculateProfit.SetCanExecute(true);
                    LoadHistoricalCharts.SetCanExecute(true);
                    SwitchManually.SetCanExecute(true);
                }
                else
                {
                    CalculateProfit.SetCanExecute(false);
                    LoadHistoricalCharts.SetCanExecute(false);
                    SwitchManually.SetCanExecute(false);
                }
            }

            if (e.PropertyName == "CoinType")
            {
                Helpers.MouseCursorWait();
                IsInitializingWtm = true;
                ScanLanCancelSource = new CancellationTokenSource();
                var token = ScanLanCancelSource.Token;
                await LoadWtmCoins(token).ContinueWith(action => IsInitializingWtm = false);
            }
        }

        private void ProfitTables_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Export.RaiseCanExecuteChanged();
        }

        private void ClearProfitTablesCommand(object obj)
        {
            ProfitTables.Tables.Clear();
        }

        private async void ScanLanCommand(object obj)
        {
            ScanLanIsInProgress = true;
            ScanLanCancelSource = new CancellationTokenSource();
            var token = ScanLanCancelSource.Token;

            var taskList = new List<Task>();

            foreach (var table in ProfitTables.Tables)
            {
                foreach (var pc in table.Computers)
                {
                    pc.OnlineStatus = Computer.OperationStatus.OperationInProgress;
                    Task task = Task.Run(async () =>
                    //Func<Task> function = (async () =>
                    {
                        var serverAddress = "net.tcp://" + pc.Name + ":" + NetHelper.Port + Constants.AccessPoint;
                        var channel = Service.NewChannel(serverAddress, TimeSpan.FromSeconds(15));
                        try
                        {
                            var info = await channel.GetInfoAsync().WithCancellation(token).ConfigureAwait(false);
                            Debug.WriteLine($"{pc.Name} coin: {info?.CurrentCoin?.GetSymbol()}.");
                            if (info != null)
                            {
                                pc.OnlineStatus = Computer.OperationStatus.Success;
                                pc.CurrentCoinName = info?.CurrentCoin?.GetCoinInfo("Name");
                                pc.CurrentCoinSymbol = info?.CurrentCoin?.GetCoinInfo("SYMBOL");
                                pc.ApplicationMode = info.ApplicationMode;
                            }
                            else
                            {
                                pc.OnlineStatus = Computer.OperationStatus.Failure;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"{pc.Name} is not responding. " + ex.Message);
                            pc.OnlineStatus = Computer.OperationStatus.Failure;
                        }
                        finally
                        {
                            NetHelper.CloseChannel(channel);
                        }
                    });

                    // This line blocks UI when a disconnected machine is to be polled over WiFi.
                    // That is why the entire block is wrapped into Task.Run().
                    //Task task = function();

                    taskList.Add(task);
                }
            }


            await Task.WhenAll(taskList);

            ScanLanIsInProgress = false;
        }

        private void ScanLanStopCommand(object obj)
        {
            if (ScanLanCancelSource != null)
                ScanLanCancelSource.Cancel();
        }

        private bool CalculateProfit_CanExecute(object obj)
        {
            Debug.WriteLine("!!!!!!!!!!!!!!!!!!! Can Execute");
            var somethingToDo = Workers.WorkerList.FirstOrDefault(item => item.Query == true);
            return somethingToDo != null ? true : false;
        }

        private async void CalculateProfitCommand(object parameter)
        {
            CalculateProfit.SetCanExecute(false);
            LoadHistoricalCharts.SetCanExecute(false);
            SwitchManually.SetCanExecute(false);

            ProfitTablesEnabled = true;

            var coinList = Workers.GetCoins(Workers.WorkerList, true);
            if (!coinList.Contains("Bitcoin"))
                coinList.Add("Bitcoin");

            //Update Yahoo rates if necessary
            if (WtmSettings.UseYahooRates
                && WtmSettings.DisplayCurrency != "USD"
                && WtmSettings.DisplayCurrency != "BTC"
                && WtmSettingsObject.DisplayCurrencyListDate.Date != DateTime.Today)
            {
                await WtmSettings.GetYahooRates();
                if (WtmSettings.DisplayCurrencyList.Count == 2)
                {
                    BypassUndo(() => WtmSettings.UseYahooRates = false);
                    MessageBox.Show($"{Constants.AppName} could not download the list of currencies from Yahoo. Values will be displayed in USD.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Calculate mean hashrate
            var coinHashList = GetHashrates(coinList);

            // Get WTM coin data
            Dictionary<string, WtmData> wtmDataDict = null;
            (Dictionary<string, WtmData> data, WhatToMine.GetWtmCoinDataResult) wtmDataResult = (null, WhatToMine.GetWtmCoinDataResult.Fail);
            try
            {
                wtmDataResult = await WhatToMine.GetWtmCoinData(coinHashList, true);
            }
            catch (Exception ex)
            {

            }
            wtmDataDict = wtmDataResult.data;
            if (wtmDataDict == null)
            {
                CalculateProfitCommandQuit();
                return;
            }


            var btc = wtmDataDict["Bitcoin"];
            string keyName;
            if (WtmSettings.Average24)
                keyName = "exchange_rate24";
            else
                keyName = "exchange_rate";

            string btcValue;
            btc.Json.TryGetValue(keyName, out btcValue);
            if (btcValue != null)
                WtmSettings.DisplayCurrencyList["BTC"] = Helpers.StringToDecimal(btcValue);
            else
            {
                MessageBox.Show("Failed to read BTC price from whattomine.com", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Form profit tables from wtmDataList
            ProfitTables.Tables.Clear();
            var workersChecked = Workers.WorkerList.Where(w => w.Query).ToList();
            var profitTables = WhatToMine.CreateProfitTables(wtmDataDict, workersChecked, (decimal)Workers.PowerCost, WtmSettings);
            foreach (var table in profitTables)
                ProfitTables.Tables.Add(table);

            CalculateProfitCommandQuit();

            ScanLan.RaiseCanExecuteChanged();
            ScanLanCommand(null);

        }

        private void CalculateProfitCommandQuit()
        {
            CalculateProfit.SetCanExecute(true);
            LoadHistoricalCharts.SetCanExecute(true);
            SwitchManually.SetCanExecute(true);
            UpdateNextJobStatus();
        }

        private HistoricalData TestMethod()
        {
            return new HistoricalData();
        }

        private void AboutCommand(object obj)
        {
            var coin = Shortcut.GetCurrentCoin()?.GetNameAndSymbol();
            var aboutWindow = new About();
            aboutWindow.HeaderText.Text = $"About {Constants.AppName}";
            aboutWindow.VersionText.Text = Helpers.ApplicationVersion();
            aboutWindow.ApplicationModeText.Text = WtmSettings.ApplicationMode;
            aboutWindow.CoinText.Text = string.IsNullOrEmpty(coin) ? "---" : coin;
            aboutWindow.ShowDialog();
        }

        private void ExitCommand(object obj)
        {
            Application.Current.Shutdown();
        }

        // Fot testing only
        private void TestCommand(object parameter)
        {
            //MessageBox.Show("TEST COMMAND", "", MessageBoxButton.OK);

            //var sheduleTimeString = DateTime.UtcNow.AddSeconds(5).ToString("o");
            //ClearJob(JobType.Switch);
            //SetRegistryKeyValue(Constants.SwitchRegistryKey, "Schedule", sheduleTimeString);
            //ResetScheduledJob(JobType.Switch, sheduleTimeString);
        }

    } // End ViewModel class
}
