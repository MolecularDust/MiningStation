using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mining_Station
{
    // Workers related methods
    public partial class ViewModel : NotifyObject
    {
        private void WorkerQueryAllCommand(object obj)
        {
            foreach (var w in Workers.WorkerList)
                w.Query = true;
        }

        private void WorkerQueryNoneCommand(object obj)
        {
            foreach (var w in Workers.WorkerList)
                w.Query = false;
        }

        private void CopyWorkerCommand(object obj)
        {
            var itemToCopy = obj as WorkerCopy;
            if (itemToCopy != null)
            {
                var newIndex = Workers.WorkerList.IndexOf(itemToCopy.DestinationWorker);
                if (newIndex != -1)
                    Workers.WorkerListInsert(newIndex, itemToCopy.SourceWorker);
            }
        }

        private void MoveWorkerCommand(object obj)
        {
            var itemToMove = obj as WorkerCopy;
            if (itemToMove != null)
            {
                int oldIndex = 0; int newIndex = 0; int i = 0;
                foreach (var w in Workers.WorkerList)
                {
                    if (w == itemToMove.SourceWorker)
                        oldIndex = i;
                    if (w == itemToMove.DestinationWorker)
                        newIndex = i;
                    i++;
                }
                if (oldIndex != newIndex)
                    Workers.WorkerListMove(oldIndex, newIndex);
            }
        }

        private void CopyCoinTableCommand(object obj)
        {
            var itemToCopy = obj as CoinTableCopy;
            if (itemToCopy == null)
                return;
            var destinationWorkerIndex = Workers.WorkerList.IndexOf(itemToCopy.DestinationWorker);
            Workers.WorkerList[destinationWorkerIndex].CoinTableInsert(itemToCopy.DestinationCoinTableIndex, itemToCopy.SourceCoinTable.Clone());
        }

        private void MoveCoinTableCommand(object obj)
        {
            var itemToCopy = obj as CoinTableCopy;
            if (itemToCopy == null)
                return;
            var destinationWorkerIndex = Workers.WorkerList.IndexOf(itemToCopy.DestinationWorker);
            var sourceWorkerIndex = Workers.WorkerList.IndexOf(itemToCopy.SourceWorker);
            if (destinationWorkerIndex == sourceWorkerIndex) // Move within the same worker
            {
                if (itemToCopy.SourceCoinTableIndex == itemToCopy.DestinationCoinTableIndex)
                    return;
                else Workers.WorkerList[sourceWorkerIndex].CoinTableMove(itemToCopy.SourceCoinTableIndex, itemToCopy.DestinationCoinTableIndex);
            }
            else
            {
                Workers.WorkerList[sourceWorkerIndex].CoinTableRemoveAt(itemToCopy.SourceCoinTableIndex);
                Workers.WorkerList[destinationWorkerIndex].CoinTableInsert(itemToCopy.DestinationCoinTableIndex, itemToCopy.SourceCoinTable.Clone());
            }
        }

        private void WorkersExpandAllCommand(object obj)
        {
            Helpers.MouseCursorWait();
            bool someChanged = false;
            foreach (var w in Workers.WorkerList)
            {
                if (!w.IsExpanded)
                    someChanged = true;
                w.IsExpanded = true;
            }
            if (!someChanged)
                Helpers.MouseCursorNormal();
        }

        private void WorkersCollapseAllCommand(object obj)
        {
            foreach (var w in Workers.WorkerList)
                w.IsExpanded = false;
        }

        private void WorkerSelectNoneCommand(object obj)
        {
            Debug.WriteLine("Select None");
            var w = (Worker)obj;
            if (w != null)
            {
                foreach (var row in w.CoinList)
                    row.Switch = false;
            }
        }

        private void WorkerSelectAllCommand(object obj)
        {
            Debug.WriteLine("Select All");
            var w = (Worker)obj;
            if (w != null)
            {
                foreach (var row in w.CoinList)
                    row.Switch = true;
            }
        }

        private void AddWorkerCommand(object parameter)
        {
            var workerIndex = Workers.WorkerList.IndexOf((Worker)parameter);
            Workers.WorkerIndex = workerIndex;
            if (workerIndex != -1)
            {
                var newWorker = Workers.WorkerList[workerIndex].Clone();
                Workers.WorkerListInsert(workerIndex, newWorker);
            }
        }

        private void DeleteWorkerCommand(object parameter)
        {
            var workerIndex = Workers.WorkerList.IndexOf((Worker)parameter);
            Workers.WorkerIndex = workerIndex;
            if (workerIndex != -1)
            {
                var name = Workers.WorkerList[workerIndex].Name;
                if (name == string.Empty)
                    name = "unnamed worker";
                var response = MessageBox.Show("Delete " + name + "?", "Delete worker", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (response == MessageBoxResult.Yes)
                    Workers.WorkerListRemoveAt(workerIndex);
            }
        }

        private void NewWorkerCommand(object parameter)
        {
            var workerIndex = Workers.WorkerList.IndexOf((Worker)parameter);
            Workers.WorkerIndex = workerIndex;
            if (workerIndex != -1)
            {
                var index = workerIndex + 1;
                if (index < Workers.WorkerList.Count)
                    Workers.WorkerListInsert(index, Worker.DefaultWorker());
                else Workers.WorkerListAdd(Worker.DefaultWorker());
            }
        }

        private async void ExportWorkersCommand(object parameter)
        {
            var window = new ExportImportWorkers();
            var vm = new ExportImportWorkersVM();
            vm.Workers = Workers.WorkerList;

            //Store current Query data and reset Query checkmarks
            bool[] queries = new bool[Workers.WorkerList.Count];
            for (int i = 0; i < Workers.WorkerList.Count; i++)
            {
                queries[i] = Workers.WorkerList[i].Query;
                Workers.WorkerList[i].Query = false;
            }

            Action restoreQueries = () => {
                for (int i = 0; i < queries.Length; i++)
                    Workers.WorkerList[i].Query = queries[i];
            };

            window.DataContext = vm;
            vm.Title = "Export";
            var dialogResult = window.ShowDialog();
            if (dialogResult == false)
            {
                restoreQueries();
                return;
            }

            var workersToSave = Workers.WorkerList.Where(x => x.Query).ToList();
            if (workersToSave == null || workersToSave.Count == 0)
            {
                restoreQueries();
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON file|*.json";
            saveFileDialog.OverwritePrompt = true;
            var saveDialogResult = saveFileDialog.ShowDialog();
            if (saveDialogResult == false)
            {
                restoreQueries();
                return;
            }

            var json = JsonConverter.ConvertToJson(workersToSave);
            if (json == null)
            {
                await Task.Delay(100);
                MessageBox.Show("Failed to convert selected workers to JSON format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                restoreQueries();
                return;
            }
            string jsonFormatted = JsonConverter.FormatJson(json);
            Helpers.WriteToTxtFile(saveFileDialog.FileName, jsonFormatted);
            restoreQueries();
        }

        private async void ImportWorkersCommand(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON file|*.json";
            var openFileDialogResult = openFileDialog.ShowDialog();
            if (openFileDialogResult == false || string.IsNullOrEmpty(openFileDialog.FileName))
                return;

            string workersContent = null;
            ObservableCollection<Worker> convertedWorkers = null;
            try { workersContent = System.IO.File.ReadAllText(openFileDialog.FileName); }
            catch
            {
                await Task.Delay(100);
                MessageBox.Show($"There was an error while reading from \"{openFileDialog.FileName}\"", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            convertedWorkers = JsonConverter.ConvertFromJson<ObservableCollection<Worker>>(workersContent, false);
            if (convertedWorkers == null || convertedWorkers.Count == 0)
            {
                await Task.Delay(100);
                Helpers.ShowErrorMessage($"Failed to intepret JSON information from \"{openFileDialog.FileName}\"", "Error");
                return;
            }

            var window = new ExportImportWorkers();
            var vm = new ExportImportWorkersVM();
            vm.Workers = convertedWorkers;
            foreach (var worker in vm.Workers)
                worker.Query = true;
            window.DataContext = vm;
            vm.Title = "Import";
            var dialogResult = window.ShowDialog();
            if (dialogResult == false)
            {
                return;
            }

            var selectedWorkers = vm.Workers.Where(x => x.Query).ToList();
            if (selectedWorkers == null || selectedWorkers.Count == 0)
                return;

            var workerIndex = Workers.WorkerList.IndexOf((Worker)parameter);
            Workers.WorkerListAddRangeAt(selectedWorkers, workerIndex);
        }

        private void AddCoinTableCommand(object parameter)
        {
            Debug.WriteLine("+ Add coin table clicked");
            var workerIndex = Workers.WorkerList.IndexOf((Worker)((object[])parameter)[0]);
            CoinTableIndex = Workers.WorkerList[workerIndex].CoinList.IndexOf((CoinTable)((object[])parameter)[1]);
            if (workerIndex != -1 && CoinTableIndex != -1)
            {
                var newCoinTable = Workers.WorkerList[workerIndex].CoinList[CoinTableIndex].Clone();
                Workers.WorkerList[workerIndex].CoinTableInsert(CoinTableIndex + 1, newCoinTable);
            }
        }

        private void DeleteCoinTableCommand(object parameter)
        {
            var workerIndex = Workers.WorkerList.IndexOf((Worker)((object[])parameter)[0]);
            CoinTableIndex = Workers.WorkerList[workerIndex].CoinList.IndexOf((CoinTable)((object[])parameter)[1]);
            if (workerIndex != -1 && CoinTableIndex != -1)
            {
                string showName = string.Empty;
                var fullName = Workers.WorkerList[workerIndex].CoinList[CoinTableIndex].FullName;
                var fullSymbol = Workers.WorkerList[workerIndex].CoinList[CoinTableIndex].FullSymbol;
                if (fullName == string.Empty || fullSymbol == string.Empty)
                    showName = "coin record";
                else showName = fullName + " (" + fullSymbol + ")";
                var response = MessageBox.Show("Delete " + showName + "?", "Delete row", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (response == MessageBoxResult.Yes)
                    Workers.WorkerList[workerIndex].CoinTableRemoveAt(CoinTableIndex);
            }
        }

        private void MoveWorkerUpCommand(object parameter)
        {
            var workerIndex = Workers.WorkerList.IndexOf((Worker)parameter);
            if (workerIndex != -1)
            {
                Workers.WorkerListMove(workerIndex, workerIndex - 1);
            }
        }

        private void MoveWorkerDownCommand(object parameter)
        {
            var workerIndex = Workers.WorkerList.IndexOf((Worker)parameter);
            if (workerIndex != -1)
            {
                Workers.WorkerListMove(workerIndex, workerIndex + 1);
            }
        }

        private void AddCoinCommand(object parameter)
        {
            Debug.WriteLine("ADD Coin: " + parameter);
            var workerIndex = Workers.WorkerList.IndexOf((Worker)((object[])parameter)[0]);
            CoinTableIndex = Workers.WorkerList[workerIndex].CoinList.IndexOf((CoinTable)((object[])parameter)[1]);
            CoinIndex = Workers.WorkerList[workerIndex].CoinList[CoinTableIndex].Coins.IndexOf((Coin)((object[])parameter)[2]);
            Debug.WriteLine("ADD Coin to Worker: " + workerIndex + " of " + Workers.WorkerList.Count + " CoinTableIndex: " + CoinTableIndex + " of " + Workers.WorkerList[workerIndex].CoinList.Count + " CoinIndex: " + CoinIndex + " of " + Workers.WorkerList[workerIndex].CoinList[CoinTableIndex].Coins.Count);
            if (workerIndex != -1 && CoinTableIndex != -1 && CoinIndex != -1)
            {
                var newCoin = Workers.WorkerList[workerIndex].CoinList[CoinTableIndex].Coins[CoinIndex].Clone();
                Workers.WorkerList[workerIndex].CoinList[CoinTableIndex].CoinInsert(CoinIndex + 1, newCoin);
            }
        }

        private void DeleteCoinCommand(object parameter)
        {
            var workerIndex = Workers.WorkerList.IndexOf((Worker)((object[])parameter)[0]);
            CoinTableIndex = Workers.WorkerList[workerIndex].CoinList.IndexOf((CoinTable)((object[])parameter)[1]);
            CoinIndex = Workers.WorkerList[workerIndex].CoinList[CoinTableIndex].Coins.IndexOf((Coin)((object[])parameter)[2]);
            if (workerIndex != -1 && CoinTableIndex != -1 && CoinIndex != -1)
            {
                Workers.WorkerList[workerIndex].CoinList[CoinTableIndex].CoinRemoveAt(CoinIndex);
            }
        }

        private void AddComputersCommand(object obj)
        {
            var addComputersWindow = new AddComputers();
            var vm = new AddComputersVM();
            addComputersWindow.DataContext = vm;
            var dialogResult = addComputersWindow.ShowDialog();
            if (dialogResult == true)
            {
                var worker = obj as Worker;
                if (worker == null)
                    return;
                worker.RaiseProperychanging("Computers");
                foreach (dynamic pc in vm.Computers)
                {
                    if (pc.IsSelected)
                        worker.Computers.Add(pc.Name);
                }
                worker.RaiseProperychanged("Computers");
            }
            vm.Dispose();
        }

        private void EditPathCommand(object obj)
        {
            (object CoinTable, string Path) t;
            try
            {
                t = ((object CoinTable, string Path))obj;
            }
            catch
            {
                return;
            }
            var coinTable = t.CoinTable as CoinTable;
            if (coinTable == null)
                return;
            var path = t.Path as string;
            if (path == null)
                return;
            coinTable.Path = path;
        }

        private void OpenInExplorerCommand(object obj)
        {
            var t = obj as CoinTable;
            if (t == null)
                return;
            var path = t.Path;
            if (!File.Exists(path))
                MessageBox.Show($"File not found.\n{path}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                string argument = "/select, \"" + path + "\"";
                Process.Start("explorer.exe", argument);
            }
        }
    }
}
