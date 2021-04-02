using System;
using System.Windows;
using System.Windows.Controls;

namespace LarkatorGUI
{
    public sealed class FontSizeHelper
    {
        public static readonly DependencyProperty RelativeFontSizeProperty = DependencyProperty.RegisterAttached(
            "RelativeFontSize", typeof(double), typeof(FontSizeHelper), new PropertyMetadata(0.0, RelativeFontSizeChanged));

        public static double GetRelativeFontSize(DependencyObject do_grfs)
        {
            if (do_grfs == null)
                throw new ArgumentNullException(nameof(do_grfs), "in GetRelativeFontSize");

            return (double)do_grfs.GetValue(RelativeFontSizeProperty);
        }

        public static void SetRelativeFontSize(DependencyObject do_srfs, double value)
        {
            if (do_srfs == null)
                throw new ArgumentNullException(nameof(do_srfs), "in SetRelativeFontSize");

            do_srfs.SetValue(RelativeFontSizeProperty, value);
        }

        private static void RelativeFontSizeChanged(DependencyObject do_rsfc, DependencyPropertyChangedEventArgs e)
        {
            if (do_rsfc == null)
                throw new ArgumentNullException(nameof(do_rsfc), "in RelativeFontSizeChanged");

            do_rsfc.ClearValue(TextBlock.FontSizeProperty);
            var old = (double)do_rsfc.GetValue(TextBlock.FontSizeProperty);
            do_rsfc.SetValue(TextBlock.FontSizeProperty, Math.Max(old + (double)e.NewValue, 0));
        }
    }
}
