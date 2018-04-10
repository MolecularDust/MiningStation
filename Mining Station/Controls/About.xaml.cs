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
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string clipboardText = string.Empty;
            clipboardText = this.VersionTitle.Text + " " + this.VersionText.Text + "\r\n";
            clipboardText += this.ApplicationModeTitle.Text + " " + this.ApplicationModeText.Text + "\r\n";
            clipboardText += this.MinedCoinTitle.Text + " " + this.CoinText.Text;
            Clipboard.SetText(clipboardText);
        }

        private void AboutWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                MenuItem_Click(this, new RoutedEventArgs());
            }
        }
    }
}