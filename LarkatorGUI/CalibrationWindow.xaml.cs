using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LarkatorGUI
{
    /// <summary>
    /// Interaction logic for CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        private bool dragging;
        private Bounds boundsMult;
        private Calibration calibration;

        public CalibrationWindow(Calibration calibration)
        {
            DataContext = calibration;
            this.calibration = calibration;

            InitializeComponent();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var img = (Image)sender;
            var pos = Mouse.GetPosition(img);
            pos.X /= img.ActualWidth;
            pos.Y /= img.ActualHeight;

            dragging = true;
            boundsMult = new Bounds
            {
                X1 = pos.X < 0.5 ? 1 : 0,
                X2 = pos.X > 0.5 ? 1 : 0,
                Y1 = pos.Y < 0.5 ? 1 : 0,
                Y2 = pos.Y > 0.5 ? 1 : 0
            };

            img.CaptureMouse();
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!dragging)
                return;
            var img = (Image)sender;
            img.ReleaseMouseCapture();
            dragging = false;
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging)
                return;

            var img = (Image)sender;
            var pos = Mouse.GetPosition(img);

            calibration.Bounds.X1 = UpdateBound(calibration.Bounds.X1, boundsMult.X1, pos.X);
            calibration.Bounds.X2 = UpdateBound(calibration.Bounds.X2, boundsMult.X2, pos.X);
            calibration.Bounds.Y1 = UpdateBound(calibration.Bounds.Y1, boundsMult.Y1, pos.Y);
            calibration.Bounds.Y2 = UpdateBound(calibration.Bounds.Y2, boundsMult.Y2, pos.Y);

            calibration.Recalculate();
        }

        private double UpdateBound(double cal, double mult, double x)
        {
            return cal * (1 - mult) + x * mult;
        }

        private void Filename_TextChanged(object sender, TextChangedEventArgs e)
        {
            calibration.Image = $"imgs\\map_{((TextBox)sender).Text}.jpg";
        }

        private void UpdateOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            calibration.Recalculate();
        }
    }

    public class ExampleCalibration : Calibration
    {
        public ExampleCalibration()
        {
            Bounds = new Bounds { X1 = 100, X2 = 800, Y1 = 150, Y2 = 750 };
        }
    }

    public class Calibration : DependencyObject
    {
        public Calibration()
        {
            LatDivisor = LonDivisor = 8000;
            LatOffset = LonOffset = 50;
        }

        public Bounds Bounds
        {
            get { return (Bounds)GetValue(BoundsProperty); }
            set { SetValue(BoundsProperty, value); }
        }

        public string Filename
        {
            get { return (string)GetValue(FilenameProperty); }
            set { SetValue(FilenameProperty, value); }
        }

        public string Image
        {
            get { return (string)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public double PixelOffsetX
        {
            get { return (double)GetValue(PixelOffsetXProperty); }
            set { SetValue(PixelOffsetXProperty, value); }
        }

        public double PixelOffsetY
        {
            get { return (double)GetValue(PixelOffsetYProperty); }
            set { SetValue(PixelOffsetYProperty, value); }
        }

        public double PixelScaleX
        {
            get { return (double)GetValue(PixelScaleXProperty); }
            set { SetValue(PixelScaleXProperty, value); }
        }

        public double PixelScaleY
        {
            get { return (double)GetValue(PixelScaleYProperty); }
            set { SetValue(PixelScaleYProperty, value); }
        }

        public double LatOffset
        {
            get { return (double)GetValue(LatOffsetProperty); }
            set { SetValue(LatOffsetProperty, value); }
        }

        public double LonOffset
        {
            get { return (double)GetValue(LonOffsetProperty); }
            set { SetValue(LonOffsetProperty, value); }
        }

        public double LatDivisor
        {
            get { return (double)GetValue(LatDivisorProperty); }
            set { SetValue(LatDivisorProperty, value); }
        }

        public double LonDivisor
        {
            get { return (double)GetValue(LonDivisorProperty); }
            set { SetValue(LonDivisorProperty, value); }
        }

        public string Output
        {
            get { return (string)GetValue(OutputProperty); }
            set { SetValue(OutputProperty, value); }
        }

        public static readonly DependencyProperty OutputProperty =
            DependencyProperty.Register("Output", typeof(string), typeof(Calibration), new PropertyMetadata(""));

        public static readonly DependencyProperty LonDivisorProperty =
            DependencyProperty.Register("LonDivisor", typeof(double), typeof(Calibration), new PropertyMetadata(0.0));

        public static readonly DependencyProperty LatDivisorProperty =
            DependencyProperty.Register("LatDivisor", typeof(double), typeof(Calibration), new PropertyMetadata(0.0));

        public static readonly DependencyProperty LonOffsetProperty =
            DependencyProperty.Register("LonOffset", typeof(double), typeof(Calibration), new PropertyMetadata(0.0));

        public static readonly DependencyProperty LatOffsetProperty =
            DependencyProperty.Register("LatOffset", typeof(double), typeof(Calibration), new PropertyMetadata(0.0));

        public static readonly DependencyProperty PixelScaleYProperty =
            DependencyProperty.Register("PixelScaleY", typeof(double), typeof(Calibration), new PropertyMetadata(0.0));

        public static readonly DependencyProperty PixelScaleXProperty =
            DependencyProperty.Register("PixelScaleX", typeof(double), typeof(Calibration), new PropertyMetadata(0.0));

        public static readonly DependencyProperty PixelOffsetYProperty =
            DependencyProperty.Register("PixelOffsetY", typeof(double), typeof(Calibration), new PropertyMetadata(0.0));

        public static readonly DependencyProperty PixelOffsetXProperty =
            DependencyProperty.Register("PixelOffsetX", typeof(double), typeof(Calibration), new PropertyMetadata(0.0));

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(string), typeof(Calibration), new PropertyMetadata(""));

        public static readonly DependencyProperty FilenameProperty =
            DependencyProperty.Register("Filename", typeof(string), typeof(Calibration), new PropertyMetadata(""));

        public static readonly DependencyProperty BoundsProperty =
            DependencyProperty.Register("Bounds", typeof(Bounds), typeof(Calibration), new PropertyMetadata(null));

        public void Recalculate()
        {
            if (Bounds == null)
                return;

            var minX = Math.Min(Bounds.X1, Bounds.X2);
            var minY = Math.Min(Bounds.Y1, Bounds.Y2);
            var maxX = Math.Max(Bounds.X1, Bounds.X2);
            var maxY = Math.Max(Bounds.Y1, Bounds.Y2);

            PixelScaleX = (maxX - minX) / 80.0;
            PixelScaleY = (maxY - minY) / 80.0;

            PixelOffsetX = minX - PixelScaleX * 10;
            PixelOffsetY = minY - PixelScaleY * 10;

            Output = RecreateOutput();
        }

        private string RecreateOutput()
        {
            return $"{{\n" +
                $"  \"Filename\": \"{Filename}\",\n" +
                $"  \"LatOffset\": {LatOffset},\n" +
                $"  \"LatDivisor\": {LatDivisor},\n" +
                $"  \"LonOffset\": {LonOffset},\n" +
                $"  \"LonDivisor\": {LonDivisor},\n" +
                $"  \"PixelOffsetX\": {PixelOffsetX},\n" +
                $"  \"PixelOffsetY\": {PixelOffsetY},\n" +
                $"  \"PixelScaleX\": {PixelScaleX},\n" +
                $"  \"PixelScaleY\": {PixelScaleY}\n" +
                $"}},";
        }
    }

    [DebuggerDisplay("X={X1}-{X2}, Y={Y1}-{Y2}")]
    public class Bounds : DependencyObject
    {
        public double X1
        {
            get { return (double)GetValue(X1Property); }
            set { SetValue(X1Property, value); }
        }

        public double X2
        {
            get { return (double)GetValue(X2Property); }
            set { SetValue(X2Property, value); }
        }

        public double Y1
        {
            get { return (double)GetValue(Y1Property); }
            set { SetValue(Y1Property, value); }
        }

        public double Y2
        {
            get { return (double)GetValue(Y2Property); }
            set { SetValue(Y2Property, value); }
        }

        public static readonly DependencyProperty Y2Property =
            DependencyProperty.Register("Y2", typeof(double), typeof(Bounds), new PropertyMetadata(900.0));

        public static readonly DependencyProperty Y1Property =
            DependencyProperty.Register("Y1", typeof(double), typeof(Bounds), new PropertyMetadata(100.0));

        public static readonly DependencyProperty X2Property =
            DependencyProperty.Register("X2", typeof(double), typeof(Bounds), new PropertyMetadata(900.0));

        public static readonly DependencyProperty X1Property =
            DependencyProperty.Register("X1", typeof(double), typeof(Bounds), new PropertyMetadata(100.0));
    }
}
