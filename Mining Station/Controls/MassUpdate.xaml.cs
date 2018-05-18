using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Globalization;

namespace Mining_Station
{

    public class MassUpdateVM : NotifyObject
    {
        public static MassUpdateVM Instance { get; set; }

        private string _windowTitle;
        public string WindowTitle
        {
            get { return _windowTitle; }
            set { _windowTitle = value; OnPropertyChanged("WindowTitle"); }
        }

        private string _header;
        public string Header
        {
            get { return _header; }
            set { _header = value; OnPropertyChanged("Header"); }
        }

        private string _subSubHeader;
        public string SubHeader
        {
            get { return _subSubHeader; }
            set { _subSubHeader = value; OnPropertyChanged("SubHeader"); }
        }

        private string _columnHeader;
        public string ColumnHeader
        {
            get { return _columnHeader; }
            set { _columnHeader = value; OnPropertyChanged("ColumnHeader"); }
        }

        public ObservableCollection<Computer> Computers { get; set; }
        public RelayCommand Update { get; private set; }
        public RelayCommand Stop { get; private set; }
        public RelayCommand Quit { get; private set; }
        public RelayCommand UpdateAll { get; private set; }
        public RelayCommand UpdateNone { get; private set; }
        public CancellationTokenSource UpdateCancelSource { get; set; }

        private string _reportTitle;
        public string ReportTitle
        {
            get { return _reportTitle; }
            set { _reportTitle = value; OnPropertyChanged("ReportTitle"); }
        }

        private bool _scanIsInProgress;
        public bool ScanIsInProgress
        {
            get { return _scanIsInProgress; }
            set { _scanIsInProgress = value; OnPropertyChanged("ScanIsInProgress"); }
        }

        private bool _updateIsInProgress;
        public bool UpdateIsInProgress
        {
            get { return _updateIsInProgress; }
            set { _updateIsInProgress = value; OnPropertyChanged("UpdateIsInProgress"); }
        }

        private bool _updateIsFinished;
        public bool UpdateIsFinished
        {
            get { return _updateIsFinished; }
            set { _updateIsFinished = value; OnPropertyChanged("UpdateIsFinished"); }
        }

        private Paragraph _newParagraph;
        public Paragraph NewParagraph
        {
            get { return _newParagraph; }
            set { _newParagraph = value; OnPropertyChanged("NewParagraph"); }
        }

        private Run _newRun;
        public Run NewRun
        {
            get { return _newRun; }
            set { _newRun = value; OnPropertyChanged("NewRun"); }
        }

        public MassUpdateVM() { }

        public MassUpdateVM(ObservableCollection<Computer> computers)
        {
            Instance = this;
            this.Computers = computers;
            Update = new RelayCommand(UpdateCommand, Update_CanExecute);
            Stop = new RelayCommand(StopCommand);
            Quit = new RelayCommand(QuitCommand);
            UpdateAll = new RelayCommand(UpdateAllCommand);
            UpdateNone = new RelayCommand(UpdateNoneCommand);
            ReportTitle = "Report:";
        }

        private void UpdateAllCommand(object obj)
        {
            MassUpdateChange(true);
        }

        private void UpdateNoneCommand(object obj)
        {
            MassUpdateChange(false);
        }

        private void MassUpdateChange(bool change)
        {
            foreach (var pc in Computers)
                if (pc.SwitchStatus != Computer.OperationStatus.NotPossible)
                    pc.Switch = change;
        }

        private bool Update_CanExecute(object obj)
        {
            var firstChecked = Computers.FirstOrDefault(x => x.Switch);
            if (firstChecked != null)
                return true;
            else return false;
        }

