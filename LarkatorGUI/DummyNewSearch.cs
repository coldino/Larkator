using Larkator.Common;

namespace LarkatorGUI
{
    public class DummyNewSearch : SearchCriteria
    {
        public DummyNewSearch()
        {
            this.Species = "Alpha ";
            this.Group = "Shopping List";
            this.MinLevel = 100;
            this.MaxLevel = 150;
            this.GroupSearch = true;
        }
    }
}
