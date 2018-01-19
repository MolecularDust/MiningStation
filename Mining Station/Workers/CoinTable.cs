using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mining_Station
{
    public class CoinTable : NotifyObject
    {
        public ObservableCollection<Coin> Coins { get; set; }

        public CoinTable() { }
        private CoinTable(NotifyCollectionChangedEventHandler eventHandler)
        {
            this.Coins = new ObservableCollection<Coin>();
            this.Coins.CollectionChanged += Coins_CollectionChanged;
            //CollectionChangedEventManager.AddHandler(this.Coins, Coins_CollectionChanged);
        }
        public CoinTable(ObservableCollection<Coin> initCoins, double initPower, double initFees, bool initSwitch, string initPath, string initArgs, string initNotes)
        {
            //Debug.WriteLine("#### CoinTable constructor");
            this.Coins = new ObservableCollection<Coin>();
            this.Coins.CollectionChanged += Coins_CollectionChanged;
            //CollectionChangedEventManager.AddHandler(this.Coins, Coins_CollectionChanged);

            foreach (Coin c in initCoins)
                this.Coins.Add(c);
            this.Power = initPower;
            this.Fees = initFees;
            this.Switch = initSwitch;
            this.Path = initPath;
            this.Arguments = initArgs;
            this.Notes = initNotes;
        }

        public void CoinInsert(int index, Coin coin)
        {
            OnPropertyChanging("Coins");
            this.Coins.Insert(index, coin);
        }

        public void CoinRemoveAt(int index)
        {
            OnPropertyChanging("Coins");
            this.Coins.RemoveAt(index);
        }

        private void Coins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //Debug.WriteLine("#### Coins_CollectionChanged");
            Helpers.HookUpPropertyChanged<Coin>(e, Coin_PropertyChanged);
            Helpers.HookUpPropertyChanging<Coin>(e, Coin_PropertyChanging);
            OnPropertyChanged("FullName");
            OnPropertyChanged("FullSymbol");
            OnPropertyChanged("FullAlgorithm");
            OnPropertyChanged("FirstHashrate");
            OnPropertyChanged("FullHashrate");
        }

        private void Coin_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            OnPropertyChanging("Coins");
        }

        private void Coin_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("FullName");
            OnPropertyChanged("FullSymbol");
            OnPropertyChanged("FullAlgorithm");
            OnPropertyChanged("FirstHashrate");
            OnPropertyChanged("FullHashrate");
        }

        [ScriptIgnore]
        public string FullName
        {
            get
            {
                string str = string.Empty;
                foreach (Coin c in Coins)
                    str += c.Name + "+";
                return str.TrimEnd('+');
            }
            set { OnPropertyChanged("FullName"); }
        }

        [ScriptIgnore]
        public string FullSymbol
        {
            get
            {
                string str = string.Empty;
                foreach (Coin c in Coins)
                    str += c.Symbol + "+";
                return str.TrimEnd('+');
            }
            set { OnPropertyChanged("FullSymbol"); }
        }

        [ScriptIgnore]
        public string FullAlgorithm
        {
            get
            {
                string str = string.Empty;
                foreach (Coin c in Coins)
                    str += c.Algorithm + "+";
                return str.TrimEnd('+');
            }
            set { OnPropertyChanged("FullAlgorithm"); }
        }

        [ScriptIgnore]
        public double FirstHashrate
        {
            get
            {
                var coin = Coins.FirstOrDefault();
                if (coin != null)
                    return coin.Hashrate;
                return 0;
            }
            set { OnPropertyChanged("FirstHashrate"); }
        }

        [ScriptIgnore]
        public string FullHashrate
        {
            get
            {
                string str = string.Empty;
                foreach (Coin c in Coins)
                    str += c.Hashrate.ToString(CultureInfo.InvariantCulture) + "+";
                return str.TrimEnd('+');
            }
            set { OnPropertyChanged("FullHashrate"); }
        }

        private double _power;
        public double Power
        {
            get { return _power; }
            set { OnPropertyChanging("Power"); _power = value; OnPropertyChanged("Power"); }
        }
        private double _fees;
        public double Fees
        {
            get { return _fees; }
            set { OnPropertyChanging("Fees"); _fees = value; OnPropertyChanged("Fees"); }
        }
        private bool _switch;
        public bool Switch
        {
            get { return _switch; }
            set { OnPropertyChanging("Switch"); _switch = value; OnPropertyChanged("Switch"); }
        }
        private string _path;
        public string Path
        {
            get { return _path; }
            set { OnPropertyChanging("Path"); _path = value; OnPropertyChanged("Path"); }
        }
        private string _arguments;
        public string Arguments
        {
            get { return _arguments; }
            set { OnPropertyChanging("Arguments"); _arguments = value; OnPropertyChanged("Arguments"); }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set { OnPropertyChanging("Notes"); _notes = value; OnPropertyChanged("Notes"); }
        }

        public CoinTable Clone()
        {
            CoinTable _coinTable = new CoinTable(Coins_CollectionChanged);
            foreach (var c in this.Coins)
                _coinTable.Coins.Add(c.Clone());
            _coinTable.Power = this.Power;
            _coinTable.Fees = this.Fees;
            _coinTable.Switch = this.Switch;
            _coinTable.Path = this.Path;
            _coinTable.Arguments = this.Arguments;
            _coinTable.Notes = this.Notes;
            return _coinTable;
        }
        public CoinTable CloneNoEvents()
        {
            CoinTable _coinTable = new CoinTable();
            _coinTable.Coins = new ObservableCollection<Coin>();
            foreach (var c in this.Coins)
                _coinTable.Coins.Add(c.Clone());
            _coinTable.Power = this.Power;
            _coinTable.Fees = this.Fees;
            _coinTable.Switch = this.Switch;
            _coinTable.Path = this.Path;
            _coinTable.Arguments = this.Arguments;
            _coinTable.Notes = this.Notes;
            return _coinTable;
        }
    }
}
