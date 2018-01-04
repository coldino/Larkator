using DuoVia.FuzzyStrings;
using Larkator.Common;
using Mono.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Larkator
{
    class Program
    {
        public const string PROGRAM_NAME = "Larkator";

        const string ARK_TOOLS_EXE = "ark-tools.exe";
        const double FUZZY_TOO_SIMILAR_PCT = 0.95;
        const double FUZZY_THRESHOLD = 0.25;
        const double FUZZY_SHOW_THRESHOLD = 0.15;
        private const string CLASSES_JSON = "classes.json";
        readonly int[] ColumnWidths = new int[] { 4, 6, 18, 12 };

        public CommandLineParameters Options { get; private set; }
        public JArray Species { get; private set; }
        public string[] SpeciesNames { get; private set; }
        public ConcurrentDictionary<string, SearchCompletion> Results { get; private set; } = new ConcurrentDictionary<string, SearchCompletion>();
        public Dictionary<string, string> ClassMapping { get; private set; }
        public List<string> SearchNames { get; private set; }

        static IReadOnlyDictionary<string, string> DefaultConfiguration { get; } = new Dictionary<string, string>()
        {
            ["arkFile"] = null,
        };

        static void Main(string[] args)
        {
            var main = new Program();
            Task.WaitAll(main.Execute(args));
        }

        private Program()
        {
            //this.Config = config;
        }

        private async Task Execute(string[] args)
        {
            Console.WriteLine($"{PROGRAM_NAME}: Dino Locator");

            HandleCommandLine(args);

            await ConvertArkIfRequiredAsync();
            GuessSearchNames();
            await PerformAllSearchesAsync();

            DisplayResults();
        }

        private void HandleCommandLine(string[] args)
        {
            Options = new CommandLineParameters();
            var optionDefinitions = new OptionSet {
                { "h|help", "show this message and exit", h => Options.ShowHelp = h != null },
                { "d|dryrun", "do not execute anything, just print what would be done", v => Options.DryRun = (v != null) },
                { "q|quiet", "reduce output, only outputting results", v => Options.Quiet = (v != null) },
                { "t|arktools=", "specify the path to the arktools directory", (string p) => Options.ArkToolsPath = p },
                { "f|savefile=", "specify the path to the ARK save file", (string p) => Options.SaveFile = p },
                { "xyz", "write locations using x,y,z format for teleport", v => Options.PositionFormat = (v != null) ? PositionFormat.XYZ : Options.PositionFormat },
                { "latlon", "write locations using map lat,long format", v => Options.PositionFormat = (v != null) ? PositionFormat.LatLong : Options.PositionFormat },
                { "tmp=", "specify the path to store temporary data output from arktools", (string p) => Options.TempDir = p },
                { "s|gender=", "specify the gender of the dinosaur to find", (string s) => Options.Female = ParseGender(s) },
                { "l|minlevel=", "specify the minimum level of the dinosaur to find", (int l) => Options.MinLevel = l },
                { "m|maxlevel=", "specify the maximum level of the dinosaur to find", (int l) => Options.MaxLevel = l },
                { "tamingspeed=", "the taming speed multiplier of your ARK", (double m) => Options.TamingSpeedMultiplier = m },
                { "tamingfoodrate=", "the taming food rate multiplier of your ARK", (double m) => Options.TamingFoodRateMultiplier = m },
                { "showmatches", "show debug information about name matching", v => Options.ShowNameMatchingResults = (v != null) },
                { "<>", "list of creatures to track", (string c) => Options.Creatures.Add(c) }
            };

            try
            {
                optionDefinitions.Parse(args);

                if (Options.ShowHelp)
                {
                    ShowHelp(optionDefinitions);
                    Abort();
                }

                if (Options.Creatures.Count < 1) throw new OptionException("No creatures specified", "<>");

                VerifyArkTools();
                VerifySavedArk();
                VerifyTempDir();
            }
            catch (OptionException e)
            {
                Console.Write($"{PROGRAM_NAME}: ");
                Console.WriteLine(e.Message);
                Console.WriteLine($"Try `{PROGRAM_NAME} --help' for more information.");
                Abort();
            }
        }

        private async Task ConvertArkIfRequiredAsync()
        {
            if (IsConvertRequired())
            {
                await ConvertArkAsync();
            }

            await LoadCreatureIndexAsync();
        }

        private void GuessSearchNames()
        {
            SearchNames = Options.Creatures.Select(ParseCreature).ToList();
            if (SearchNames.Any(n => String.IsNullOrWhiteSpace(n)))
                Abort(); // error already shown
        }

        private void Abort()
        {
            Process.GetCurrentProcess().Kill();
        }

        private void WriteLine(string msg)
        {
            if (!Options.Quiet) Console.WriteLine(msg);
        }

        private bool IsConvertRequired()
        {
            var classFile = Path.Combine(Options.TempDir, CLASSES_JSON);
            if (!File.Exists(classFile)) return true;
            var arkTimestamp = File.GetLastWriteTimeUtc(Options.SaveFile);
            var convertTimestamp = File.GetLastWriteTimeUtc(classFile);
            return (arkTimestamp >= convertTimestamp);
        }

        private Task ConvertArkAsync()
        {
            WriteLine("Reading ARK using ARKTools...");
            return Task.Run(() => ExecuteArkTools($"-s -p wild \"{Options.SaveFile}\" \"{Options.TempDir}\""));
        }

        private void ClearTemp()
        {
            foreach (var file in Directory.GetFiles(Options.TempDir))
                File.Delete(file);
        }

        private async Task LoadCreatureIndexAsync()
        {
            var classesOutput = JArray.Parse(await File.ReadAllTextAsync(Path.Combine(Options.TempDir, CLASSES_JSON)));
            ClassMapping = classesOutput
                .Select(mapping => new { Cls = mapping["cls"].Value<string>(), Name = mapping["name"].Value<string>() })
                .Where(m => !m.Cls.Contains("BP_Ocean_C"))
                .ToDictionary(m => m.Name, m => m.Cls);

            SpeciesNames = ClassMapping.Keys.ToArray();
        }

        private async Task PerformAllSearchesAsync()
        {
            var searchTasks = SearchNames.Select(speciesName => PerformSearchAsync(speciesName));
            await Task.WhenAll(searchTasks);
        }

        private async Task PerformSearchAsync(string speciesNameFromDb)
        {
            IEnumerable<Dino> dinos = await LoadSpeciesAsync(speciesNameFromDb);

            // Apply search criteria
            if (Options.MinLevel.HasValue) dinos = dinos.Where(dino => dino.BaseLevel >= Options.MinLevel);
            if (Options.MaxLevel.HasValue) dinos = dinos.Where(dino => dino.BaseLevel <= Options.MaxLevel);
            if (Options.Female.HasValue) dinos = dinos.Where(dino => dino.Female == Options.Female);

            // Store the results
            Results[speciesNameFromDb] = new SearchCompletion { ValuesName = speciesNameFromDb, Dinos = dinos.ToList() };
        }

        private async Task<IList<Dino>> LoadSpeciesAsync(string speciesNameFromDb)
        {
            // Lookup class id
            var speciesNameFromArkTools = ClassMapping.ContainsKey(speciesNameFromDb) ? speciesNameFromDb : ClassMapping.Keys.BestFuzzyMatch(speciesNameFromDb);
            var speciesId = ClassMapping[speciesNameFromArkTools];

            //Console.WriteLine($"Reading {classId}...");

            // Read class file
            var contents = await File.ReadAllTextAsync(Path.Combine(Options.TempDir, speciesId + ".json"));
            var dinos = JsonConvert.DeserializeObject<List<Dino>>(contents);

            return dinos;
        }

        private void DisplayResults()
        {
            TableWrite("Sex", "Lvl", "Hp/St/We/Me/Sp", "Position");

            foreach (var searchName in SearchNames)
            {
                var result = Results[searchName];

                Console.WriteLine(result.ValuesName + ":");

                var orderedResults = result.Dinos.OrderByDescending(dino => dino.BaseLevel);
                var empty = true;
                foreach (var dino in orderedResults)
                {
                    empty = false;
                    TableWrite((dino.Female ? "F" : "M"), dino.BaseLevel.ToString(), dino.WildLevels.ToString(true), dino.Location.ToString(Options.PositionFormat));
                }
                if (empty) Console.WriteLine("  -");
            }
        }

        private void TableWrite(params string[] columns)
        {
            Console.WriteLine("  " + String.Join("", columns.Zip(ColumnWidths, (text, width) => text.PadRight(width))));
        }

        private async Task ExecuteArkTools(string args)
        {
            if (Options.DryRun)
            {
                Console.WriteLine(Options.ArkToolsPath + " " + args);
            }
            else
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/C \"cd {Options.ArkToolsPath} && {ARK_TOOLS_EXE} {args}\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };
                var proc = new Process { StartInfo = psi };
                var completionTask = new TaskCompletionSource<int>();
                var errorOutput = "";
                proc.Exited += (s, e) => completionTask.SetResult(proc.ExitCode);
                proc.ErrorDataReceived += (s, e) => { if (!String.IsNullOrWhiteSpace(e.Data)) errorOutput += e.Data; };
                proc.BeginErrorReadLine();
                proc.Start();
                proc.CancelErrorRead();
                await completionTask.Task;
                if (proc.ExitCode != 0) throw new InvalidOperationException("ARK Tools failed with exit code " + proc.ExitCode);
                if (!String.IsNullOrWhiteSpace(errorOutput)) throw new InvalidOperationException("ARK Tools failed with error: " + errorOutput);
            }
        }

        private void DisplaySearchCriteria()
        {
            Console.WriteLine("Search criteria:");
            if (Options.MinLevel.HasValue) Console.WriteLine($"  Min level: {Options.MinLevel}");
            if (Options.MaxLevel.HasValue) Console.WriteLine($"  Max level: {Options.MaxLevel}");
            if (Options.Female.HasValue) Console.WriteLine($"  Gender: {(Options.Female.Value ? "Female" : "Male")}");
        }

#if OBSOLETE
        private async Task LoadDatabaseAsync()
        {
            database = JObject.Parse(await File.ReadAllTextAsync("values.json"));
            Species = database.species;
            SpeciesNames = Species.Select(species => ((string)species["name"])).ToArray();
            WriteLine($"Database version {database.ver} contains {database.species.Count} species");
        }
#endif

        private bool ParseGender(string input)
        {
            switch (input.ToLowerInvariant())
            {
                case "m":
                case "male":
                    return false;
                case "f":
                case "fem":
                case "female":
                    return true;
                default:
                    throw new OptionException("Could not parse gender", "sex");
            }
        }

#region Commandline handling
        private static void ShowHelp(OptionSet optionDefinitions)
        {
            Console.WriteLine($"Usage: {PROGRAM_NAME} [OPTIONS] <creature> (<creature> ...)");
            Console.WriteLine("Find wild dinos within your ARK");
            Console.WriteLine($"e.g. {PROGRAM_NAME} -t <arktools-path> -s <ark-file> -l50 -sF Quetz");
            Console.WriteLine();

            // output the options
            Console.WriteLine("Options:");
            optionDefinitions.WriteOptionDescriptions(Console.Out);
        }

        private string ParseCreature(string shortName)
        {
            var matches = SpeciesNames.Select((name, i) => new { Score = name.FuzzyMatch(shortName), Index = i, Name = name })
                .Where(result => result.Score > FUZZY_SHOW_THRESHOLD)
                .OrderByDescending(result => result.Score).Take(5).ToArray();

            if (Options.ShowNameMatchingResults)
            {
                Console.WriteLine($"Matches for '{shortName}':");
                foreach (var result in matches) Console.WriteLine($"  {result.Name} ({result.Score})");
            }

            var goodMatches = matches.Where(result => result.Score >= FUZZY_THRESHOLD).ToArray();

            if (goodMatches.Length == 0 || goodMatches.Count(result => result.Score >= goodMatches.First().Score * FUZZY_TOO_SIMILAR_PCT) > 1)
            {
                Console.WriteLine($"Unable to match '{shortName}': {String.Join(", ", matches.Select(result => result.Name))}?");
                return null;
            }

            var bestMatch = goodMatches.First().Index;
            return SpeciesNames[bestMatch];
        }

        private void VerifyArkTools()
        {
            if (!Directory.Exists(Options.ArkToolsPath))
                throw new OptionException("ARK Tools directory not found", "arktools");

            if (!File.Exists(Path.Combine(Options.ArkToolsPath, ARK_TOOLS_EXE)))
                throw new OptionException("ARK Tools executable not found within specified directory", "arktools");

            Options.ArkToolsPath = Path.GetFullPath(Options.ArkToolsPath);
        }

        private void VerifySavedArk()
        {
            if (!File.Exists(Options.SaveFile))
                throw new OptionException("Saved ARK file not found", "arktools");

            Options.SaveFile = Path.GetFullPath(Options.SaveFile);
        }

        private void VerifyTempDir()
        {
            // Set to default if not set
            if (String.IsNullOrWhiteSpace(Options.TempDir)) Options.TempDir = Path.Combine(Path.GetTempPath(), PROGRAM_NAME);

            // Try to create temp dir if it doesn't already exist
            if (!Directory.Exists(Options.TempDir))
            {
                try
                {
                    if (Options.DryRun)
                        WriteLine("Creating temp directory: " + Options.TempDir);
                    else
                        Directory.CreateDirectory(Options.TempDir);
                }
                catch (Exception ex)
                {
                    throw new OptionException("Unable to create temp directory: " + ex.Message, "tmp");
                }
            }

            //Console.WriteLine("Using temp directory: " + Options.TempDir);
        }

        public class SearchCompletion
        {
            public string ValuesName { get; set; }
            public List<Dino> Dinos { get; set; }
        }
#endregion
    }
}