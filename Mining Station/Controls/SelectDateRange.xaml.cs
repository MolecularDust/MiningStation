using System;
using System.Collections.Generic;
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

    public class SelectDateRangeVM : NotifyObject
    {
        private DateTime _fromDate;
        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; OnPropertyChanged("FromDate"); }
        }

        private DateTime _toDate;
        public DateTime ToDate
        {
            get { return _toDate; }
            set { _toDate = value; OnPropertyChanged("ToDate"); }
        }

        private DateTime _minimumDate;
        public DateTime MinimumDate
        {
            get { return _minimumDate; }
            set { _minimumDate = value; OnPropertyChanged("MinimumDate"); }
        }

        private DateTime _maximumDate;
        public DateTime MaximumDate
        {
            get { return _maximumDate; }
            set { _maximumDate = value; OnPropertyChanged("MaximumDate"); }
        }

        public RelayCommand SelectAll { get; private set; }
        public RelayCommand SelectLastMonth { get; private set; }
        public RelayCommand SelectLastThreeMonths { get; private set; }
        public RelayCommand SelectLastSixMonths { get; private set; }
        public RelayCommand SelectLastYear { get; private set; }

        public SelectDateRangeVM(){}

        public SelectDateRangeVM(DateTime fromDate, DateTime toDate)
        {
            MinimumDate = fromDate;
            FromDate = fromDate;
            MaximumDate = toDate;
            ToDate = toDate;

            SelectAll = new RelayCommand(SelectAllCommand);
            SelectLastMonth = new RelayCommand(SelectLastMonthCommand, SelectLastMonth_CanExecute);
            SelectLastThreeMonths = new RelayCommand(SelectLastThreeMonthsCommand, SelectLastThreeMonths_CanExecute);
            SelectLastSixMonths = new RelayCommand(SelectLastSixMonthsCommand, SelectLastSixMonths_CanExecute);
            SelectLastYear = new RelayCommand(SelectLastYearCommand, SelectLastYear_CanExecute);
        }

        private bool SelectLastYear_CanExecute(object obj)
        {
            if (MaximumDate.AddYears(-1) >= MinimumDate)
                return true;
            else return false;
        }

        private void SelectLastYearCommand(object obj)
        {
            FromDate = MaximumDate.AddYears(-1);
            ToDate = MaximumDate;
        }

        private bool SelectLastSixMonths_CanExecute(object obj)
        {
            if (MaximumDate.AddMonths(-6) >= MinimumDate)
                return true;
            else return false;
        }

        private void SelectLastSixMonthsCommand(object obj)
        {
            FromDate = MaximumDate.AddMonths(-6);
            ToDate = MaximumDate;
        }

        private bool SelectLastThreeMonths_CanExecute(object obj)
        {
            if (MaximumDate.AddMonths(-3) >= MinimumDate)
                return true;
            else return false;
        }

        private void SelectLastThreeMonthsCommand(object obj)
        {
            FromDate = MaximumDate.AddMonths(-3);
            ToDate = MaximumDate;
        }

        private bool SelectLastMonth_CanExecute(object obj)
        {
            if (MaximumDate.AddMonths(-1) >= MinimumDate)
                return true;
            else return false;
        }

        private void SelectLastMonthCommand(object obj)
        {
            FromDate = MaximumDate.AddMonths(-1);
            ToDate = MaximumDate;
        }

        private void SelectAllCommand(object obj)
        {
            FromDate = MinimumDate;
            ToDate = MaximumDate;
        }
    }

    public partial class SelectDateRange : Window
    {
        public SelectDateRange()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
