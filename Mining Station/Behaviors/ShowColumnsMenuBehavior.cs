using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Mining_Station
{
    public class ShowColumnsMenuBehavior : Behavior<ContextMenu>
    {
        MenuItem ShowColumns;
        MenuItem SortByMenu;
        MenuItem SecondarySortByMenu;
        string SecondarySortByParameter;

        public ICommand SortBy
        {
            get { return (ICommand)GetValue(SortByProperty); }
            set { SetValue(SortByProperty, value); }
        }

        public static readonly DependencyProperty SortByProperty =
            DependencyProperty.Register("SortBy", typeof(ICommand), typeof(ShowColumnsMenuBehavior), new PropertyMetadata(null));


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
            SortByMenu = new MenuItem { Header = "Sort By" };
            SecondarySortByMenu = new MenuItem { Header = "Secondary Sort By" };

            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                var column = dataGrid.Columns[i];
                if (column.Header == null)
                    continue;
                var showMenuItem = new MenuItem();
                showMenuItem.Header = column.Header;
                showMenuItem.IsChecked = column.Visibility == Visibility.Visible ? true : false;
                showMenuItem.IsCheckable = true;
                showMenuItem.Checked += ShowMenuItem_Checked;
                showMenuItem.Unchecked += ShowMenuItem_Checked;
                ShowColumns.Items.Add(showMenuItem);

                if (i == 0)
                {
                    foreach (var entry in new string[] { "Coin Name", "Coin Symbol" })
                    {
                        SortByMenuAdd(entry);
                        SecondarySortByMenuAdd(entry);
                    }
                }
                else
                {
                    SortByMenuAdd(column.Header.ToString());
                    SecondarySortByMenuAdd(column.Header.ToString());
                }
            }
            this.AssociatedObject.Items.Add(ShowColumns);
            SortByMenu.Items.Add(SecondarySortByMenu);
            this.AssociatedObject.Items.Add(SortByMenu);
        }

        private void SortByMenuAdd(string menuHeader)
        {
            var sortMenuItem = new MenuItem();
            sortMenuItem.Header = menuHeader;
            sortMenuItem.Click += SortMenuItem_Click;
            SortByMenu.Items.Add(sortMenuItem);
        }

        private void SecondarySortByMenuAdd(string menuHeader)
        {
            var secondaryItem = new MenuItem
            {
                Header = menuHeader,
                IsCheckable = true,
                IsChecked = false,
                StaysOpenOnClick = true
            };
            secondaryItem.Click += SecondaryItem_Clicked;
            SecondarySortByMenu.Items.Add(secondaryItem);
        }

        private void SecondaryItem_Clicked(object sender, RoutedEventArgs e)
        {
            var clickedItem = sender as MenuItem;
            if (clickedItem == null)
                return;
            SecondarySortByParameter = clickedItem.IsChecked ? clickedItem.Header.ToString() : null;
            foreach (var item in SecondarySortByMenu.Items)
            {
                var mi = item as MenuItem;
                if (mi == null)
                    continue;
                if (mi.Header != clickedItem.Header)
                    mi.IsChecked = false;
            }
        }

        private void SortMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var dg = AssociatedObject.PlacementTarget as DataGrid;
            if (menuItem == null || SortBy == null || dg == null)
                return;

            SortBy.Execute(new Tuple<object, object, string>(menuItem.Header, dg.DataContext, SecondarySortByParameter));
        }

        private void ShowMenuItem_Checked(object sender, RoutedEventArgs e)
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
