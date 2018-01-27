using Larkator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                if (cal == null || pos == null) return;

                tx.X = pos.Lon * cal.ScaleX + cal.OffsetX;
                tx.Y = pos.Lat * cal.ScaleY + cal.OffsetY;
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

        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
    }
}
