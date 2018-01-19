using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mining_Station
{
    /// <summary>
    /// Interaction logic for EditableCell.xaml
    /// </summary>
    public partial class EditableCell : UserControl
    {
        public EditableCell()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(EditableCell),
            new PropertyMetadata(""));

        public VerticalAlignment VerticalAlignmentInner
        {
            get { return (VerticalAlignment)GetValue(VerticalAlignmentInnerProperty); }
            set { SetValue(VerticalAlignmentInnerProperty, value); }
        }
        public static readonly DependencyProperty VerticalAlignmentInnerProperty =
            DependencyProperty.Register(
                "VerticalAlignmentInner",
                typeof(VerticalAlignment),
                typeof(EditableCell),
                new PropertyMetadata(VerticalAlignment.Center));

        public HorizontalAlignment HorizontalAlignmentInner
        {
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentInnerProperty); }
            set { SetValue(HorizontalAlignmentInnerProperty, value); }
        }
        public static readonly DependencyProperty HorizontalAlignmentInnerProperty =
            DependencyProperty.Register(
                "HorizontalAlignmentInner",
                typeof(HorizontalAlignment),
                typeof(EditableCell),
                new PropertyMetadata(HorizontalAlignment.Center));

        public Thickness MarginInner
        {
            get { return (Thickness)GetValue(MarginInnerProperty); }
            set { SetValue(MarginInnerProperty, value); }
        }
        public static readonly DependencyProperty MarginInnerProperty =
            DependencyProperty.Register(
                "MarginInner",
                typeof(Thickness),
                typeof(EditableCell),
                new PropertyMetadata(new Thickness()));

        public string TextBuffer
        {
            get { return (string)GetValue(TextBufferProperty); }
            set { SetValue(TextBufferProperty, value); }
        }

        public static readonly DependencyProperty TextBufferProperty =
            DependencyProperty.Register(
                "TextBuffer",
                typeof(string),
                typeof(EditableCell),
                new PropertyMetadata(string.Empty));



        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(
                "IsEditing",
                typeof(bool),
                typeof(EditableCell), 
                new PropertyMetadata(false));

        private int _clickCount;
        public int ClickCount
        {
            get { return _clickCount; }
            set { _clickCount = value; }
        }

        private bool Cancel;

        private void MainControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine($"PreviewMouseLeftButtonDown Text: {Text} Clicks: {ClickCount} IsEditing {IsEditing} Original Source: {e.OriginalSource}");
            Debug.WriteLine($"PreviewMouseLeftButtonDown IsMouseOver: {this.IsMouseOver} Directrly over: {Mouse.DirectlyOver == this} e.GetPosition(this): {e.GetPosition(this)}");
            if (this.InputHitTest(e.GetPosition(this)) == null)
            {
                this.ReleaseMouseCapture();
                ClickCount = 0;
                return;
            }

            if (ClickCount == 0)
                this.CaptureMouse();

            ClickCount++;
            

            if (ClickCount < 2 || IsEditing)
            {
                //if (!this.IsFocused && !IsEditing)
                //    this.Focus();
                return;
            }
            BeginEdit();
            e.Handled = true;
        }

        private void MainTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine($"PreviewKeyDown Text: {Text} Clicks: {ClickCount} IsEditing: {IsEditing}");
            if (e.Key == Key.Escape)
            {
                IsEditing = false;
                Cancel = true;
                e.Handled = true;
            }
        }

        private void BeginEdit()
        {
            Debug.WriteLine($"BeginEdit Text: {Text} Clicks: {ClickCount} IsEditing: {IsEditing}");
            this.TextBuffer = this.Text;
            IsEditing = true;
        }

        private void EndEdit()
        {
            Debug.WriteLine("EndEdit");
            if (TextBuffer != Text)
                Text = TextBuffer;
            IsEditing = false;
        }

        private void MainTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"MainTextBox_LostFocus Text: {Text} Clicks: {ClickCount} IsEditing: {IsEditing}");

            if (!Cancel)
            {
                if (IsEditing)
                    EndEdit();
                ClickCount = 0;
            }
            else
            {
                Keyboard.Focus(this);
            }
            
            e.Handled = true;
        }

        private void MainTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            //ClickCount = 0;

            Debug.WriteLine($"MainTextBox_Loaded Text: {Text} Clicks: {ClickCount} IsEditing: {IsEditing}");
            if (!IsEditing)
                return;
            var tb = ((TextBox)sender);
            tb.Focus();
            var charIndex = tb.GetCharacterIndexFromPoint(Mouse.GetPosition(tb), true);
            if (charIndex + 1 == TextBuffer.Length)
                tb.SelectAll();
            else tb.CaretIndex = charIndex;
            Debug.WriteLine("ReleaseMouseCapture");
            this.ReleaseMouseCapture();
        }

        private void MainTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"MainTextBlock_Loaded Text: {Text} Clicks: {ClickCount} IsEditing: {IsEditing}");
            if (!Cancel)
                return;

            // If Escape key was pressed
            //this.CaptureMouse();
            ClickCount = 1;
            Cancel = false;
            var tb = ((TextBlock)sender);
            tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
        }

        private void MainControl_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"MainControl_GotFocus Text: {Text} Clicks: {ClickCount} IsEditing: {IsEditing}");
        }

        private void MainControl_LostFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"MainControl_LostFocus Text: {Text} Clicks: {ClickCount} IsEditing: {IsEditing}");
            //if (!IsEditing)
            //    ClickCount = 0;
        }

        private void MainControl_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"MainControl_Loaded Text: {Text} Clicks: {ClickCount} IsEditing: {IsEditing}");
        }

        private void MainControl_LostMouseCapture(object sender, MouseEventArgs e)
        {
            Debug.WriteLine($"MainControl_LostMouseCapture Text: {Text} Clicks: {ClickCount} IsEditing: {IsEditing}");
            //ClickCount = 0;
            //this.ReleaseMouseCapture();
            //e.Handled = true;
        }

        private void MainControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"MainControl_Unloaded Text: {Text} Clicks: {ClickCount} IsEditing: {IsEditing}");
            this.ReleaseMouseCapture();
        }
    }
}
