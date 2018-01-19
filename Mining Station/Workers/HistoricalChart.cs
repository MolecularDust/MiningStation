using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Mining_Station
{
    public class HistoricalChart : NotifyObject
    {
        public string Name { get; set; }

        private ObservableCollection<string> _computers;
        public ObservableCollection<string> Computers
        {
            get { return _computers; }
            set
            {
                _computers = value;
                if (_computers.FirstOrDefault(str => string.Equals(str, Environment.MachineName, StringComparison.CurrentCultureIgnoreCase)) != null)
                    this.ThisPC = true;
                else this.ThisPC = false;
            }
        }

        [ScriptIgnore]
        public bool ThisPC { get; set; }
        public string Description { get; set; }

        public ObservablePairCollection<string, ChartCoin> Coins { get; set; }

        private string _displayCoinAs;
        public string DisplayCoinAs
        {
            get { return _displayCoinAs; }
            set { _displayCoinAs = value; OnPropertyChanged("DisplayCoinAs"); }
        }

        public string AutoSwitchCoinName { get; set; }

        private PlotModel plotModel;
        public PlotModel PlotModel
        {
            get { return plotModel; }
            set { plotModel = value; OnPropertyChanged("PlotModel"); }
        }

        private long _horizontalRangeStart;
        public long HorizontalRangeStart
        {
            get { return _horizontalRangeStart; }
            set { _horizontalRangeStart = value; OnPropertyChanged("HorizontalRangeStart"); }
        }

        private long _horizontalRangeStop;
        public long HorizontalRangeStop
        {
            get { return _horizontalRangeStop; }
            set { _horizontalRangeStop = value; OnPropertyChanged("HorizontalRangeStop"); }
        }


        private long _verticalRangeStart;
        public long VerticalRangeStart
        {
            get { return _verticalRangeStart; }
            set { _verticalRangeStart = value; OnPropertyChanged("VerticalRangeStart"); }
        }

        private long _verticalRangeStop;
        public long VerticalRangeStop
        {
            get { return _verticalRangeStop; }
            set { _verticalRangeStop = value; OnPropertyChanged("VerticalRangeStop"); }
        }

        public void HookUpCoinsCollectionChanged()
        {
            Coins.CollectionChanged += Coins_CollectionChanged;
        }

        private void Coins_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Helpers.HookUpPropertyChanged<KeyValueEditablePair<string,ChartCoin>>(e, Coin_PropertyChanged);
        }

        // Enable/disable plot line on checkbox click
        private void Coin_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                var kv = sender as KeyValueEditablePair<string, ChartCoin>;
                if (kv == null) return;
                var serie = this.PlotModel.Series[kv.Value.Index];
                serie.IsVisible = kv.Value.IsChecked;
                this.PlotModel.InvalidatePlot(true);
            }
        }

        public HistoricalChart() {
            HorizontalRangeStart = 0;
            HorizontalRangeStop = 100;
            VerticalRangeStart = 0;
            VerticalRangeStop = 100;
        }

        public KeyValuePair<string, ChartCoin> GetMostProfitableCoin()
        {
            KeyValuePair<string, ChartCoin> mostProfitableCoin = new KeyValuePair<string, ChartCoin>(null, null);
            decimal maxProfit = 0M;
            foreach (var coin in this.Coins)
            {
                if (coin.Value.TodaysProfit > maxProfit)
                {
                    mostProfitableCoin = coin;
                    maxProfit = mostProfitableCoin.Value.TodaysProfit;
                }
            }
            return mostProfitableCoin;
        }
    }
}
