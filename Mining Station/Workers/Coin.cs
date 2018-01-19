using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mining_Station
{
    public class Coin : NotifyObject
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set {
                OnPropertyChanging("Name");
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; OnPropertyChanged("Symbol"); }
        }

        private string _algorithm;
        public string Algorithm
        {
            get { return _algorithm; }
            set { _algorithm = value; OnPropertyChanged("Algorithm"); }
        }

        private string _status;
        [ScriptIgnore]
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }

        [ScriptIgnore]
        public string NameAndSymbol
        {
            get { return $"{this.Name} ({this.Symbol})"; }
        }

        private double _hashrate;
        public double Hashrate
        {
            get { return _hashrate; }
            set { OnPropertyChanging("Hashrate"); _hashrate = value; OnPropertyChanged("Hashrate"); }
        }

        public Coin Clone()
        {
            Coin _new = new Coin();
            _new.Name = this.Name;
            _new.Symbol = this.Symbol;
            _new.Algorithm = this.Algorithm;
            _new.Hashrate = this.Hashrate;
            _new.Status = this.Status;
            return _new;
        }
    }
}
