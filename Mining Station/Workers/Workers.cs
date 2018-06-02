using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Data;

namespace Mining_Station
{
    public class Workers : NotifyObject
    {
        private double _powerCost;
        public double PowerCost
        {
            get { return _powerCost; }
            set { OnPropertyChanging("PowerCost"); _powerCost = value; OnPropertyChanged("PowerCost"); }
        }

        [ScriptIgnore]
        public List<string> CoinTypes { get; set; } = new List<string> { "All", "Active", "GPU", "ASIC" };

        private string _coinType;
        public string CoinType
        {
            get { return _coinType; }
            set { OnPropertyChanging("CoinType"); _coinType = value; OnPropertyChanged("CoinType"); }
        }

        [ScriptIgnore]
        public List<string> DisplayCoinAsOptions { get; set; } = new List<string> { "Name", "SYMBOL", "Name (SYMBOL)" };

        private string _displayCoinAs;
        public string DisplayCoinAs
        {
            get { return _displayCoinAs; }
            set { OnPropertyChanging("DisplayCoinAs"); _displayCoinAs = value; OnPropertyChanged("DisplayCoinAs"); }
        }

        private string _networkScanMethod;
        public string NetworkScanMethod
        {
            get { return _networkScanMethod; }
            set { _networkScanMethod = value; OnPropertyChanged("NetworkScanMethod"); }
        }

        public ObservableCollection<Worker> WorkerList { get; set; }

        [ScriptIgnore]
        public int WorkerIndex { get; set; }
        [ScriptIgnore]
        public int WorkerCount { get; set; }
        [ScriptIgnore]
        public int WorkerNewIndex { get; set; }

        public Workers() { }

        private Workers(NotifyCollectionChangedEventHandler eventHandler)
        {
            this.CoinType = "All";
            this.DisplayCoinAs = "Name";
            this.NetworkScanMethod = "NetApi32";
            this.WorkerList = new ObservableCollection<Worker>();
            this.WorkerList.CollectionChanged += WorkerList_CollectionChanged;
            //CollectionChangedEventManager.AddHandler(this.WorkerList, WorkerList_CollectionChanged);
        }

        public Workers(ObservableCollection<Worker> initWorkerList, double initPowerCost, string coinType, string displayCoinAs, string networkScanMethod)
        {
            this.CoinType = coinType ?? "All";
            this.DisplayCoinAs = displayCoinAs ?? "Name";
            this.NetworkScanMethod = networkScanMethod ?? "NetApi32";
            this.WorkerList = new ObservableCollection<Worker>();
            this.WorkerList.CollectionChanged += WorkerList_CollectionChanged;
            //CollectionChangedEventManager.AddHandler(this.WorkerList, WorkerList_CollectionChanged);

            foreach (var worker in initWorkerList)
                this.WorkerList.Add(worker.Clone());
            this.PowerCost = initPowerCost;
        }

        public Workers(bool defaultWorker)
        {
            this.PowerCost = 0;
            this.CoinType = "Active";
            this.DisplayCoinAs = "Name (SYMBOL)";
            this.NetworkScanMethod = "NetApi32";
            this.WorkerList = new ObservableCollection<Worker> {
                    Worker.DefaultWorker()};
            this.WorkerList.CollectionChanged += WorkerList_CollectionChanged;
            //CollectionChangedEventManager.AddHandler(this.WorkerList, WorkerList_CollectionChanged);
        }

        public static List<string> GetCoins(IList<Worker> workerList, bool checkForQuery = false, bool checkForSwitch = false, bool fullName = false)
        {
            var coins = new HashSet<string>();
            foreach (Worker w in workerList)
            {
                if (checkForQuery && !w.Query)
                    continue;

                foreach (CoinTable ct in w.CoinList)
                {
                    if (checkForSwitch && !ct.Switch)
                        continue;

                    if (fullName)
                    {
                        coins.Add(ct.FullName);
                    }
                    else
                    {
                        foreach (Coin c in ct.Coins)
                            coins.Add(c.Name);
                    }
                }
            }
            return coins.ToList();
        }


        //public static List<AlgoCoin> GetCoins(IList<Worker> workerList, bool checkForQuery = false, bool checkForSwitch = false)
        //{
        //    var coinList = new List<AlgoCoin>();
        //    foreach (Worker w in workerList)
        //    {
        //        if (checkForQuery && !w.Query)
        //            continue;

        //        foreach (CoinTable ct in w.CoinList)
        //        {
        //            if (checkForSwitch && !ct.Switch)
        //                continue;
        //            foreach (Coin c in ct.Coins)
        //                coinList.Add(new AlgoCoin(c.Name, c.Symbol, c.Algorithm));
        //        }
        //    }
        //    return coinList;
        //}

        public static Workers ReadWorkers(bool showError = true)
        {
            string workersContent = null;
            Workers convertedWorkers = null;
            try { workersContent = System.IO.File.ReadAllText(Constants.WorkersFile); }
            catch { return null; }
            convertedWorkers = JsonConverter.ConvertFromJson<Workers>(workersContent, showError);
            if (convertedWorkers != null)
                return new Workers(
                    convertedWorkers.WorkerList,
                    convertedWorkers.PowerCost,
                    convertedWorkers.CoinType,
                    convertedWorkers.DisplayCoinAs,
                    convertedWorkers.NetworkScanMethod);
            else return null;
        }

