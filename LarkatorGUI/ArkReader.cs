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
        public MapCalibration MapCalibration { get; set; }

        public Dictionary<string, List<Dino>> WildDinos { get; } = new Dictionary<string, List<Dino>>();
        public Dictionary<string, List<Dino>> TamedDinos { get; } = new Dictionary<string, List<Dino>>();
        public List<string> AllSpecies { get; } = new List<string>();
        public List<string> TamedSpecies { get; } = new List<string>();
        public List<string> WildSpecies { get; } = new List<string>();
        public int NumberOfTamedSpecies { get => TamedSpecies.Count; }
        public int NumberOfWildSpecies { get => WildSpecies.Count; }

        public void SetArkData(ArkData data)
        {
            arkData = data;

            // Create some easy to use mappings for better performance
            classMap = arkData.Creatures.ToDictionary(c => c.Class, c => c.Name);
        }

        private static readonly string[] RAFT_CLASSES = { "Raft_BP_C", "MotorRaft_BP_C", "Barge_BP_C" };
        private ArkData arkData;
        private Dictionary<string, string> classMap;

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
            if (MapCalibration == null)
                throw new ArgumentNullException(nameof(MapCalibration), "Callibration required");

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

            var tameObjects = creatureObjects.Where(o => !o.IsWild()).GroupBy(o => SpeciesName(o.ClassString));
            TamedSpecies.AddRange(tameObjects.Select(o => o.Key).Distinct().OrderBy(name => name));
            foreach (var group in tameObjects)
                TamedDinos.Add(group.Key, group.Select(o => ConvertCreature(o)).ToList());

            var wildObjects = creatureObjects.Where(o => o.IsWild()).GroupBy(o => SpeciesName(o.ClassString));
            WildSpecies.AddRange(wildObjects.Select(o => o.Key).Distinct().OrderBy(name => name));
            foreach (var group in wildObjects)
                WildDinos.Add(group.Key, group.Select(o => ConvertCreature(o)).ToList());

            AllSpecies.AddRange(creatureObjects.Select(o => SpeciesName(o.ClassString)).Distinct().OrderBy(name => name));
        }

        private string SpeciesName(string className)
        {
            if (classMap.TryGetValue(className, out var output))
                return output;

            return className;
        }

        private Dino ConvertCreature(GameObject obj)
        {
            var dino = new Dino
            {
                Type = SpeciesName(obj.ClassString),
                Female = obj.IsFemale(),
                Id = (ulong)obj.GetDinoId(),
                BaseLevel = obj.GetBaseLevel(),
                Name = obj.GetPropertyValue("TamedName", defaultValue: ""),
                IsTameable = obj.GetPropertyValue("bForceDisabledTaming",defaultValue: true),
                Location = ConvertCoordsToLatLong(obj.Location),
                WildLevels = new StatPoints(),
            };

            if (dino.Type.Contains("Polar"))
            {
                //stuff
                int x = 1;
            }

            var status = obj.CharacterStatusComponent();
            if (status != null)
            {
                var defaultValue = new ArkByteValue(0);
                dino.WildLevels.Health = status.GetPropertyValue("NumberOfLevelUpPointsApplied", 0, defaultValue).ByteValue;
                dino.WildLevels.Stamina = status.GetPropertyValue("NumberOfLevelUpPointsApplied", 1, defaultValue).ByteValue;
                dino.WildLevels.Oxygen = status.GetPropertyValue("NumberOfLevelUpPointsApplied", 3, defaultValue).ByteValue;
                dino.WildLevels.Food = status.GetPropertyValue("NumberOfLevelUpPointsApplied", 4, defaultValue).ByteValue;
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

                Lat = MapCalibration.LatOffset + location.Y / MapCalibration.LatDivisor,
                Lon = MapCalibration.LonOffset + location.X / MapCalibration.LonDivisor,
            };
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
