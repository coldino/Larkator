using System;

namespace Larkator.Common
{
    public class Position : IEquatable<Position>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Position);
        }

        public bool Equals(Position other)
        {
            return other != null &&
                   X == other.X &&
                   Y == other.Y &&
                   Z == other.Z &&
                   Lat == other.Lat &&
                   Lon == other.Lon;
        }

        public override int GetHashCode()
        {
            var hashCode = -1898883508;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            hashCode = hashCode * -1521134295 + Lat.GetHashCode();
            hashCode = hashCode * -1521134295 + Lon.GetHashCode();
            return hashCode;
        }

        public string ToString(PositionFormat format)
        {
            return format == PositionFormat.LatLong ? $"{Lat:0.0} , {Lon:0.0}" : $"{X:0.00} , {Y:0.00} , {Z:0.00}";
        }
    }

    public enum PositionFormat
    {
        LatLong,
        XYZ,
    }
}