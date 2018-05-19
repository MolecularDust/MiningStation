using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    public partial class SelectWorkers : Window
    {
        public SelectWorkers()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            SelectAllNone(true);
        }

        private void SelectNone_Click(object sender, RoutedEventArgs e)
        {
            SelectAllNone(false);
        }

        private void SelectAllNone(bool query)
        {
            var vm = this.DataContext as SelectWorkersVM;
            foreach (var worker in vm.Workers)
                worker.Query = query;
        }
    }

    public class SelectWorkersVM : NotifyObject
    {
        public string Title { get; set; }
        public ObservableCollection<Worker> Workers { get; set; }
        public RelayCommand Ok { get; private set; }
        public RelayCommand Closing { get; private set; }
        public CancellationTokenSource CancelSource { get; set; }

        private string _buttonTitle;
        public string ButtonTitle
        {
            get { return _buttonTitle; }
            set { _buttonTitle = value; OnPropertyChanged("ButtonTitle"); }
        }

        private bool _closeDialog;
        public bool CloseDialog
        {
            get { return _closeDialog; }
            set { _closeDialog = value; OnPropertyChanged("CloseDialog"); }
        }

        private string _template;
        public string Template
        {
            get { return _template; }
            set { _template = value; OnPropertyChanged("Template"); }
        }

        private string _waitMessage;
        public string WaitMessage
        {
            get { return _waitMessage; }
            set { _waitMessage = value; OnPropertyChanged("WaitMessage"); }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set { _notes = value; OnPropertyChanged("Notes"); }
        }

        private Paragraph _newParagraph;
        public Paragraph NewParagraph
        {
            get { return _newParagraph; }
            set { _newParagraph = value; OnPropertyChanged("NewParagraph"); }
        }

        private bool _showNotes;
        public bool ShowNotes
        {
            get { return _showNotes; }
            set { _showNotes = value; OnPropertyChanged("ShowNotes"); }
        }

        private Paragraph paragraph;
        private bool updateFinished;

        enum Templates
        {
            Select,
            Wait,
            Report
        }

        public SelectWorkersVM(bool showNotes = false)
        {
            Ok = new RelayCommand(OkCommand, Ok_CanExecute);
            Closing = new RelayCommand(ClosingCommand);

            Template = Templates.Select.ToString();
            ShowNotes = showNotes;
        }

        private bool Ok_CanExecute(object obj)
        {
            var firstCheck = Workers.FirstOrDefault(x => x.Query);
            return firstCheck != null ? true : false;
        }

        private async void OkCommand(object obj)
        {
            if (ButtonTitle == "Update" && !updateFinished)
            {
                CancelSource = new CancellationTokenSource();
                var token = CancelSource.Token;
                WaitMessage = "Accessing whattomine.com...";
                Template = Templates.Wait.ToString();
                paragraph = new Paragraph();
                await UpdateCoins(token);
                updateFinished = true;
                Template = Templates.Report.ToString();
                await Task.Delay(100);
                NewParagraph = paragraph;
                ButtonTitle = "OK";
            }
            else
            {
                CloseDialog = true;
            }
        }

        private void ClosingCommand(object obj)
        {
            CancelSource.Cancel();
        }

        private async Task UpdateCoins(CancellationToken token)
        {
            var allWtmAlgos = await Algorithm.GetWtmData(token);
            if (allWtmAlgos == null)
            {
                paragraph.Inlines.Add(new Run("ERROR:").FontWeight(FontWeights.Bold).Color(Colors.Salmon));
                paragraph.Inlines.Add(new Run(" Failed to connect to whattomine.com."));
                return;
            }

            bool changeDetected = false;

            var reportDict = new Dictionary<string, List<string>>();
            WaitMessage = "Processing...";
            foreach (var worker in Workers)
            {
                if (worker.Query == false)
                    continue;
                var workerAlgos = Algorithm.GetWorkersAlgorithms(worker);
                foreach (var algo in workerAlgos)
                {
                    var algoWtm = allWtmAlgos.FirstOrDefault(x => x.Name == algo.Name);
                    if (algoWtm == null)
                        continue;
                    var difference = algoWtm.Coins.Where(x => x.Status == "Active" && algo.Coins.FirstOrDefault(y => y.Name == x.Name) == null);
                    if (difference == null || difference.Count() == 0)
                        continue;

                    if (changeDetected == false)
                    {
                        ViewModel.Instance.SaveUndoRedo(History.UndoOperationType.WorkersAll.ToString());
                        changeDetected = true;
                    }

                    foreach (var coinId in difference)
                    {
                        worker.CoinList.Add(new CoinTable(
                            initCoins: new ObservableCollection<Coin>()
                            {
                                new Coin()
                                {
                                    Name = coinId.Name,
                                    Symbol = coinId.Symbol,
                                    Algorithm = algo.Name,
                                    Hashrate = algo.Hashrate
                                }
                            },
                            initPower: algo.Power,
                            initFees: algo.Fees,
                            initSwitch: false,
                            initPath: null,
                            initArgs: null,
                            initNotes: Notes
                            ));

                        if (reportDict.ContainsKey(worker.Name))
                            reportDict[worker.Name].Add(coinId.Name);
                        else
                            reportDict.Add(worker.Name, new List<string> { coinId.Name });
                    }
                }
                if (reportDict.ContainsKey(worker.Name))
                {
                    paragraph.Inlines.Add(new Run($"{worker.Name}").FontWeight(FontWeights.Bold));
                    paragraph.Inlines.Add(new Run(" has been updated with: "));
                    paragraph.Inlines.Add(string.Join(", ", reportDict[worker.Name]) + ".\r\n");
                }
                else
                {
                    paragraph.Inlines.Add(new Run($"{worker.Name}").FontWeight(FontWeights.Bold));
                    paragraph.Inlines.Add(new Run(" is up to date.\r\n"));
                }
            }
        }
    }
}
