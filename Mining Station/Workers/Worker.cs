using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mining_Station
{
    public class Worker : NotifyObject
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { OnPropertyChanging("Name"); _name = value; OnPropertyChanged("Name"); }
        }
        private string _description;
        public string Description
        {
            get { return _description; }
            set { OnPropertyChanging("Description"); _description = value; OnPropertyChanged("Description"); }
        }

        private ObservableCollection<string> _computers;
        public ObservableCollection<string> Computers
        {
            get { return _computers; }
            set { OnPropertyChanging("Computers"); _computers = value; OnPropertyChanged("Computers"); }
        }

        private bool _query;
        [ScriptIgnore]
        public bool Query
        {
            get { return _query; }
            set { _query = value; OnPropertyChanged("Query"); }
        }

        private bool _isFirstEnabled;
        [ScriptIgnore]
        public bool IsFirstEnabled
        {
            get { return _isFirstEnabled; }
            set { _isFirstEnabled = value; OnPropertyChanged("IsFirstEnabled"); }
        }

        private bool _isLastEnabled;
        [ScriptIgnore]
        public bool IsLastEnabled
        {
            get { return _isLastEnabled; }
            set { _isLastEnabled = value; OnPropertyChanged("IsLastEnabled"); }
        }

        private bool _isExpanded;
        [ScriptIgnore]
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; OnPropertyChanged("IsExpanded"); }
        }

        private double _coinColumnWidth;
        [ScriptIgnore]
        public double CoinColumnWidth
        {
            get { return _coinColumnWidth; }
            set { _coinColumnWidth = value; OnPropertyChanged("CoinColumnWidth"); }
        }

        public ObservableCollection<CoinTable> CoinList { get; set; }

        public Worker()
        {
        }

        public Worker(string initName, string initDescription, ObservableCollection<string> initComputers, ObservableCollection<CoinTable> initCoinList)
        {
            this.Name = initName;
            this.Description = initDescription;
            this.Computers = initComputers;
            this.CoinList = new ObservableCollection<CoinTable>();
            this.CoinList.CollectionChanged += CoinList_CollectionChanged;
            //CollectionChangedEventManager.AddHandler(this.CoinList, CoinList_CollectionChanged);
            foreach (CoinTable c in initCoinList)
                this.CoinList.Add(c);
        }

        private Worker(NotifyCollectionChangedEventHandler eventHandler)
        {
            this.CoinList = new ObservableCollection<CoinTable>();
            this.CoinList.CollectionChanged += CoinList_CollectionChanged;
            //CollectionChangedEventManager.AddHandler(this.CoinList, CoinList_CollectionChanged);
        }

        public void CoinTableInsert(int index, CoinTable coinTable)
        {
            OnPropertyChanging("CoinList");
            this.CoinList.Insert(index, coinTable);
        }

        public void CoinTableRemoveAt(int index)
        {
            OnPropertyChanging("CoinList");
            this.CoinList.RemoveAt(index);
        }

        public void CoinTableMove(int oldRowIndex, int newRowIndex)
        {
            OnPropertyChanging("CoinList");
            this.CoinList.Move(oldRowIndex, newRowIndex);

        }

        private void CoinList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //Debug.WriteLine("### CoinList_CollectionChanged ");
            Helpers.HookUpPropertyChanged<CoinTable>(e, CoinTable_PropertyChanged);
            Helpers.HookUpPropertyChanging<CoinTable>(e, CoinTable_PropertyChanging);
            OnPropertyChanged("CoinList");
        }

        private void CoinTable_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            OnPropertyChanging("CoinList");
        }

        private void CoinTable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Debug.WriteLine("#### CoinTable_PropertyChanged" + sender + "Argument: " + e.PropertyName);
            OnPropertyChanged("CoinList");
        }

        public Worker Clone()
        {
            Worker _worker = new Worker(CoinList_CollectionChanged);
            _worker.Name = this.Name;
            _worker.Description = this.Description;
            _worker.IsExpanded = this.IsExpanded;
            _worker.Computers = new ObservableCollection<string>();
            if (this.Computers != null)
                foreach (var computer in this.Computers)
                    _worker.Computers.Add(computer);
            foreach (CoinTable ct in this.CoinList)
                _worker.CoinList.Add(ct.Clone());
            return _worker;
        }
        public Worker CloneNoEvents()
        {
            Worker _worker = new Worker();
            _worker.Name = this.Name;
            _worker.Description = this.Description;
            _worker.IsExpanded = this.IsExpanded;
            _worker.Computers = new ObservableCollection<string>();
            if (this.Computers != null)
                foreach (var computer in this.Computers)
                    _worker.Computers.Add(computer);
            _worker.CoinList = new ObservableCollection<CoinTable>();
            foreach (CoinTable ct in this.CoinList)
                _worker.CoinList.Add(ct.CloneNoEvents());
            return _worker;
        }
    }
}
