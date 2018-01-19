using FluentScheduler;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mining_Station
{
    public partial class ViewModel : NotifyObject
    {
        private Schedule GetNextJob()
        {
            List<Schedule> jobList = JobManager.AllSchedules.ToList();
            if (jobList == null || jobList.Count == 0)
                return null;
            Schedule nextJob = jobList.First();
            foreach (var job in jobList)
            {
                if (job.NextRun < nextJob.NextRun)
                    nextJob = job;
            }
            return nextJob;
        }

        public void UpdateNextJobStatus()
        {
            var nextJob = GetNextJob();
            if (nextJob != null)
                StatusBarText = "Next job scheduled: " + nextJob.Name + " " + nextJob.NextRun.ToLocalTime();
            else StatusBarText = "No job scheduled.";
        }

        public enum JobType
        {
            Switch,
            UpdatePriceHistory
        }

        private void ResetScheduledJob(JobType jobType, string time = "")
        {
            bool settingsCondition = false;
            string registryKey = string.Empty;
            Func<Task> job = null;
            ScheduleDayTime settingsTime = null;
            int randomInterval = 0;
            string period = string.Empty;
            switch (jobType)
            {
                case JobType.Switch:
                    settingsCondition = WtmSettings.AutoSwitch;
                    registryKey = Constants.SwitchRegistryKey;
                    job = (async () => await SwitchTaskWrapper().ConfigureAwait(false));
                    period = WtmSettings.SwitchPeriod.TrimEnd('s');
                    if (period == "Day")
                        settingsTime = new ScheduleDayTime(WtmSettings.SwitchPeriodCount, WtmSettings.SwitchTimeFrom);
                    if (period == "Hour")
                        settingsTime = new ScheduleDayTime(0, WtmSettings.SwitchPeriodCount, WtmSettings.SwitchTimeFrom.Minute);
                    randomInterval = ScheduleTime.Difference(WtmSettings.SwitchTimeTo, WtmSettings.SwitchTimeFrom);
                    break;
                case JobType.UpdatePriceHistory:
                    settingsCondition = WtmSettings.BackupHistoricalPrices;
                    registryKey = Constants.UpdatePriceHistoryRegistryKey;
                    job = (async () => await UpdatePriceHistoryTaskWrapper().ConfigureAwait(false));
                    period = "Day";
                    settingsTime = new ScheduleDayTime(1, WtmSettings.HistoryTimeFrom);
                    randomInterval = ScheduleTime.Difference(WtmSettings.HistoryTimeTo, WtmSettings.HistoryTimeFrom);
                    break;
            }
            JobManager.RemoveJob(jobType.ToString());
            if (settingsCondition)
            {
                DateTime scheduleTime = new DateTime();
                if (time != string.Empty) // Schedule job at given time. This branch is used when there's a pending job in registry after reboot.
                {
                    var pendingTime = Convert.ToDateTime(time, DateTimeFormatInfo.InvariantInfo).ToUniversalTime();
                    if (pendingTime > DateTime.UtcNow)
                        scheduleTime = pendingTime;
                    else // Over-protection, should not actually happen?
                    {
                        SetRegistryKeyValue(registryKey, "Schedule", string.Empty);
                        ResetScheduledJob(jobType);
                        return;
                    }
                }
                else // Schedule job from current WtmSettings.SwitchTimeFrom. This branch is used when applying new settings.
                {
                    using (RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryKey, true))
                    {
                        DateTime now = DateTime.Now;
                        DateTime newTime = new DateTime();
                        switch (period)
                        {
                            case "Day":
                                newTime = new DateTime(now.Year, now.Month, now.Day, settingsTime.Hour, settingsTime.Minute, 0, DateTimeKind.Local);
                                if (newTime < now)
                                    newTime = newTime.AddDays(settingsTime.Day);
                                break;
                            case "Hour":
                                newTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, settingsTime.Minute, 0, DateTimeKind.Local);
                                if (newTime < now)
                                    newTime = newTime.AddHours(settingsTime.Hour);
                                break;
                        }

                        scheduleTime = newTime.ToUniversalTime();
                        if (randomInterval > 0)
                        {
                            scheduleTime = scheduleTime.AddMinutes(Helpers.Random.Next(randomInterval));
                        }
                        regKey.SetValue("Schedule", scheduleTime.ToString("o"), RegistryValueKind.String);
                        if (jobType == JobType.Switch)
                            SwitchSchedule = scheduleTime.ToLocalTime();
                        if (jobType == JobType.UpdatePriceHistory)
                            HistoricalPricesSchedule = scheduleTime.ToLocalTime();
                    }
                }

                JobManager.AddJob(
                   async () =>
                   {
                       await job().ConfigureAwait(false);
                       ResetScheduledJob(jobType);
                       if (WtmSettings.RestartComputer && RestartComputerPending)
                       {
                           Helpers.RestartComputer(5);
                       }
                   },
                   (sch) => sch.WithName(jobType.ToString()).ToRunOnceAt(scheduleTime));
            }
            UpdateNextJobStatus();
        }

        // Attempts to execute a scheduled job 10 times
        public async Task Repeater(JobType jobType, CancellationToken token)
        {
            string registryKey = string.Empty;
            Func<Task<Enum>> job = null;
            string logFileName = string.Empty;
            string errorMessage = string.Empty;
            switch (jobType)
            {
                case JobType.Switch:
                    registryKey = Constants.SwitchRegistryKey;
                    job = async () => await SwitchStandalone().ConfigureAwait(false);
                    logFileName = Constants.SwitchLog;
                    errorMessage = "Failed to switch coin after 10 attempts.";
                    break;
                case JobType.UpdatePriceHistory:
                    registryKey = Constants.UpdatePriceHistoryRegistryKey;
                    job = async () => await UpdatePriceHistory().ConfigureAwait(false);
                    logFileName = "hitorical-data.log";
                    errorMessage = "Failed to update historical prices after 10 attempts.";
                    break;
            }

            using (RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryKey, true))
            {
                Debug.WriteLine(DateTime.Now + " " + "Repeater task execution started. Jop type: " + jobType.ToString());

                int i;
                bool breakLoop = false;
                Enum result;

                if ((string)regKey.GetValue("IsInProgress") == "True")
                {
                    i = (int)regKey.GetValue("Round");
                }
                else
                {
                    regKey.SetValue("IsInProgress", true);
                    regKey.SetValue("Round", 1);
                    i = 1;
                }

                while (i <= 10)
                {
                    result = await job().ConfigureAwait(false);
                    if (
                        (result is SwitchResult &&
                            ((SwitchResult)result == SwitchResult.NoNeedToSwitch
                            || (SwitchResult)result == SwitchResult.NothingToDo
                            || (SwitchResult)result == SwitchResult.SwitchedSuccessfully
                            || (SwitchResult)result == SwitchResult.CoinNotFound
                            || (SwitchResult)result == SwitchResult.ThisPcIsNotListed
                            || (SwitchResult)result == SwitchResult.DelayIsNotOver
                            || (SwitchResult)result == SwitchResult.Terminate))
                            ||
                         (result is UpdatePriceHistoryResult &&
                            ((UpdatePriceHistoryResult)result == UpdatePriceHistoryResult.Success
                            || (UpdatePriceHistoryResult)result == UpdatePriceHistoryResult.CoinNotFound
                            || (UpdatePriceHistoryResult)result == UpdatePriceHistoryResult.AlreadyUpToDate))
                            )
                    {

                        breakLoop = true;
                        break;
                    }
                    // Wait for the next round if this attempt was unsuccessfull
                    var delay = Helpers.Fibonacci(i) * 1000 * 60;
                    DateTime nextRoundTime = DateTime.Now.AddMilliseconds(delay);
                    StatusBarText = $"Attempt {i} of {jobType} job failed. Next try is scheduled at {nextRoundTime.ToLocalTime().ToString("HH:mm:ss")}.";
                    await Task.Delay(delay, token).ConfigureAwait(false);
                    i++;
                    regKey.SetValue("Round", i);
                }
                if (i > 10 && !breakLoop)
                {
                    using (var logFile = new StreamWriter(logFileName, true))
                    {
                        logFile.WriteLine(errorMessage);
                    }
                }
                var now = DateTime.UtcNow;
                regKey.SetValue("LastUpdate", now.ToString("o"));
                regKey.SetValue("IsInProgress", false);
                regKey.SetValue("Round", 0);
                if (jobType == JobType.Switch)
                    SwitchLastTime = now.ToLocalTime();
                if (jobType == JobType.UpdatePriceHistory)
                    HistoricalPricesLastTime = now.ToLocalTime();
            }
        }
    }
}
