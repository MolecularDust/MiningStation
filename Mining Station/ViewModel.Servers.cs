using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Mining_Station
{
    public partial class ViewModel : NotifyObject
    {
        private bool TestConnection_CanExecute(object obj)
        {
            if (WtmSettings.ApplicationMode == "Client")
                return true;
            else return false;
        }

        public static string BuildServerAddress(string computer, string serviceSuffix)
        {
            return "net.tcp://" + computer + ":" + NetHelper.Port + serviceSuffix;
        }

        private bool ValidateServerAddress(StreamWriter logFile)
        {
            if (WtmSettings.ServerName == string.Empty)
            {
                logFile?.WriteLine("ERROR: Server address cannot be empty. Specify a machine name or IP address.");
                return false;
            }

            if (WtmSettings.ServerName == "localhost")
            {
                logFile?.WriteLine("ERROR: Server address cannot be 'localhost'. Specify a machine name or IP address.");
                return false;
            }

            return true;
        }

        private async void TestConnectionCommand(object obj)
        {
            var serverAddress = BuildServerAddress(WtmSettings.ServerName, Constants.AccessPoint);
            var channel = Service.NewChannel(serverAddress);

            TestConnection.SetCanExecute(false);
            Helpers.MouseCursorWait();

            var cancelSource = new CancellationTokenSource();
            var token = cancelSource.Token;
            ProgressManager pm = new ProgressManager($"Accessing {WtmSettings.ServerName}...", cancelSource, 1000, true);

            try
            {
                var testResult = await channel.GetInfoAsync().WithCancellation(token).ConfigureAwait(false);
                if (testResult != null)
                {
                    pm.IsAlive = false;
                    pm.Close();
                    await Task.Delay(100);
                    Helpers.MouseCursorNormal();
                    if (testResult.ApplicationMode == "Server")
                        MessageBox.Show($"{Constants.AppName} server is alive and responsive on {WtmSettings.ServerName}.", "Success", MessageBoxButton.OK);
                    else MessageBox.Show($"{Constants.AppName} is up on {WtmSettings.ServerName} but it is not in server mode.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                pm.Close();
                await Task.Delay(100);
                Helpers.MouseCursorNormal();
                MessageBox.Show("No response from the server.\n\n" + ex.Message, "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                var error = NetHelper.CloseChannel(channel);
                Application.Current.Dispatcher.Invoke(() => TestConnection.SetCanExecute(true));
                if (pm.IsAlive)
                    pm.Close();
            }
        }

        private void ApplyServerSettingsCommand(object obj)
        {
            WtmSettings.ServerSettingsAreUpdated = true;
            NetHelper.Port = WtmSettings.TcpPort;
            //if (WtmSettings.ApplicationMode == "Server")
            //{
            //    if (WtmProxyServerServiceHost != null)
            //        WtmProxyServerServiceHost.Close();
            //    WtmProxyServerServiceHost = new ServiceHost(typeof(WtmProxyServer));
            //    WtmProxyServerServiceHost.Open();
            //}
        }

        private void GenerateRandomPortCommand(object obj)
        {
            WtmSettings.TcpPort = Helpers.Random.Next(0, 65535);
        }

        private void UpdateWtmHttpClient()
        {
            if (WtmSettings.UseProxy && WtmSettings.Proxy != null && WtmSettings.Proxy != string.Empty)
            {
                var clientHandler = new HttpClientHandler
                {
                    Proxy = new WebProxy(WtmSettings.Proxy),
                    UseProxy = WtmSettings.UseProxy
                };

                bool notAnonymous = !WtmSettings.AnonymousProxy
                    && WtmSettings.ProxyUserName != null
                    && WtmSettings.ProxyPassword != null
                    && WtmSettings.ProxyUserName != string.Empty
                    && WtmSettings.ProxyPassword.Length != 0;

                if (notAnonymous)
                {
                    var credentials = new NetworkCredential(
                        WtmSettings.ProxyUserName,
                        WtmSettings.ProxyPassword);
                    clientHandler.Proxy.Credentials = credentials;
                    clientHandler.PreAuthenticate = true;
                    clientHandler.UseDefaultCredentials = false;
                }
                WhatToMine.WtmHttpClientHandler = clientHandler;
                WhatToMine.WtmHttpClient = new HttpClient(clientHandler);
            }
            else // No Proxy
            {
                WhatToMine.WtmHttpClientHandler = new HttpClientHandler();
                WhatToMine.WtmHttpClient = new HttpClient();
            }
        }
    }
}
