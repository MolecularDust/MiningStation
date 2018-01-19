using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Mining_Station
{
    public class ShowColumnsMenuBehavior : Behavior<ContextMenu>
    {
        MenuItem ShowColumns;

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.AssociatedObject.Items.Contains(ShowColumns)) return;

            var dataGrid = this.AssociatedObject.PlacementTarget as DataGrid;
            if (dataGrid == null) return;

            ShowColumns = new MenuItem { Header = "Show Columns" };
            foreach (var column in dataGrid.Columns)
            {
                if (column.Header == null)
                    continue;
                var menuItem = new MenuItem();
                menuItem.Header = column.Header;
                menuItem.IsChecked = column.Visibility == Visibility.Visible ? true : false;
                menuItem.IsCheckable = true;
                menuItem.Checked += MenuItem_Checked;
                menuItem.Unchecked += MenuItem_Checked;
                ShowColumns.Items.Add(menuItem);
            }
            this.AssociatedObject.Items.Add(ShowColumns);
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var parent = menuItem.Parent;
            while (!(parent is ContextMenu))
            {
                parent = (FrameworkElement)LogicalTreeHelper.GetParent(parent);
                if (parent is null) return;
            }
            var contextMenu = parent as ContextMenu;
            if (contextMenu == null) return;
            var datagrid = contextMenu.PlacementTarget as DataGrid;
            if (datagrid == null) return;
            var column = datagrid.Columns.FirstOrDefault(x => x.Header.ToString() == menuItem.Header.ToString());
            if (column == null) return;
            column.Visibility = menuItem.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
