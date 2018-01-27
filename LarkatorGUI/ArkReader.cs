using Larkator.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        public bool ForceNextConversion { get; set; }

        Dictionary<string, string> ClassMapping = new Dictionary<string, string>();
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

        public async Task PerformConversion(bool force, string dirName)
        {
            outputDir = Path.Combine(Properties.Settings.Default.OutputDir, dirName, Tamed ? "tamed" : "wild");
            EnsureDirectory(outputDir);

            if (force || ForceNextConversion || IsConversionRequired())
            {
                ForceNextConversion = false;
                await RunArkTools();
            }

            if (ClassMapping.Count == 0)
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
            try
            {
                await ExecuteArkTools(Tamed ? "tamed" : "wild", Properties.Settings.Default.SaveFile, outputDir);
            }
            finally
            {
                executing = false;
            }

            // Clear previously loaded data
            ClassMapping.Clear();
            LoadedSpecies.Clear();
            FoundDinos.Clear();
        }

        public static async Task ExecuteArkTools(string op, params string[] args)
        {
            var exe = Properties.Resources.ArkToolsExe;
            var exeDir = Path.GetDirectoryName(Properties.Settings.Default.ArkTools);
            var sb = new StringBuilder($"/S /C {exe} -p {op}");
            foreach (var arg in args)
                sb.Append(" \"" + arg + "\"");

            var result = await ExecuteCommand(sb.ToString(), exeDir);

            if (!String.IsNullOrWhiteSpace(result.ErrorOutput))
                throw new ExternalToolsException(result.ErrorOutput);

            if (result.ExitCode != 0)
                throw new ExternalToolsException("ARK Tools failed with no output but exit code " + result.ExitCode);
        }

        private async Task LoadClassesJson()
        {
            string content = await ReadFileAsync(Path.Combine(outputDir, Properties.Resources.ClassesJson));
            var classesOutput = JArray.Parse(content);
            ClassMapping = classesOutput
                .Select(mapping => new { Cls = mapping["cls"].Value<string>(), Name = mapping["name"].Value<string>() })
                .Where(m => !m.Cls.Contains("BP_Ocean_C"))
                .ToDictionary(m => m.Name, m => m.Cls);

#if WARN_ON_EMPTY_CLASSES
            if (ClassMapping.Count <= 0)
                throw new ExternalToolsException("ARK Tools produced no classes output");
#endif
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

        private static async Task<ProcessResult> ExecuteCommand(string commandLine, string workingDir)
        {
            var completionTask = new TaskCompletionSource<int>();
            var psi = new ProcessStartInfo("cmd.exe", commandLine)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                ErrorDialog = true,
                WorkingDirectory = workingDir,
            };
            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = psi,
            };

            var collectedErrors = new StringBuilder();
            process.ErrorDataReceived += (s, e) => { if (!String.IsNullOrWhiteSpace(e.Data)) collectedErrors.Append(e.Data); };
            process.Exited += (s, e) => completionTask.SetResult(process.ExitCode);
            process.Start();
            process.BeginErrorReadLine();
            await completionTask.Task;
            process.CancelErrorRead();

            return new ProcessResult { ErrorOutput = collectedErrors.ToString(), ExitCode = process.ExitCode };
        }

        private static async Task<string> ReadFileAsync(string path)
        {
            string content;
            using (var reader = File.OpenText(path))
            {
                content = await reader.ReadToEndAsync();
            }

            return content;
        }

        private class ProcessResult
        {
            public string ErrorOutput { get; set; }
            public int ExitCode { get; set; }
        }
    }
}
