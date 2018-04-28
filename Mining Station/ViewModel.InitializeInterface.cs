using FluentScheduler;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mining_Station
{
    // Initialize interface
    public partial class ViewModel : NotifyObject
    {
        private async void InitializeCommand(object obj)
        {
            try
            {
                await InitializeInterface();
            }
            catch (Exception ex)
            {
                var t = Task.Run(() =>
                MessageBox.Show(ex.Message, "Error initializing interface", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        public async Task InitializeInterface()
        {
            // Delete registry key left-over
            var startWithWindows = GetRegistryKeyValue(Constants.RunRegistryKey, Constants.AppName);
            if (startWithWindows != null && !WtmSettings.StartWithWindows)
                DeleteRegistryKeyValue(Constants.RunRegistryKey, Constants.AppName);

            ApplyServerSettingsCommand(null); // Start server if app's role is 'Server'

            NetHelper.Port = WtmSettings.TcpPort;
            AccessPoinServiceHost = new ServiceHost(typeof(Service));
            AccessPoinServiceHost.Open();

            string failedToDecrypt = "The encryption is based on machine configuration. "
                        + "If you have copied settings to a different machine, logged on as a different user or changed your Windows password you need to re-enter ";

            ScanLanCancelSource = new System.Threading.CancellationTokenSource();
            var token = ScanLanCancelSource.Token;

            Func<Task> taskLoadWtmCoins = (async () =>
            {
                //Load Whattomine available coins
                if (WtmSettings.ProxyPasswordEncrypted != null && WtmSettings.ProxyPasswordEncrypted != string.Empty)
                {
                    try
                    {
                        WtmSettings.ProxyPassword = WtmSettings.ProxyPasswordEncrypted.DecryptSecure();
                    }
                    catch (Exception)
                    {
                        WtmSettings.ProxyPasswordEncrypted = null;
                        string msg = "Failed to decrypt WhatToMine proxy password. " + failedToDecrypt + "the proxy password.";
                        var t = Task.Run(() => MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                    }
                }
                await LoadWtmCoins(token);

                // Update coins statuses
                if (WtmCoins != null)
                {
                    UpdateCoinStatus();
                }
                IsInitializingWtm = false;
            });

            Func<Task> taskLoadYahooCurrencies = (async () =>
            {
                //Load Yahoo currencies
                SaveUndoIsEnabled = false;
                await WtmSettings.GetYahooRates();
                SaveUndoIsEnabled = true;
            });

            var tasks = new List<Task>() { taskLoadWtmCoins() };
            if (WtmSettings.UseYahooRates)
                tasks.Add(taskLoadYahooCurrencies());
            try
            {
                await Task.WhenAll(tasks).WithCancellation(token);
            }
            catch (Exception ex)
            {
                Task<MessageBoxResult> t = null;
                if (ex.Message != "The operation was canceled.")
                    t = Task.Run(() => MessageBox.Show("Error while waiting for taskLoadWtmCoins or taskLoadYahooCurrencies.\n" + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error));
            }
            finally
            {
                IsInitializingWtm = false;
            }

            // Update callback
            using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Constants.BaseRegistryKey, true))
            {
                if (key != null)
                {
                    var callerHostName = key.GetValue("CallerHostName") as string;
                    if (callerHostName != null && callerHostName != string.Empty)
                    {
                        var address = BuildServerAddress(callerHostName, Constants.AccessPoint);
                        var channel = Service.NewChannel(address, TimeSpan.FromSeconds(60));
                        try
                        {
                            await channel.UpdateApplicationCallback(Environment.MachineName, Helpers.ApplicationVersion());
                        }
                        catch { }
                        finally
                        {
                            NetHelper.CloseChannel(channel);
                        }
                        key.DeleteValue("CallerHostName");
                    }
                }
            }

            bool workersExist = File.Exists(Constants.WorkersFile);
            bool settingsExist = File.Exists(Constants.WtmSettingsFile);
            if (!workersExist && !settingsExist)
            {
                var t = Task.Run(() =>
                MessageBox.Show($"If you're running a fresh copy of {Constants.AppName} please define your workers inside 'Workers' tab and click 'Save' icon.", "Default configuration used", MessageBoxButton.OK, MessageBoxImage.Information));
            }

            else
            {
                if (DefaultWorkers)
                {
                    var t = Task.Run(() =>
                    MessageBox.Show($"{Constants.WorkersFile} is missing or corrupt.", "", MessageBoxButton.OK, MessageBoxImage.Warning));
                }
                if (DefaultWtmSettings)
                {
                    var t = Task.Run(() =>
                    MessageBox.Show($"{Constants.WtmSettingsFile} is missing or corrupt. Default settings are used.", "", MessageBoxButton.OK, MessageBoxImage.Warning));
                }
            }

            // Check if there are any pending jobs in registry
            bool switchJobIsInProgress = false;
            bool updatePriceJobIsInProgress = false;
            string switchTime = string.Empty;
            string switchLastTime = string.Empty;
            string updatePriceTime = string.Empty;
            string updatePriceLastUpdate = string.Empty;

            using (RegistryKey switchKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Constants.SwitchRegistryKey, true))
            using (RegistryKey updatePriceKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Constants.UpdatePriceHistoryRegistryKey, true))
            {
                if ((string)switchKey.GetValue("IsInProgress") == "True")
                    switchJobIsInProgress = true;
                switchTime = (string)switchKey.GetValue("Schedule");
                if (switchTime != null && switchTime != string.Empty)
                    SwitchSchedule = Convert.ToDateTime(switchTime, DateTimeFormatInfo.InvariantInfo).ToLocalTime();
                switchLastTime = (string)switchKey.GetValue("LastUpdate");
                if (switchLastTime != null && switchLastTime != string.Empty)
                    SwitchLastTime = Convert.ToDateTime(switchLastTime, DateTimeFormatInfo.InvariantInfo).ToLocalTime();

                if ((string)updatePriceKey.GetValue("IsInProgress") == "True")
                    updatePriceJobIsInProgress = true;
                updatePriceTime = (string)updatePriceKey.GetValue("Schedule");
                if (updatePriceTime != null && updatePriceTime != string.Empty)
                    HistoricalPricesSchedule = Convert.ToDateTime(updatePriceTime, DateTimeFormatInfo.InvariantInfo).ToLocalTime();
                updatePriceLastUpdate = (string)updatePriceKey.GetValue("LastUpdate");
                if (updatePriceLastUpdate != null && updatePriceLastUpdate != string.Empty)
                    HistoricalPricesLastTime = Convert.ToDateTime(updatePriceLastUpdate, DateTimeFormatInfo.InvariantInfo).ToLocalTime();
            }

            // Start job if one is still pending after system restart
            if (switchJobIsInProgress && workersExist)
            {
                await SwitchTaskWrapper();
                switchTime = string.Empty;
            }
            if (updatePriceJobIsInProgress && workersExist)
            {
                await UpdatePriceHistoryTaskWrapper();
                updatePriceTime = string.Empty;
            }

            // Delete Mass Update left-overs if they exist
            Helpers.DeleteUpdateLeftOvers();

            // Check if Switch job has been missed today/this hour
            if (WtmSettings.AutoSwitch)
            {
                if (switchTime != string.Empty)
                {
                    DateTime schedule = Convert.ToDateTime(switchTime, DateTimeFormatInfo.InvariantInfo);
                    DateTime now = DateTime.Now;
                    DateTime expiryTime = GetSwitchExpiry(schedule);
                    if (schedule < now && now < expiryTime)
                    {
                        await SwitchTaskWrapper();
                        switchTime = string.Empty;
                    }
                }
            }

            // Check if UpdateHistoricalPrices job has been missed today
            DateTime updatePriceLastUpdateTime;
            if (WtmSettings.BackupHistoricalPrices)
            {
                if (updatePriceLastUpdate != string.Empty)
                {
                    updatePriceLastUpdateTime = Convert.ToDateTime(updatePriceLastUpdate, DateTimeFormatInfo.InvariantInfo);
                    DateTime now = DateTime.Now;
                    DateTime historyBackupTime = new DateTime(now.Year, now.Month, now.Day, WtmSettings.HistoryTimeTo.Hour, WtmSettings.HistoryTimeTo.Minute, 0, DateTimeKind.Local);
                    if (now > historyBackupTime && updatePriceLastUpdateTime.Day < now.Day) // Now is past daily schedule and last update had been done before today
                    {
                        await UpdatePriceHistoryTaskWrapper();
                        updatePriceTime = string.Empty;
                    }
                }
            }

            // Schedule jobs
            JobManager.UseUtcTime();
            if (WtmSettings.AutoSwitch == true && workersExist)
            {
                Debug.WriteLine(DateTime.Now + " Scheduling a Switch task at startup.");
                if (switchTime != string.Empty)
                    ResetScheduledJob(JobType.Switch, switchTime);
                else ResetScheduledJob(JobType.Switch);
            }
            if (WtmSettings.BackupHistoricalPrices == true && workersExist)
            {
                if (updatePriceTime != string.Empty)
                    ResetScheduledJob(JobType.UpdatePriceHistory, updatePriceTime);
                else ResetScheduledJob(JobType.UpdatePriceHistory);
            }
            UpdateNextJobStatus();
        }

        private void UpdateCoinStatus()
        {
            var wtmDict = WtmCoins.ToDictionary(x => x.Name, y => y.Status);
            foreach (var worker in Workers.WorkerList)
            {
                foreach (var coinTable in worker.CoinList)
                {
                    foreach (var coin in coinTable.Coins)
                    {
                        string status = null;
                        wtmDict.TryGetValue(coin.Name, out status);
                        coin.Status = status;
                    }
                }
            }
        }
    }
}
