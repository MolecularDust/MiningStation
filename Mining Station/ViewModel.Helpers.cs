using FluentScheduler;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Mining_Station
{
    // Misc items
    public partial class ViewModel : NotifyObject
    {
        public static object GetRegistryKeyValue(string key, string valueName)
        {
            using (RegistryKey currentKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key))
            {
                if (currentKey != null)
                    return currentKey.GetValue(valueName);
                else return null;
            }
        }

        public static void SetRegistryKeyValue(string key, string valueName, object value)
        {
            using (RegistryKey currentKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, true))
            {
                if (currentKey != null)
                    currentKey.SetValue(valueName, value);
            }
        }

        public static void DeleteRegistryKeyValue(string key, string valueName)
        {
            using (RegistryKey currentKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, true))
            {
                if (currentKey != null)
                {
                    var keyToCheck = currentKey.GetValue(valueName);
                    if (keyToCheck != null)
                        currentKey.DeleteValue(valueName);
                }
            }
        }

        private (bool success, List<string> failList) KillProcesses(IList<string> killList)
        {
            if (killList == null || killList.Count == 0)
                return (false, null);
            var failList = new List<string>();
            foreach (var fileName in killList)
            {
                var name = fileName.Replace(".exe", string.Empty);
                foreach (var process in Process.GetProcessesByName(name))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        failList.Add(fileName);
                    }
                }
            }
            if (failList.Count == 0)
                return (true, null);
            else return (false, failList);
        }

        private string ApiKeySymbolCounter(SecureString secString)
        {
            if (secString == null || secString.Length == 0)
                return "?";
            if (secString.Length == 1)
                return "1 symbol";
            return secString.Length.ToString() + " symbols";
        }

        private async Task LoadWtmCoins(CancellationToken token = default(CancellationToken))
        {
            List<AlgoCoin> tempCoinList = null;

            switch (Workers.CoinType)
            {
                case "All":
                    tempCoinList = await WhatToMine.GetAllWtmSelectableCoins(false, token);
                    break;
                case "Active":
                    tempCoinList = await WhatToMine.GetAllWtmSelectableCoins(true, token);
                    break;
                default:
                    tempCoinList = await WhatToMine.GetWtmCoinNames(Workers.CoinType, token);
                    break;
            }

            if (tempCoinList == null)
            {
                WtmCoins = null;
                if (!token.IsCancellationRequested)
                {
                    var t = Task.Run(() => MessageBox.Show("Could not download the list of coins from\nwww.whattomine.com", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                }
                    
            }
            else { WtmCoins = new ObservableCollection<AlgoCoin>(tempCoinList); }
        }

        private bool EvaluateWtmDataTimeRange(DateTime dateTime)
        {
            var registryTime = GetRegistryKeyValue(Constants.SwitchRegistryKey, "Schedule") as string;
            if (registryTime == null || registryTime == string.Empty)
                return false;
            var schedule = Convert.ToDateTime(registryTime, DateTimeFormatInfo.InvariantInfo);
            DateTime schedulePeriodStart = default(DateTime);
            switch (WtmSettings.SwitchPeriod.TrimEnd('s'))
            {
                case "Day":
                    schedulePeriodStart = new DateTime(schedule.Year, schedule.Month, schedule.Day, 0, 0, 0, DateTimeKind.Local).AddDays(-WtmSettings.SwitchPeriodCount + 1);
                    break;
                case "Hour":
                    schedulePeriodStart = new DateTime(schedule.Year, schedule.Month, schedule.Day, schedule.Hour, 0, 0, 0, DateTimeKind.Local).AddHours(-WtmSettings.SwitchPeriodCount + 1);
                    break;
                default: return false;
            }
            if (dateTime > schedulePeriodStart && schedulePeriodStart != default(DateTime))
                return true;
            else return false;
        }


        private bool EvaluateHistoricalPricesTimeRange(DateTime dateTime)
        {
            var registryTime = GetRegistryKeyValue(Constants.UpdatePriceHistoryRegistryKey, "Schedule") as string;
            if (registryTime == null || registryTime == string.Empty)
                return false;
            var schedule = Convert.ToDateTime(registryTime, DateTimeFormatInfo.InvariantInfo);
            var schedulePeriodStart = new DateTime(schedule.Year, schedule.Month, schedule.Day, 0, 0, 0, DateTimeKind.Local).AddDays(-1);
            if (dateTime > schedulePeriodStart && schedulePeriodStart != default(DateTime))
                return true;
            else return false;
        }

        public Worker GetWorkerByPCName(string name)
        {
            Worker foundWorker = null;
            foreach (var w in Workers.WorkerList)
            {
                var pcName = w.Computers.FirstOrDefault(s => string.Equals(s, name, StringComparison.CurrentCultureIgnoreCase));
                if (pcName != null)
                {
                    foundWorker = w;
                    break;
                }
            }
            return foundWorker;
        }

        private void ClearJob(JobType jobType)
        {
            string registryKey = string.Empty;
            switch(jobType)
            {
                case JobType.Switch:
                    SwitchCancelSource?.Cancel();
                    registryKey = Constants.SwitchRegistryKey;
                    break;
                case JobType.UpdatePriceHistory:
                    UpdatePriceCancelSource?.Cancel();
                    registryKey = Constants.UpdatePriceHistoryRegistryKey;
                    break;
            }
            JobManager.RemoveJob(jobType.ToString());
            SetRegistryKeyValue(registryKey, "Schedule", string.Empty);
            SetRegistryKeyValue(registryKey, "IsInProgress", "False");
            SetRegistryKeyValue(registryKey, "Round", 0);
        }

        public void BypassUndo(Action action)
        {
            SaveUndoIsEnabled = false;
            action();
            SaveUndoIsEnabled = true;
        }
    }
}
