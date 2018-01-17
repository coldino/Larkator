using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LarkatorGUI
{
    /// <summary>
    /// Thanks to Ben Watson, from http://www.philosophicalgeek.com/2009/11/16/a-wpf-numeric-entry-control/
    /// </summary>
    public partial class NumericEntryControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
            typeof(Int32), typeof(NumericEntryControl),
            new PropertyMetadata(0));

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue",
            typeof(Int32), typeof(NumericEntryControl),
            new PropertyMetadata(100));

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue",
                    typeof(Int32), typeof(NumericEntryControl),
                    new PropertyMetadata(0));

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment",
            typeof(Int32), typeof(NumericEntryControl),
            new PropertyMetadata(1));

        public static readonly DependencyProperty LargeIncrementProperty = DependencyProperty.Register("LargeIncrement",
            typeof(Int32), typeof(NumericEntryControl),
            new PropertyMetadata(5));

        private int _previousValue = 0;
        private DispatcherTimer _timer = new DispatcherTimer();
        private static int _delayRate = SystemParameters.KeyboardDelay;
        private static int _repeatSpeed = Math.Max(1, SystemParameters.KeyboardSpeed);

        private bool _isIncrementing = false;

        public Int32 Value
        {
            get
            {
                return (Int32)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public Int32 MaxValue
        {
            get
            {
                return (Int32)GetValue(MaxValueProperty);
            }
            set
            {
                SetValue(MaxValueProperty, value);
            }
        }

        public Int32 MinValue
        {
            get
            {
                return (Int32)GetValue(MinValueProperty);
            }
            set
            {
                SetValue(MinValueProperty, value);
            }
        }

        public Int32 Increment
        {
            get
            {
                return (Int32)GetValue(IncrementProperty);
            }
            set
            {
                SetValue(IncrementProperty, value);
            }
        }

        public Int32 LargeIncrement
        {
            get
            {
                return (Int32)GetValue(LargeIncrementProperty);
            }
            set
            {
                SetValue(LargeIncrementProperty, value);
            }
        }

        public NumericEntryControl()
        {
            InitializeComponent();

            _textbox.PreviewTextInput += new TextCompositionEventHandler(_textbox_PreviewTextInput);
            _textbox.PreviewKeyDown += new KeyEventHandler(_textbox_PreviewKeyDown);
            _textbox.GotFocus += new RoutedEventHandler(_textbox_GotFocus);
            _textbox.LostFocus += new RoutedEventHandler(_textbox_LostFocus);

            buttonIncrement.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(buttonIncrement_PreviewMouseLeftButtonDown);
            buttonIncrement.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(buttonIncrement_PreviewMouseLeftButtonUp);

            buttonDecrement.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(buttonDecrement_PreviewMouseLeftButtonDown);
            buttonDecrement.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(buttonDecrement_PreviewMouseLeftButtonUp);

            _timer.Tick += new EventHandler(_timer_Tick);
        }

        void buttonIncrement_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            buttonIncrement.CaptureMouse();
            _timer.Interval = TimeSpan.FromMilliseconds(_delayRate * 250);
            _timer.Start();

            _isIncrementing = true;
        }

        void buttonIncrement_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _timer.Stop();
            buttonIncrement.ReleaseMouseCapture();
            IncrementValue();
        }

        void buttonDecrement_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            buttonDecrement.CaptureMouse();
            _timer.Interval = TimeSpan.FromMilliseconds(_delayRate * 250);
            _timer.Start();

            _isIncrementing = false;
        }

        void buttonDecrement_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _timer.Stop();
            buttonDecrement.ReleaseMouseCapture();
            DecrementValue();
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            if (_isIncrementing)
            {
                IncrementValue();
            }
            else
            {
                DecrementValue();
            }
            _timer.Interval = TimeSpan.FromMilliseconds(1000.0 / _repeatSpeed);

        }

        void _textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            _previousValue = Value;
        }

        void _textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Int32.TryParse(_textbox.Text, out int newValue))
            {
                if (newValue > MaxValue)
                {
                    newValue = MaxValue;
                }
                else if (newValue < MinValue)
                {
                    newValue = MinValue;
                }
            }
            else
            {
                newValue = _previousValue;
            }
            _textbox.Text = newValue.ToString();
        }

        void _textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsNumericInput(e.Text))
            {
                e.Handled = true;
                return;
            }
        }

        private bool IsNumericInput(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        void _textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    IncrementValue();
                    break;
                case Key.Down:
                    DecrementValue();
                    break;
                case Key.PageUp:
                    Value = Math.Min(Value + LargeIncrement, MaxValue);
                    break;
                case Key.PageDown:
                    Value = Math.Max(Value - LargeIncrement, MinValue);
                    break;
                default:
                    //do nothing
                    break;
            }
        }

        private void IncrementValue()
        {
            Value = Math.Min(Value + Increment, MaxValue);
        }

        private void DecrementValue()
        {
            Value = Math.Max(Value - Increment, MinValue);
        }
    }
}
