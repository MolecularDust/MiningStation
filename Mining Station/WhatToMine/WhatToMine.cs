using mshtml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using static Mining_Station.Workers;

namespace Mining_Station
{
    public class WhatToMine
    {
        public static int RequestInterval {
            get;
            set;
        }
        public static DateTime PreviousRequest = DateTime.UtcNow;

        public static async Task RespectTimeLimit()
        {
            if (RequestInterval == 0)
                return;
            var timeDiff = DateTime.UtcNow.Subtract(PreviousRequest).TotalMilliseconds;
            if (timeDiff < RequestInterval)
            {
                var delay = Convert.ToInt32(RequestInterval - timeDiff);
                await Task.Delay(delay);
            }
            PreviousRequest = DateTime.UtcNow;
        }

        public static HttpClientHandler WtmHttpClientHandler;
        public static HttpClient WtmHttpClient;// = new HttpClient(WtmHttpClientHandler);

        public static string WebDownload(string url)
        {
            RespectTimeLimit().RunSynchronously();
            try
            {
                string result = WtmHttpClient.GetStringAsync(url).Result;
                return result;
            }
            catch (Exception e)
            {
                Helpers.ShowErrorMessage(e.Message);
                return null;
            }
        }

        public static async Task<string> WebDownloadAsync(string url, bool showErrors = true, CancellationToken cancelToken = default(CancellationToken))
        {
            await RespectTimeLimit();
            try
            {
                var httpResult = await WtmHttpClient.GetAsync(url, cancelToken).ConfigureAwait(false);
                httpResult.EnsureSuccessStatusCode();
                string result = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                if (showErrors)
                    Helpers.ShowErrorMessage(e.Message);
                return null;
            }
        }


        // Obsolete method. Extracts coin links by parsing HTML. Use GetWtmLinksFromJson instead.
        public static async Task<Dictionary<string, WtmLinks>> GetWtmLinks(CancellationToken cancelToken = default(CancellationToken))
        {
            string allCoinsHTML = await WebDownloadAsync(@"http://whattomine.com/calculators", false, cancelToken).ConfigureAwait(false);
            if (allCoinsHTML == null)
                return null;

            var result = new Dictionary<string, WtmLinks>();

            IHTMLDocument2 doc = (IHTMLDocument2)(new HTMLDocument());
            doc.write(allCoinsHTML);

            var hs = new HashSet<string>();
            foreach (IHTMLAnchorElement link in doc.links)
                hs.Add(link.href);

            Regex reg = new Regex(@"^(about:/coins/)\d{1,3}");
            Regex regCoin = new Regex(@"^(about:/coins/)\d{1,3}-(?<Coin>\w+)-");

            foreach (var link in hs)
            {
                var match = reg.Match(link);
                if (match.Success)
                {
                    var matchCoin = regCoin.Match(link);
                    result[(matchCoin.Groups["Coin"]).ToString().ToUpper()] = new WtmLinks
                    {
                        CoinLink = link.Replace("about:", "http://whattomine.com"),
                        JsonLink = match.Value.Replace("about:", "http://whattomine.com") + ".json"
                    };
                }
            }
            return result;
        }

        public static async Task<Dictionary<string, WtmLinks>> GetWtmLinksFromJson(CancellationToken cancelToken = default(CancellationToken))
        {
            var allCoins = await GetAllCoinsJson(cancelToken);
            var result = new Dictionary<string, WtmLinks>();
            if (allCoins == null)
                return null;
            foreach (var coin in allCoins)
            {
                var value = (Dictionary<string, object>)coin.Value;
                string status = ((string)value["status"]);
                result[coin.Key] = new WtmLinks
                {
                    CoinLink = "http://whattomine.com/coins/" + value["id"],
                    JsonLink = "http://whattomine.com/coins/" + value["id"] + ".json",
                    Status = status
                };
            }
            return result;
        }

        // Obsolete method. Extracts coin names by parsing HTML. Use GetAllWtmCoinNamesFromJson instead.
        public static async Task<List<string>> GetAllWtmCoinNamesFromWeb()
        {
            string allCoinsHTML = await WebDownloadAsync(@"http://whattomine.com/calculators", false).ConfigureAwait(false);
            if (allCoinsHTML == null)
                return null;

            IHTMLDocument2 doc = (IHTMLDocument2)(new HTMLDocument());
            doc.write(allCoinsHTML);
            var links = new List<string>();

            Regex reg = new Regex(@"^(about:/coins/)\d{1,3}-(?<Coin>\w+)-");
            foreach (IHTMLAnchorElement link in doc.links)
            {

                var match = reg.Match(link.href);
                var coin = (match.Groups["Coin"]).ToString();
                if (coin != string.Empty)
                    links.Add(coin.ToUpper());
            }
            var allCoins = links.Distinct().ToList();
            return allCoins;
        }

