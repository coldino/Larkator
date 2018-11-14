using Larkator.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LarkatorGUI
{
    public class DummyMainWindow
    {
        public string ApplicationVersion { get => "DUMMY"; }

        public string SearchText { get; set; } = "search";

        public Collection<SearchCriteria> ListSearches { get => searches; }
        public Collection<DinoViewModel> ListResults { get => results; }

        public MapCalibration MapCalibration { get => calibration; }

        private Collection<SearchCriteria> searches = new Collection<SearchCriteria>()
        {
            new SearchCriteria { Species="Zombie", MaxLevel=4, Female=true, Group="Minecraft" },
            new SearchCriteria { Species="Creeper", MinLevel=100, Group="Minecraft" },
            new SearchCriteria { Species="Slender", MinLevel=50, Group="Other" },
            new SearchCriteria { Species="Kermit", Female=false, Group="Other" },
        };

        private Collection<DinoViewModel> results = new Collection<DinoViewModel>()
        {
            new DinoViewModel(new Dino { BaseLevel=90, Location=new Position{ Lat=40,Lon=10 }, Name="Foud" }),
            new DinoViewModel(new Dino { BaseLevel=150, Location=new Position{ Lat=10,Lon=10 }, Name="One" }),
            new DinoViewModel(new Dino { BaseLevel=110, Location=new Position{ Lat=30,Lon=10 }, Name="Three" }),
            new DinoViewModel(new Dino { BaseLevel=130, Location=new Position{ Lat=20,Lon=10 }, Name="Two" }),
        };

        private MapCalibration calibration = new MapCalibration
        {
            Filename = "TheIsland",
            PixelOffsetX = 13.75,
            PixelOffsetY = 23.75,
            PixelScaleX = 9.8875,
            PixelScaleY = 9.625
        };

        public DummyMainWindow()
        {
            foreach (var i in Enumerable.Range(0, 40))
                searches.Add(new SearchCriteria { Species = "Long thingy name", Group = "Overrun" });

            var rnd = new Random();
            foreach (var dvm in results)
            {
                dvm.Dino.Id = (ulong)rnd.Next();
                dvm.Dino.WildLevels = new StatPoints() { Health = rnd.Next(50), Stamina = rnd.Next(50), Melee = rnd.Next(50), Speed = rnd.Next(50), Weight = rnd.Next(50) };
            }
        }
    }
}
