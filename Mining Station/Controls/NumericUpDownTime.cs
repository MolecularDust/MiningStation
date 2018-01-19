using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;

namespace Mining_Station
{
    public class NumericUpDownTime : Control
    {
        private const int DefaultMinimum = 0;
        private const int DefaultMaximum = 100;
        private const int DefaultIncrement = 1;

        private DispatcherTimer WheelTimer;
        private DispatcherTimer RepeatTimer;
        private int RepeatCounter;
        private Button IncreaseButton;
        private Button DecreaseButton;
        private TextBox _textBox;
        private TextBox _textBoxHour;
        private TextBox _textBoxMinute;
        private int _lastValue;
        private string _lastText;



        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register("IsEditing", typeof(bool), typeof(NumericUpDownTime), new PropertyMetadata(false));


        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register("StringFormat", typeof(string), typeof(NumericUpDownTime), new PropertyMetadata(string.Empty));

        public int Increment
        {
            get { return (int)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register("Increment", typeof(int), typeof(NumericUpDownTime), new PropertyMetadata(DefaultIncrement));

        public int ValueHour
        {
            get { return (int)GetValue(ValueHourProperty); }
            set { SetValue(ValueHourProperty, value); }
        }

        public static readonly DependencyProperty ValueHourProperty =
            DependencyProperty.Register("ValueHour", typeof(int), typeof(NumericUpDownTime), new PropertyMetadata(DefaultMinimum, OnValueHourChanged, CoerceValueHour));

        private static object CoerceValueHour(DependencyObject d, object baseValue)
        {
            var newValue = (int)baseValue;
            var control = (NumericUpDownTime)d;
            newValue = Math.Max(control.MinimumHour, Math.Min(control.MaximumHour, newValue));
            return newValue;
        }

        private static void OnValueHourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownTime control = (NumericUpDownTime)d;
            control.UpdateText(e.Property.Name);
        }

        public int FinalValueHour
        {
            get { return (int)GetValue(FinalValueHourProperty); }
            set { SetValue(FinalValueHourProperty, value); }
        }

        public static readonly DependencyProperty FinalValueHourProperty =
            DependencyProperty.Register("FinalValueHour", typeof(int), typeof(NumericUpDownTime), new PropertyMetadata(DefaultMinimum, OnFinalValueHourChanged));

        private static void OnFinalValueHourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownTime control = (NumericUpDownTime)d;
            control.UpdateText(e.Property.Name);
            if (control.ValueHour != control.FinalValueHour)
                control.ValueHour = control.FinalValueHour;
        }

        public int MinimumHour
        {
            get { return (int)GetValue(MinimumHourProperty); }
            set { SetValue(MinimumHourProperty, value); }
        }

        public static readonly DependencyProperty MinimumHourProperty =
            DependencyProperty.Register("MinimumHour", typeof(int), typeof(NumericUpDownTime), new PropertyMetadata(DefaultMinimum, OnMinimumHourChanged));

        private static void OnMinimumHourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownTime control = (NumericUpDownTime)d;
            if (control.FinalValueHour < control.MinimumHour)
                control.FinalValueHour = control.MinimumHour;
        }

        public int MaximumHour
        {
            get { return (int)GetValue(MaximumHourProperty); }
            set { SetValue(MaximumHourProperty, value); }
        }

        public static readonly DependencyProperty MaximumHourProperty =
            DependencyProperty.Register("MaximumHour", typeof(int), typeof(NumericUpDownTime), new PropertyMetadata(23));

        public int ValueMinute
        {
            get { return (int)GetValue(ValueMinuteProperty); }
            set { SetValue(ValueMinuteProperty, value); }
        }

        public static readonly DependencyProperty ValueMinuteProperty =
            DependencyProperty.Register("ValueMinute", typeof(int), typeof(NumericUpDownTime), new PropertyMetadata(DefaultMinimum, OnValueMinuteChanged, CoerceValueMinute));


        private static object CoerceValueMinute(DependencyObject d, object baseValue)
        {
            var newValue = (int)baseValue;
            var control = (NumericUpDownTime)d;
            newValue = Math.Max(control.MinimumMinute, Math.Min(control.MaximumMinute, newValue));
            return newValue;
        }

