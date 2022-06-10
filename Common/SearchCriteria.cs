using System;
using System.Collections.Generic;
using System.Linq;

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
            GroupSearch = copy.GroupSearch;
        }

        public string Group { get; set; }
        public double Order { get; set; }
        public string Species { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public bool? Female { get; set; }
        public bool GroupSearch { get; set; }

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

        public List<Dino> Matches(Dictionary<string, List<Dino>> dinos, out int num_dinos_matching_species_criteria)
        {
            string species_criteria = this.Species;
            bool species_criteria_is_partial = this.GroupSearch;
            int min_level_criteria = this.MinLevel ?? int.MinValue;
            int max_level_criteria = this.MaxLevel ?? int.MaxValue;
            bool? female_criteria = this.Female;

            List<Dino> result_list_of_dinos = new List<Dino>();
            num_dinos_matching_species_criteria = 0;

            foreach (var kvp in dinos)
            {
                string current_species = kvp.Key;
                List<Dino> dinos_for_species = kvp.Value;
                if (matches_species(current_species))
                {
                    num_dinos_matching_species_criteria += dinos_for_species.Count;
                    result_list_of_dinos.AddRange(dinos_for_species.Where(matches_level_and_sex));
                }
            }

            return result_list_of_dinos;

            bool matches_level_and_sex(Dino dino)
                => min_level_criteria <= dino.BaseLevel
                && max_level_criteria >= dino.BaseLevel
                && (female_criteria == null || female_criteria.Value == dino.Female);

            bool matches_species(string species)
            {
                if (string.IsNullOrWhiteSpace(species_criteria))
                    return true;
                else if (species_criteria_is_partial)
                    return species.Contains(species_criteria);
                else
                    return species.Equals(species_criteria);
            }
        }
    }
}
