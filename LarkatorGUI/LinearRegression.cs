using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarkatorGUI
{
    public static class LinearRegression
    {
        public static Tuple<double, double> Fit(double[] x, double[] y)
        {
            double mx = x.Sum() / x.Length;
            double my = y.Sum() / y.Length;

            double covariance = 0.0;
            double variance = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                double diff = x[i] - mx;
                covariance += diff * (y[i] - my);
                variance += diff * diff;
            }

            var b = covariance / variance;
            return new Tuple<double, double>(my - b * mx, b);
        }

        public static double RSquared(double[] generated, double[] expected)
        {
            int n = 0;

            double meanY = 0;
            double ssTot = 0;
            double ssRes = 0;

            for (int i = 0; i < generated.Length; i++)
            {
                double currentY = expected[i];
                double currentF = generated[i];

                double deltaY = currentY - meanY;
                double scaleDeltaY = deltaY / ++n;

                meanY += scaleDeltaY;
                ssTot += scaleDeltaY * deltaY * (n - 1);

                ssRes += (currentY - currentF) * (currentY - currentF);
            }

            return 1d - ssRes / ssTot;
        }
    }
}
