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
    // Based on https://github.com/T-Alex/WPFControls

    [DefaultProperty("Value"), DefaultEvent("ValueChanged")]
    public class NumericUpDown : Control
    {
        #region Fields

        private const decimal DefaultMinimum = 0M;
        private const decimal DefaultMaximum = 100M;
        private const decimal DefaultIncrement = 1M;

        private DispatcherTimer WheelTimer;
        private DispatcherTimer RepeatTimer;
        private int RepeatCounter;
        private Button IncreaseButton;
        private Button DecreaseButton;
        private TextBox _textBox;
        private decimal _lastValue;
        private string _lastText;

        private bool IsEditidingEnabled = false;

        private const NumberStyles NumberStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register("TextColor", typeof(Brush), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(null, OnTextColorChanged));

        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(false));

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        public static readonly DependencyProperty DynamicIncrementProperty = DependencyProperty.Register("DynamicIncrement", typeof(bool), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(false));

        public bool DynamicIncrement
        {
            get { return (bool)GetValue(DynamicIncrementProperty); }
            set { SetValue(DynamicIncrementProperty, value); }
        }

        public decimal DynamicIncrementMinimum
        {
            get { return (decimal)GetValue(DynamicIncrementMinimumProperty); }
            set { SetValue(DynamicIncrementMinimumProperty, value); }
        }

        public static readonly DependencyProperty DynamicIncrementMinimumProperty =
            DependencyProperty.Register("DynamicIncrementMinimum", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(0.00000001M));

        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register("StringFormat", typeof(string), typeof(NumericUpDown),
            new PropertyMetadata(string.Empty));

        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(DefaultMinimum, OnValueChanged, CoerceValue));

        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty FinalValueProperty = DependencyProperty.Register("FinalValue", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(DefaultMinimum, OnFinalValueChanged));

        public decimal FinalValue
        {
            get { return (decimal)GetValue(FinalValueProperty); }
            set { SetValue(FinalValueProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(DefaultMinimum, OnMinimumChanged));


        public decimal Minimum
        {
            get { return (decimal)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(DefaultMaximum, OnMaximumChanged));

        public decimal Maximum
        {
            get { return (decimal)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(decimal), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(DefaultIncrement, null, CoerceIncrement), ValidateIncrement);

        public decimal Increment
        {
            get { return (decimal)GetValue(IncrementProperty); }

            set { SetValue(IncrementProperty, value); }
        }

        public static readonly DependencyProperty InterceptArrowKeysProperty = DependencyProperty.Register("InterceptArrowKeys", typeof(bool), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(true));

        public bool InterceptArrowKeys
        {
            get { return (bool)GetValue(InterceptArrowKeysProperty); }
            set { SetValue(InterceptArrowKeysProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty NumberFormatInfoProperty = DependencyProperty.Register("NumberFormatInfo", typeof(NumberFormatInfo), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(NumberFormatInfo.CurrentInfo.Clone(), OnNumberFormatInfoChanged));

        public NumberFormatInfo NumberFormatInfo
        {
            get { return (NumberFormatInfo)GetValue(NumberFormatInfoProperty); }
            set { SetValue(NumberFormatInfoProperty, value); }
        }

        public static readonly RoutedEvent ValueChangedEvent;

        #endregion

        #region Events

        public event RoutedPropertyChangedEventHandler<decimal> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }

            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion

        #region Constructors

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));

            ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<decimal>), typeof(NumericUpDown));

            //Listen to MouseLeftButtonDown event to determine if NumericUpDown should move focus to itself
            EventManager.RegisterClassHandler(typeof(NumericUpDown),
                Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
        }

        private static void OnTextColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown control = (NumericUpDown)obj;
            control._textBox.Foreground = control.TextColor;
        }

        public NumericUpDown()
            : base()
        {
            _lastText = String.Empty;
        }

        #endregion

        #region Methods

        #region Statics

        private static decimal GetDynamicIncrement(decimal value)
        {
            return (decimal)Math.Pow(10, -Math.Ceiling(-Math.Log10((double)value)));
        }

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown control = (NumericUpDown)obj;

            decimal oldValue = (decimal)args.OldValue;
            decimal newValue = (decimal)args.NewValue;

            RoutedPropertyChangedEventArgs<decimal> e = new RoutedPropertyChangedEventArgs<decimal>(
                oldValue, newValue, ValueChangedEvent);
            control.OnValueChanged(e);
            control.UpdateText();
        }

        private static void OnFinalValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown control = (NumericUpDown)obj;
            if (control.Value != control.FinalValue)
                control.Value = control.FinalValue;
            control.EndEditingEnabled();

            if (control.DynamicIncrement)
            {
                var newIncValue = GetDynamicIncrement(control.Value);
                if (newIncValue < control.DynamicIncrementMinimum)
                    control.Increment = control.DynamicIncrementMinimum;
                else control.Increment = newIncValue;
            }

            control.EndEditingEnabled();
            control.EndEdit();
        }

        private static void OnMinimumChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown control = element as NumericUpDown;
            if (control.Minimum > control.Value)
            {
                control.Value = control.Minimum;
            }

            if (control.Minimum > control.FinalValue)
            {
                control.FinalValue = control.Minimum;
            }

            element.CoerceValue(MaximumProperty);
            element.CoerceValue(ValueProperty);
        }

        private static void OnMaximumChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            element.CoerceValue(ValueProperty);
        }

        private static void OnIsReadOnlyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown control = element as NumericUpDown;
            bool readOnly = (bool)args.NewValue;

            if (readOnly != control._textBox.IsReadOnly)
            {
                control._textBox.IsReadOnly = readOnly;
            }
        }

        private static void OnNumberFormatInfoChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown control = element as NumericUpDown;
            control.UpdateText();
        }

        private static object CoerceValue(DependencyObject element, object value)
        {
            decimal newValue = (decimal)value;
            NumericUpDown control = (NumericUpDown)element;
            newValue = Math.Max(control.Minimum, Math.Min(control.Maximum, newValue));
            return newValue;
        }

        private static object CoerceIncrement(DependencyObject d, object baseValue)
        {
            decimal newValue = (decimal)baseValue;
            NumericUpDown control = (NumericUpDown)d;
            if (control.Increment < Decimal.MinValue)
                return Decimal.MinValue;
            else return newValue;
        }

        private static bool ValidateIncrement(object value)
        {
            return (decimal)value > 0;
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NumericUpDown control = (NumericUpDown)sender;
            // When someone click on a part in the NumericUpDown and it's not focusable
            // NumericUpDown needs to take the focus in order to process keyboard correctly
            if (!control.IsKeyboardFocusWithin)
            {
                e.Handled = control.Focus() || e.Handled;
            }
        }

        #endregion

        #region Dynamics

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

            if (_textBox != null)
            {
                _textBox.TextChanged -= new TextChangedEventHandler(OnTextBoxTextChanged);
                _textBox.PreviewKeyDown -= new KeyEventHandler(OnTextBoxPreviewKeyDown);
                _textBox.PreviewKeyUp -= new KeyEventHandler(OnTextBoxPreviewKeyUp);
            }

            _textBox = (TextBox)GetTemplateChild("TextBox");

            if (_textBox != null)
            {
                _textBox.TextChanged += new TextChangedEventHandler(OnTextBoxTextChanged);
                _textBox.PreviewKeyDown += new KeyEventHandler(OnTextBoxPreviewKeyDown);
                _textBox.PreviewKeyUp += new KeyEventHandler(OnTextBoxPreviewKeyUp);
                _textBox.IsReadOnly = false;
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

            UpdateText();
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


        private void BeginEditingEnabled()
        {
            if (!IsEditidingEnabled)
                IsEditidingEnabled = true;
        }

        private void EndEditingEnabled()
        {
            if (IsEditidingEnabled)
                IsEditidingEnabled = false;
        }

        private void RepeatButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BeginEditingEnabled();
            RepeatTimer.Start();
        }

        private void RepeatButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RepeatTimer.Stop();
            RepeatCounter = 0;
            if (FinalValue != Value)
                FinalValue = Value;
            EndEditingEnabled();
        }

        private void RepeatTimer_Tick(object sender, EventArgs e)
        {
            if (RepeatCounter == 0 || RepeatCounter * RepeatTimer.Interval.TotalMilliseconds > 250)
            {
                if (IncreaseButton.IsPressed)
                {
                    ParseText(); 
                    OnIncrease();
                }
                if (DecreaseButton.IsPressed)
                {
                    ParseText();
                    OnDecrease();
                }
            }
            RepeatCounter++;
        }

        private void WheelTimer_Tick(object sender, EventArgs e)
        {
            //Prevent timer from looping
            ((DispatcherTimer)sender).Stop();
            if (FinalValue != Value)
                FinalValue = Value;
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                OnGotFocus();
            }
            else
            {
                OnLostFocus();
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            WheelTimer.Stop();
            WheelTimer.Start();
            BeginEditingEnabled();

            base.OnMouseWheel(e);

            if (IsKeyboardFocusWithin)
            {
                if (e.Delta > 0)
                {
                    OnIncrease();
                }
                else
                {
                    OnDecrease();
                }
            }
            e.Handled = true;
        }

        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<decimal> args)
        {
            RaiseEvent(args);
        }

        protected virtual void OnIncrease()
        {
            if (DynamicIncrement)
            {
                Increment = GetDynamicIncrement(Value + Increment);
            }

            decimal newValue = Value + Increment;
            if (newValue <= Maximum)
            {
                if (newValue != _lastValue)
                {
                    BeginEdit();
                    Value = newValue;
                }
                    
            }
            else
            {
                if (Value != Maximum)
                {
                    BeginEdit();
                    Value = Maximum;
                }
            }

        }

        protected virtual void OnDecrease()
        {
            if (DynamicIncrement)
            {
                var decreasedValue = Value - Increment;
                if (decreasedValue < Increment)
                {
                    var oneTenth = Increment / 10;
                    if (oneTenth > DynamicIncrementMinimum)
                    {
                        Increment = oneTenth;
                        Value -= decreasedValue - (decreasedValue % (decimal)Increment);
                    }
                }
            }

            decimal newValue = Value - Increment;
            if (newValue >= Minimum)
            {
                if (newValue != _lastValue)
                {
                    BeginEdit();
                    Value = newValue;
                }
            }
            else
            {
                if (Value != Minimum)
                {
                    BeginEdit();
                    Value = Minimum;
                }
            }
        }

        private void OnGotFocus()
        {
            if (_textBox != null)
            {
                _textBox.Focus();
            }
        }

        private void OnLostFocus()
        {
            if (IsEditidingEnabled)
            {
                ParseText();
                EndEdit();
            }
                

        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsReadOnly)
            {
                _lastText = _textBox.Text;
                _lastValue = Value;
            }
        }

        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            BeginEditingEnabled();

            bool CtrlPressed = Keyboard.Modifiers == ModifierKeys.Control ? true : false;
            if (e.Key == Key.Z && CtrlPressed)
            {
                UpdateText();
                EndEditingEnabled();
                Keyboard.ClearFocus(); // First reverse view than clear focus so that OnLostFocus does not parse
                e.Handled = true;
                return;
            }

            switch (e.Key)
            {
                case Key.Up:
                    if (InterceptArrowKeys)
                    {
                        if (IsEditing)
                            ParseText();
                        OnIncrease();
                    }
                    break;

                case Key.Down:
                    if (InterceptArrowKeys)
                    {
                        if (IsEditing)
                            ParseText();
                        OnDecrease();
                    }
                    break;

                case Key.Return: // First clear focus so that OnLostFocus can parse view
                    Keyboard.ClearFocus();
                    EndEditingEnabled();
                    break;

                case Key.Escape: // First reverse view than clear focus so that OnLostFocus does not parse
                    UpdateText();
                    EndEditingEnabled();
                    Keyboard.ClearFocus();
                    break;
                default:
                    return;
            }

            e.Handled = true;
        }

        private void ParseText()
        {
            string text = _textBox.Text;
            decimal parsedValue = 0M;
            if (decimal.TryParse(text, NumberStyle, NumberFormatInfo, out parsedValue) && TestValue(parsedValue))
            {
                if (parsedValue != Value)
                {
                    BeginEdit();
                    Value = parsedValue;
                    FinalValue = parsedValue;
                }
                _lastText = text;
                _lastValue = parsedValue;
            }
            else ReturnPreviousInput();
        }

        private bool TestValue(decimal value)
        {
            if (Minimum <= value && value <= Maximum)
                return true;
            else return false;
        }

        private void OnTextBoxPreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (FinalValue != Value)
                        FinalValue = Value;
                    break;

                case Key.Down:
                    if (FinalValue != Value)
                        FinalValue = Value;
                    break;
                default:
                    return;
            }

            e.Handled = true;
        }

        internal void UpdateText()
        {
            NumberFormatInfo formatInfo = (NumberFormatInfo)NumberFormatInfo.Clone();
            formatInfo.NumberGroupSeparator = String.Empty;

            string formattedValue = Value.ToString(StringFormat, formatInfo);

            if (_textBox != null)
            {
                _lastText = formattedValue;
                _textBox.Text = formattedValue;
            }
        }

        private void ReturnPreviousInput()
        {
            int selectionLenght = _textBox.SelectionLength;
            int selectionStart = _textBox.SelectionStart;

            _textBox.Text = _lastText;
            _textBox.SelectionStart = (selectionStart == 0) ? 0 : (selectionStart - 1);
            _textBox.SelectionLength = selectionLenght;
        }

        #endregion

        #endregion
    }
}
