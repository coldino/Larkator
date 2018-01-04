using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larkator.Common
{
    public class SearchResult : IEquatable<SearchResult>
    {
        public string Species { get; set; }
        public int Level { get; set; }
        public bool Female { get; set; }
        public Position Location { get; set; }

        public string Tooltip { get { return $"{Species} {(Female ? "F" : "M")}{Level} {Location.Lat}, {Location.Lon}"; } }

        public override bool Equals(object obj)
        {
            return Equals(obj as SearchResult);
        }

        public bool Equals(SearchResult other)
        {
            return other != null &&
                   Species == other.Species &&
                   Level == other.Level &&
                   Female == other.Female &&
                   EqualityComparer<Position>.Default.Equals(Location, other.Location);
        }

        public override int GetHashCode()
        {
            var hashCode = 1223319261;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Species);
            hashCode = hashCode * -1521134295 + Level.GetHashCode();
            hashCode = hashCode * -1521134295 + Female.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Position>.Default.GetHashCode(Location);
            return hashCode;
        }
    }
}