        private async void UpdateCommand(object obj)
        {
            UpdateIsInProgress = true;

            var workersDate = Workers.GetWorkersLastUpdateTime();
            var wtmSettingsDate = WtmSettingsObject.GetWtmSettingsLastUpdateTime();

            UpdateCancelSource = new CancellationTokenSource();
            var token = UpdateCancelSource.Token;

            var taskList = new List<Task>();
            var failList = new List<ProfitTable>();
            FlowDocument report = new FlowDocument();
            int errorCount = 0;

            string appVersion = Helpers.ApplicationVersion();

            var jobCount = Computers.Count;
            ReportTitle = $"Progress: 0 of {jobCount}.";
            for (int i = 0; i < jobCount; i++)
            {
                var pc = Computers[i];
                if (!pc.Switch)
                    continue;
                Task task = null;
                switch (ColumnHeader)
                {
                    case "Version":
                        task = Task.Run(async () => { errorCount = await UpdateVersion(pc, token, errorCount, i, jobCount, appVersion); });
                        break;
                    case "Workers Date":
                        task = Task.Run(async () => { errorCount = await UpdateWorkers(pc, token, errorCount, i, jobCount, workersDate); });
                        break;
                    case "Settings Date":
                        task = Task.Run(async () => { errorCount = await UpdateWtmSettings(pc, token, errorCount, i, jobCount, wtmSettingsDate); });
                        break;
                }
                taskList.Add(task);
            }

            await Task.WhenAll(taskList);

            if (errorCount == 0)
            {
                ReportTitle = "Report:";
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run("Operation has finished successfully."));
                NewParagraph = paragraph;
            }
            else
            {
                ReportTitle = "Error report:";
            }

            UpdateIsInProgress = false;
            UpdateIsFinished = true;
        }

