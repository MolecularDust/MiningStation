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
    class CoinColumnResizeBehavior : Behavior<DataGrid>
    {
        public string Option
        {
            get { return (string)GetValue(OptionProperty); }
            set { SetValue(OptionProperty, value); }
        }

        public static readonly DependencyProperty OptionProperty =
            DependencyProperty.Register("Option", typeof(string), typeof(CoinColumnResizeBehavior), new PropertyMetadata(string.Empty, OnOptionChanged));

        private static void OnOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CoinColumnResizeBehavior;
            var dg = control?.AssociatedObject as DataGrid;
            if (dg == null)
                return;
            //var coinColumn = dg.Columns.First(x => x.Header.ToString() == "Coin");
            //var cell = coinColumn.CellTemplate;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
        }
    }
}
