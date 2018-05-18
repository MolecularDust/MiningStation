using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public partial class ViewModel : NotifyObject
    {
        private void MassUpdateApplicationCommand(object obj)
        {
            var massUpdateWindow = new MassUpdate();
            var vm = new MassUpdateVM(Workers.GetComputers(Computer.OperationStatus.OperationInProgress));
            vm.Header = $"Update {Constants.AppName}";
            vm.WindowTitle = $"Mass Update";
            vm.ColumnHeader = "Version";
            vm.SubHeader = $"{Environment.MachineName}: {Helpers.ApplicationVersion()}";
            massUpdateWindow.DataContext = vm;
            var dialogResult = massUpdateWindow.ShowDialog();
        }

        private void MassUpdateWorkersCommand(object obj)
        {
            var massUpdateWindow = new MassUpdate();
            var vm = new MassUpdateVM(Workers.GetComputers(Computer.OperationStatus.OperationInProgress));
            vm.Header = $"Update Workers";
            vm.WindowTitle = $"Mass Update";
            vm.ColumnHeader = "Workers Date";
            vm.SubHeader = $"{Environment.MachineName}: {Workers.GetWorkersLastUpdateTime()}";
            massUpdateWindow.DataContext = vm;
            var dialogResult = massUpdateWindow.ShowDialog();
        }

        private void MassUpdateWtmSettingsCommand(object obj)
        {
            var massUpdateWindow = new MassUpdate();
            var vm = new MassUpdateVM(Workers.GetComputers(Computer.OperationStatus.OperationInProgress));
            vm.Header = $"Update Settings";
            vm.WindowTitle = $"Mass Update";
            vm.ColumnHeader = "Settings Date";
            vm.SubHeader = $"{Environment.MachineName}: {WtmSettingsObject.GetWtmSettingsLastUpdateTime()}";
            massUpdateWindow.DataContext = vm;
            var dialogResult = massUpdateWindow.ShowDialog();
        }

        private void AddCoinsByAlgorithmCommand(object obj)
        {
            var algoSelector = new AlgorithmSelector();
            var vm = new AlgorithmSelectorVM();
            vm.DisplayCoinAs = Workers.DisplayCoinAs;
            algoSelector.DataContext = vm;
            var dialogResult = algoSelector.ShowDialog();
            if (dialogResult == false)
                return;
            var wtmDict = WtmCoins.ToDictionary(x => x.Name, y => y.Status);
            var coinTables = new ObservableCollection<CoinTable>();
            foreach (var algo in vm.Algorithms)
            {
                foreach (var coin in algo.Coins)
                {
                    string status = null;
                    wtmDict.TryGetValue(coin.Name, out status);
                    if (coin.IsChecked && coin.Show)
                        coinTables.Add(new CoinTable(new ObservableCollection<Coin> {
                            new Coin { Name = coin.Name, Symbol = coin.Symbol, Hashrate = vm.Hashrate, Algorithm = algo.Name, Status = status } }, 0, 0, false, string.Empty, string.Empty, string.Empty));
                }
            }
            if (coinTables.Count == 0)
                return;

            if (vm.Option == AlgorithmSelectorVM.WorkerOptions.AddToExisting)
            {
                var worker = Workers.WorkerList.FirstOrDefault(x => x.Name == vm.SelectedWorker);
                if (worker != null)
                {
                    worker.RaiseProperychanging("CoinList");
                    foreach (var ct in coinTables)
                        worker.CoinList.Add(ct.Clone());
                    worker.RaiseProperychanged("CoinList");
                }
            }
            if (vm.Option == AlgorithmSelectorVM.WorkerOptions.AddToNew)
            {
                var worker = new Worker("NEW WORKER", "", new ObservableCollection<string>(), new ObservableCollection<CoinTable>(coinTables));
                Workers.WorkerListAdd(worker);
            }
        }

        private void UpdateCoinsCommand(object obj)
        {
            var window = new SelectWorkers();
            var vm = new SelectWorkersVM(showNotes: true);
            vm.Title = "Update Coins In Workers";
            vm.ButtonTitle = "Update";
            var workersCopy = new ObservableCollection<Worker>();
            bool[] queries = SaveQueries();
            vm.Workers = Workers.WorkerList;
            window.DataContext = vm;
            var dialogResult = window.ShowDialog();
            RestoreQueries(queries);
        }
    }
}
