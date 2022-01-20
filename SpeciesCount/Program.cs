using CommandLine;
using SavegameToolkit;
using SavegameToolkitAdditions;

internal class Program
{
    public class Options
    {
        [Option('w', "wild", HelpText = "Include wild creatures.")]
        public bool IncludeWild { get; set; }

        [Option('t', "tamed", HelpText = "Include tamed creatures.")]
        public bool IncludeTamed { get; set; }

        [Option('o', "output", HelpText = "Redirect output to the specified file.")]
        public string? OutputFilename { get; set; }

        [Value(0, Required = true, MetaName = "savegame", HelpText = "Savegame .ark to read.")]
        public string? ArkFilename { get; set; }
    }

    static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(ParseSavegame);
    }

    static async Task ParseSavegame(Options options)
    {
        if (!options.IncludeTamed && !options.IncludeWild)
        {
            Console.Error.WriteLine("ERROR: Must specify either --wild or --tamed");
            Environment.Exit(100);
        }

        var gameObjects = await ReadSavegameFile(options.ArkFilename!);

        bool ShouldInclude(GameObject creature)
        {
            if (options.IncludeWild && creature.IsWild()) return true;
            if (options.IncludeTamed && creature.IsTamed()) return true;
            return false;
        }

        var counts = gameObjects
            .Where(o => o.IsCreature() && !o.IsUnclaimedBaby() && ShouldInclude(o))
            .GroupBy(o => o.ClassString)
            .Select(group => (classname: group.Key, count: group.Count()))
            .OrderByDescending(pair => pair.count)
            .ToArray();

        TextWriter output = Console.Out;
        if (options.OutputFilename != null && options.OutputFilename != "-")
        {
            output = new StreamWriter(options.OutputFilename);
        }

        output.WriteLine("Species,Count");
        foreach (var (classname, count) in counts)
        {
            output.WriteLine($"{classname}, {count}");
        }
    }

    private static Task<GameObjectContainer> ReadSavegameFile(string fileName)
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
                return (arkSavegame);

            var combinedObjects = arkSavegame.Objects;

            foreach (var entry in arkSavegame.HibernationEntries)
            {
                var collector = new ObjectCollector(entry, 1);
                combinedObjects.AddRange(collector.Remap(combinedObjects.Count));
            }

            return new GameObjectContainer(combinedObjects);
        });
    }
}