using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mining_Station
{
    public class WindowCloser
    {
        public static bool GetCloseDialog(DependencyObject obj)
        {
            return (bool)obj.GetValue(CloseDialogProperty);
        }

        public static void SetCloseDialog(DependencyObject obj, bool value)
        {
            obj.SetValue(CloseDialogProperty, value);
        }

        public static readonly DependencyProperty CloseDialogProperty =
            DependencyProperty.RegisterAttached("CloseDialog", typeof(bool), typeof(WindowCloser), new PropertyMetadata(false, OnCloseDialogChanged));

        private static void OnCloseDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window == null)
                return;
            if ((bool)e.NewValue == true)
                window.DialogResult = true;
        }
    }
}
