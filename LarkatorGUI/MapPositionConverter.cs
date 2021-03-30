using Larkator.Common;

using System;
using System.Windows;
using System.Windows.Media;

namespace LarkatorGUI
{
    public class MapPositionConverter : DependencyObject
    {

        public static MapCalibration GetCalibration(DependencyObject obj)
        {
            return (MapCalibration)obj.GetValue(CalibrationProperty);
        }

        public static void SetCalibration(DependencyObject obj, MapCalibration value)
        {
            obj.SetValue(CalibrationProperty, value);
        }

        public static Position GetPosition(DependencyObject obj)
        {
            return (Position)obj.GetValue(PositionProperty);
        }

        public static void SetPosition(DependencyObject obj, Position value)
        {
            obj.SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.RegisterAttached("Position", typeof(Position), typeof(MapPositionConverter), new PropertyMetadata(null, OnChanged));

        public static readonly DependencyProperty CalibrationProperty =
            DependencyProperty.RegisterAttached("Calibration", typeof(MapCalibration), typeof(MapPositionConverter), new PropertyMetadata(null, OnChanged));


        public static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TranslateTransform tx)
            {
                var cal = GetCalibration(d);
                var pos = GetPosition(d);
                if (cal == null || pos == null)
                    return;

                tx.X = pos.Lon * cal.PixelScaleX + cal.PixelOffsetX;
                tx.Y = pos.Lat * cal.PixelScaleY + cal.PixelOffsetY;
            }
            else
            {
                throw new InvalidOperationException("MapPositionConverter should only be attached to a TranslateTransform");
            }
        }
    }

    public class MapCalibration
    {
        public string Filename { get; set; }

        public double PixelOffsetX { get; set; }
        public double PixelOffsetY { get; set; }

        public double PixelScaleX { get; set; }
        public double PixelScaleY { get; set; }

        public double LatOffset { get; set; }
        public double LonOffset { get; set; }

        public double LatDivisor { get; set; }
        public double LonDivisor { get; set; }
    }
}
