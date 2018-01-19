using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public class ProcessEntry : NotifyObject
    {
        private string _process;
        public string Process
        {
            get { return _process; }
            set { _process = value; OnPropertyChanged("Process"); }
        }

        public ProcessEntry() {}

        public ProcessEntry(string process)
        {
            this.Process = process;
        }
    }

    public class EditKillListVM : NotifyObject
    {
        public ObservableCollection<ProcessEntry> KillList { get; set; }
        private List<Worker> WorkerList { get; set; }

        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged("IsBusy"); }
        }

        private bool rawLines;

        public RelayCommand RemoveRow { get; private set; }
        public RelayCommand InsertRow { get; private set; }
        public RelayCommand Clear { get; private set; }
        public RelayCommand CopyAll { get; private set; }
        public RelayCommand PasteAsNewRow { get; private set; }
        public RelayCommand PasteAcrossRows { get; private set; }
        public RelayCommand ExtractFromWorkers { get; private set; }
        public RelayCommand ExtractRawLinesFromWorkers { get; private set; }

        public EditKillListVM() { }

        public EditKillListVM( IList<string> killList, IList<Worker> workerList)
        {
            RemoveRow = new RelayCommand (RemoveRowCommand, Row_CanExecute);
            InsertRow = new RelayCommand (InsertRowCommand, Row_CanExecute);
            Clear = new RelayCommand(ClearCommand);
            CopyAll = new RelayCommand(CopyAllCommand, Row_CanExecute);
            PasteAsNewRow = new RelayCommand(PasteAsNewRowCommand, Paste_CanExecute);
            PasteAcrossRows = new RelayCommand(PasteAcrossRowsCommand, Paste_CanExecute);
            ExtractFromWorkers = new RelayCommand(ExtractFromWorkersCommand, ExtractFromWorkers_CanExecute);
            ExtractRawLinesFromWorkers = new RelayCommand(ExtractRawLinesFromWorkersCommand, ExtractFromWorkers_CanExecute);

            this.KillList = new ObservableCollection<ProcessEntry>();
            foreach (var entry in killList)
                this.KillList.Add(new ProcessEntry(entry));
            this.WorkerList = new List<Worker>();
            foreach (var w in workerList)
                this.WorkerList.Add(w.Clone());
        }

        private void ExtractRawLinesFromWorkersCommand(object obj)
        {
            rawLines = true;
            ExtractFromWorkersCommand(obj);
            rawLines = false;
        }

        private bool ExtractFromWorkers_CanExecute(object obj)
        {
            return (WorkerList == null || WorkerList.Count == 0) ? false : true;
        }

        private void ExtractFromWorkersCommand(object obj)
        {
            IsBusy = true;

            List<string> tempList = new List<string>();
            foreach (var w in WorkerList)
            {
                foreach (var ct in w.CoinList)
                {
                    var ext = System.IO.Path.GetExtension(ct.Path);
                    if (ext == ".exe" || ext == ".bat" | ext == ".ps1" || ext == ".py")
                        tempList.Add(ct.Path);
                }
            }
            if (tempList.Count == 0)
            {
                MessageBox.Show("No .exe or script entries have been found in Workers.", "Nothing found", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var allFound = tempList.Distinct().ToList();
            tempList.Clear();
            Regex regex = new Regex(@"[^\s\\]\w+\.exe($|\W)");
            foreach (var path in allFound)
            {
                var ext = System.IO.Path.GetExtension(path);
                if (ext == ".exe")
                {
                    if (!rawLines)
                        KillList.Add(new ProcessEntry(System.IO.Path.GetFileName(path)));
                    else KillList.Add(new ProcessEntry(path));
                }
                else
                {
                    string [] lines = null;
                    if (System.IO.File.Exists(path))
                         lines = System.IO.File.ReadAllLines(path);
                    if (lines == null || lines.Length == 0)
                        continue;
                    foreach (var line in lines)
                    {
                        var matches = regex.Matches(line);
                        if (matches == null || matches.Count == 0)
                            continue;
                        if (!rawLines)
                        {
                            foreach (var match in matches)
                            {
                                string fileName = match.ToString();
                                fileName = fileName.Trim();
                                fileName = Regex.Replace(fileName, @"[\\""']", string.Empty);
                                tempList.Add(fileName);
                            }
                        }
                        else tempList.Add(line);
                    }
                }
            }
            var clearList = tempList.Distinct().ToList();
            foreach (var entry in clearList)
                KillList.Add(new ProcessEntry(entry));

            IsBusy = false;
        }

        private void CopyAllCommand(object obj)
        {
            var clipBoard = new StringBuilder();
            foreach (var entry in KillList)
                clipBoard.AppendLine(entry.Process);
            Clipboard.SetText(clipBoard.ToString());
        }

        private void ClearCommand(object obj)
        {
            KillList.Clear();
        }

        private bool Row_CanExecute(object obj)
        {
            if (KillList.Count > 0)
                return true;
            else return false;
        }

        private bool Paste_CanExecute(object obj)
        {
            return Clipboard.ContainsText();
        }

        private void PasteAcrossRowsCommand(object obj)
        {
            var clipBoard = GetClipboardContent();
            if (clipBoard == null)
                return;
            var split = clipBoard.Trim().Split(new Char[] { ',', ';', '\n' });
            var item = obj as ProcessEntry;
            if (item == null)
            {
                foreach (var entry in split)
                    KillList.Add(new ProcessEntry(entry.Trim()));
                return;
            }
            var index = KillList.IndexOf(item);
            if (index == -1)
                index = KillList.Count;
            for (int i = 1; i <= split.Length; i++)
            {
                KillList.Insert(index + i, new ProcessEntry(split[i-1].Trim()));
            }
        }

        private void PasteAsNewRowCommand(object obj)
        {
            var clipBoard = GetClipboardContent().Trim();
            if (clipBoard == null)
                return;
            var process = new ProcessEntry(clipBoard);
            var item = obj as ProcessEntry;
            if (item == null)
            {
                KillList.Add(process);
                return;
            }
            var index = KillList.IndexOf(item);
            if (index != -1)
                KillList.Insert(++index, process);
        }

        private void InsertRowCommand(object obj)
        {
            var item = obj as ProcessEntry;
            if (item == null)
                return;
            var index = KillList.IndexOf(item);
            var process = new ProcessEntry();
            if (index == -1)
                KillList.Insert(++index, process);
        }

        private void RemoveRowCommand(object obj)
        {
            var item = obj as ProcessEntry;
            if (item == null)
                return;
            KillList.Remove(item);
        }

        private string GetClipboardContent()
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
                return Clipboard.GetText(TextDataFormat.Text);
            else return null;
        }
    }

    public partial class EditKillListDialog : Window
    {
        public EditKillListDialog()
        {
            InitializeComponent();
            DataContext = new EditKillListVM();
            this.Owner = Application.Current.MainWindow;
        }

        private void ButtonSubmit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
