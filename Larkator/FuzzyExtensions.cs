using DuoVia.FuzzyStrings;
using System.Collections.Generic;
using System.Linq;

namespace Larkator
{
    public static class FuzzyExtensions
    {
        public static string BestFuzzyMatch(this IEnumerable<string> source, string target)
        {
            return source.Select(s => new { Score = s.FuzzyMatch(target), Value = s }).OrderByDescending(v => v.Score).First().Value;
        }
    }
}