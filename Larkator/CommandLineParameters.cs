using Larkator.Common;
using System.Collections.Generic;

namespace Larkator
{
    public class CommandLineParameters
    {
        public bool ShowHelp { get; set; } = false;
        public bool Quiet { get; set; } = false;
        public string ArkToolsPath { get; set; }
        public List<string> Creatures { get; set; } = new List<string>();
        public string SaveFile { get; set; }
        public PositionFormat PositionFormat { get; set; } = PositionFormat.LatLong;
        public bool? Female { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public string TempDir { get; set; }
        public bool DryRun { get; set; } = false;
        public bool ShowNameMatchingResults { get; set; } = false;
        public bool SkipConversion { get; set; } = false;
        public bool SkipClear { get; set; } = false;
        public double? TamingSpeedMultiplier { get; internal set; }
        public double? TamingFoodRateMultiplier { get; internal set; }
    }
}