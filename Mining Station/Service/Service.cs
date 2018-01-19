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
        public static int StreamBuffer = 1024 * 64;

        public static void Configure(ServiceConfiguration config)
        {
            ServiceEndpoint accessPointEndpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IAccessPoint)),
                new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:" + NetHelper.Port.ToString() + Constants.AccessPoint));
            config.AddServiceEndpoint(accessPointEndpoint);

            ServiceEndpoint streamServerEndpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IStreamServer)),
                StreamedTcpBinding(), new EndpointAddress("net.tcp://localhost:" + NetHelper.Port.ToString() + Constants.StreamServer));
            streamServerEndpoint.EndpointBehaviors.Add(new DispatcherSynchronizationBehavior { AsynchronousSendEnabled = true, MaxPendingReceives = 10 });
            config.AddServiceEndpoint(streamServerEndpoint);

            config.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = false, HttpsGetEnabled = false });

            //ServiceThrottlingBehavior stb = new ServiceThrottlingBehavior
            //{
            //    MaxConcurrentSessions = 100,
            //    MaxConcurrentCalls = 100,
            //    MaxConcurrentInstances = 100
            //};
            //config.Description.Behaviors.Add(stb);
        }

        public static NetTcpBinding StreamedTcpBinding()
        {
            NetTcpBinding tcpBinding = new NetTcpBinding();
            tcpBinding.TransferMode = TransferMode.Streamed;
            tcpBinding.MaxReceivedMessageSize = 2147483647;
            tcpBinding.ReceiveTimeout = TimeSpan.FromHours(1);
            tcpBinding.SendTimeout = TimeSpan.FromMinutes(10);
            tcpBinding.CloseTimeout = TimeSpan.FromMinutes(1);
            tcpBinding.OpenTimeout = TimeSpan.FromMinutes(1);
            return tcpBinding;
        }


        public static IAccessPoint NewChannel(string serverAddress, TimeSpan timeOut = default(TimeSpan)) 
        {
            NetTcpBinding binding = new NetTcpBinding();

            if (timeOut != default(TimeSpan))
                binding.SendTimeout = timeOut;

            EndpointAddress endpoint = new EndpointAddress(serverAddress);
            ChannelFactory<IAccessPoint> channelFactory = new ChannelFactory<IAccessPoint>(binding, endpoint);
            IAccessPoint channel = channelFactory.CreateChannel();
            return channel;
        }

        public static IStreamServer NewStreamChannel(string serverAddress, TimeSpan timeOut = default(TimeSpan))
        {
            NetTcpBinding binding = StreamedTcpBinding();

            if (timeOut != default(TimeSpan))
                binding.SendTimeout = timeOut;

            EndpointAddress endpoint = new EndpointAddress(serverAddress);
            ChannelFactory<IStreamServer> channelFactory = new ChannelFactory<IStreamServer>(binding, endpoint);
            IStreamServer channel = channelFactory.CreateChannel();
            return channel;
        }
    }
}
