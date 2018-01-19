using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Mining_Station
{
    public class ChartCoin : NotifyObject
    {
        public int Index { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name");
            }
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

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; OnPropertyChanged("IsChecked"); }
        }

        private SolidColorBrush _color;
        public SolidColorBrush Color
        {
            get { return _color; }
            set { _color = value; OnPropertyChanged("Color"); }
        }

        private decimal _totalProfit;
        public decimal TotalProfit
        {
            get { return _totalProfit; }
            set { _totalProfit = value; OnPropertyChanged("TotalProfit"); }
        }

        public decimal TodaysProfit { get; set; }
        public Accumulator Accumulator { get; set; }
        public double PowerAccumulator { get; set; }
        public DateTime LockedUntil { get; set; }

        public ChartCoin() { }
    }
}
