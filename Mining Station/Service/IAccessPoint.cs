using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IAccessPoint" in both code and config file together.
    [ServiceContract]
    public interface IAccessPoint
    {
        [OperationContract]
        Task<ComputerInfo> GetInfoAsync();
        [OperationContract]
        Task<bool> SetCurrentCoinAsync(string coinName, string coinSymbol, string coinAlgorithm, string path, string arguments);
        [OperationContract]
        Task<bool> RestartComputerAsync(int delay);
        [OperationContract]
        Task<DateTime> GetWorkersDateAsync();
        [OperationContract]
        Task<DateTime> GetWtmSettingsDateAsync();
        [OperationContract]
        Task<bool> UpdateWorkersAsync();
        [OperationContract]
        Task UpdateApplicationCallback(string computerName, string version);
    }

    [DataContract]
    public class ComputerInfo
    {
        [DataMember]
        public string ApplicationMode { get; set; }

        [DataMember]
        public Shortcut CurrentCoin { get; set; }

        [DataMember]
        public string Version { get; set; }
    }
}