        public static bool SaveWorkers(Workers workers)
        {
            try
            {
                string json = JsonConverter.ConvertToJson(workers);
                string jsonFormatted = JsonConverter.FormatJson(json);
                Helpers.WriteToTxtFile(Constants.WorkersFile, jsonFormatted);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static DateTime GetWorkersLastUpdateTime()
        {
            if (!System.IO.File.Exists(Constants.WorkersFile))
                return default(DateTime);
            var dateTime = System.IO.File.GetLastWriteTimeUtc(Constants.WorkersFile);
            return dateTime;
        }

        public ObservableCollection<Computer> GetComputers(Computer.OperationStatus status, bool includeThisPc = false)
        {
            var computers = new ObservableCollection<Computer>();
            var computerNames = new HashSet<string>();

            foreach (var worker in this.WorkerList)
            {
                foreach (var pc in worker.Computers)
                {
                    var pcUpper = pc.ToUpper();
                    if (!includeThisPc && string.Equals(pc, Environment.MachineName, StringComparison.CurrentCultureIgnoreCase))
                        continue;
                    bool added = computerNames.Add(pcUpper);
                    if (added)
                        computers.Add(new Computer { Name = pcUpper, OnlineStatus = status });
                }
            }
            return computers;
        }

        public Workers Clone()
        {
            Workers _new = new Workers(WorkerList_CollectionChanged);

            foreach (Worker w in this.WorkerList)
                _new.WorkerList.Add(w.Clone());

            _new.PowerCost = this.PowerCost;
            _new.CoinType = this.CoinType;
            _new.DisplayCoinAs = this.DisplayCoinAs;
            _new.NetworkScanMethod = this.NetworkScanMethod;
            return _new;
        }

        public Workers CloneNoEvents()
        {
            Workers _new = new Workers();
            _new.WorkerList = new ObservableCollection<Worker>();

            foreach (Worker w in this.WorkerList)
                _new.WorkerList.Add(w.CloneNoEvents());

            _new.PowerCost = this.PowerCost;
            _new.CoinType = this.CoinType;
            _new.DisplayCoinAs = this.DisplayCoinAs;
            _new.NetworkScanMethod = this.NetworkScanMethod;
            return _new;
        }

        public void WorkerListReplaceItem(int index, Worker worker)
        {
            this.WorkerIndex = index;
            OnPropertyChanging("WorkerList");
            this.WorkerList[index] = worker;
        }

        public void WorkerListAdd(Worker worker)
        {
            OnPropertyChanging("WorkerAdd");
            this.WorkerList.Add(worker);
        }

        public void WorkerListAddRangeAt(IList<Worker> workers, int index)
        {
            this.WorkerIndex = index + 1;
            this.WorkerCount = workers.Count;
            OnPropertyChanging("WorkerAddRange");
            if (index < this.WorkerList.Count)
            {
                for (int i = 0; i < workers.Count; i++)
                {
                    this.WorkerList.Insert(index + 1 + i, workers[i].Clone());
                }
            }
            else
            {
                foreach (var worker in workers)
                    this.WorkerList.Add(worker.Clone());
            }
        }

        public void WorkerListInsert(int index, Worker worker)
        {
            this.WorkerIndex = index;
            OnPropertyChanging("WorkerInsert");
            this.WorkerList.Insert(index, worker);
        }

        public void WorkerListRemoveAt(int index)
        {
            this.WorkerIndex = index;
            OnPropertyChanging("WorkerRemove");
            this.WorkerList.RemoveAt(index);
        }

        public void WorkerListRemoveRangeAt(int index, int count)
        {
            this.WorkerIndex = index - 1;
            this.WorkerCount = count;
            OnPropertyChanging("WorkerRemoveRange");
            for (int i = 0; i < count; i++)
                this.WorkerList.RemoveAt(index);
        }

        public void WorkerListMove(int workerOldIndex, int workerNewIndex)
        {
            this.WorkerIndex = workerOldIndex;
            this.WorkerNewIndex = workerNewIndex;
            OnPropertyChanging("WorkerMove");
            this.WorkerList.Move(workerOldIndex, WorkerNewIndex);
        }

        private void WorkerList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //Debug.WriteLine("01 WorkerList_CollectionChanged: " + sender.GetType().ToString());
            Helpers.HookUpPropertyChanging<Worker>(e, Worker_PropertyChanging);
            Helpers.HookUpPropertyChanged<Worker>(e, Worker_PropertyChanged);

            var i = 1; var cnt = this.WorkerList.Count;
            foreach (Worker w in this.WorkerList)
            {
                if (i == 1)
                    w.IsFirstEnabled = false;
                else w.IsFirstEnabled = true;
                if (i == cnt)
                    w.IsLastEnabled = false;
                else w.IsLastEnabled = true;
                i++;
            }
            OnPropertyChanged("WorkerList");

        }

        private void Worker_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            Debug.WriteLine("02 Worker_PropertyChanging: " + sender.GetType().ToString() + "Args: " + e.PropertyName);
            this.WorkerIndex = this.WorkerList.IndexOf((Worker)sender);
            OnPropertyChanging("WorkerList");
        }

        private void Worker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsFirstEnabled" || e.PropertyName == "IsLastEnabled")
                return;
            //Debug.WriteLine("02 Worker_PropertyChanged: " + sender.GetType().ToString() + "Args: " + e.PropertyName);
            if (e.PropertyName == "Query")
                OnPropertyChanged("Query");
            else OnPropertyChanged("WorkerList");

        }
    }
}
