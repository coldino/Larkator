using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SavegameToolkit;
using SavegameToolkitAdditions;

using MathNet.Numerics;


namespace MapCalibrator
{
    /// <summary>
    /// Coordinate conversion discovery.
    /// </summary>
    /// <remarks>
    /// Reads the given savegame and finds all storage containers with a name "Calibration: NN.N, NN.N",
    /// where NN.N, NN.N is Lat and Lon from your GPS.
    /// Calculates the best coordinate conversion values that matches all of the found containers.
    /// </remarks>
    class Program
    {
        static async Task Main(string[] args)
        {
            var savegameFile = args.FirstOrDefault();
            if (savegameFile == null)
            {
                Console.Error.WriteLine("Usage: MapCalibrator.exe <savegame>");
                Environment.Exit(1);
            }

            var arkDataFile = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Larkator"), "ark-data.json");
            var arkData = ArkDataReader.ReadFromFile(arkDataFile);

            (GameObjectContainer gameObjects, float gameTime) = await ReadSavegameFile(savegameFile);

            // Find any objects that have a relevant BoxName
            var items = gameObjects
                .Where(o => o.Parent == null && o.GetPropertyValue<string>("BoxName", defaultValue: "").StartsWith("Calibration:"))
                .ToList();

            // Extract XYZ location and calibration lat/lon
            var inputs = items.Select(o => (o.Location, LatLon: LatLongFromName(o.GetPropertyValue<string>("BoxName")))).ToArray();

            // Perform linear regression on the values for best fit, separately for X and Y
            double[] xValues = inputs.Select(i => (double)i.Location.X).ToArray();
            double[] yValues = inputs.Select(i => (double)i.Location.Y).ToArray();
            double[] lonValues = inputs.Select(i => i.LatLon.Lon).ToArray();
            double[] latValues = inputs.Select(i => i.LatLon.Lat).ToArray();
            var (xOffset, xMult) = Fit.Line(xValues, lonValues);
            var (yOffset, yMult) = Fit.Line(yValues, latValues);
            var xCorr = GoodnessOfFit.RSquared(xValues.Select(x => xOffset + xMult * x), lonValues);
            var yCorr = GoodnessOfFit.RSquared(yValues.Select(y => yOffset + yMult * y), latValues);

            Console.WriteLine($"X: {xOffset} + X/{1 / xMult}  (corr {xCorr})");
            Console.WriteLine($"Y: {yOffset} + X/{1 / yMult}  (corr {yCorr})");

            Console.ReadLine();
        }

        private static LatLon LatLongFromName(string name)
        {
            var coordsString = name.Split(':').Select(s => s.Trim()).Skip(1).FirstOrDefault();
            var parts = coordsString.Split(',').Select(s => s.Trim());
            var values = parts.Select(s => double.Parse(s)).ToArray();
            var result = new LatLon(values[0], values[1]);
            return result;
        }

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

        public struct LatLon
        {
            public LatLon(double lat, double lon) : this()
            {
                Lat = lat;
                Lon = lon;
            }

            public double Lat { get; set; }
            public double Lon { get; set; }
        }
    }
}
