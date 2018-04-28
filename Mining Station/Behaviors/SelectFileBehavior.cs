using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Mining_Station
{
    public class SelectFileBehavior : Behavior<ContextMenu>
    {
        MenuItem SelectFile;
        ICommand EditPathBuffer;
        MenuItem OpenInExplorerMenu;
        ICommand OpenInExplorerBuffer;

        public ICommand EditPath
        {
            get { return (ICommand)GetValue(EditPathProperty); }
            set { SetValue(EditPathProperty, value); }
        }

        public static readonly DependencyProperty EditPathProperty =
            DependencyProperty.Register("EditPath", typeof(ICommand), typeof(SelectFileBehavior), new PropertyMetadata(null, OnEditPathChanged));

        private static void OnEditPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SelectFileBehavior;
            if (control.EditPath == null)
                return;
            else control.EditPathBuffer = control.EditPath;
        }

        public ICommand OpenInExplorer
        {
            get { return (ICommand)GetValue(OpenInExplorerProperty); }
            set { SetValue(OpenInExplorerProperty, value); }
        }

        public static readonly DependencyProperty OpenInExplorerProperty =
            DependencyProperty.Register("OpenInExplorer", typeof(ICommand), typeof(SelectFileBehavior), new PropertyMetadata(null, OnOpenInExplorerChanged));

        private static void OnOpenInExplorerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SelectFileBehavior;
            if (control.OpenInExplorer == null)
                return;
            else control.OpenInExplorerBuffer = control.OpenInExplorer;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectFile == null)
            {
                SelectFile = new MenuItem { Header = "Select File" };
                SelectFile.Click += SelectFile_Click; ;
                this.AssociatedObject.Items.Add(SelectFile);
                OpenInExplorerMenu = new MenuItem { Header = "Open In Explorer" };
                OpenInExplorerMenu.Click += OpenInExplorerMenu_Click;
                this.AssociatedObject.Items.Add(OpenInExplorerMenu);
            }

            var dataGrid = this.AssociatedObject.PlacementTarget as DataGrid;
            if (dataGrid == null)
            {
                Debug.WriteLine("dataGrid == null");
                return;
            }

            var column = dataGrid.CurrentColumn;
            Debug.WriteLine($"AssociatedObject_Loaded {column?.Header}");
            if (column != null && column.Header.ToString() == "Path")
            {

                SelectFile.Visibility = Visibility.Visible;
                OpenInExplorerMenu.Visibility = Visibility.Visible;
            }
            else
            {
                SelectFile.Visibility = Visibility.Collapsed;
                OpenInExplorerMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenInExplorerMenu_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var dataGrid = this.AssociatedObject.PlacementTarget as DataGrid;
            if (dataGrid == null) return;

            Debug.WriteLine("");
            OpenInExplorerBuffer.Execute(dataGrid.CurrentCell.Item);
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var dataGrid = this.AssociatedObject.PlacementTarget as DataGrid;
            if (dataGrid == null) return;

            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Debug.WriteLine(openFileDialog.FileName);
                EditPathBuffer.Execute((dataGrid.CurrentCell.Item, openFileDialog.FileName));
            }
        }
    }

    public static class DataGridExtensions
    {
        public static DataGridRow GetSelectedRow(this DataGrid grid)
        {
            return (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
        }

        private static DataGridRow GetRow(this DataGrid grid, int index)
        {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // May be virtualized, bring into view and try again.
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column)
        {
            if (row != null)
            {
                DataGridCellsPresenter presenter = Helpers.GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null)
                {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = Helpers.GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }
    }
}
