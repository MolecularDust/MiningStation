using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mining_Station
{
    /// <summary>
    /// Interaction logic for WorkerTable.xaml
    /// </summary>
    public partial class WorkerTable : UserControl
    {
        public WorkerTable()
        {
            InitializeComponent();
        }

        // Sets wait cursor on Expander click
        private void ExpanderMenu_Click(object sender, RoutedEventArgs e)
        {
            Helpers.MouseCursorWait();
        }

        // Sets wait cursor on Expander click
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Helpers.MouseCursorWait();
        }

        // Ensures edit functionality for nested datagrids
        private void Coins_GotFocus(object sender, RoutedEventArgs e)
        {
            DatagridCommitEdit(sender);
        }

        // Ensures edit functionality for nested datagrids
        private void DatagridCommitEdit(object sender)
        {
            var dg = (DataGrid)Helpers.FindAncestor((DependencyObject)sender, typeof(DataGrid), 2);
            if (dg == null)
                return;
            IEditableCollectionView itemsView = dg.Items;
            if (itemsView.IsEditingItem)
                dg.CommitEdit();
        }

        // Ensures edit functionality for nested datagrids
        private void DataGridHashrate_GotFocus(object sender, RoutedEventArgs e)
        {
            var dg = (DataGrid)Helpers.FindAncestor((DependencyObject)sender, typeof(DataGrid), 2);
            Debug.WriteLine("ACTUAL WIDTH: " + dg.RowHeaderActualWidth);
            DatagridCommitEdit(sender);
        }

        // Resize Coin column width on text size changed
        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Debug.WriteLine($"TextBlock_SizeChanged {e.NewSize.Width}");
            var dg = Helpers.FindAncestor((DependencyObject)sender, typeof(DataGrid), 1) as DataGrid;
            if (dg == null)
                return;
            var nameColumn = dg.Columns.Where(x => (string)x.Header == "Coin").FirstOrDefault();
            if (nameColumn == null)
                return;
            nameColumn.Width = 0;
            nameColumn.Width = new DataGridLength();
        }

        // Focus cell on right mouse click for Select File add-on menu to work
        private void DataGridCoins_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cell = Helpers.FindAncestor((DependencyObject)e.OriginalSource, typeof(DataGridCell), 1) as DataGridCell;
            if (cell == null)
                return;
            cell.Focus();
            var dg = sender as DataGrid;
            if (dg == null)
                return;
            
            Debug.WriteLine($"DataGridCoins_PreviewMouseRightButtonDown {dg.CurrentColumn?.Header.ToString()}");
        }
    }

    public class NameAndSymbolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var name = values[0] as string;
            var symbol = values[1] as string;
            if (name == null || symbol == null)
                return string.Empty;
            return $"{name} ({symbol})";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (text == null)
                return null;
            var split = text.Split('(');
            return new object[] { split[0].Trim(), split[1].TrimEnd(')') };
        }
    }
}
