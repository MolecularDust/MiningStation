using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Mining_Station
{
    public partial class ViewModel : NotifyObject
    {
        public static List<HistoricalData> ReadHistoricalData(int numberOfRecords)
        {
            var list = new List<HistoricalData>();
            using (var db = new LiteDatabase(Constants.DataBase))
            {
                var collection = db.GetCollection<HistoricalData>(Constants.LightDB_HistoricalDataCollection);
                var day = DateTime.Today.AddDays(-numberOfRecords + 1);
                var result = collection.Find(x => x.Date >= day).Reverse();
                foreach (var entry in result)
                    list.Add(entry.Clone());
            }
            return list;
        }

        public async Task UpdatePriceHistoryTaskWrapper()
        {
            UpdatePriceCancelSource = new CancellationTokenSource();
            var token = UpdatePriceCancelSource.Token;
            await UpdatePriceSemaphore.WaitAsync();
            UpdatePriceIsInProgress = true;
            await Repeater(JobType.UpdatePriceHistory, token).ContinueWith(emptyAction => {
                UpdatePriceIsInProgress = false;
                SetRegistryKeyValue(Constants.UpdatePriceHistoryRegistryKey, "Schedule", "");
            });
            UpdatePriceSemaphore.Release();
        }

        public HistoricalData ReadLastHistoryRecord()
        {
            var history = ReadHistoricalData(1);
            if (history != null && history.Count != 0)
            {
                return history[0];
            }
            return null;
        }

        public async Task<HistoricalData> GetNewPriceHistoryRecord(StreamWriter logFile)
        {
            var coinList = Workers.GetCoins(Workers.WorkerList);
            if (!coinList.Contains("Bitcoin"))
                coinList.Add("Bitcoin");

            var coinHashList = GetHashrates(coinList);

            var wtmRequestIntervalOld = WtmSettings.WtmRequestInterval;

            if (WtmSettings.SaveAllCoins)
            {
                var allCoins = await WhatToMine.GetAllWtmCoinNamesFromJson(true);
                var difference = allCoins.Except(coinList);
                foreach (var coin in difference)
                    coinHashList[coin] = 1000;
            }

            // Get WTM coin data
            var wtmDataResult = await WhatToMine.GetWtmCoinData(coinHashList, false, logFile).ConfigureAwait(false);
            if (wtmDataResult.result == WhatToMine.GetWtmCoinDataResult.CoinNotFound)
                return null;
            var wtmDataDict = wtmDataResult.data;
            if (wtmDataDict != null)
                return new HistoricalData(DateTime.Now, wtmDataDict);
            else return null;
        }

        public enum UpdatePriceHistoryResult
        {
            Success,
            FailedToWriteToFile,
            NoWtmData,
            CoinNotFound,
            AlreadyUpToDate
        }

        private bool HistoryRecordIsUpToDate(HistoricalData record)
        {
            var now = DateTime.Now;
            var today = now.Date;
            if (record != null)
            {
                var schedule = new DateTime(now.Year, now.Month, now.Day, WtmSettings.HistoryTimeTo.Hour, WtmSettings.HistoryTimeTo.Minute, 0);
                if (record.Date > schedule)
                    return true;
                else return false;
            }
            return false;
        }

        public async Task<UpdatePriceHistoryResult> UpdatePriceHistory()
        {
            var lastRecord = ReadLastHistoryRecord();
            if (HistoryRecordIsUpToDate(lastRecord))
                return UpdatePriceHistoryResult.AlreadyUpToDate;

            using (var logFile = new StreamWriter(Constants.HistoricalDataLog, true))
            using (var db = new LiteDatabase(Constants.DataBase))
            {
                var collection = db.GetCollection<HistoricalData>(Constants.LightDB_HistoricalDataCollection);

                if (new FileInfo(Constants.HistoricalDataLog).Length > 0)
                    logFile.WriteLine(string.Empty);
                logFile.WriteLine(DateTime.Now);

                // Get coin defenitions from WhatToMine
                var newRecord = await GetNewPriceHistoryRecord(logFile);
                if (newRecord == null)
                {
                    logFile.WriteLine("Failed to download the list of coin prices from www.whattomine.com.");
                    return UpdatePriceHistoryResult.NoWtmData;
                }

                try
                {
                    // If there already are today's entries in the database delete them and insert the fresh one.
                    var today = DateTime.Today; var tomorrow = today.AddDays(1); // LiteDB doesn't understand DateTime.Today in queries
                    collection.Delete(x => x.Date >= today && x.Date < tomorrow);
                    collection.Insert(newRecord);
                }
                catch
                {
                    logFile.WriteLine("Could not access database to make a new record.");
                    return UpdatePriceHistoryResult.FailedToWriteToFile;
                }
                Debug.WriteLine("Historical data updated.");
                logFile.WriteLine("Historical prices have been successfully updated.");
                return UpdatePriceHistoryResult.Success;
            }
        }

        // Obsolete. Currently is not in use anywhere.
        //public static DateTime GetLastHistoricalPriceUpdateTime()
        //{
        //    using (var db = new LiteDatabase(Constants.DataBase))
        //    {
        //        var collection = db.GetCollection<HistoricalData>(Constants.LightDB_HistoricalDataCollection);
        //        var last = collection.FindAll().Last();
        //        return last !=null ? last.Date : default(DateTime);
        //    }
        //}

        // Obsolete. Currently is not in use anywhere.
        //private async Task<List<HistoricalData>> GetHistoricalData(int period, StreamWriter logFile)
        //{
        //    var result = await UpdatePriceHistory();
        //    if (result != UpdatePriceHistoryResult.AlreadyUpToDate || result != UpdatePriceHistoryResult.Success)
        //        logFile.WriteLine("Failed to update historical prices.");
        //    var historicalDataList = ReadHistoricalData(period);
        //    return historicalDataList != null ? historicalDataList : null;
        //}
    }
}
