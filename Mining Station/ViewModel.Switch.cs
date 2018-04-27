using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Mining_Station
{
    public partial class ViewModel : NotifyObject
    {
        private bool SwitchManually_CanExecute(object obj)
        {
            var somethingToDo = Workers.WorkerList.FirstOrDefault(item => item.Query == true);
            return somethingToDo != null ? true : false;
        }

        private async void SwitchManuallyCommand(object obj)
        {
            ProfitTablesEnabled = true;
            if (ProfitTables.Tables == null || ProfitTables.Tables.Count == 0)
            {
                ProfitTables.Tables.Clear();
                for (var i = 0; i < Workers.WorkerList.Count; i++)
                {
                    var worker = Workers.WorkerList[i];

                    if (worker.Query)
                    {
                        var newTable = new ProfitTable
                        {
                            Name = worker.Name,
                            Index = i + 1,
                            ThisPC = Helpers.ListContainsThisPC(worker.Computers),
                            Description = worker.Description,
                            Computers = new ObservableCollection<Computer>(),
                            ProfitList = new List<ProfitTableRow>()
                        };
                        foreach (var pc in worker.Computers)
                            newTable.Computers.Add(new Computer { Name = pc, IsExpanded = true });
                        foreach (var ct in worker.CoinList)
                        {
                            newTable.ProfitList.Add(new ProfitTableRow
                            {
                                Name = ct.FullName,
                                Symbol = ct.FullSymbol,
                                Algorithm = ct.FullAlgorithm,
                                Hashrate = ct.FullHashrate,
                                Path = ct.Path,
                                Arguments = ct.Arguments
                            });
                        }
                        var first = newTable.ProfitList.FirstOrDefault();
                        if (first != null)
                            first.ManualSwitch = true;

                        //Show the topmost coin as the new coin in Computers list
                        newTable.HookPropertyChanched();
                        newTable.Row_PropertyChanged(newTable.ProfitList.FirstOrDefault(), new PropertyChangedEventArgs("ManualSwitch"));

                        ProfitTables.Tables.Add(newTable);
                    }
                }

                ScanLan.RaiseCanExecuteChanged();
                ScanLanCommand(null);
                return;
            }

            Computer thisPc = null;
            var toDoList = new ObservableCollection<ProfitTable>();

            bool noComputerIsOnline = true;

            var negativeProfitCoinLsit = new List<(string worker, string coin, string profit)>();
            var alreadyMiningPcLsit = new List<(string worker, string pc, string coin)>();
            var lessProfitablePcLsit = new List<(string worker, string pc, string currentCoin, string switchCoin)>();

            foreach (var table in ProfitTables.Tables)
            {
                var selectedCoinRow = table.ProfitList.FirstOrDefault(x => x.ManualSwitch);

                var newTable = new ProfitTable
                {
                    Name = table.Name,
                    ProfitList = new List<ProfitTableRow> { selectedCoinRow },
                    Computers = new ObservableCollection<Computer>()
                };

                bool switchChecked = false;

                foreach (var pc in table.Computers)
                {
                    if (pc.OnlineStatus == Computer.OperationStatus.Success)
                        noComputerIsOnline = false;

                    if (pc.Switch) switchChecked = true;

                    if (pc.OnlineStatus == Computer.OperationStatus.Success && (pc.Switch || pc.Restart))
                    {
                        // Detect empty path
                        if (pc.Switch && (selectedCoinRow.Path == null || selectedCoinRow.Path == string.Empty))
                        {
                            MessageBox.Show($"The selected coin {selectedCoinRow.NameAndSymbol} in {table.Name} has the Path field empty. Define a path to an executable and try again.", "Path cannot be empty", MessageBoxButton.OK, MessageBoxImage.Error);
                            OnSwitchManuallyExit();
                            return;
                        }

                        var fullName = $"{pc.CurrentCoinName} ({pc.CurrentCoinSymbol})";
                        // Detect that the SwitchCoin is already mined
                        if (pc.Switch && selectedCoinRow.Name == pc.CurrentCoinName)
                            alreadyMiningPcLsit.Add((table.Name, pc.Name, fullName));

                        // Detect that the SwitchCoin is less profitable that the currently mined one
                        var currentlyMinedCoinRow = table.ProfitList.FirstOrDefault(x => x.Name == pc.CurrentCoinName);
                        if (pc.Switch && currentlyMinedCoinRow != null && currentlyMinedCoinRow.ProfitDay > selectedCoinRow.ProfitDay)
                            lessProfitablePcLsit.Add((table.Name, pc.Name, fullName, selectedCoinRow.NameAndSymbol));

                        var computer = new Computer
                        {
                            Name = pc.Name,
                            CurrentCoinName = pc.CurrentCoinName,
                            CurrentCoinSymbol = pc.CurrentCoinSymbol,
                            Switch = pc.Switch,
                            Restart = pc.Restart,
                            SwitchStatus = Computer.OperationStatus.Indeterminate,
                            RestartStatus = Computer.OperationStatus.Indeterminate,
                        };
                        if (computer.IsThisPc && thisPc == null)
                            thisPc = computer;
                        newTable.Computers.Add(computer);
                    }
                }

                // Detect negative profit
                if (switchChecked && selectedCoinRow.ProfitDay <= 0)
                    negativeProfitCoinLsit.Add((table.Name, selectedCoinRow.NameAndSymbol, selectedCoinRow.ProfitDay.ToString("N2")));

                if (newTable.Computers.Count != 0)
                    toDoList.Add(newTable);
            }

            if (toDoList.Count == 0)
            {
                string message = null;
                if (noComputerIsOnline)
                    message = "There are no computers online.";
                else message = "There are some computers online but none is checked either for Switch or Restart.";

                MessageBox.Show(message, "Nothing to do", MessageBoxButton.OK, MessageBoxImage.Information);
                OnSwitchManuallyExit();
                return;
            }

            // Disable Switch button. Call OnSwitchManuallyExit before every return to re-enable it.
            SwitchManuallyIsInProgress = true;

            // Check if localhost is among the pcList machines and it has a background switch job running
            if (SwitchIsInProgress && thisPc != null)
            {
                var result = MessageBox.Show($"An unfinished switching task is currently running in the background. It may overwrite the results of this operation on {thisPc.Name} when it is done. Stop the background job and switch anyway?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    SwitchCancelSource.Cancel();
                    while (SwitchIsInProgress) { await Task.Delay(100); }
                }
                else
                {
                    OnSwitchManuallyExit();
                    return;
                }
            }

            // Warn if any issues detected
            if (negativeProfitCoinLsit.Count != 0 || alreadyMiningPcLsit.Count != 0 || lessProfitablePcLsit.Count != 0)
            {
                FlowDocument document = new FlowDocument();
                int cnt = negativeProfitCoinLsit.Count;
                if (cnt != 0)
                {
                    var paragraph = new Paragraph(new Run("The following selected coins have zero or negative profit:\r\n"));
                    for (int i = 0; i < cnt; i++)
                    {
                        var entry = negativeProfitCoinLsit[i];
                        Run worker = new Run($"{entry.worker}: ");
                        worker.FontWeight = FontWeights.Bold;
                        paragraph.Inlines.Add(worker);
                        paragraph.Inlines.Add($"The daily profit of {entry.coin} is {entry.profit}.");
                        if (i < cnt - 1)
                            paragraph.Inlines.Add("\r\n");
                    }

                    document.Blocks.Add(paragraph);

                }
                cnt = alreadyMiningPcLsit.Count;
                if (alreadyMiningPcLsit.Count != 0)
                {
                    var paragraph = new Paragraph(new Run("The following computers are already mining the selected coin:\r\n"));
                    document.Blocks.Add(paragraph);
                    for (int i = 0; i < cnt; i++)
                    {
                        var entry = alreadyMiningPcLsit[i];
                        Run worker = new Run($"{entry.worker}: ");
                        worker.FontWeight = FontWeights.Bold;
                        paragraph.Inlines.Add(worker);
                        paragraph.Inlines.Add($"{entry.pc} is already mining {entry.coin}.");
                        if (i < cnt - 1)
                            paragraph.Inlines.Add("\r\n");
                    }

                    document.Blocks.Add(paragraph);
                }
                cnt = lessProfitablePcLsit.Count;
                if (lessProfitablePcLsit.Count != 0)
                {
                    var paragraph = new Paragraph(new Run("The following computers are mining a coin that is more profitable than the one you've selected:\r\n"));
                    document.Blocks.Add(paragraph);
                    for (int i = 0; i < cnt; i++)
                    {
                        var entry = lessProfitablePcLsit[i];
                        Run worker = new Run($"{entry.worker}: ");
                        worker.FontWeight = FontWeights.Bold;
                        paragraph.Inlines.Add(worker);
                        paragraph.Inlines.Add($"{entry.pc} is mining {entry.currentCoin} which is more profitable than {entry.switchCoin}.");
                        if (i < cnt - 1)
                            paragraph.Inlines.Add("\r\n");
                    }

                    document.Blocks.Add(paragraph);
                }

                var reportWindow = new Report();
                reportWindow.RichTextBox.Document = document;
                reportWindow.Title = "Switch Inspector";
                reportWindow.Header.Text = $"{Constants.AppName} has detected some issues:";
                reportWindow.Footer.Text = "Press 'OK' to ignore warnings and proceed or 'Cancel' to abort switching:";

                var reportDialogResult = reportWindow.ShowDialog();
                if (reportDialogResult == false)
                {
                    OnSwitchManuallyExit();
                    return;
                }
            }

            // Show Switch window

            var switchWindow = new SwitchWindow();
            var vm = new SwitchWindowVM(toDoList);
            switchWindow.DataContext = vm;
            var switchDialogResult = switchWindow.ShowDialog();
            if (vm.SwitchIsInProgress && vm.ManualSwitchCancelSource != null)
                vm.ManualSwitchCancelSource.Cancel();
            if (vm.SwitchIsInProgress || (vm.SwitchIsFinished && switchDialogResult == true))
            {
                ScanLanCommand(null);
            }

            OnSwitchManuallyExit();
        }

        private void OnSwitchManuallyExit()
        {
            SwitchManuallyIsInProgress = false;
        }

        public enum SwitchResult
        {
            WorkersReadError,
            NoWtmData,
            CoinNotFound,
            ThisPcIsNotListed,
            NothingToDo,
            NoNeedToSwitch,
            SwitchedSuccessfully,
            LinkCreationFailed,
            DelayIsNotOver,
            Error,
            Terminate
        }

        public async Task<SwitchResult> SwitchStandalone()
        {
            var streamServerAddress = BuildServerAddress(WtmSettings.ServerName, Constants.StreamServer);

            using (var logFile = new StreamWriter(Constants.SwitchLog, true))
            {
                if (new FileInfo(Constants.SwitchLog).Length > 0)
                    logFile.WriteLine(string.Empty);
                logFile.WriteLine(DateTime.Now);

                // Respect the DelayNextSwitchTime setting
                if (WtmSettings.DelayNextSwitchTime != 0)
                {
                    var lastTime = new DateTime();
                    var lastTimeObj = GetRegistryKeyValue(Constants.SwitchRegistryKey, "LastSuccess");
                    string lastTimeStr = lastTimeObj as string;
                    if (lastTimeStr != null && lastTimeStr != string.Empty)
                    {
                        lastTime = Convert.ToDateTime(lastTimeStr, DateTimeFormatInfo.InvariantInfo).ToUniversalTime();
                        var now = DateTime.UtcNow;
                        var lastTimePlusDelay = lastTime.AddHours(WtmSettings.DelayNextSwitchTime);
                        if (now < lastTimePlusDelay)
                        {
                            logFile.WriteLine($"Because of the Delay option checked Auto Switch can proceed only after {lastTimePlusDelay.ToLocalTime()}.");
                            return SwitchResult.DelayIsNotOver;
                        }
                    }
                }

                // Update Workers from server
                if (WtmSettings.ApplicationMode == "Client" && WtmSettings.UpdateWorkersFromServer)
                {
                    await UpdateWorkersFromServer(logFile);
                }

                //Find localhost name in Workers list (used in Standalone and Client modes )
                var thisPcName = Environment.MachineName;
                Worker thisWorker = GetWorkerByPCName(thisPcName);
                if (thisWorker == null && WtmSettings.ApplicationMode != "Server")
                {
                    string msg = $"{thisPcName} was not found in any worker.";
                    logFile.WriteLine(msg);
                    return SwitchResult.ThisPcIsNotListed;
                }

                // Get coin names from worker description
                var coinList = new List<string>();
                var allCoinList = new List<string>();
                var workersList = new List<Worker>();
                string noCoinChecked = string.Empty;
                if (thisWorker != null)
                    noCoinChecked = $"No coin is checked as switchable in {thisWorker.Name}.";
                else noCoinChecked = "No coin is checked as switchable.";
                if (WtmSettings.ApplicationMode != "Server") // Standalone or Client mode - just the worker that contains localhost name
                {
                    workersList = new List<Worker> { thisWorker };
                }
                else // Server mode - all workers, all coins
                {
                    foreach (var w in Workers.WorkerList)
                        workersList.Add(w.Clone());
                    allCoinList = Workers.GetCoins(workersList, false, false);
                    if (allCoinList == null || allCoinList.Count == 0)
                    {
                        logFile.WriteLine(noCoinChecked);
                        return SwitchResult.NothingToDo;
                    }
                    if (!allCoinList.Contains("Bitcoin"))
                        allCoinList.Add("Bitcoin");
                }
                coinList = Workers.GetCoins(workersList, false, true); // Only coins with Switch checked
                if (coinList == null || coinList.Count == 0)
                {
                    logFile.WriteLine(noCoinChecked);
                    return SwitchResult.NothingToDo;
                }
                if (!coinList.Contains("Bitcoin"))
                    coinList.Add("Bitcoin");


                // Get WTM coins JSON data
                Dictionary<string, WtmData> wtmDataDict = new Dictionary<string, WtmData>();
                // Attempt to download data from local server
                HistoricalData localDataCopy = null;
                if (WtmSettings.ApplicationMode == "Client")
                {
                    var channel = Service.NewStreamChannel(streamServerAddress, TimeSpan.FromSeconds(60));
                    try
                    {
                        var response = await channel.GetWtmLocalDataAsync();
                        if (response != null)
                        {
                            var memoryStream = new MemoryStream();
                            await response.Stream.CopyToAsync(memoryStream);
                            localDataCopy = NetHelper.DeserializeFromStream<HistoricalData>(memoryStream);
                        }

                        if (localDataCopy == null)
                            throw new NullReferenceException("Local data from server are null.");
                    }
                    catch (Exception ex)
                    {
                        logFile.WriteLine($"Failed to download proxy data from local server {WtmSettings.ServerName}. " + ex.Message);
                        if (!WtmSettings.QueryWtmOnLocalServerFail)
                            return SwitchResult.NoWtmData;
                    }
                    finally
                    {
                        NetHelper.CloseChannel(channel);
                    }

                    // Check if the received data are up to date
                    if (localDataCopy != null)
                    {
                        bool dataTimestampGood = EvaluateWtmDataTimeRange(localDataCopy.Date);
                        if (!dataTimestampGood && !WtmSettings.QueryWtmOnLocalServerFail)
                        {
                            logFile.WriteLine("The server cache data is expired."
                                + " Make sure that AutoSwitch on the client launches later than on the server."
                                + " For example, if AutoSwitch runs on a daily basis schedule AutoSwitch on the server to run at 9:00 and on the client machines at anyting between 10:00-23:59."
                                + " This way the cache data obtained in the morning is valid for the rest of the day.");
                            return SwitchResult.Terminate;
                        }
                        if (dataTimestampGood)
                        {
                            // Filter out coins downloded from server that are not checked for AutoSwitch locally
                            wtmDataDict = localDataCopy.PriceData.Where(x => coinList.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                        }
                    }
                }
                // Download data from WhatToMine (Standalone or Server)
                if (wtmDataDict.Count == 0)
                {
                    StatusBarText = "Downloading coin definitions from whattomine.com...";
                    (Dictionary<string, WtmData> data, WhatToMine.GetWtmCoinDataResult result) wtmDataResult = (null, WhatToMine.GetWtmCoinDataResult.OK);
                    if (WtmSettings.ApplicationMode == "Server")
                    {
                        var allCoinHashList = GetHashrates(allCoinList);
                        wtmDataResult = await WhatToMine.GetWtmCoinData(allCoinHashList, false, logFile).ConfigureAwait(false);
                    }
                    else
                    {
                        var coinHashList = GetHashrates(coinList);
                        wtmDataResult = await WhatToMine.GetWtmCoinData(coinHashList, false, logFile).ConfigureAwait(false);
                    }
                    if (wtmDataResult.result == WhatToMine.GetWtmCoinDataResult.CoinNotFound)
                        return SwitchResult.CoinNotFound;
                    wtmDataDict = wtmDataResult.data;
                    if (wtmDataDict == null)
                        return SwitchResult.CoinNotFound;
                }

                // Update WTM coin data with historical averages from local database if opted to
                var historicalDataList = new List<HistoricalData>();
                bool historicalDataUpToDate = false;
                if (WtmSettings.UseHistoricalAverage)
                {
                    if (WtmSettings.ApplicationMode == "Client")
                    {
                        //Needs longer timeout for the server might be downloading all coins from whattomine.com
                        var channel = Service.NewStreamChannel(streamServerAddress, TimeSpan.FromSeconds(180));

                        try
                        {
                            var response = await channel.GetPriceHistoryAsync(new StreamDownloadRequest { Period = WtmSettings.HistoricalAveragePeriod }).ConfigureAwait(false);
                            if (response != null)
                            {
                                var memoryStream = new MemoryStream();
                                await response.Stream.CopyToAsync(memoryStream);
                                historicalDataList = NetHelper.DeserializeFromStream<List<HistoricalData>>(memoryStream);
                                historicalDataUpToDate = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            logFile.WriteLine($"Failed to obtain historical prices from local server {WtmSettings.ServerName}. " + ex.Message);
                            return SwitchResult.Error;
                        }
                        finally
                        {
                            NetHelper.CloseChannel(channel);
                        }
                    }
                    else
                    {
                        var result = await UpdatePriceHistory();
                        if (result == UpdatePriceHistoryResult.Success || result == UpdatePriceHistoryResult.AlreadyUpToDate)
                        {
                            historicalDataList = ReadHistoricalData(WtmSettings.HistoricalAveragePeriod);
                            historicalDataUpToDate = true;
                        }
                    }
                    // Calculate historical prices for wtmDataList
                    if (historicalDataUpToDate)
                    {
                        GetHistoricalAverages(wtmDataDict, historicalDataList);
                    }
                }

                // Update WtmLocalData if in server mode
                if (WtmSettings.ApplicationMode == "Server")
                {
                    if (wtmDataDict != null && wtmDataDict.Count != 0)
                    {
                        using (var db = new LiteDatabase(Constants.DataBase))
                        {
                            var collection = db.GetCollection<HistoricalData>(Constants.LightDB_WtmCacheCollection);
                            collection.Delete(LiteDB.Query.All());
                            collection.Insert(new HistoricalData(DateTime.Now, wtmDataDict));
                        }
                    }
                }

                // Abandon if in server mode and DontSwitchServer is checked 
                if (WtmSettings.ApplicationMode == "Server" && WtmSettings.DontSwitchServer)
                    return SwitchResult.NoNeedToSwitch;
                if (WtmSettings.ApplicationMode == "Server" && !WtmSettings.DontSwitchServer && thisWorker == null)
                {
                    logFile.WriteLine("The server cannot switch because it is not listed in any worker table.");
                    return SwitchResult.ThisPcIsNotListed;
                }

                // Calculate profit table for this PC and analize it
                var profitTables = WhatToMine.CreateProfitTables(wtmDataDict, workersList, (decimal)Workers.PowerCost, WtmSettings, true);
                var profitTable = profitTables.First();
                var currentCoinShortcut = Shortcut.GetCurrentCoin(); // Get current coin from Startup folder .lnk
                ProfitTableRow maxCoinRow = null; decimal maxProfit = 0; decimal currentProfit = 0;
                bool nothingChecked = false;
                WhatToMine.GetProfit(profitTable, currentCoinShortcut, out maxCoinRow, out maxProfit, out currentProfit, out nothingChecked);
                if (nothingChecked)
                {
                    string msg = "Nothing to do. Make sure there are actual switchable coins defined in worker.";
                    logFile.WriteLine(msg);
                    return SwitchResult.NothingToDo;
                }
                if (maxProfit == 0)
                {
                    string msg = $"There is no coin with profit above 0 for {thisPcName}.";
                    logFile.WriteLine(msg);
                    return SwitchResult.NothingToDo;
                }

                //Check profitability and price margin
                bool currentCoinMatch = currentCoinShortcut?.GetName() == maxCoinRow.Name;
                bool currentCoinPathMatch = currentCoinShortcut?.Path == maxCoinRow.Path;
                bool currentPforitIsHigherOrEqual = maxProfit <= currentProfit;
                bool currentProfitIsWithinMargin = false;
                if (WtmSettings.PriceMargin > 0)
                    currentProfitIsWithinMargin = maxProfit <= (currentProfit + (currentProfit * WtmSettings.PriceMargin / 100));

                if (currentPforitIsHigherOrEqual)
                {
                    string msg;
                    msg = $"No need to switch. {thisPcName} is already set to mine the most profitable coin {currentCoinShortcut?.GetNameAndSymbol()}.";
                    logFile.WriteLine(msg);
                    return SwitchResult.NoNeedToSwitch;
                }
                if (currentProfitIsWithinMargin)
                {
                    string msg;
                    if (currentCoinMatch && !currentCoinPathMatch)
                        msg = $"No need to switch. {thisPcName} is set to mine {currentCoinShortcut?.GetNameAndSymbol()} started by \"{currentCoinShortcut?.Path}\". It is less profitable than {maxCoinRow.NameAndSymbol} started by \"{maxCoinRow.Path}\" but the difference is within price margin.";
                    else msg = $"No need to switch. {thisPcName} is set to mine {currentCoinShortcut?.GetNameAndSymbol()}. It is less profitable than {maxCoinRow.NameAndSymbol} but the difference is within price margin.";
                    logFile.WriteLine(msg);
                    return SwitchResult.NoNeedToSwitch;
                }

                // Check if executable path exists
                if (!File.Exists(maxCoinRow.Path))
                {
                    logFile.WriteLine($"{maxCoinRow.Path} - file does not exist on {thisPcName}.");
                    return SwitchResult.CoinNotFound;
                }

                bool startOk = false; // Start miner process flag
                try
                {
                    Shortcut.CreateCoinShortcut(maxCoinRow.Name, maxCoinRow.Symbol, maxCoinRow.Algorithm, maxCoinRow.Path, maxCoinRow.Arguments);
                    if (WtmSettings.RestartComputer)
                        RestartComputerPending = true;
                    if (WtmSettings.RestartMiner)
                    {
                        // Kill processes from KillList
                        var killResponse = KillProcesses(WtmSettings.KillList);
                        if (!killResponse.success && killResponse.failList != null)
                        {
                            string errorMessage = $"Startup shortcut to mine {maxCoinRow.NameAndSymbol} has been created but some processes could not be killed:";
                            logFile.WriteLine(errorMessage);
                            foreach (var entry in killResponse.failList)
                            {
                                if (entry != string.Empty)
                                    logFile.WriteLine(entry);
                            }
                            logFile.WriteLine($"{Environment.MachineName} will restart.");
                            RestartComputerPending = true;
                            SetRegistryKeyValue(Constants.SwitchRegistryKey, "LastSuccess", DateTime.UtcNow.ToString("o"));
                            return SwitchResult.SwitchedSuccessfully;
                        }

                        if (!System.IO.File.Exists(maxCoinRow.Path))
                        {
                            logFile.WriteLine($"ERROR: File not found. {maxCoinRow.Path}");
                            return SwitchResult.Error;
                        }

                        // Start miner process
                        var startInfo = new ProcessStartInfo(maxCoinRow.Path, maxCoinRow.Arguments);
                        Process process = new Process();
                        process.StartInfo = startInfo;
                        startOk = process.Start();
                        if (!startOk)
                            throw new Exception($"\"{Path.GetFileName(maxCoinRow.Path)}\" failed to start.");
                    }
                    SetRegistryKeyValue(Constants.SwitchRegistryKey, "LastSuccess", DateTime.UtcNow.ToString("o"));
                    logFile.WriteLine($"Switched to {maxCoinRow.NameAndSymbol} successfully.");
                    return SwitchResult.SwitchedSuccessfully;
                }
                catch (Exception ex)
                {
                    logFile.WriteLine($"Failed to switch to {maxCoinRow.NameAndSymbol}: {ex.Message}");
                    if (WtmSettings.RestartMiner && !startOk)
                    {
                        RestartComputerPending = true;
                        logFile.WriteLine($"{Environment.MachineName} will restart.");
                    }
                    return SwitchResult.Error;
                }
            }
        }

        public async Task SwitchTaskWrapper()
        {
            SwitchCancelSource = new CancellationTokenSource();
            var token = SwitchCancelSource.Token;
            await SwitchSemaphore.WaitAsync();
            SwitchIsInProgress = true;
            await Repeater(JobType.Switch, token).ContinueWith(emptyAction => {
                SwitchIsInProgress = false;
                SetRegistryKeyValue(Constants.SwitchRegistryKey, "Schedule", "");
            });
            SwitchSemaphore.Release();
        }

        // Calculate historical averages in wtmDataDict from historical data of historicalData
        public void GetHistoricalAverages(Dictionary<string, WtmData> wtmDataDict, List<HistoricalData> historicalData)
        {
            if (wtmDataDict == null || wtmDataDict.Count == 0 || historicalData == null || historicalData.Count == 0)
                return;

            var historyCount = historicalData.Count;
            foreach (var coin in wtmDataDict)
            {
                decimal priceAccum = 0; bool coinAverageSuccess = true; var date = DateTime.Now.Date;
                foreach (var day in historicalData)
                {
                    Debug.WriteLine("Loop round " + historicalData.IndexOf(day));
                    if (day.Date.Date != date)
                    {
                        coinAverageSuccess = false;
                        break;
                    }
                    WtmData historicalRecord = null;
                    day.PriceData.TryGetValue(coin.Key, out historicalRecord);
                    if (historicalRecord != null)
                    {
                        var tmpPrice = Helpers.StringToDecimal(historicalRecord.Json["exchange_rate24"]);
                        priceAccum += tmpPrice;
                    }
                    else
                    {
                        coinAverageSuccess = false;
                        break;
                    }
                    date = date.Date.AddDays(-1);
                }
                if (coinAverageSuccess)
                {
                    coin.Value.HasAverage = true;
                    coin.Value.AverageDays = historyCount;
                    coin.Value.AveragePrice = priceAccum / historyCount;
                }
            }
        }

        private async Task UpdateWorkersFromServer(StreamWriter logFile)
        {
            bool addressOk = ValidateServerAddress(logFile);
            if (!addressOk)
                return;

            var serverAddress = BuildServerAddress(WtmSettings.ServerName, Constants.AccessPoint);
            var channel = Service.NewChannel(serverAddress);

            DateTime remoteWorkersDate = default(DateTime);
            string errorMessage = "Failed to update Workers from local server.";
            try
            {
                remoteWorkersDate = await channel.GetWorkersDateAsync();
            }
            catch (Exception ex)
            {
                logFile?.WriteLine(errorMessage + " " + ex.Message);
                NetHelper.CloseChannel(channel);
                return;
            }

            if (remoteWorkersDate == default(DateTime))
            {
                NetHelper.CloseChannel(channel);
                logFile?.WriteLine(errorMessage);
                return;
            }

            var localWorkersDate = Workers.GetWorkersLastUpdateTime();
            if (localWorkersDate >= remoteWorkersDate)
            {
                logFile?.WriteLine($"{Constants.WorkersFile} is up to date.");
                NetHelper.CloseChannel(channel);
                return;
            }

            NetHelper.CloseChannel(channel);

            // Open stream channel for workers download
            serverAddress = BuildServerAddress(WtmSettings.ServerName, Constants.StreamServer);
            var streamChannel = Service.NewStreamChannel(serverAddress);

            Workers workers = null;
            try
            {
                var response = await streamChannel.GetWorkersAsync().ConfigureAwait(false);
                if (response != null)
                {
                    var memoryStream = new MemoryStream();
                    await response.Stream.CopyToAsync(memoryStream);
                    workers = NetHelper.DeserializeFromStream<Workers>(memoryStream);
                }

                if (workers == null)
                    throw new NullReferenceException("The received data are null.");
            }
            catch (Exception ex)
            {
                logFile?.WriteLine(errorMessage + " " + ex.Message);
                NetHelper.CloseChannel(streamChannel);
                return;
            }

            bool wResult = Workers.SaveWorkers(workers);

            if (!wResult)
            {
                logFile?.WriteLine("Workers have been received from local server but could not be saved.");
                NetHelper.CloseChannel(streamChannel);
                return;
            }

            IsInitializingWtm = true;

            Workers.PropertyChanging -= Workers_PropertyChanging;
            Workers.PropertyChanged -= Workers_PropertyChanged;

            Workers = new Workers(workers.WorkerList, workers.PowerCost, workers.CoinType, workers.DisplayCoinAs, workers.NetworkScanMethod);

            Workers.PropertyChanging += Workers_PropertyChanging;
            Workers.PropertyChanged += Workers_PropertyChanged;

            IsInitializingWtm = false;

            logFile?.WriteLine("Workers have been received from local server and successfully updated.");
            NetHelper.CloseChannel(streamChannel);
        }

        private SortedDictionary<string, double> GetHashrates(IList<string> coinList)
        {
            var coinHashList = new SortedDictionary<string, double>();
            foreach (var coin in coinList)
            {
                var meanHashrate = GetMeanHashrate(coin);
                if (meanHashrate == 0)
                    meanHashrate = 1000;
                coinHashList[coin] = meanHashrate;
            }
            return coinHashList;
        }

        private double GetMeanHashrate(string coin)
        {
            double totalHashrate = 0;
            int cnt = 0;
            foreach (var worker in Workers.WorkerList)
            {
                foreach (var ct in worker.CoinList)
                {
                    foreach (var c in ct.Coins)
                    {
                        if (c.Name == coin)
                        {
                            totalHashrate += c.Hashrate;
                            cnt++;
                        }
                    }
                }
            }
            if (cnt != 0)
                return Math.Round(totalHashrate / cnt);
            else return 0;
        }

        private DateTime GetSwitchExpiry(DateTime schedule)
        {
            switch (WtmSettings.SwitchPeriod)
            {
                case "Days":
                    return new DateTime(schedule.Year, schedule.Month, schedule.Day, 0, 0, 0).AddDays(1);
                case "Hours":
                    return new DateTime(schedule.Year, schedule.Month, schedule.Day, schedule.Hour, 0, 0).AddHours(1);
                default:
                    return default(DateTime);
            }
        }
    }
}
