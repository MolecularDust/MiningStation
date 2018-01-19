using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

namespace Mining_Station
{
    public class SwitchWindowVM : NotifyObject
    {
        public ObservableCollection<ProfitTable> ProfitTables { get; set; }
        public RelayCommand Start { get; private set; }
        public RelayCommand Stop { get; private set; }
        public RelayCommand RestartComputer { get; private set; }
        public RelayCommand Quit { get; private set; }
        public RelayCommand SwitchAll { get; private set; }
        public RelayCommand SwitchNone { get; private set; }
        public RelayCommand RestartAll { get; private set; }
        public RelayCommand RestartNone { get; private set; }
        public CancellationTokenSource ManualSwitchCancelSource { get; set; }
        private string _reportTitle;
        public string ReportTitle
        {
            get { return _reportTitle; }
            set { _reportTitle = value; OnPropertyChanged("ReportTitle"); }
        }

        private bool _switchIsInProgress;
        public bool SwitchIsInProgress
        {
            get { return _switchIsInProgress; }
            set { _switchIsInProgress = value; OnPropertyChanged("SwitchIsInProgress"); }
        }

        private bool _switchIsFinished;
        public bool SwitchIsFinished
        {
            get { return _switchIsFinished; }
            set { _switchIsFinished = value; OnPropertyChanged("SwitchIsFinished"); }
        }

