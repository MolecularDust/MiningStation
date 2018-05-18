using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class CoinId : NotifyObject
    {
        public int Id { get; set; }

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

        private string _status;
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; OnPropertyChanged("IsChecked"); }
        }

        private bool _show;
        public bool Show
        {
            get { return _show; }
            set { _show = value; OnPropertyChanged("Show"); }
        }
    }

    public class Algorithm : NotifyObject
    {
        private bool IsCheckedBypass = false;

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                Coin_PropertyChangedBypass = true;
                if (!IsCheckedBypass)
                {
                    foreach (var coin in this.Coins)
                        coin.IsChecked = value;
                }
                Coin_PropertyChangedBypass = false;
                OnPropertyChanged("IsChecked");
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        private bool _show;
        public bool Show
        {
            get { return _show; }
            set { _show = value; OnPropertyChanged("Show"); }
        }

        private double _hashrate;
        public double Hashrate
        {
            get { return _hashrate; }
            set { _hashrate = value; OnPropertyChanged("Hashrate"); }
        }

        private double _power;
        public double Power
        {
            get { return _power; }
            set { _power = value; OnPropertyChanged("Power"); }
        }

        private double _fees;
        public double Fees
        {
            get { return _fees; }
            set { _fees = value; OnPropertyChanged("Fees"); }
        }



        public ObservableCollection<CoinId> Coins { get; set; }

        public void AddCoinsCollectionChanged()
        {
            Coins.CollectionChanged += Coins_CollectionChanged;
        }

        private void Coins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Helpers.HookUpPropertyChanged<CoinId>(e, Coin_PropertyChanged);
        }

        bool Coin_PropertyChangedBypass = false;

        private void Coin_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Coin_PropertyChangedBypass)
                return;
            if (e.PropertyName == "IsChecked")
            {
                IsCheckedBypass = true;
                var firshChecked = this.Coins.FirstOrDefault(x => x.IsChecked);
                if (firshChecked != null)
                {
                    if (this.IsChecked == false)
                        this.IsChecked = true;
                }

                else this.IsChecked = false;
                IsCheckedBypass = false;
            }
        }

        public static async Task<ObservableCollection<Algorithm>> GetWtmData(CancellationToken token = default(CancellationToken))
        {
            var allCoins = await WhatToMine.GetAllCoinsJson(token);
            if (allCoins == null)
                return null;
            var sortedByAlgo = new ObservableCollection<Algorithm>();
            foreach (var coin in allCoins)
            {
                var algoName = (string)((Dictionary<string, object>)coin.Value)["algorithm"];
                var newCoin = new CoinId
                {
                    Id = (int)((Dictionary<string, object>)coin.Value)["id"],
                    Name = coin.Key,
                    Symbol = (string)((Dictionary<string, object>)coin.Value)["tag"],
                    Status = (string)((Dictionary<string, object>)coin.Value)["status"],
                };
                newCoin.Show = string.Equals(newCoin.Status, "Active", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                var algo = sortedByAlgo.FirstOrDefault(x => x.Name == algoName);
                if (algo != null)
                {
                    var algoCoin = algo.Coins.FirstOrDefault(x => x.Name == coin.Key);
                    if (algoCoin == null)
                    {
                        algo.Coins.Add(newCoin);
                    }
                }
                else
                {
                    var newAlgo = new Algorithm
                    {
                        Name = algoName,
                        Coins = new ObservableCollection<CoinId>()
                    };
                    newAlgo.AddCoinsCollectionChanged();
                    newAlgo.Coins.Add(newCoin);
                    sortedByAlgo.Add(newAlgo);
                }
            }
            sortedByAlgo = new ObservableCollection<Algorithm>(sortedByAlgo.OrderBy(x => x.Name));
            return sortedByAlgo;
        }

        public static ObservableCollection<Algorithm> GetWorkersAlgorithms(Worker worker)
        {
            var algos = new ObservableCollection<Algorithm>();
            foreach (var ct in worker.CoinList)
            {
                foreach (var coin in ct.Coins)
                {
                    var algo = algos.FirstOrDefault(x => x.Name == coin.Algorithm);
                    if (algo != null)
                    {
                        if (ct.Power > algo.Power)
                            algo.Power = ct.Power;
                        if (ct.Fees > algo.Fees)
                            algo.Fees = ct.Fees;

                        var coinFound = algo.Coins.FirstOrDefault(x => x.Name == coin.Name);
                        if (coinFound != null)
                        {
                            if (coin.Hashrate > algo.Hashrate)
                                algo.Hashrate = coin.Hashrate;
                        }
                        else
                        {
                            var newCoinId = new CoinId()
                            {
                                Name = coin.Name,
                                Symbol = coin.Symbol
                            };
                            algo.Coins.Add(newCoinId);
                        }
                    }
                    else
                    {
                        var newCoinId = new CoinId()
                        {
                            Name = coin.Name,
                            Symbol = coin.Symbol
                        };
                        var newAlgo = new Algorithm()
                        {
                            Name = coin.Algorithm,
                            Coins = new ObservableCollection<CoinId>(),
                            Hashrate = coin.Hashrate,
                            Power = ct.Power,
                            Fees = ct.Fees
                        };
                        newAlgo.Coins.Add(newCoinId);
                        algos.Add(newAlgo);
                    }
                }
            }
            return algos;
        }
    }


    public class AlgorithmSelectorVM : NotifyObject
    {
        private bool _isInitializing;
        public bool IsInitializing
        {
            get { return _isInitializing; }
            set { _isInitializing = value; OnPropertyChanged("IsInitializing"); }
        }

        public CancellationTokenSource CancelSource { get; set; }

        public RelayCommand Process { get; private set; }
        public RelayCommand CoinsSelectAll { get; private set; }
        public RelayCommand CoinsSelectNone { get; private set; }
        public RelayCommand AlgorithmsSelectAll { get; private set; }
        public RelayCommand AlgorithmsSelectNone { get; private set; }

        public enum WorkerOptions { AddToExisting, AddToNew }

        private WorkerOptions _option;
        public WorkerOptions Option
        {
            get { return _option; }
            set { _option = value; OnPropertyChanged("Option"); }
        }

        private ObservableCollection<Algorithm> _algorithms;
        public ObservableCollection<Algorithm> Algorithms
        {
            get { return _algorithms; }
            set { _algorithms = value; OnPropertyChanged("Algorithms"); }
        }

        public List<string> Workers { get; set; }

        private string _selectedWorker;
        public string SelectedWorker
        {
            get { return _selectedWorker; }
            set { _selectedWorker = value; OnPropertyChanged("SelectedWorker"); }
        }

        private double _hashrate;
        public double Hashrate
        {
            get { return _hashrate; }
            set { _hashrate = value; OnPropertyChanged("Hashrate"); }
        }

        private string _displayCoinAs;
        public string DisplayCoinAs
        {
            get { return _displayCoinAs; }
            set { _displayCoinAs = value; OnPropertyChanged("DisplayCoinAs"); }
        }

        private bool _showActiveCoinsOnly;
        public bool ShowActiveCoinsOnly
        {
            get { return _showActiveCoinsOnly; }
            set { _showActiveCoinsOnly = value; OnPropertyChanged("ShowActiveCoinsOnly"); }
        }

        public AlgorithmSelectorVM()
        {
            Workers = ViewModel.Instance.Workers.WorkerList.Select(x => x.Name).ToList();
            SelectedWorker = Workers.FirstOrDefault();

            Process = new RelayCommand(ProcessCommand, Process_CanExecute);
            CoinsSelectAll = new RelayCommand(CoinsSelectAllCommand);
            CoinsSelectNone = new RelayCommand(CoinsSelectNoneCommand);
            AlgorithmsSelectAll = new RelayCommand(AlgorithmsSelectAllCommand);
            AlgorithmsSelectNone = new RelayCommand(AlgorithmsSelectNoneCommand);

            ShowActiveCoinsOnly = true;
            this.PropertyChanged += AlgorithmSelectorVM_PropertyChanged;
        }

        private void AlgorithmSelectorVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowActiveCoinsOnly")
            {
                ShowActiveCoins(ShowActiveCoinsOnly);
                ShowActiveAlgos(ShowActiveCoinsOnly);
            }
        }

        private void AlgorithmsSelectAllCommand(object obj)
        {
            foreach (var algo in Algorithms)
                algo.IsChecked = true;
        }

        private void AlgorithmsSelectNoneCommand(object obj)
        {
            foreach (var algo in Algorithms)
                algo.IsChecked = false;
        }

        private void CoinsSelectAllCommand(object obj)
        {
            var coins = obj as ObservableCollection<CoinId>;
            if (coins == null) return;
            foreach (var coin in coins)
                coin.IsChecked = true;
        }


        private void CoinsSelectNoneCommand(object obj)
        {
            var coins = obj as ObservableCollection<CoinId>;
            if (coins == null) return;
            foreach (var coin in coins)
                coin.IsChecked = false;
        }

        private bool Process_CanExecute(object obj)
        {
            var firstCheck = Algorithms?.SelectMany(x => x.Coins).FirstOrDefault(x => x.IsChecked);
            return firstCheck != null ? true : false;
        }

        private void ProcessCommand(object obj)
        {

        }

        public void ShowActiveCoins(bool showActiveCoinsOnly)
        {
            foreach (var algo in Algorithms)
            {
                foreach (var coin in algo.Coins)
                {
                    bool coinIsActive = string.Equals(coin.Status, "Active", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                    coin.Show = showActiveCoinsOnly ? coinIsActive : true;
                    if (coin.IsChecked && !coin.Show)
                        coin.IsChecked = false;
                }
            }
        }

        public void ShowActiveAlgos(bool showActiveAlgosOnly)
        {
            if (Algorithms == null)
                return;
            foreach (var algo in Algorithms)
            {
                bool algoIsActive = algo.Coins.FirstOrDefault(x => x.Show == true) != null;
                algo.Show = showActiveAlgosOnly ? algoIsActive : true;
                if (algo.IsChecked && !algo.Show)
                    algo.IsChecked = false;
            }
        }
    }

    public partial class AlgorithmSelector : Window
    {
        public AlgorithmSelector()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        private void FireCancel(object dataContext)
        {
            var vm = dataContext as AlgorithmSelectorVM;
            if (vm == null)
                return;
            vm.CancelSource.Cancel();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            var window = sender as Window;
            var vm = window.DataContext as AlgorithmSelectorVM;
            if (vm == null)
                return;
            vm.IsInitializing = true;
            vm.CancelSource = new CancellationTokenSource();
            var token = vm.CancelSource.Token;
            vm.Algorithms = await Algorithm.GetWtmData(token);
            vm.ShowActiveAlgos(true);
            vm.IsInitializing = false;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var listBox = Helpers.FindAncestor((DependencyObject)sender, typeof(ListBox), 1) as ListBox;
            listBox.SelectedItem = ((CheckBox)e.OriginalSource).DataContext;
        }

        private void AlgorithmsWindow_Closed(object sender, EventArgs e)
        {
            FireCancel(this.DataContext);
        }
    }

    public class OptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return Binding.DoNothing;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return Binding.DoNothing;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return Binding.DoNothing;

            return Enum.Parse(targetType, parameterString);
        }
    }
}
