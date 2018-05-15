using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mining_Station
{
    /// <summary>
    /// Interaction logic for HashrateMultiplier.xaml
    /// </summary>
    public partial class HashrateMultiplier : Window
    {
        public HashrateMultiplier()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        private void Aplly_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void HashrateMultiplierWindow_ContentRendered(object sender, EventArgs e)
        {
            MakeDataGridStretchable();
            this.MaxHeight = this.ActualHeight;
        }

        private void MakeDataGridStretchable()
        {
            this.SizeToContent = SizeToContent.Manual;
            this.Width = this.ActualWidth;
            foreach (var column in DataGridCoins.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(column.ActualWidth, DataGridLengthUnitType.Star);
            }
        }

        private void ResizeDataGridToContent()
        {
            this.SizeToContent = SizeToContent.Width;
            foreach (var column in DataGridCoins.Columns)
            {
                column.MinWidth = 0;
                column.Width = new DataGridLength(0, DataGridLengthUnitType.Auto);
            }
            DataGridCoins.UpdateLayout();
            DataGridCoins.Measure(DesiredSize);
            this.Width = DataGridCoins.DesiredSize.Width;
        }

        private void DataGridCoins_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            ResizeDataGridToContent();
        }

        private void HashrateMultiplierWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCoins.CommitEdit();
        }

        private void DataGridCoins_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ResizeDataGridToContent();
        }
    }

    public class HashrateMultiplierVM : NotifyObject
    {
        public ObservableCollection<CoinPlus> Coins { get; set; }
        public string DisplayCoinAs { get; set; }
        public static string[] RoundingOptions { get; set; } = new string[] {"No rounding","0","0.1","0.01","0.001","0.0001" };
        public string[] Operations { get; set; } = new string[] { "*", "/" };

        private string _operation;
        public string Operation
        {
            get { return _operation; }
            set
            {
                _operation = value;
                if (this.Coins != null)
                    foreach (var coin in this.Coins)
                        coin.Operation = value;
                OnPropertyChanged("Operation");
            }
        }

        public RelayCommand Apply { get; private set; } 

        public HashrateMultiplierVM() { }

        public HashrateMultiplierVM(IList<Coin> coins, string displayCoinAs)
        {

            Apply = new RelayCommand(ApplyCommand, Apply_CanExecute);
            this.DisplayCoinAs = displayCoinAs;
            this.Coins = new ObservableCollection<CoinPlus>();
            Operation = Operations[0];
            foreach (var coin in coins)
            {
                var coinPlus = new CoinPlus() {
                    Coin = coin,
                    Multiplier = 1,
                    Operation = this.Operation,
                    Result = coin.Hashrate,
                    Rounding = RoundingOptions[0]
            };
                Coins.Add(coinPlus);
            }
        }

        private bool Apply_CanExecute(object obj)
        {
            var multiplierChanged = this.Coins.FirstOrDefault(x => x.Multiplier != 1);
            return multiplierChanged != null ? true : false;
        }

        private void ApplyCommand(object obj)
        {
            
        }

        public static int RoundConverter(string s)
        {
            var index = Array.IndexOf(RoundingOptions, s);
            return index-1;
        }
    }

    public class CoinPlus : NotifyObject
    {
        private Coin _coin;
        public Coin Coin
        {
            get { return _coin; }
            set { OnPropertyChanging("Coin"); _coin = value; OnPropertyChanged("Coin"); }
        }

        private string _operation;
        public string Operation
        {
            get { return _operation; }
            set
            {
                _operation = value;
                MultiplyOrDivide();
                OnPropertyChanged("Operation");
            }
        }

        private double _multiplier;
        public double Multiplier
        {
            get { return _multiplier; }
            set
            {
                _multiplier = value;
                MultiplyOrDivide();
                OnPropertyChanged("Multiplier");
            }
        }

        private double _result;
        public double Result
        {
            get { return _result; }
            set { _result = value; OnPropertyChanged("Result"); }
        }

        private string _rounding;
        public string Rounding
        {
            get { return _rounding; }
            set { _rounding = value; OnPropertyChanged("Rounding"); }
        }

        public void MultiplyOrDivide()
        {
            switch (this.Operation)
            {
                case "*":
                    this.Result = this.Coin.Hashrate * this.Multiplier;
                    break;
                case "/":
                    this.Result = this.Coin.Hashrate / this.Multiplier;
                    break;
            }
        }
    }

    public class ResultConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 0;
            try
            {
                result = (double)values[0];
            }
            catch (Exception)
            {
                return "Error";
            }
            string rounding = values[1] as string;
            var i = HashrateMultiplierVM.RoundConverter(rounding);
            if (i >= 0)
                return Math.Round(result, i).ToString();
            else return result.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
