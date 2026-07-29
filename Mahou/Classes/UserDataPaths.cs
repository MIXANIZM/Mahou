using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Mahou {
    internal static class UserDataPaths {
        internal static readonly string ExecutableRoot = AppDomain.CurrentDomain.BaseDirectory;
        internal static readonly string RoamingRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MIXANIZM Mahou");
        internal static readonly string LocalRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MIXANIZM Mahou");

        internal static bool UsesCustomPath { get; private set; }
        internal static string DataRoot { get; private set; }

        static readonly string[] MigratedFiles = {
            "Mahou.ini", "AS_dict.txt", "history.txt", "TSDict.txt",
            "Mahou.mm", "CustomConversion.txt", "ASsymDiff.txt", "LayoutReplaces.txt"
        };

        internal static void Initialize(string[] args) {
            var custom = ParseCustomDirectory(args);
            UsesCustomPath = !String.IsNullOrEmpty(custom);
            DataRoot = UsesCustomPath ? Path.GetFullPath(custom) : RoamingRoot;
            Directory.CreateDirectory(DataRoot);
            Directory.CreateDirectory(LocalRoot);

            MahouUI.nPath = EnsureTrailingSeparator(DataRoot);
            MahouUI.mahou_folder_appd = DataRoot;
            MMain.C_SWITCH = UsesCustomPath;
            Configs.forceAppData = !UsesCustomPath;
            Configs.filePath = Path.Combine(DataRoot, "Mahou.ini");
            Logging.SetDirectory(Path.Combine(LocalRoot, "Logs"));

            MigrateLegacyFiles();
            QuarantineIncompatibleConfig();
            EnsureBundledFile("AS_dict.txt");
        }

        static string ParseCustomDirectory(string[] args) {
            if (args == null) return null;
            for (var i = 0; i < args.Length - 1; i++) {
                var arg = (args[i] ?? String.Empty).Trim();
                if (arg.Equals("/C", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-C", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("C", StringComparison.OrdinalIgnoreCase)) {
                    return args[i + 1];
                }
            }
            return null;
        }

        static string EnsureTrailingSeparator(string path) {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)) return path;
            return path + Path.DirectorySeparatorChar;
        }

        static void MigrateLegacyFiles() {
            if (PathsEqual(ExecutableRoot, DataRoot)) return;
            foreach (var name in MigratedFiles) {
                var source = Path.Combine(ExecutableRoot, name);
                var destination = Path.Combine(DataRoot, name);
                try {
                    if (File.Exists(source) && !File.Exists(destination)) File.Copy(source, destination, false);
                } catch (Exception ex) {
                    Debug.WriteLine("Legacy migration failed for " + name + ": " + ex.Message);
                }
            }
        }

        static void QuarantineIncompatibleConfig() {
            var config = Configs.filePath;
            if (!File.Exists(config)) return;
            try {
                var raw = File.ReadAllText(config);
                var modern = raw.IndexOf("[AutoSwitch]", StringComparison.OrdinalIgnoreCase) >= 0 &&
                             raw.IndexOf("ConvertLastWord_Key=", StringComparison.OrdinalIgnoreCase) >= 0;
                if (modern) return;
                var backup = config + ".legacy-v1.4." + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".bak";
                File.Move(config, backup);
            } catch (Exception ex) {
                Debug.WriteLine("Config compatibility check failed: " + ex.Message);
            }
        }

        internal static bool EnsureBundledFile(string name) {
            var source = Path.Combine(ExecutableRoot, name);
            var destination = Path.Combine(DataRoot, name);
            try {
                if (!File.Exists(source)) return File.Exists(destination);
                if (!File.Exists(destination)) File.Copy(source, destination, false);
                return File.Exists(destination);
            } catch (Exception ex) {
                Logging.Log("Bundled file migration failed for " + name + ": " + ex.Message, 2);
                return false;
            }
        }

        internal static bool RestoreBundledDictionary(string destination) {
            var source = Path.Combine(ExecutableRoot, "AS_dict.txt");
            try {
                if (!File.Exists(source)) return false;
                var destinationFull = Path.GetFullPath(destination);
                var sourceFull = Path.GetFullPath(source);
                if (!PathsEqual(sourceFull, destinationFull)) File.Copy(sourceFull, destinationFull, true);
                return File.Exists(destinationFull);
            } catch (Exception ex) {
                Logging.Log("Bundled dictionary restore failed: " + ex.Message, 1);
                return false;
            }
        }

        static bool PathsEqual(string left, string right) {
            return String.Equals(
                Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
