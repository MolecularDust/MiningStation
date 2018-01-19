using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class AlgoCoin : NotifyObject
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        private string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; OnPropertyChanged("Symbol"); }
        }

        public string NameAndSymbol
        {
            get { return $"{this.Name} ({this.Symbol})"; }
        }

        private string _algorithm;
        public string Algorithm
        {
            get { return _algorithm; }
            set { _algorithm = value; OnPropertyChanged("Algorithm"); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }

        public AlgoCoin(){ }

        public AlgoCoin(string name, string symbol, string algorithm, string status)
        {
            this.Name = name;
            this.Symbol = symbol;
            this.Algorithm = algorithm;
            this.Status = status;
        }

        public AlgoCoin Clone()
        {
            AlgoCoin _new = new AlgoCoin();
            _new.Name = this.Name;
            _new.Symbol = this.Symbol;
            _new.Algorithm = this.Algorithm;
            _new.Status = this.Status;
            return _new;
        }

        public override bool Equals(object obj)
        {
            var other = obj as AlgoCoin;
            if (obj == null)
                throw new NullReferenceException("Cannot compare AlgoCoin to null");
            return (string.Equals(this.Name, other?.Name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(this.Symbol, other?.Symbol, StringComparison.OrdinalIgnoreCase)
                && string.Equals(this.Algorithm, other?.Algorithm, StringComparison.OrdinalIgnoreCase));
        }

        public override int GetHashCode()
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode($"{this.Name},{this.Symbol},{this.Algorithm}");
        }
    }
}
