using System;

namespace Larkator.Common
{
    /// <summary>
    /// The search parameters.
    /// </summary>
    /// <remarks>
    /// Objects of this type use only a random immutable id when computing the hashcode or comparing for equality.
    /// This helps avoid issues such as:
    /// https://support.microsoft.com/en-gb/help/2909048/system-argumentexception-when-selecting-row-in-wpf-datagrid
    /// </remarks>
    public class SearchCriteria : IEquatable<SearchCriteria>
    {
        private Guid guid = Guid.NewGuid(); // to avoid something like https://support.microsoft.com/en-gb/help/2909048/system-argumentexception-when-selecting-row-in-wpf-datagrid

        public SearchCriteria()
        {
        }

        public SearchCriteria(SearchCriteria copy)
        {
            Group = copy.Group;
            Order = copy.Order;
            Species = copy.Species;
            MinLevel = copy.MinLevel;
            MaxLevel = copy.MaxLevel;
            Female = copy.Female;
        }

        public string Group { get; set; }
        public double Order { get; set; }
        public string Species { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public bool? Female { get; set; }

        /// <summary>
        /// Compares equality based only on an internal random ID assigned on creation.
        /// </summary>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SearchCriteria);
        }

        /// <summary>
        /// Compares equality based only on an internal random ID assigned on creation.
        /// </summary>
        /// <returns></returns>
        public bool Equals(SearchCriteria other)
        {
            return other != null && guid.Equals(other.guid);
        }

        /// <summary>
        /// Computes the hash code based only on an internal random ID assigned on creation.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        public bool Matches(Dino dino)
        {
            if (MinLevel.HasValue && dino.BaseLevel < MinLevel) return false;
            if (MaxLevel.HasValue && dino.BaseLevel > MaxLevel) return false;
            if (Female.HasValue && dino.Female != Female) return false;
            if (!String.IsNullOrWhiteSpace(Species) && !String.Equals(Species, dino.Type)) return false;

            return true;
        }
    }
}
