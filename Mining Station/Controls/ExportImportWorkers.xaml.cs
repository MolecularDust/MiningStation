using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// Interaction logic for ExportImportWorkers.xaml
    /// </summary>
    public partial class ExportImportWorkers : Window
    {
        public ExportImportWorkers()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
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
            var vm = this.DataContext as ExportImportWorkersVM;
            foreach (var worker in vm.Workers)
                worker.Query = query;
        }
    }

    public class ExportImportWorkersVM : NotifyObject
    {
        public string Title { get; set; }
        public ObservableCollection<Worker> Workers { get; set; }
        public RelayCommand Ok { get; private set; }

        public ExportImportWorkersVM()
        {
            Ok = new RelayCommand(OkCommand, Ok_CanExecute);
        }

        private bool Ok_CanExecute(object obj)
        {
            var firstCheck = Workers.FirstOrDefault(x => x.Query);
            return firstCheck != null ? true : false;
        }

        private void OkCommand(object obj)
        {

        }
    }

    public class TitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (str == null)
                str = "Export / Import";
            return str + " Workers";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
