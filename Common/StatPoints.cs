using System;

namespace Larkator.Common
{
    public class StatPoints : IEquatable<StatPoints>
    {
        public int Health { get; set; }
        public int Stamina { get; set; }
        public int Weight { get; set; }
        public int Melee { get; set; }
        public int Speed { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as StatPoints);
        }

        public bool Equals(StatPoints other)
        {
            return other != null &&
                   Health == other.Health &&
                   Stamina == other.Stamina &&
                   Weight == other.Weight &&
                   Melee == other.Melee &&
                   Speed == other.Speed;
        }

        public override int GetHashCode()
        {
            var hashCode = -1014974063;
            hashCode = hashCode * -1521134295 + Health.GetHashCode();
            hashCode = hashCode * -1521134295 + Stamina.GetHashCode();
            hashCode = hashCode * -1521134295 + Weight.GetHashCode();
            hashCode = hashCode * -1521134295 + Melee.GetHashCode();
            hashCode = hashCode * -1521134295 + Speed.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"{Health}/{Stamina}/{Weight}/{Melee}/{Speed}";
        }

        public string ToString(bool fixedWidth = false)
        {
            if (fixedWidth)
                return $"{Health,2}/{Stamina,2}/{Weight,2}/{Melee,2}/{Speed,2}";
            else
                return ToString();
        }
    }
}