using Larkator.Common;
using SavegameToolkit;
using SavegameToolkit.Types;
using SavegameToolkitAdditions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace LarkatorGUI
{
    public class ArkReader
    {
        public int MapSize { get; set; } = 8000;

        public Dictionary<string, List<Dino>> WildDinos { get; } = new Dictionary<string, List<Dino>>();
        public Dictionary<string, List<Dino>> TamedDinos { get; } = new Dictionary<string, List<Dino>>();
        public List<string> AllSpecies { get; } = new List<string>();
        public List<string> TamedSpecies { get; } = new List<string>();
        public List<string> WildSpecies { get; } = new List<string>();
        public int NumberOfTamedSpecies { get => TamedSpecies.Count; }
        public int NumberOfWildSpecies { get => WildSpecies.Count; }
        
        private ArkData arkData;

        private static readonly string[] RAFT_CLASSES = { "Raft_BP_C", "MotorRaft_BP_C", "Barge_BP_C" };

        private static Task<(GameObjectContainer gameObjects, float gameTime)> ReadSavegameFile(string fileName)
        {
            return Task.Run(() =>
            {
                if (new FileInfo(fileName).Length > int.MaxValue)
                    throw new Exception("Input file is too large.");

                var arkSavegame = new ArkSavegame();

                using (var stream = new MemoryStream(File.ReadAllBytes(fileName)))
                using (var archive = new ArkArchive(stream))
                {
                    arkSavegame.ReadBinary(archive, ReadingOptions.Create()
                            .WithDataFiles(false)
                            .WithEmbeddedData(false)
                            .WithDataFilesObjectMap(false)
                            .WithObjectFilter(o => !o.IsItem && (o.Parent != null || o.Components.Any()))
                            .WithBuildComponentTree(true));
                }

                if (!arkSavegame.HibernationEntries.Any())
                    return (arkSavegame, arkSavegame.GameTime);

                var combinedObjects = arkSavegame.Objects;

                foreach (var entry in arkSavegame.HibernationEntries)
                {
                    var collector = new ObjectCollector(entry, 1);
                    combinedObjects.AddRange(collector.Remap(combinedObjects.Count));
                }

                return (new GameObjectContainer(combinedObjects), arkSavegame.GameTime);
            });
        }

        public async Task PerformConversion(string saveFile)
        {
            // Clear previously loaded data
            TamedSpecies.Clear();
            WildSpecies.Clear();
            AllSpecies.Clear();
            WildDinos.Clear();
            TamedDinos.Clear();

            // Read objects directly from the savegame
            (GameObjectContainer gameObjectContainer, float gameTime) = await ReadSavegameFile(saveFile);

            // Convert read objects to a usable form
            var creatureObjects = gameObjectContainer
                .Where(o => o.IsCreature() && !o.IsUnclaimedBaby() && !RAFT_CLASSES.Contains(o.ClassString))
                .ToList();
            
            var tameObjects = creatureObjects.Where(o => !o.IsWild()).GroupBy(o => o.ClassString);
            TamedSpecies.AddRange(tameObjects.Select(o => o.Key));
            foreach (var group in tameObjects)
                TamedDinos.Add(group.Key, group.Select(o => ConvertCreature(o)).ToList());

            var wildObjects = creatureObjects.Where(o => o.IsWild()).GroupBy(o => o.ClassString);
            WildSpecies.AddRange(wildObjects.Select(o => o.Key));
            foreach (var group in wildObjects)
                WildDinos.Add(group.Key, group.Select(o => ConvertCreature(o)).ToList());

            AllSpecies.AddRange(creatureObjects.Select(o => o.ClassString).Distinct());
        }

        private Dino ConvertCreature(GameObject obj)
        {
            var dino = new Dino
            {
                Type = obj.ClassString,
                Female = obj.IsFemale(),
                Id = (ulong)obj.GetDinoId(),
                BaseLevel = obj.GetBaseLevel(),
                Name = obj.GetPropertyValue("TamedName", defaultValue:""),
                Location = ConvertCoordsToLatLong(obj.Location),
                WildLevels = new StatPoints(),
            };
            
            var status = obj.CharacterStatusComponent();
            if (status != null)
            {
                var defaultValue = new ArkByteValue(0);
                dino.WildLevels.Health = status.GetPropertyValue("NumberOfLevelUpPointsApplied", 0, defaultValue).ByteValue;
                dino.WildLevels.Stamina = status.GetPropertyValue("NumberOfLevelUpPointsApplied", 1, defaultValue).ByteValue;
                dino.WildLevels.Weight = status.GetPropertyValue("NumberOfLevelUpPointsApplied", 7, defaultValue).ByteValue;
                dino.WildLevels.Melee = status.GetPropertyValue("NumberOfLevelUpPointsApplied", 8, defaultValue).ByteValue;
                dino.WildLevels.Speed = status.GetPropertyValue("NumberOfLevelUpPointsApplied", 9, defaultValue).ByteValue;
            }

            return dino;
        }

        private Position ConvertCoordsToLatLong(LocationData location)
        {
            return new Position
            {
                X = location.X,
                Y = location.Y,
                Z = location.Z,

                Lat = 50 + location.Y / 8000,
                Lon = 50 + location.X / 8000,
            };
        }

        private bool IsConversionRequired()
        {
            return true;

            //var classFile = Path.Combine(outputDir, Properties.Resources.ClassesJson);
            //if (!File.Exists(classFile)) return true;
            //var arkTimestamp = File.GetLastWriteTimeUtc(Properties.Settings.Default.SaveFile);
            //var convertTimestamp = File.GetLastWriteTimeUtc(classFile);
            //return (arkTimestamp >= convertTimestamp);
        }

        private string GenerateNameVariant(string name, string cls)
        {
            var clsParts = cls.Split('_');
            if (clsParts.Length > 3 && clsParts.Last() == "C" && clsParts[clsParts.Length - 3] == "BP")
                return $"{name} ({clsParts[clsParts.Length - 2]})";

            return name + "*";
        }
    }
}
