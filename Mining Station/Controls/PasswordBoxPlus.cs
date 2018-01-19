using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mining_Station
{
    public class PasswordBoxPlus : Control
    {
        public SecureString RealPassword
        {
            get { return (SecureString)GetValue(RealPasswordProperty); }
            set { SetValue(RealPasswordProperty, value); }
        }

        public static readonly DependencyProperty RealPasswordProperty =
            DependencyProperty.Register("RealPassword", typeof(SecureString), typeof(PasswordBoxPlus), new PropertyMetadata(new SecureString(), OnSecurePasswordChanged));

        private static void OnSecurePasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBoxPlus control = (PasswordBoxPlus)d;
            
            control.dummyPassword = control.RealPassword != null ? new string('#', control.RealPassword.Length) : string.Empty;
            if (control.PasswordBox != null)
                control.PasswordBox.Password = control.dummyPassword;
        }

        static PasswordBoxPlus()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PasswordBoxPlus), new FrameworkPropertyMetadata(typeof(PasswordBoxPlus)));
        }

        PasswordBox PasswordBox;
        string dummyPassword = string.Empty;
        bool IsEditing;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (PasswordBox != null)
            {
                PasswordBox.PreviewKeyDown -= PasswordBox_PreviewKeyDown;
                PasswordBox.GotFocus -= PasswordBox_GotFocus;
                PasswordBox.LostFocus -= PasswordBox_LostFocus;
            }

            PasswordBox = (PasswordBox)GetTemplateChild("PasswordBox");
            PasswordBox.Password = dummyPassword;

            if (PasswordBox != null)
            {
                PasswordBox.PreviewKeyDown += PasswordBox_PreviewKeyDown;
                PasswordBox.GotFocus += PasswordBox_GotFocus;
                PasswordBox.LostFocus += PasswordBox_LostFocus;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                dummyPassword = RealPassword != null ? new string('#', RealPassword.Length) : string.Empty;
                RealPassword = PasswordBox.SecurePassword;
            }
            IsEditing = false;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!IsEditing)
            {
                PasswordBox.Password = string.Empty;
                IsEditing = true;
            }
                
        }

        private void PasswordBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsEditing)
                IsEditing = true;

            switch (e.Key)
            {
                case Key.Return:
                    this.Focus();
                    break;
                case Key.Escape:
                    PasswordBox.Password = dummyPassword;
                    IsEditing = false;
                    this.Focus();
                    break;
            }
        }
    }
}
