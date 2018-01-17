using Larkator.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LarkatorGUI
{
    public class ArkReader
    {
        private const string JSON_SUFFIX = ".json";

        public List<string> LoadedSpecies { get; private set; } = new List<string>();
        public Dictionary<string, List<Dino>> FoundDinos = new Dictionary<string, List<Dino>>();
        public List<string> AllSpecies { get { return ClassMapping.Keys.OrderBy(k => k).ToList(); } }
        public int NumberOfSpecies { get { return ClassMapping.Count; } }
        public bool Tamed { get; private set; }

        Dictionary<string, string> ClassMapping = new Dictionary<string, string>();
        private Process process;
        private bool executing;
        private string outputDir;

        public ArkReader(bool wildNotTamed)
        {
            Tamed = !wildNotTamed;
        }

        private void EnsureDirectory(string outputDir)
        {
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
        }

        public async Task PerformConversion(bool force = false)
        {
            outputDir = Path.Combine(Properties.Settings.Default.OutputDir, Tamed ? "tamed" : "wild");
            EnsureDirectory(outputDir);

            if (force || IsConversionRequired())
            {
                await RunArkTools();
                ClassMapping.Clear();
            }
            else if (ClassMapping.Count == 0)
            {
                await LoadClassesJson();
            }
        }

        private bool IsConversionRequired()
        {
            var classFile = Path.Combine(outputDir, Properties.Resources.ClassesJson);
            if (!File.Exists(classFile)) return true;
            var arkTimestamp = File.GetLastWriteTimeUtc(Properties.Settings.Default.SaveFile);
            var convertTimestamp = File.GetLastWriteTimeUtc(classFile);
            return (arkTimestamp >= convertTimestamp);
        }

        public async Task EnsureSpeciesIsLoaded(string speciesName)
        {
            if (LoadedSpecies.Contains(speciesName)) return;
            await LoadSpecies(speciesName);
        }

        private async Task RunArkTools()
        {
            if (executing)
            {
                Console.WriteLine("Skipping concurrent conversion");
                return;
            }

            executing = true;
            await ExecuteArkTools(Tamed ? "tamed" : "wild", Properties.Settings.Default.SaveFile, outputDir);
            executing = false;

            // Clear previously loaded data
            ClassMapping.Clear();
            LoadedSpecies.Clear();
            FoundDinos.Clear();
        }

        private async Task ExecuteArkTools(string op, string saveFile, string outDir)
        {
            var exe = Properties.Resources.ArkToolsExe;
            var exeDir = Path.GetDirectoryName(Properties.Settings.Default.ArkTools);
            var commandLine = $"/S /C {exe} -p {op} \"{saveFile}\" \"{outDir}\"";

            var completionTask = new TaskCompletionSource<int>();
            var psi = new ProcessStartInfo("cmd.exe", commandLine)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                ErrorDialog = true,
                WorkingDirectory = exeDir,
            };
            process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = psi,
            };

            string collectedErrors = "";
            process.ErrorDataReceived += (s, e) => { if (!String.IsNullOrWhiteSpace(e.Data)) collectedErrors += e.Data; };
            process.Exited += (s, e) => completionTask.SetResult(process.ExitCode);
            process.Start();
            process.BeginErrorReadLine();
            await completionTask.Task;
            process.CancelErrorRead();

            if (!String.IsNullOrWhiteSpace(collectedErrors))
                throw new ExternalToolsException(collectedErrors);

            if (process.ExitCode != 0)
                throw new ExternalToolsException("ARK Tools failed with no output but exit code " + process.ExitCode);
        }

        private async Task LoadClassesJson()
        {
            string content = await ReadFileAsync(Path.Combine(outputDir, Properties.Resources.ClassesJson));
            var classesOutput = JArray.Parse(content);
            ClassMapping = classesOutput
                .Select(mapping => new { Cls = mapping["cls"].Value<string>(), Name = mapping["name"].Value<string>() })
                .Where(m => !m.Cls.Contains("BP_Ocean_C"))
                .ToDictionary(m => m.Name, m => m.Cls);
        }

        private async Task LoadSpecies(string speciesName)
        {
            // Lookup class id
            var speciesId = ClassMapping[speciesName];

            // Read class file
            var path = Path.Combine(outputDir, speciesId + JSON_SUFFIX);
            if (File.Exists(path))
            {
                var contents = await ReadFileAsync(path);
                FoundDinos[speciesName] = JsonConvert.DeserializeObject<List<Dino>>(contents);
            }
            else
            {
                FoundDinos[speciesName] = new List<Dino>();
            }
            LoadedSpecies.Add(speciesName);
        }

        private async Task<string> ReadFileAsync(string path)
        {
            string content;
            using (var reader = File.OpenText(path))
            {
                content = await reader.ReadToEndAsync();
            }

            return content;
        }
    }
}