        private static void OnValueMinuteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownTime control = (NumericUpDownTime)d;
            control.UpdateText(e.Property.Name);
        }

        public int FinalValueMinute
        {
            get { return (int)GetValue(FinalValueMinuteProperty); }
            set { SetValue(FinalValueMinuteProperty, value); }
        }

        public static readonly DependencyProperty FinalValueMinuteProperty =
            DependencyProperty.Register("FinalValueMinute", typeof(int), typeof(NumericUpDownTime), new PropertyMetadata(DefaultMinimum, OnFinalValueMinuteChanged));

        private static void OnFinalValueMinuteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownTime control = (NumericUpDownTime)d;
            control.UpdateText(e.Property.Name);
            if (control.ValueMinute != control.FinalValueMinute)
                control.ValueMinute = control.FinalValueMinute;
        }

        public int MinimumMinute
        {
            get { return (int)GetValue(MinimumMinuteProperty); }
            set { SetValue(MinimumMinuteProperty, value); }
        }

        public static readonly DependencyProperty MinimumMinuteProperty =
            DependencyProperty.Register("MinimumMinute", typeof(int), typeof(NumericUpDownTime), new PropertyMetadata(DefaultMinimum, OnMinimumMinuteChanged));

        private static void OnMinimumMinuteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownTime control = (NumericUpDownTime)d;
            if (control.FinalValueMinute < control.MinimumMinute)
                control.FinalValueMinute = control.MinimumMinute;
        }

        public int MaximumMinute
        {
            get { return (int)GetValue(MaximumMinuteProperty); }
            set { SetValue(MaximumMinuteProperty, value); }
        }

        public static readonly DependencyProperty MaximumMinuteProperty =
            DependencyProperty.Register("MaximumMinute", typeof(int), typeof(NumericUpDownTime), new PropertyMetadata(59));


        static NumericUpDownTime()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDownTime), new FrameworkPropertyMetadata(typeof(NumericUpDownTime)));

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (IncreaseButton != null)
            {
                IncreaseButton.PreviewMouseLeftButtonDown -= RepeatButton_PreviewMouseLeftButtonDown;
                IncreaseButton.PreviewMouseLeftButtonUp -= RepeatButton_PreviewMouseLeftButtonUp;
            }

            IncreaseButton = (Button)GetTemplateChild("IncreaseButton");
            if (IncreaseButton != null)
            {
                IncreaseButton.PreviewMouseLeftButtonDown += RepeatButton_PreviewMouseLeftButtonDown;
                IncreaseButton.PreviewMouseLeftButtonUp += RepeatButton_PreviewMouseLeftButtonUp;
            }

            if (DecreaseButton != null)
            {
                DecreaseButton.PreviewMouseLeftButtonDown -= RepeatButton_PreviewMouseLeftButtonDown;
                DecreaseButton.PreviewMouseLeftButtonUp -= RepeatButton_PreviewMouseLeftButtonUp;
            }

            DecreaseButton = (Button)GetTemplateChild("DecreaseButton");
            if (DecreaseButton != null)
            {
                DecreaseButton.PreviewMouseLeftButtonDown += RepeatButton_PreviewMouseLeftButtonDown;
                DecreaseButton.PreviewMouseLeftButtonUp += RepeatButton_PreviewMouseLeftButtonUp;
            }

            if (_textBoxHour != null)
            {
                _textBoxHour.GotFocus -= _textBoxHour_GotFocus;
                _textBoxHour.LostFocus -= _anyTextBox_LostFocus;
                _textBoxHour.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
                _textBoxHour.PreviewKeyUp -= OnTextBoxPreviewKeyUp;
            }

            if (_textBoxMinute != null)
            {
                _textBoxMinute.GotFocus -= _textBoxMinute_GotFocus;
                _textBoxMinute.LostFocus -= _anyTextBox_LostFocus;
                _textBoxMinute.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
                _textBoxMinute.PreviewKeyUp -= OnTextBoxPreviewKeyUp;
            }

            _textBoxHour = (TextBox)GetTemplateChild("TextBoxHour");
            _textBoxMinute = (TextBox)GetTemplateChild("TextBoxMinute");

            if (_textBoxHour != null)
            {
                _textBoxHour.GotFocus += _textBoxHour_GotFocus;
                _textBoxHour.LostFocus += _anyTextBox_LostFocus;
                _textBoxHour.PreviewKeyDown += OnTextBoxPreviewKeyDown;
                _textBoxHour.PreviewKeyUp += OnTextBoxPreviewKeyUp;
            }

            if (_textBoxMinute != null)
            {
                _textBoxMinute.GotFocus += _textBoxMinute_GotFocus;
                _textBoxMinute.LostFocus += _anyTextBox_LostFocus;
                _textBoxMinute.PreviewKeyDown += OnTextBoxPreviewKeyDown;
                _textBoxMinute.PreviewKeyUp += OnTextBoxPreviewKeyUp;
            }

            if (WheelTimer != null)
                WheelTimer.Tick -= WheelTimer_Tick;
            WheelTimer = new DispatcherTimer();
            WheelTimer.Tick += WheelTimer_Tick;
            WheelTimer.Interval = TimeSpan.FromMilliseconds(75);

            if (RepeatTimer != null)
                RepeatTimer.Tick -= RepeatTimer_Tick;
            RepeatTimer = new DispatcherTimer(DispatcherPriority.Render);
            RepeatTimer.Tick += RepeatTimer_Tick;
            RepeatTimer.Interval = TimeSpan.FromMilliseconds(33);

            // Initialize Hour and Minute textboxes with respective values
            _textBox = _textBoxHour;
            UpdateText();
            _textBox = _textBoxMinute;
            UpdateText();
        }

        private void WheelTimer_Tick(object sender, EventArgs e)
        {
            //Prevent timer from looping
            ((DispatcherTimer)sender).Stop();
            if (_textBox == _textBoxHour && FinalValueHour != ValueHour)
                FinalValueHour = ValueHour;
            if (_textBox == _textBoxMinute && FinalValueMinute != ValueMinute)
                FinalValueMinute = ValueMinute;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (!_textBox.IsFocused)
                return;

            WheelTimer.Stop();
            WheelTimer.Start();
            BeginEdit();

            base.OnMouseWheel(e);
            if (e.Delta > 0)
            {
                OnIncrease();
            }
            else
            {
                OnDecrease();
            }
            e.Handled = true;
        }

        private void _anyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                ParseText();
                EndEdit();
            }
                
        }

        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            BeginEdit();

            bool CtrlPressed = Keyboard.Modifiers == ModifierKeys.Control ? true : false;
            if (e.Key == Key.Z && CtrlPressed)
            {
                UpdateText();
                EndEdit();
                Keyboard.ClearFocus(); // First reverse view than clear focus so that OnLostFocus does not parse
                e.Handled = true;
                return;
            }

            switch (e.Key)
            {
                case Key.Up:
                    if (IsEditing)
                        ParseText();
                    OnIncrease();
                    break;

                case Key.Down:
                    if (IsEditing)
                        ParseText();
                    OnDecrease();
                    break;

                case Key.Return: // First clear focus so that OnLostFocus can parse view
                    ParseText();
                    Keyboard.ClearFocus();
                    EndEdit();
                    break;

                case Key.Escape: // First reverse view than clear focus so that OnLostFocus does not parse
                    UpdateText();
                    EndEdit();
                    Keyboard.ClearFocus();
                    break;

                case Key.Tab:
                    if (_textBox == _textBoxHour)
                    {
                        _textBoxMinute.Focus();
                        e.Handled = true;
                        return;
                    }
                    if (_textBox == _textBoxMinute)
                    {
                        _textBoxHour.Focus();
                        e.Handled = true;
                        return;
                    }
                    e.Handled = true;
                    break;

                default:
                    return;
            }

            e.Handled = true;
        }

        private void OnTextBoxPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (_textBox == _textBoxHour && FinalValueHour != ValueHour)
                    FinalValueHour = ValueHour;
                if (_textBox == _textBoxMinute && FinalValueMinute != ValueMinute)
                    FinalValueMinute = ValueMinute;
            }
            else return;
            e.Handled = true;
        }

        private void BeginEdit()
        {
            if (!IsEditing)
                IsEditing = true;
        }

        private void EndEdit()
        {
            if (IsEditing)
                IsEditing = false;
        }

        private void RepeatTimer_Tick(object sender, EventArgs e)
        {
            if (RepeatCounter == 0 || RepeatCounter * RepeatTimer.Interval.TotalMilliseconds > 250)
            {
                //Debug.WriteLine(IncreaseButton.IsPressed + " " + DecreaseButton.IsPressed);
                if (IncreaseButton.IsPressed)
                    OnIncrease();
                if (DecreaseButton.IsPressed)
                    OnDecrease();
            }
            RepeatCounter++;
        }

        private void _textBoxMinute_GotFocus(object sender, RoutedEventArgs e)
        {
            _textBox = _textBoxMinute;
        }

        private void _textBoxHour_GotFocus(object sender, RoutedEventArgs e)
        {
            _textBox = _textBoxHour;
        }

        private void RepeatButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_textBoxHour.IsFocused || _textBoxMinute.IsFocused)
                this.Focus();
            BeginEdit();
            RepeatTimer.Start();
        }

        private void RepeatButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RepeatTimer.Stop();
            RepeatCounter = 0;
            if (_textBox == _textBoxHour)
                FinalValueHour = ValueHour;
            if (_textBox == _textBoxMinute)
                FinalValueMinute = ValueMinute;
            EndEdit();
        }

        private void OnIncrease()
        {
            if (_textBox == _textBoxHour)
                ValueHour += Increment;
            if (_textBox == _textBoxMinute)
                ValueMinute += Increment;
        }

        private void OnDecrease()
        {
            if (_textBox == _textBoxHour)
                ValueHour -= Increment;
            if (_textBox == _textBoxMinute)
                ValueMinute -= Increment;
        }

        private void UpdateText(string property)
        {
            if (property == "ValueHour" && _textBoxHour != null)
                _textBoxHour.Text = ValueHour.ToString(StringFormat);
            if (property == "FinalValueHour" && _textBoxHour != null)
                _textBoxHour.Text = FinalValueHour.ToString(StringFormat);
            if (property == "ValueMinute" && _textBoxMinute != null)
                _textBoxMinute.Text = ValueMinute.ToString(StringFormat);
            if (property == "FinalValueMinute" && _textBoxMinute != null)
                _textBoxMinute.Text = FinalValueMinute.ToString(StringFormat);

        }

        private void UpdateText()
        {
            string formattedValue = string.Empty;

            if (_textBox == _textBoxHour)
                formattedValue = ValueHour.ToString(StringFormat);
            if (_textBox == _textBoxMinute)
                formattedValue = ValueMinute.ToString(StringFormat);

            if (_textBox != null)
            {
                _lastText = formattedValue;
                _textBox.Text = formattedValue;
            }
        }

        private void ParseText()
        {

            string text = _textBox.Text;
            int parsedValue = 0;
            if (int.TryParse(text, out parsedValue))
            {
                if (_textBox == _textBoxHour )
                {
                    if (!TestValueHour(parsedValue))
                    {
                        UpdateText();
                        return;
                    }
                    ValueHour = parsedValue;
                    FinalValueHour = ValueHour;

                }
                if (_textBox == _textBoxMinute)
                {
                    if (!TestValueMinute(parsedValue))
                    {
                        UpdateText();
                        return;
                    }
                    ValueMinute = parsedValue;
                    FinalValueMinute = ValueMinute;
                }
                _lastText = text;
                _lastValue = parsedValue;
            }
            else UpdateText();
        }

        private bool TestValueHour(decimal value)
        {
            if (MinimumHour <= value && value <= MaximumHour)
                return true;
            else return false;
        }

        private bool TestValueMinute(decimal value)
        {
            if (MinimumMinute <= value && value <= MaximumMinute)
                return true;
            else return false;
        }
    }
}