        public static async Task<List<string>> GetAllWtmCoinNamesFromJson(bool filterOutNotActiveCoins)
        {
            var dict = await GetAllCoinsJson();
            if (filterOutNotActiveCoins)
            {
                dict = dict.Where(x => string.Equals((string)((Dictionary<string, object>)x.Value)["status"], "Active", StringComparison.InvariantCultureIgnoreCase)).ToDictionary(x => x.Key, x => x.Value);
            }
            return dict != null ? dict.Select(x => x.Key).Distinct().ToList() : null;
        }

        public static async Task<List<AlgoCoin>> GetAllWtmSelectableCoins(bool activeCoinsOnly = false, CancellationToken token = default(CancellationToken))
        {
            var dict = await GetAllCoinsJson(token);
            if (dict == null)
                return null;
            var coinList = new List<AlgoCoin>();
            foreach (var coin in dict)
            {
                var value = (Dictionary<string, object>)coin.Value;
                var status = (string)value["status"];
                if (activeCoinsOnly && !string.Equals(status, "Active", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                var tag = (string)value["tag"];
                var algorithm = (string)value["algorithm"];
                var listEntry = new AlgoCoin { Name = coin.Key, Symbol = tag, Algorithm = algorithm, Status = status };
                coinList.Add(listEntry);
            }
            return coinList;
        }

        public static async Task<Dictionary<string, object>> GetAllCoinsJson(CancellationToken token = default(CancellationToken))
        {
             var coinsJson = await WebDownloadAsync(@"http://whattomine.com/calculators.json", false, token).ConfigureAwait(false);
            if (coinsJson == null)
                return null;
            var coinsObj = JsonConverter.ConvertFromJson<Dictionary<string, object>>(coinsJson);
            if (coinsObj == null)
                return null;
            var coins = coinsObj["coins"] as Dictionary<string, object>;
            var result = new Dictionary<string, object>();
            foreach (var coin in coins)
            {
                string tag = (string)((Dictionary<string, object>)coin.Value)["tag"];
                result.Add(coin.Key, coin.Value);
            }
            return result;
        }

        public static async Task<List<AlgoCoin>> GetWtmCoinNames(string coinType, CancellationToken token = default(CancellationToken))
        {
            string url = string.Empty;
            if (coinType == "GPU")
                url = @"http://whattomine.com/coins.json";
            if (coinType == "ASIC")
                url = @"http://whattomine.com/asic.json";

            string coinsJson = await WebDownloadAsync(url, false, token).ConfigureAwait(false);
            if (coinsJson == null)
                return null;

            var dict = JsonConverter.ConvertFromJson<Dictionary<string, object>>(coinsJson) as Dictionary<string, object>;
            if (dict == null)
                return null;
            object coinsOutput;
            dict.TryGetValue("coins", out coinsOutput);
            var coins = coinsOutput as Dictionary<string, object>;
            if (coins == null)
                return null;
            var coinList = new List<AlgoCoin>();
            foreach (var coin in coins)
            {
                var property = coin.Value as Dictionary<string, object>;
                var tag = (string)property["tag"];
                var algorithm = (string)property["algorithm"];
                if (tag != "NICEHASH")
                    coinList.Add(new AlgoCoin { Name = coin.Key, Symbol = tag, Algorithm = algorithm });
            }
            coinList.OrderBy(x => x.Name);
            return coinList;
        }

        public enum GetWtmCoinDataResult
        {
            OK,
            Fail,
            CoinNotFound
        }

        public static async Task<(Dictionary<string, WtmData> data, GetWtmCoinDataResult result)> GetWtmCoinData(IDictionary<string, double> coinHashList, bool interactive, StreamWriter logFile = null)
        {
            string statusBarText = "Downloading coin definitions from whattomine.com";
            ViewModel.Instance.StatusBarText = statusBarText + "...";

            int i = 0; int cnt = coinHashList.Count;
            var wtmRequestIntervalOld = ViewModel.Instance.WtmSettings.WtmRequestInterval;
            if (cnt > ViewModel.Instance.WtmSettings.DynamicRequestTrigger)
            {
                ViewModel.Instance.BypassUndo(() => {
                    if (ViewModel.Instance.WtmSettings.WtmRequestInterval < ViewModel.Instance.WtmSettings.DynamicRequestInterval)
                        ViewModel.Instance.WtmSettings.WtmRequestInterval = ViewModel.Instance.WtmSettings.DynamicRequestInterval;
                });
            }

            bool errorFlag = false;
            bool continueFlag = false;
            bool exitEarly = false;

            CancellationTokenSource cancelSource = new CancellationTokenSource();
            CancellationToken token = cancelSource.Token;
            ProgressManager pm = null;


            if (interactive)
            {
                pm = new ProgressManager("Accessing whattomine.com...", cancelSource);
                pm.SetIndeterminate(true);
                pm.SetProgressMaxValue(cnt);
            }

            await RespectTimeLimit();
            var wtmLinks = await WhatToMine.GetWtmLinksFromJson(token).ConfigureAwait(false);
            if (token.IsCancellationRequested)
            {

                pm?.Close();
                return (null, GetWtmCoinDataResult.Fail);
            }
            if (wtmLinks == null)
            {
                string errorMessage = "Failed to get the list of available coins from whattomine.com.";
                if (interactive)
                {
                    pm?.Close();
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (logFile != null)
                        logFile.WriteLine(errorMessage);
                }
                return (null, GetWtmCoinDataResult.Fail);
            }
            if (interactive && pm != null)
            {
                pm.SetIndeterminate(false);
                pm.SetText("Downloading " + i + " of " + cnt);
                pm.SetProgressValue(0);
            }

            var wtmDataDict = new Dictionary<string, WtmData>();
            string currentJsonStr = string.Empty;
            string currentCoinHtml = string.Empty;
            GetWtmCoinDataResult methodResult = GetWtmCoinDataResult.OK;

            foreach (var coin in coinHashList)
            {
                continueFlag = false;
                List<string> httpResults = new List<string>();
                WtmLinks entry;
                wtmLinks.TryGetValue(coin.Key, out entry);
                if (entry == null)
                {
                    string errorMessage = $"{coin.Key} has not been found among coins at http://whattomine.com/calculators. Execution aborted.";
                    errorFlag = true; methodResult = GetWtmCoinDataResult.CoinNotFound;
                    if (interactive)
                        Helpers.ShowErrorMessage(errorMessage);
                    else
                    {
                        if (logFile != null)
                            logFile.WriteLine(errorMessage);
                    }
                    break;
                }

                //Check whattomine coin status
                if (!string.Equals(entry.Status, "Active", StringComparison.InvariantCultureIgnoreCase))
                {
                    MessageBoxResult response = MessageBoxResult.OK;
                    string message = $"The status of {coin.Key} is reported as \"{entry.Status}\" by whattomine.com.";
                    if (interactive)
                       response = MessageBox.Show(message, "Coin is not active", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    else logFile.WriteLine(message);
                    if (interactive && response == MessageBoxResult.Cancel)
                    {
                        errorFlag = true;
                        break;
                    }
                    continue;
                }

                await RespectTimeLimit();
                try
                {
                    var response = await WtmHttpClient.GetAsync(entry.JsonLink + "?hr=" + coin.Value.ToString(CultureInfo.InvariantCulture), token).ConfigureAwait(false);
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (result != null && result.Contains("errors"))
                    {
                        var wtmErrorDict = JsonConverter.ConvertFromJson<Dictionary<string,object>>(result);
                        if (wtmErrorDict != null)
                        {
                            object errorObj = null;
                            wtmErrorDict.TryGetValue("errors", out errorObj);
                            var errorList = errorObj as ArrayList;
                            if (errorList != null)
                            {
                                string errorMessage = string.Join(". ", errorList.ToArray());
                                if (errorMessage != null)
                                    throw new Exception(errorMessage);
                            }
                        }
                    }

                    response.EnsureSuccessStatusCode();
                    httpResults.Add(result);
                }
                catch (Exception e)
                {
                    if (interactive && e.Message == "A task was canceled.")
                    {
                        errorFlag = true; methodResult = GetWtmCoinDataResult.Fail;
                    }
                    else
                    {
                        string errorMessage = $"Failed to download {coin.Key} definition from whattomine.com.";
                        //continueFlag = true;
                        errorFlag = true;
                        methodResult = GetWtmCoinDataResult.Fail;
                        if (interactive)
                            Helpers.ShowErrorMessage(errorMessage + "\n\n" + e.Message);
                        else
                        {
                            if (logFile != null)
                                logFile.WriteLine(errorMessage);
                        }
                    }
                }

                //if (continueFlag)
                //    continue;

                if (errorFlag || httpResults == null)
                    break;

                // Interpret JSON
                currentJsonStr = httpResults[0];
                Dictionary<string, string> json = new Dictionary<string, string>();
                json = JsonConverter.ConvertFromJson<Dictionary<string, string>>(currentJsonStr);

                if (json == null)
                {
                    string errorMessage = $"Failed to interpret {coin.Key} definition from whattomine.com.";
                    errorFlag = true; methodResult = GetWtmCoinDataResult.Fail;
                    if (interactive)
                        Helpers.ShowErrorMessage(errorMessage);
                    else
                    {
                        if (logFile != null)
                            logFile.WriteLine(errorMessage);
                    }
                    break;
                }
                else
                {
                    var defaultHashrate = coin.Value;
                    wtmDataDict.Add(coin.Key, new WtmData { DefaultHashrate = defaultHashrate, Json = json });
                }

                if (pm !=null && !pm.IsAlive)
                {
                    exitEarly = true;
                    break;
                }

                i++;
                if (interactive && pm != null)
                {
                    pm.SetText("Downloaded " + i + " of " + cnt);
                    pm.SetProgressValue(i);
                }
                else
                {
                    ViewModel.Instance.StatusBarText = statusBarText + ": " + i + " of " + cnt;
                }
            }

            ViewModel.Instance.BypassUndo(() => ViewModel.Instance.WtmSettings.WtmRequestInterval = wtmRequestIntervalOld);
            ViewModel.Instance.UpdateNextJobStatus();

            if (!exitEarly && interactive)
                pm?.Close();
            if (errorFlag || exitEarly)
                return (null, methodResult);
            if (!interactive && (logFile != null))
            {
                var noun = wtmDataDict.Count == 1 ? "coin" : "coins";
                logFile.WriteLine($"The list of {wtmDataDict.Count} whattomine.com {noun} has been downloaded.");
            }
            return (wtmDataDict, GetWtmCoinDataResult.OK);
        }

        // Obsolete hashrate extractor from html. !!!!!!!!!!!!!!!!!!!!!!!!!
        private static double GetDefaultHashrateFromHtml(string html)
        {
            var doc = new HTMLDocument();
            IHTMLDocument2 doc2 = (IHTMLDocument2)doc;
            doc2.clear();
            doc2.write(html);
            IHTMLDocument3 doc3 = (IHTMLDocument3)doc2;
            var hr = doc3.getElementById("hr");
            var defaultHashrate = Convert.ToDouble(hr.getAttribute("defaultValue"), CultureInfo.InvariantCulture);
            return (double)defaultHashrate;
        }

        public static List<ProfitTable> CreateProfitTables(Dictionary<string, WtmData> wtmDataDict, List<Worker> workerList, decimal powerCost, WtmSettingsObject settings, bool switchableOnly = false)
        {
            var btc = wtmDataDict["Bitcoin"];
            var profitList = new List<ProfitTableRow>();
            var profitTables = new List<ProfitTable>();

            int j = 1; var workerCount = workerList.Count;
            foreach (var worker in workerList)
            {
                foreach (var entry in worker.CoinList)
                {
                    if (switchableOnly && !entry.Switch)
                        continue;
                    var profitResult = CalculateProfit(entry, wtmDataDict, btc, settings, powerCost);
                    decimal profit24 = profitResult.profit24;
                    decimal revenueAllCoins = profitResult.revenue;
                    bool multicell = (entry.Coins.Count > 1);
                    var newRow = new ProfitTableRow
                    {
                        Name = entry.FullName,
                        Symbol = entry.FullSymbol,
                        Algorithm = entry.FullAlgorithm,
                        Hashrate = entry.FullHashrate,
                        Multicell = multicell,
                        Switchable = entry.Switch,
                        Revenue = revenueAllCoins,
                        ProfitDay = profit24,
                        ProfitWeek = profit24 * 7,
                        ProfitMonth = profit24 * 30,
                        ProfitYear = profit24 * 365,
                        Path = entry.Path ?? string.Empty,
                        Arguments = entry.Arguments ?? string.Empty,
                        Notes = entry.Notes,
                    };
                    profitList.Add(newRow);

                    newRow = null;
                }
                var newProfitTable = new ProfitTable
                {
                    Name = worker.Name,
                    Index = j,
                    ThisPC = Helpers.ListContainsThisPC(worker.Computers),
                    Computers = new ObservableCollection<Computer>(worker?.Computers.Select(computer => new Computer { Name = computer })),
                    Description = worker.Description,
                    ProfitList = profitList.OrderByDescending(p => p.ProfitDay).ToList()
                };

                var firstCoin = newProfitTable.ProfitList.FirstOrDefault();
                if (firstCoin != null)
                    firstCoin.ManualSwitch = true;

                //Show the topmost coin as the new coin in Computers list
                newProfitTable.HookPropertyChanched();
                newProfitTable.Row_PropertyChanged(newProfitTable.ProfitList.FirstOrDefault(), new PropertyChangedEventArgs("ManualSwitch"));

                profitTables.Add(newProfitTable);
                profitList.Clear();
                j++;
            }
            return profitTables;
        }

        public static void GetProfit(ProfitTable profitTable, Shortcut currentlyMinedCoin, out ProfitTableRow maxCoinRow, out decimal maxProfit, out decimal currentProfit, out bool nothingChecked)
        {
            maxProfit = 0;
            maxCoinRow = null;
            nothingChecked = true;
            ProfitTableRow currentCoinRow = null;
            var coinName = currentlyMinedCoin?.GetName();
            foreach (var row in profitTable.ProfitList)
            {
                if (currentlyMinedCoin != null
                    && row.Name == coinName
                    && row.Path == currentlyMinedCoin?.Path 
                    && row.Arguments == currentlyMinedCoin?.Arguments)
                    currentCoinRow = row;
                if (row.Switchable)
                    nothingChecked = false;
                if (row.Switchable && row.ProfitDay > maxProfit)
                {
                    maxCoinRow = row;
                    maxProfit = row.ProfitDay;
                }
            }
            currentProfit = 0;
            if (currentlyMinedCoin != null && currentCoinRow != null)
                currentProfit = currentCoinRow.ProfitDay;
        }

        public static (decimal profit24, decimal revenue) CalculateProfit(
            CoinTable coinTable, 
            Dictionary<string,WtmData> wtmDataDict, 
            WtmData btc, 
            WtmSettingsObject settings, 
            decimal powerCost 
            )
        {
            decimal revenueAllCoins = 0;
            revenueAllCoins = 0;

            decimal coinRate = 0; decimal btcRate = 0;

            foreach (Coin coin in coinTable.Coins)
            {
                WtmData currentCoin = null;
                wtmDataDict.TryGetValue(coin.Name, out currentCoin);
                if (currentCoin == null)
                    continue;
                if (settings.UseHistoricalAverage && currentCoin.HasAverage)
                {
                    coinRate = currentCoin.AveragePrice;
                    btcRate = btc != null ? btc.AveragePrice : 0;
                }
                else
                {
                    string keyName;
                    if (settings.Average24)
                        keyName = "exchange_rate24";
                    else
                        keyName = "exchange_rate";
                    string coinValue;
                    currentCoin.Json.TryGetValue(keyName, out coinValue);
                    if (coinValue != null)
                        coinRate = Helpers.StringToDecimal(coinValue);
                    string btcValue = null;
                    btc?.Json.TryGetValue(keyName, out btcValue);
                    if (btcValue != null)
                        btcRate = Helpers.StringToDecimal(btcValue);
                }

                decimal ratio = (decimal)coin.Hashrate / (decimal)currentCoin.DefaultHashrate;
                decimal revenueOneCoin = 0;
                decimal estimatedRewards = Helpers.StringToDecimal(currentCoin.Json["estimated_rewards"]);
                revenueOneCoin = ratio * estimatedRewards * coinRate * btcRate;
                revenueAllCoins += revenueOneCoin;
                //totalHashrate += Convert.ToString(coin.Hashrate, CultureInfo.InvariantCulture) + "+";
                //name += coin.Name + "+";
                //algorithm += currentCoin.Json["algorithm"] + "+";
            }
            decimal profit24 = 0;

            //name = name.TrimEnd('+');
            profit24 = revenueAllCoins - (revenueAllCoins * (decimal)coinTable.Fees / 100) - (((decimal)coinTable.Power / 1000) * 24 * powerCost);
            return (profit24, revenueAllCoins);
        }
    }
}
