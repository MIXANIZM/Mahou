using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Mahou.InputSurfaceProbe {
    internal static class Program {
        private const int DefaultCountdownSeconds = 5;

        [STAThread]
        private static int Main(string[] args) {
            try {
                var options = Options.Parse(args ?? new string[0]);
                var commit = ReadEmbeddedCommit();
                if (options.VersionJson) {
                    Console.WriteLine("{\"schema_version\":\"" + ProbeConstants.SchemaVersion + "\",\"probe_commit\":\"" + commit + "\"}");
                    return 0;
                }

                Console.WriteLine("Mahou Input Surface Probe — read-only one-shot capture");
                Console.WriteLine("No text, window title, UIA Name, URL or clipboard content is collected.");
                for (var remaining = options.CountdownSeconds; remaining > 0; remaining--) {
                    Console.WriteLine("Focus the target field. Capture in " + remaining.ToString(CultureInfo.InvariantCulture) + "...");
                    Thread.Sleep(1000);
                }

                var report = WindowsProbe.Capture(commit);
                var output = options.OutputPath ?? DefaultOutputPath();
                JsonReportWriter.Write(output, report);
                Console.WriteLine("Report written: " + Path.GetFileName(output));
                Console.WriteLine("Classification: " + report.classification + " (" + report.confidence + ")");
                Console.WriteLine("This result does not authorize text mutation.");
                return 0;
            } catch (ArgumentException) {
                Console.Error.WriteLine("Invalid arguments. Use --countdown 0..30, --output <file>, or --version-json.");
                return 2;
            } catch (Exception error) {
                Console.Error.WriteLine("Probe failed safely: " + SafeErrorCode(error));
                return 1;
            }
        }

        private static string DefaultOutputPath() {
            return "input-surface-probe-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + ".json";
        }

        private static string SafeErrorCode(Exception error) {
            if (error == null) return "unknown-error";
            return error.GetType().Name.ToLowerInvariant();
        }

        private static string ReadEmbeddedCommit() {
            var assembly = Assembly.GetExecutingAssembly();
            var resource = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith("probe-commit.txt", StringComparison.OrdinalIgnoreCase));
            if (resource == null) return "UNSTAMPED";
            using (var stream = assembly.GetManifestResourceStream(resource))
            using (var reader = stream == null ? null : new StreamReader(stream)) {
                var value = reader == null ? null : reader.ReadToEnd();
                value = String.IsNullOrWhiteSpace(value) ? "UNSTAMPED" : value.Trim();
                return value.Length == 40 && value.All(IsHex) ? value.ToLowerInvariant() : "UNSTAMPED";
            }
        }

        private static bool IsHex(char value) {
            return (value >= '0' && value <= '9') || (value >= 'a' && value <= 'f') || (value >= 'A' && value <= 'F');
        }

        private sealed class Options {
            internal int CountdownSeconds;
            internal string OutputPath;
            internal bool VersionJson;

            internal static Options Parse(string[] args) {
                var result = new Options { CountdownSeconds = DefaultCountdownSeconds };
                for (var i = 0; i < args.Length; i++) {
                    var current = args[i];
                    if (String.Equals(current, "--version-json", StringComparison.OrdinalIgnoreCase)) {
                        result.VersionJson = true;
                        continue;
                    }
                    if (String.Equals(current, "--countdown", StringComparison.OrdinalIgnoreCase)) {
                        if (++i >= args.Length) throw new ArgumentException("Missing countdown value.");
                        int seconds;
                        if (!Int32.TryParse(args[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds) || seconds < 0 || seconds > 30)
                            throw new ArgumentException("Invalid countdown value.");
                        result.CountdownSeconds = seconds;
                        continue;
                    }
                    if (String.Equals(current, "--output", StringComparison.OrdinalIgnoreCase)) {
                        if (++i >= args.Length || String.IsNullOrWhiteSpace(args[i])) throw new ArgumentException("Missing output path.");
                        result.OutputPath = args[i];
                        continue;
                    }
                    throw new ArgumentException("Unknown argument.");
                }
                return result;
            }
        }
    }
}
