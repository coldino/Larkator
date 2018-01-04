using System;
using System.Collections.Generic;

namespace Larkator.Common
{
    public class Dino : IEquatable<Dino>
    {
        public UInt64? Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int BaseLevel { get; set; }
        public bool Female { get; set; }
        public Position Location { get; set; }
        public StatPoints WildLevels { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Dino);
        }

        public bool Equals(Dino other)
        {
            return other != null &&
                   EqualityComparer<ulong?>.Default.Equals(Id, other.Id) &&
                   Type == other.Type &&
                   Name == other.Name &&
                   BaseLevel == other.BaseLevel &&
                   Female == other.Female &&
                   EqualityComparer<Position>.Default.Equals(Location, other.Location) &&
                   EqualityComparer<StatPoints>.Default.Equals(WildLevels, other.WildLevels);
        }

        public override int GetHashCode()
        {
            var hashCode = 1606339474;
            hashCode = hashCode * -1521134295 + EqualityComparer<ulong?>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + BaseLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + Female.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Position>.Default.GetHashCode(Location);
            hashCode = hashCode * -1521134295 + EqualityComparer<StatPoints>.Default.GetHashCode(WildLevels);
            return hashCode;
        }
    }
}