        private async Task<int> UpdateVersion(Computer pc, CancellationToken token, int errorCount, int iteration, int jobCount, string appVersion)
        {
            pc.SwitchStatus = Computer.OperationStatus.OperationInProgress;
            pc.UpdateSuccessfull = new TaskCompletionSource<bool>();

            var address = ViewModel.BuildServerAddress(pc.Name, Constants.StreamServer);
            var channel = Service.NewStreamChannel(address, TimeSpan.FromSeconds(60 * 10));
            FileStream outputStream = null;
            try
            {
                string appFileName = Helpers.ApplicationPath();
                System.IO.FileInfo info = new System.IO.FileInfo(appFileName);
                outputStream = new FileStream(appFileName, FileMode.Open, FileAccess.Read, FileShare.Read, Service.StreamBuffer, true);
                FileRequest request = new FileRequest();
                request.CallerHostName = Environment.MachineName;
                request.FileName = info.Name;
                request.Length = info.Length;
                request.FileStream = outputStream;

                var reply = await channel.UpdateApplication(request).WithCancellation(token).ConfigureAwait(false);
                var awaitCallbackTask = pc.UpdateSuccessfull.Task;
                await Task.WhenAny(awaitCallbackTask, Task.Delay(1000 * 30)).WithCancellation(token).ConfigureAwait(false);
                bool callbackResult = false;
                if (awaitCallbackTask.Status == TaskStatus.RanToCompletion)
                    callbackResult = awaitCallbackTask.Result;
                if (!callbackResult)
                    throw new Exception($"{pc.Name} has downloaded the new version but failed to report success.");

                // Update row colors, green (NotNecessary) - the version is up to date, red (Necessary) - the version is older.
                if (new Version(appVersion).CompareTo(new Version(pc.Version)) == 1)
                {
                    pc.UpdateStatus = Computer.OperationStatus.Necessary;
                }
                else
                {
                    pc.UpdateStatus = Computer.OperationStatus.NotNecessary;
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                pc.SwitchStatus = Computer.OperationStatus.Failure;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var p = new Paragraph();
                    p.Inlines.Add(new Run($"{pc.Name}: ").FontWeight(FontWeights.Bold).Color(Colors.Salmon));
                    p.Inlines.Add(new Run($"Failed to update.\r\n"));
                    p.Inlines.Add(new Run(ex.Message + "\r\n"));
                    NewParagraph = p;
                });
            }
            finally
            {
                NetHelper.CloseChannel(channel);
                if (outputStream != null)
                    outputStream.Close();
                ReportTitle = $"Progress: {iteration + 1} of {jobCount}.";
            }
            return errorCount;
        }

        private async Task<int> UpdateWorkers(Computer pc, CancellationToken token, int errorCount, int iteration, int jobCount, DateTime workersDate)
        {
            pc.SwitchStatus = Computer.OperationStatus.OperationInProgress;

            var address = ViewModel.BuildServerAddress(pc.Name, Constants.StreamServer);
            var channel = Service.NewStreamChannel(address, TimeSpan.FromSeconds(60));

            MemoryStream stream = NetHelper.SerializeToStream<Workers>(ViewModel.Instance.Workers);

            try
            {

                var response = await channel.UpdateWorkers(new StreamUploadRequest { Stream = stream }).WithCancellation(token).ConfigureAwait(false);
                if (response.ResponseFlag)
                {
                    pc.SwitchStatus = Computer.OperationStatus.Success;
                    pc.WorkersDate = response.Date;
                    if (pc.WorkersDate >= workersDate)
                        pc.UpdateStatus = Computer.OperationStatus.NotNecessary;
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                pc.SwitchStatus = Computer.OperationStatus.Failure;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var p = new Paragraph();
                    p.Inlines.Add(new Run($"{pc.Name}: ").FontWeight(FontWeights.Bold).Color(Colors.Salmon));
                    p.Inlines.Add(new Run($"Failed to update.\r\n"));
                    p.Inlines.Add(new Run(ex.Message + "\r\n"));
                    NewParagraph = p;
                });
            }
            finally
            {
                NetHelper.CloseChannel(channel);
                stream.Dispose();
                ReportTitle = $"Progress: {iteration + 1} of {jobCount}.";
            }
            return errorCount;
        }

        private async Task<int> UpdateWtmSettings(Computer pc, CancellationToken token, int errorCount, int iteration, int jobCount, DateTime wtmSettingsDate)
        {
            pc.SwitchStatus = Computer.OperationStatus.OperationInProgress;

            var address = ViewModel.BuildServerAddress(pc.Name, Constants.StreamServer);
            var channel = Service.NewStreamChannel(address, TimeSpan.FromSeconds(60));

            MemoryStream stream = NetHelper.SerializeToStream<WtmSettingsObject>(ViewModel.Instance.WtmSettings);
            var uploadRequest = new StreamUploadRequest { Stream = stream };

            try
            {
                if (ViewModel.Instance.WtmSettings.ProxyPassword != null && ViewModel.Instance.WtmSettings.ProxyPassword.Length != 0)
                {
                    uploadRequest.ProxyPassword = ViewModel.Instance.WtmSettings.ProxyPassword.ToCharArray();
                }
                var response = await channel.UpdateWtmSettings(uploadRequest).WithCancellation(token).ConfigureAwait(false);
                if (response.ResponseFlag)
                {
                    pc.SwitchStatus = Computer.OperationStatus.Success;
                    pc.WtmSettingsDate = response.Date;
                    if (pc.WtmSettingsDate >= wtmSettingsDate)
                        pc.UpdateStatus = Computer.OperationStatus.NotNecessary;
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                pc.SwitchStatus = Computer.OperationStatus.Failure;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var p = new Paragraph();
                    p.Inlines.Add(new Run($"{pc.Name}: ").FontWeight(FontWeights.Bold).Color(Colors.Salmon));
                    p.Inlines.Add(new Run($"Failed to update.\r\n"));
                    p.Inlines.Add(new Run(ex.Message + "\r\n"));
                    NewParagraph = p;
                });
            }
            finally
            {
                if (uploadRequest.ProxyPassword.Length != 0)
                    Array.Clear(uploadRequest.ProxyPassword, 0, uploadRequest.ProxyPassword.Length);
                NetHelper.CloseChannel(channel);
                stream.Dispose();
                ReportTitle = $"Progress: {iteration + 1} of {jobCount}.";
            }
            return errorCount;
        }


        private void StopCommand(object obj)
        {
            if (UpdateCancelSource != null)
                UpdateCancelSource.Cancel();
        }

        private void QuitCommand(object obj)
        {
            var window = obj as Window;
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        public async Task ScanLan()
        {
            ScanIsInProgress = true;

            UpdateCancelSource = new CancellationTokenSource();
            var token = UpdateCancelSource.Token;
            var tasks = new List<Task>();

            var appVersion = new Version(Helpers.ApplicationVersion());
            var workersDate = Workers.GetWorkersLastUpdateTime();
            var wtmSettingsDate = WtmSettingsObject.GetWtmSettingsLastUpdateTime();

            foreach (var pc in Computers)
            {
                Task task = null;
                switch (ColumnHeader)
                {
                    case "Version":
                        task = Task.Run(async () => await QueryRemoteVersion(pc, token, appVersion));
                        break;
                    case "Workers Date":
                        task = Task.Run(async () => await QueryRemoteWorkersDate(pc, token, workersDate));
                        break;
                    case "Settings Date":
                        task = Task.Run(async () => await QueryRemoteWtmSettingsDate(pc, token, wtmSettingsDate));
                        break;
                }
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            ScanIsInProgress = false;
        }

        private async Task QueryRemoteVersion(Computer pc, CancellationToken token, Version appVersion)
        {
            var address = ViewModel.BuildServerAddress(pc.Name, Constants.AccessPoint);
            var channel = Service.NewChannel(address);
            try
            {
                var info = await channel.GetInfoAsync().WithCancellation(token).ConfigureAwait(false);
                if (info != null)
                {
                    pc.OnlineStatus = Computer.OperationStatus.Success;
                    pc.ApplicationMode = info.ApplicationMode;
                    pc.Version = info.Version;
                    if (appVersion.CompareTo(new Version(pc.Version)) == 1)
                    {
                        pc.Switch = true;
                        pc.SwitchStatus = Computer.OperationStatus.Possible;
                        pc.UpdateStatus = Computer.OperationStatus.Necessary;
                    }
                    else
                    {
                        pc.SwitchStatus = Computer.OperationStatus.NotPossible;
                        pc.UpdateStatus = Computer.OperationStatus.NotNecessary;
                    }
                }
            }
            catch (Exception)
            {
                pc.OnlineStatus = Computer.OperationStatus.Failure;
                pc.SwitchStatus = Computer.OperationStatus.NotPossible;
            }
            finally
            {
                NetHelper.CloseChannel(channel);
            }
            return;
        }

        private async Task QueryRemoteWorkersDate(Computer pc, CancellationToken token, DateTime workersDate)
        {
            var address = ViewModel.BuildServerAddress(pc.Name, Constants.AccessPoint);
            var channel = Service.NewChannel(address);
            try
            {
                var info = await channel.GetInfoAsync().WithCancellation(token).ConfigureAwait(false);
                var remoteWorkersDate = await channel.GetWorkersDateAsync().WithCancellation(token).ConfigureAwait(false);
                if (info != null)
                {
                    pc.OnlineStatus = Computer.OperationStatus.Success;
                    pc.ApplicationMode = info.ApplicationMode;
                    pc.WorkersDate = remoteWorkersDate;
                    if (workersDate > remoteWorkersDate)
                    {
                        pc.Switch = true;
                        pc.SwitchStatus = Computer.OperationStatus.Possible;
                        pc.UpdateStatus = Computer.OperationStatus.Necessary;
                    }
                    else
                    {
                        pc.SwitchStatus = Computer.OperationStatus.NotPossible;
                        pc.UpdateStatus = Computer.OperationStatus.NotNecessary;
                    }
                }
            }
            catch (Exception)
            {
                pc.OnlineStatus = Computer.OperationStatus.Failure;
                pc.SwitchStatus = Computer.OperationStatus.NotPossible;
            }
            finally
            {
                NetHelper.CloseChannel(channel);
            }
            return;
        }

        private async Task QueryRemoteWtmSettingsDate(Computer pc, CancellationToken token, DateTime wtmSettingsDate)
        {
            var address = ViewModel.BuildServerAddress(pc.Name, Constants.AccessPoint);
            var channel = Service.NewChannel(address);
            try
            {
                var info = await channel.GetInfoAsync().WithCancellation(token).ConfigureAwait(false);
                var remoteSettingsDate = await channel.GetWtmSettingsDateAsync().WithCancellation(token).ConfigureAwait(false);
                if (info != null)
                {
                    pc.OnlineStatus = Computer.OperationStatus.Success;
                    pc.ApplicationMode = info.ApplicationMode;
                    pc.WtmSettingsDate = remoteSettingsDate;
                    if (wtmSettingsDate > remoteSettingsDate)
                    {
                        pc.Switch = true;
                        pc.SwitchStatus = Computer.OperationStatus.Possible;
                        pc.UpdateStatus = Computer.OperationStatus.Necessary;
                    }
                    else
                    {
                        pc.SwitchStatus = Computer.OperationStatus.NotPossible;
                        pc.UpdateStatus = Computer.OperationStatus.NotNecessary;
                    }
                }
            }
            catch (Exception)
            {
                pc.OnlineStatus = Computer.OperationStatus.Failure;
                pc.SwitchStatus = Computer.OperationStatus.NotPossible;
            }
            finally
            {
                NetHelper.CloseChannel(channel);
            }
            return;
        }
    }

    public partial class MassUpdate : Window
    {
        public MassUpdate()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void TheMassUpdateWindow_ContentRendered(object sender, EventArgs e)
        {
            var window = sender as Window;
            var vm = window.DataContext as MassUpdateVM;
            if (vm == null)
                return;
            await vm.ScanLan();
        }
    }

    public class DateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)values[0];
            var onlineStatus = (Computer.OperationStatus)values[1];
            if (onlineStatus != Computer.OperationStatus.Success)
                return null;
            if (date == default(DateTime))
                return "?";
            else return date.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
