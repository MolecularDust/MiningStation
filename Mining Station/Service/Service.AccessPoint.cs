using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mining_Station
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AccessPoint" in both code and config file together.
    public partial class Service : IAccessPoint, IStreamServer
    {

        public string genericError = "Server received request but failed to process it.";

        public Task<ComputerInfo> GetInfoAsync()
        {
            Shortcut shortcut = null;
            shortcut = Shortcut.GetCurrentCoin();
            var info = new ComputerInfo
            {
                ApplicationMode = ViewModel.Instance.WtmSettings.ApplicationMode,
                CurrentCoin = shortcut,
                Version = Helpers.ApplicationVersion()
            };
            return Task.FromResult(info);
        }

        public Task<bool> SetCurrentCoinAsync(string coinName, string coinSymbol, string coinAlgorithm, string path, string arguments)
        {
            try
            {
                if (!System.IO.File.Exists(path))
                    throw new System.IO.FileNotFoundException($"File not found: {path}");
                Shortcut.CreateCoinShortcut(coinName, coinSymbol, coinAlgorithm, path, arguments);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public Task<bool> RestartComputerAsync(int delay)
        {
            return Task.FromResult(Helpers.RestartComputer(delay));
        }

        public Task<DateTime> GetWorkersDateAsync()
        {
            DateTime updateTime = Workers.GetWorkersLastUpdateTime();
            return Task.FromResult(updateTime);
        }

        public Task<DateTime> GetWtmSettingsDateAsync()
        {
            DateTime updateTime = WtmSettingsObject.GetWtmSettingsLastUpdateTime();
            return Task.FromResult(updateTime);
        }

        public Task<bool> UpdateWorkersAsync()
        {
            throw new NotImplementedException();
        }

        public static readonly Task CompletedTask = Task.FromResult(0);

        public Task UpdateApplicationCallback(string computerName, string version)
        {
            if (MassUpdateVM.Instance.Computers == null)
                return CompletedTask;
            var pc = MassUpdateVM.Instance.Computers.FirstOrDefault(x => x.Name == computerName);
            if (pc != null)
            {
                pc.Version = version;
                pc.SwitchStatus = Computer.OperationStatus.Success;
                if (pc.UpdateSuccessfull != null)
                {
                    var result = pc.UpdateSuccessfull.TrySetResult(true);
                    if (result == false)
                    {
                        MessageBox.Show("Update callback failed. The update might have been successful or might have failed. Restart update to see whether the remote version is actually up to date.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //throw new Exception("Could not set a task's state to RunToCompletion");
                    }
                }
            }
            return CompletedTask;
        }
    }
}
