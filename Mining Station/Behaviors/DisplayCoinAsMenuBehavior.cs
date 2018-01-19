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
    public class DisplayCoinAsMenuBehavior : Behavior<MenuItem>
    {
        public string Option
        {
            get { return (string)GetValue(OptionProperty); }
            set { SetValue(OptionProperty, value); }
        }

        public static readonly DependencyProperty OptionProperty =
            DependencyProperty.Register("Option", typeof(string), typeof(DisplayCoinAsMenuBehavior), new PropertyMetadata(string.Empty, OnOptionChanged));

        private static void OnOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DisplayCoinAsMenuBehavior;
            foreach (var item in control.AssociatedObject.Items)
            {
                var menuItem = item as MenuItem;
                if ((string)menuItem.Header == control.Option)
                    menuItem.IsChecked = true;
                else menuItem.IsChecked = false;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            foreach (var item in this.AssociatedObject.Items)
            {
                var menuItem = item as MenuItem;
                menuItem.Click += MenuItem_Click;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var thisItem = sender as MenuItem;
            this.Option = (string)thisItem.Header;
        }
    }
}
