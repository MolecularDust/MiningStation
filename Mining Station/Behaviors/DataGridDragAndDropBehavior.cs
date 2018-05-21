using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using static Mining_Station.Workers;

namespace Mining_Station
{
    public class DataGridDragAndDropBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty MoveCommandProperty =
            DependencyProperty.Register("MoveCommand", typeof(ICommand), typeof(DataGridDragAndDropBehavior), new UIPropertyMetadata(null));

        public ICommand MoveCommand
        {
            get { return (ICommand)GetValue(MoveCommandProperty); }
            set { SetValue(MoveCommandProperty, value); }
        }

        public static readonly DependencyProperty CopyCommandProperty =
            DependencyProperty.Register("CopyCommand", typeof(ICommand), typeof(DataGridDragAndDropBehavior), new UIPropertyMetadata(null));

        public ICommand CopyCommand
        {
            get { return (ICommand)GetValue(CopyCommandProperty); }
            set { SetValue(CopyCommandProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
            this.AssociatedObject.PreviewMouseMove += AssociatedObject_PreviewMouseMove;
            this.AssociatedObject.DragOver += AssociatedObject_DragOver;
            this.AssociatedObject.Drop += AssociatedObject_Drop;
            this.AssociatedObject.DragLeave += AssociatedObject_DragLeave;
            this.AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            this.AssociatedObject.BeginningEdit += AssociatedObject_BeginningEdit;

        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
            this.AssociatedObject.PreviewMouseMove -= AssociatedObject_PreviewMouseMove;
            this.AssociatedObject.DragOver -= AssociatedObject_DragOver;
            this.AssociatedObject.Drop -= AssociatedObject_Drop;
            this.AssociatedObject.DragLeave -= AssociatedObject_DragLeave;
            this.AssociatedObject.DragEnter -= AssociatedObject_DragEnter;
            this.AssociatedObject.BeginningEdit -= AssociatedObject_BeginningEdit;
        }

        // Variables
        private bool isEditing = false;
        private bool isDragging = false;
        private bool initializing = false;
        private DragDropEffects effect;
        private int sourceIndex;
        private CoinTable sourceItem;
        Point StartPoint;
        List<Type> noGoCallerTypes1 = new List<Type>() { typeof(ComboBox), typeof(Button), typeof(CheckBox), typeof(TextBox) };
        List<Type> noGoCallerTypes2 = new List<Type>() { typeof(ComboBox) };
        const string rowName = "CoinTableRow";

        private ToolTip floatingTip = new ToolTip { HasDropShadow = false, Placement = PlacementMode.Relative };

        private void AssociatedObject_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            isEditing = true;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            var currentRow = FindRow(e.OriginalSource, rowName, noGoCallerTypes2);
            if (currentRow == null)
                return;
            var currentRowIndex = currentRow.GetIndex();
            var itemToCopy = e.Data.GetData(typeof(CoinTableCopy)) as CoinTableCopy;
            if (itemToCopy == null)
                return;
            itemToCopy.DestinationWorker = (Worker)((DataGrid)sender).DataContext;
            itemToCopy.DestinationCoinTableIndex = currentRowIndex;
            if (effect == DragDropEffects.Move)
            {
                if (MoveCommand != null)
                    MoveCommand.Execute(itemToCopy);
            }
            if (effect == DragDropEffects.Copy)
            {
                if (CopyCommand != null)
                    CopyCommand.Execute(itemToCopy);
            }
            var dg = ((DataGrid)sender);
            dg.SelectedIndex = currentRowIndex;
            dg.Focus();
            floatingTip.IsOpen = false;
        }

        DataGridRow SourceRowCoin = null;

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow sourceRow = null;
            var row = Helpers.FindAncestorWithExclusion((DependencyObject)e.OriginalSource, typeof(DataGridRow), 1, noGoCallerTypes1) as DataGridRow;
            if (row == null)
                return;
            if (row.DataContext is Coin)
            {
                SourceRowCoin = row;
                sourceRow = FindRow(row, rowName, noGoCallerTypes1);
            }
            else sourceRow = row;
            if (sourceRow == null)
                return;
            isEditing = sourceRow.IsEditing;
            if (isEditing || (SourceRowCoin != null && SourceRowCoin.IsEditing))
                return;
            StartPoint = e.GetPosition(null);
            sourceIndex = sourceRow.GetIndex();
            sourceItem = (CoinTable)sourceRow.Item;
            initializing = true;
        }

