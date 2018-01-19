using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
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
    public class AddComputersVM : NotifyObject, IDisposable
    {
        public RelayCommand Process { get; private set; }
        public RelayCommand SelectAll { get; private set; }
        public RelayCommand SelectNone { get; private set; }
        public RelayCommand ScanLan { get; private set; }
        public RelayCommand Cancel { get; private set; }

        private ObservableCollection<ExpandoObject> _computers;
        public ObservableCollection<ExpandoObject> Computers
        {
            get { return _computers; }
            set { _computers = value; OnPropertyChanged("Computers"); }
        }

        public enum Statuses
        {
            IsSearching,
            Ready,
            NothingFound
        }

        private Statuses _status;
        public Statuses Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }

        public List<string> NetworkScanMethodList { get; set; } = new List<string> { "NetApi32", "ArpTable" };

        public ViewModel MainWindowDataContext { get; set; }

        CancellationTokenSource CancelSource { get; set; }


        public AddComputersVM()
        {
            Process = new RelayCommand(ProcessCommand, Process_CanExecute);
            SelectAll = new RelayCommand(SelectAllCommand);
            SelectNone = new RelayCommand(SelectNoneCommand);
            ScanLan = new RelayCommand(ScanLanCommand);
            Cancel = new RelayCommand(CancelCommand);
            this.Computers = new ObservableCollection<ExpandoObject>();
            MainWindowDataContext = ViewModel.Instance;

            //ViewModel.Instance.Workers.PropertyChanged += Workers_PropertyChanged;
            PropertyChangedEventManager.AddHandler(ViewModel.Instance.Workers, Workers_PropertyChanged, string.Empty);
        }

        private void CancelCommand(object obj)
        {
            if (CancelSource != null)
                CancelSource.Cancel();
        }

        private async void Workers_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NetworkScanMethod")
            {
                Debug.WriteLine($"$$$ Workers_PropertyChanged {e.PropertyName} {MainWindowDataContext.Workers.NetworkScanMethod}");
                Status = Statuses.IsSearching;
                await ScanLan_Wrapper();
            }
                
        }

        private async void ScanLanCommand(object obj)
        {
            await ScanLan_Wrapper();
        }

        private async Task ScanLan_Wrapper()
        {
            try
            {
                await ScanLan_AsyncBody();
            }
            catch (Exception ex)
            {
                if (ex.Message != "The operation was canceled.")
                    MessageBox.Show("An unknown error occurred while searching.", "Nothing found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ScanLan_AsyncBody()
        {
            Status = AddComputersVM.Statuses.IsSearching;
            List<string> list = null;
            string networkScanMethod = ViewModel.Instance.Workers.NetworkScanMethod;
            CancelSource = new CancellationTokenSource();
            var token = CancelSource.Token;
            switch (networkScanMethod)
            {
                case "NetApi32":
                    list = await Task.Run(() => 
                    (new NetworkBrowser()).GetNetworkComputers()
                    ).WithCancellation(token);
                    break;
                case "ArpTable":
                    list = await Task.Run(async () => {
                        var ipList = ArpTable.GetIpAddresses();
                        if (ipList == null)
                            return null;
                        var hostNames = await ArpTable.GetHostNames(ipList).WithCancellation(token);
                        return hostNames;
                    });
                    break;
            }
            if (list == null || list.Count == 0)
            {
                Status = AddComputersVM.Statuses.NothingFound;

                string message = "No LAN computer was detected.";
                string alternative = " Or you may select an alternative network scan method, this will re-initiate the search.";
                switch (networkScanMethod)
                {
                    case "NetApi32":
                        message += " Check your cable and network settings and run \"net view\" CMD command to see if Windows actually sees anything on the network."
                            + alternative;
                        break;
                    case "ArpTable":
                        message += " Check your cable and network settings and run \"arp -a\" CMD command to see if Windows actually detects any LAN IP."
                            + alternative;
                        break;
                    default:
                        message = "Incorrect setting detected. Workers.cfg must contain either \"GetComputerNamesMethod\":\"NetApi32\" or \"GetComputerNamesMethod\":\"ArpTable\".";
                        break;
                }

                await Task.Delay(100);
                MessageBox.Show(message, "Nothing found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Computers.Clear();
            foreach (var entry in list)
            {
                dynamic pc = new ExpandoObject();
                pc.Name = entry as string;
                pc.IsSelected = false;
                Computers.Add(pc);
            }
            Status = Statuses.Ready;
        }


        private void SelectNoneCommand(object obj)
        {
            foreach (dynamic pc in Computers)
                pc.IsSelected = false;
        }

        private void SelectAllCommand(object obj)
        {
            foreach (dynamic pc in Computers)
                pc.IsSelected = true;
        }

        private bool Process_CanExecute(object obj)
        {
            return Computers.Cast<dynamic>().FirstOrDefault(x => (bool)x.IsSelected) != null ? true : false;
        }

        private void ProcessCommand(object obj)
        {
            
        }

        public void Dispose()
        {
            PropertyChangedEventManager.RemoveHandler(ViewModel.Instance.Workers, Workers_PropertyChanged, string.Empty);
        }
    }

    public partial class AddComputers : Window
    {
        public AddComputers()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var listBox = Helpers.FindAncestor((DependencyObject)sender, typeof(ListBox), 1) as ListBox;
            listBox.SelectedItem = ((CheckBox)e.OriginalSource).DataContext;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
