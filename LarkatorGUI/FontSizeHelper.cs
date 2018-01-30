using System;
using System.Windows;
using System.Windows.Controls;

namespace LarkatorGUI
{
    public sealed class FontSizeHelper
    {
        public static readonly DependencyProperty RelativeFontSizeProperty = DependencyProperty.RegisterAttached(
            "RelativeFontSize", typeof(double), typeof(FontSizeHelper), new PropertyMetadata(0.0, RelativeFontSizeChanged));

        public static double GetRelativeFontSize(DependencyObject d)
        {
            if (d == null) throw new ArgumentNullException(nameof(d), "in GetRelativeFontSize");

            return (double)d.GetValue(RelativeFontSizeProperty);
        }

        public static void SetRelativeFontSize(DependencyObject d, double value)
        {
            if (d == null) throw new ArgumentNullException(nameof(d), "in SetRelativeFontSize");

            d.SetValue(RelativeFontSizeProperty, value);
        }

        private static void RelativeFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null) throw new ArgumentNullException(nameof(d), "in RelativeFontSizeChanged");

            d.ClearValue(TextBlock.FontSizeProperty);
            var old = (double)d.GetValue(TextBlock.FontSizeProperty);
            d.SetValue(TextBlock.FontSizeProperty, Math.Max(old + (double)e.NewValue, 0));
        }
    }
}