        private void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            initializing = false;
        }

        private void AssociatedObject_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && initializing && !isEditing && sourceItem != null)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = StartPoint - mousePos;
                if (!(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                    return;
                if ((SourceRowCoin != null && SourceRowCoin.IsEditing))
                    return;
                DragDropEffects dragdropeffects;
                floatingTip.Content = sourceItem.FullName;
                floatingTip.IsOpen = true;
                UpdateFloatingTipPosition(e.GetPosition((UIElement)sender));
                dragdropeffects = DragDropEffects.Copy | DragDropEffects.Move;
                var sourceObject = new CoinTableCopy { SourceWorker = (Worker)((DataGrid)sender).DataContext, SourceCoinTable = sourceItem, SourceCoinTableIndex = sourceIndex };
                DragDrop.DoDragDrop(this.AssociatedObject, sourceObject, dragdropeffects);
                floatingTip.IsOpen = false;
                sourceItem = null;
                isDragging = false;
            }
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            isDragging = true;
            UpdateFloatingTipPosition(e.GetPosition((UIElement)sender));
            var currentRow = FindRow(e.OriginalSource, rowName, noGoCallerTypes1);
            if (currentRow != null)
                ((DataGrid)sender).SelectedItem = currentRow.Item;
            if (e.KeyStates == DragDropKeyStates.LeftMouseButton)
                effect = DragDropEffects.Move;
            if (e.KeyStates == (DragDropKeyStates.ControlKey | DragDropKeyStates.LeftMouseButton))
                effect = DragDropEffects.Copy;
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            floatingTip.PlacementTarget = (UIElement)sender;
            isDragging = true;
            if (e.Data.GetDataPresent(typeof(CoinTableCopy)))
            {
                var item = e.Data.GetData(typeof(CoinTableCopy)) as CoinTableCopy;
                if (item != null)
                    floatingTip.Content = item.SourceCoinTable.FullName;
                floatingTip.IsOpen = true;
            }
        }

        private void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            isDragging = false;
            // Delay tooltip turn-off so that it does not flicker while dragging
            this.AssociatedObject.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (isDragging == false) floatingTip.IsOpen = false;
            }));
        }

        private void UpdateFloatingTipPosition(Point p)
        {
            floatingTip.HorizontalOffset = p.X;
            floatingTip.VerticalOffset = p.Y + 30;
        }

        // Search ancestor tree for DataGridRow that has DataContext of CoinTable. Adandon search if parent is Button or ComboBox  
        private DataGridRow FindRow(object source, string name, List<Type> exclusions)
        {
            var dObject = source as DependencyObject;
            if (dObject == null)
                return null;
            var row = (DataGridRow)Helpers.FindAncestorByTag(dObject, typeof(DataGridRow), 2, name, exclusions);
            if (row != null)
                return row;
            else return null;
        }

        //public static DependencyObject FindAncestorWithExclusionSpecial(DependencyObject obj, Type type, int maxMatchLevels, List<Type> noGoTypes)
        //{
        //    int currentLevel = 0;
        //    while (obj != null)
        //    {
        //        var currentType = obj.GetType();
        //        if (currentType == typeof(TextBox) && ((TextBox)obj).IsReadOnly == false)
        //            return null;
        //        if (noGoTypes.Contains(currentType))
        //            return null;
        //        if (currentType == type)
        //        {
        //            currentLevel++;
        //            if (currentLevel <= maxMatchLevels)
        //                return obj;
        //            else return null;
        //        }
        //        obj = VisualTreeHelper.GetParent(obj);
        //    };
        //    return null;
        //}
    }
}
