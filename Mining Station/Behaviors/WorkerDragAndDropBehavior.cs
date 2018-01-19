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
using static Mining_Station.Workers;

namespace Mining_Station
{
    public class WorkerDragAndDropBehavior : Behavior<TextBlock>
    {
        public static readonly DependencyProperty MoveCommandProperty =
            DependencyProperty.Register("MoveCommand", typeof(ICommand), typeof(WorkerDragAndDropBehavior), new UIPropertyMetadata(null));

        public ICommand MoveCommand
        {
            get { return (ICommand)GetValue(MoveCommandProperty); }
            set { SetValue(MoveCommandProperty, value); }
        }

        public static readonly DependencyProperty CopyCommandProperty =
            DependencyProperty.Register("CopyCommand", typeof(ICommand), typeof(WorkerDragAndDropBehavior), new UIPropertyMetadata(null));

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
        }

        // Variables
        private bool isDragging = false;
        private bool initializing = false;
        private DragDropEffects effect;
        private Worker sourceItem;

        private ToolTip floatingTip = new ToolTip { HasDropShadow = false, Placement = PlacementMode.Relative };

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TextBlock))
                return;
            sourceItem = ((TextBlock)sender).DataContext as Worker;
            if (sourceItem == null)
                return;
            initializing = true;
        }

        private void AssociatedObject_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && initializing && sourceItem != null)
            {
                DragDropEffects dragdropeffects;
                floatingTip.IsOpen = true;
                UpdateFloatingTipPosition(e.GetPosition((UIElement)sender));
                dragdropeffects = DragDropEffects.Copy | DragDropEffects.Move;
                DragDrop.DoDragDrop(this.AssociatedObject, new WorkerCopy { SourceWorker = sourceItem }, dragdropeffects);
                floatingTip.IsOpen = false;
                sourceItem = null;
                isDragging = false;
            }
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            isDragging = true;
            var data = e.Data.GetData(typeof(WorkerCopy)) as WorkerCopy;
            if (data == null)
                return;
            floatingTip.Content = data.SourceWorker.Name;
            UpdateFloatingTipPosition(e.GetPosition((UIElement)sender));
            if (e.KeyStates == DragDropKeyStates.LeftMouseButton)
                effect = DragDropEffects.Move;
            if (e.KeyStates == (DragDropKeyStates.ControlKey | DragDropKeyStates.LeftMouseButton))
                effect = DragDropEffects.Copy;
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(typeof(WorkerCopy)) as WorkerCopy;
            if (data == null)
                return;
            floatingTip.PlacementTarget = (UIElement)sender;
            floatingTip.IsOpen = true;
            isDragging = true;
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

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            if (!(sender is TextBlock))
                return;
            var currentItem = ((TextBlock)sender).DataContext as Worker;
            if (currentItem == null)
                return;
            var itemToCopy = e.Data.GetData(typeof(WorkerCopy)) as WorkerCopy;
            if (itemToCopy == null)
                return;
            itemToCopy.DestinationWorker = (Worker)((TextBlock)sender).DataContext;
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
            floatingTip.IsOpen = false;
        }

        private void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            initializing = false;
        }

        private void UpdateFloatingTipPosition(Point p)
        {
            floatingTip.HorizontalOffset = p.X;
            floatingTip.VerticalOffset = p.Y + 30;
        }
    }
}