        private bool _restartPending;
        public bool RestartPending
        {
            get { return _restartPending; }
            set { _restartPending = value; OnPropertyChanged("RestartPending"); }
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

        private string _displayCoinAs;
        public string DisplayCoinAs
        {
            get { return _displayCoinAs; }
            set { _displayCoinAs = value; OnPropertyChanged("DisplayCoinAs"); }
        }

        public SwitchWindowVM() {}

        public SwitchWindowVM(ObservableCollection<ProfitTable> profitTables)
        {
            this.ProfitTables = profitTables;
            Start = new RelayCommand(StartCommand);
            Stop = new RelayCommand(StopCommand);
            RestartComputer = new RelayCommand(RestartComputerCommand);
            Quit = new RelayCommand(QuitCommand);
            SwitchAll = new RelayCommand(SwitchAllCommand);
            SwitchNone = new RelayCommand(SwitchNoneCommand);
            RestartAll = new RelayCommand(RestartAllCommand);
            RestartNone = new RelayCommand(RestartNoneCommand);
            ReportTitle = "Report:";
            DisplayCoinAs = ViewModel.Instance.Workers.DisplayCoinAs;
        }

        private void RestartAllCommand(object obj)
        {
            MassRestartChange(true);
        }

        private void RestartNoneCommand(object obj)
        {
            MassRestartChange(false);
        }

        private void MassRestartChange(bool change)
        {
            foreach (var table in ProfitTables)
            {
                foreach (var pc in table.Computers)
                {
                    pc.Restart = change;
                }
            }
        }

        private void SwitchAllCommand(object obj)
        {
            MassSwitchChange(true);
        }

        private void SwitchNoneCommand(object obj)
        {
            MassSwitchChange(false);
        }

        private void MassSwitchChange(bool change)
        {
            foreach (var table in ProfitTables)
            {
                foreach (var pc in table.Computers)
                {
                    pc.Switch = change;
                }
            }
        }

        private async void StartCommand(object obj)
        {
            SwitchIsInProgress = true;

            ManualSwitchCancelSource = new CancellationTokenSource();
            var token = ManualSwitchCancelSource.Token;
            var taskList = new List<Task>();
            var jobCount = ProfitTables.Sum(x => x.Computers.Count);
            var failList = new List<ProfitTable>();
            FlowDocument report = new FlowDocument();
            int errorCount = 0;

            await Task.Run(() =>
            {
                ReportTitle = $"Progress: 0 of {jobCount}.";
                int i = 1;

                foreach (var table in ProfitTables)
                {
                    var currentCoinRow = table.ProfitList[0];

                    foreach (var pc in table.Computers)
                    {
                        pc.RestartStatus = pc.Restart ? Computer.OperationStatus.OperationInProgress : Computer.OperationStatus.Indeterminate;
                        pc.SwitchStatus = pc.Switch ? Computer.OperationStatus.OperationInProgress : Computer.OperationStatus.Indeterminate;

                        Func<Task> function = (async () =>
                        {
                            var serverAddress = "net.tcp://" + pc.Name + ":" + NetHelper.Port + Constants.AccessPoint;
                            var channel = Service.NewChannel(serverAddress, TimeSpan.FromSeconds(10));

                            try
                            {
                                try
                                {
                                    if (pc.Switch)
                                    {
                                        bool switchResult = await channel.SetCurrentCoinAsync(
                                            currentCoinRow.Name, 
                                            currentCoinRow.Symbol, 
                                            currentCoinRow.Algorithm, 
                                            currentCoinRow.Path, 
                                            currentCoinRow.Arguments).WithCancellation(token);
                                        if (switchResult)
                                        {
                                            pc.SwitchStatus = Computer.OperationStatus.Success;
                                        }
                                        else pc.SwitchStatus = Computer.OperationStatus.Failure;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    pc.SwitchStatus = Computer.OperationStatus.Failure;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        var p = new Paragraph();
                                        p.Inlines.Add(new Run($"{table.Name}:").FontWeight(FontWeights.Bold).Color(Colors.Salmon));
                                        p.Inlines.Add(new Run($"{pc.Name} Failed to switch to {currentCoinRow.NameAndSymbol}.\r\n"));
                                        p.Inlines.Add(new Run(ex.Message + "\r\n"));
                                        NewParagraph = p;
                                    });
                                    if (pc.Restart)
                                        pc.RestartStatus = Computer.OperationStatus.Failure;
                                    else pc.RestartStatus = Computer.OperationStatus.NotPossible;
                                    return;
                                }
                                try
                                {
                                    if (pc.Restart)
                                    {
                                        // Schedule delayed restart if pc is localhost
                                        if (string.Equals(pc.Name, Environment.MachineName, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            RestartPending = true;
                                            pc.RestartStatus = Computer.OperationStatus.Pending;
                                        }
                                        else
                                        {
                                            bool restartResult = await channel.RestartComputerAsync(5).WithCancellation(token);
                                            if (restartResult)
                                            {
                                                pc.RestartStatus = Computer.OperationStatus.Success;
                                            }
                                            else pc.RestartStatus = Computer.OperationStatus.Failure;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    pc.RestartStatus = Computer.OperationStatus.Failure;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        var p = new Paragraph();
                                        p.Inlines.Add(new Run($"{table.Name}:").FontWeight(FontWeights.Bold).Color(Colors.Salmon));
                                        p.Inlines.Add(new Run($"{pc.Name} Failed to restart.\r\n"));
                                        p.Inlines.Add(new Run(ex.Message + "\r\n"));
                                        NewParagraph = p;
                                    });
                                }
                            }
                            finally
                            {
                                NetHelper.CloseChannel(channel);
                                ReportTitle = $"Progress: {i} of {jobCount}.";
                                i++;
                            }
                        });
                        // This line blocks UI upon DNS lookup of a non-existing or disconnected machine.
                        // That is why the entire block is wrapped into Task.Run().
                        Task task = function();
                        taskList.Add(task);
                    }
                }
            });

            await Task.WhenAll(taskList);

            if (errorCount ==0)
            {
                ReportTitle = "Report:";
                NewParagraph = new Paragraph(new Run("Operation has finished successfully."));
            }
            else
            {
                ReportTitle = "Error report:";
            }

            SwitchIsInProgress = false;
            SwitchIsFinished = true;
        }

        private void StopCommand(object obj)
        {
            if (ManualSwitchCancelSource != null)
                ManualSwitchCancelSource.Cancel();
        }

        private void RestartComputerCommand(object obj)
        {
            if (RestartPending)
            {
                Helpers.RestartComputer(5);
                Application.Current.Shutdown();
            }
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
    }

    public partial class SwitchWindow : Window
    {
        public SwitchWindow()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void TheSwitchWindow_ContentRendered(object sender, EventArgs e)
        {
            MakeDataGridStretchable();
        }

        private bool MenuClicked = false;

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuClicked = true;
        }

        private void TheSwitchWindow_LayoutUpdated(object sender, EventArgs e)
        {
            if (!MenuClicked)
                return;
            MenuClicked = false;
            ResizeDataGridToContent();
            MakeDataGridStretchable();
        }

        private void ResizeDataGridToContent()
        {
            ReportBox.Visibility = Visibility.Hidden;
            ReportBox.Width = 0;
            this.SizeToContent = SizeToContent.Width;
            foreach (var column in DataGridJobs.Columns)
            {
                column.MinWidth = 0;
                column.Width = new DataGridLength(0, DataGridLengthUnitType.Auto);
            }
            DataGridJobs.UpdateLayout();
            DataGridJobs.Measure(DesiredSize);
            this.Width = DataGridJobs.DesiredSize.Width;
        }

        private void MakeDataGridStretchable()
        {
            this.SizeToContent = SizeToContent.Manual;
            this.Width = this.ActualWidth;
            ReportBox.Width = double.NaN;
            ReportBox.Visibility = Visibility.Visible;
            foreach (var column in DataGridJobs.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(column.ActualWidth, DataGridLengthUnitType.Star);
            }
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            ResizeDataGridToContent();
            MakeDataGridStretchable();
        }
    }
}
