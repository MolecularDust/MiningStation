using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace Mining_Station
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon() { Visible = true };
        public MainWindow()
        {
            this.Title = Constants.AppName + " : " + Environment.MachineName;
            InitializeComponent();
            DataContext = new ViewModel();

            using (Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/icon.ico")).Stream)
                notifyIcon.Icon = new System.Drawing.Icon(iconStream);

            var contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem { Text = "Show" });
            contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem { Text = "Exit" });
            contextMenu.MenuItems[0].Click += NotifyIcon_Click;
            contextMenu.MenuItems[1].Click += ExitMenu_Click;

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Click += NotifyIcon_Click;

        }

        // 'Exit' tray menu
        private void ExitMenu_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        // 'Show' tray menu
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
            this.WindowState = WindowState.Normal;

        }

        // Remove tray icon upon app close
        private void TheMainWindow_Closed(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
        }

        // Hide window to tray
        protected override void OnStateChanged(EventArgs e)
        {
            var minimized = (this.WindowState == WindowState.Minimized);
            this.ShowInTaskbar = !minimized;
            if (minimized)
                this.Hide();
            else
                this.Show();

            base.OnStateChanged(e);
        }

        // Adds the ability to scroll parent control using mouse wheel while the cursor is inside a datagrid
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var dobj = e.OriginalSource as DependencyObject;
            if (dobj != null)
            {
                if (Helpers.DetectAncestorOfBadTypes(dobj, new List<Type> { typeof(NumericUpDown), typeof(OxyPlot.Wpf.PlotView) }))
                    return;
            }
            ScrollViewer scv = (ScrollViewer)sender;
            if (scv == null) return;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        // Sets cursor back to normal after long layout update
        private void ItemsControlWorkers_LayoutUpdated(object sender, EventArgs e)
        {
            Helpers.MouseCursorNormal();
        }

        // Coin textbox edit functionality in offline mode
        private void textBoxEdit_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Dispatcher.BeginInvoke((Action)(() => tb.SelectAll()));
        }

        // Sets normal mouse cursor upon historical charts load
        private void TableOrChartsContent_LayoutUpdated(object sender, EventArgs e)
        {
            Helpers.MouseCursorNormal();
        }
    }
}
