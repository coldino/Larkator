using Larkator.Common;
using System.Collections.Generic;

namespace Larkator
{
    public class SearchResult
    {
        public string ValuesName { get; set; }
        //public string ClassName { get; set; }
        public List<Dino> Dinos { get; set; } = new List<Dino>();
    }
}