using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Mahou {
    internal static class StartupManager {
        const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string ValueName = "MIXANIZM Mahou";

        internal static bool IsEnabled() {
            try {
                using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false)) {
                    var value = key == null ? null : key.GetValue(ValueName) as string;
                    if (String.IsNullOrWhiteSpace(value)) return false;
                    return NormalizeCommand(value) == NormalizeCommand(BuildCommand());
                }
            } catch (Exception ex) {
                Logging.Log("Cannot read startup registry value: " + ex.Message, 2);
                return false;
            }
        }

        internal static void SetEnabled(bool enabled) {
            try {
                using (var key = Registry.CurrentUser.CreateSubKey(RunKeyPath)) {
                    if (enabled) key.SetValue(ValueName, BuildCommand(), RegistryValueKind.String);
                    else key.DeleteValue(ValueName, false);
                }
                MigrateLegacyStartup();
            } catch (Exception ex) {
                Logging.Log("Cannot update startup registry value: " + ex.Message, 1);
                throw;
            }
        }

        internal static void MigrateLegacyStartup() {
            try {
                var shortcut = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Mahou.lnk");
                if (File.Exists(shortcut)) File.Delete(shortcut);
            } catch (Exception ex) {
                Logging.Log("Cannot remove legacy startup shortcut: " + ex.Message, 2);
            }

            try {
                var psi = new ProcessStartInfo {
                    FileName = "schtasks.exe",
                    Arguments = "/Delete /TN MahouAutoStart+ /F",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (var process = Process.Start(psi)) {
                    if (process != null) process.WaitForExit(3000);
                }
            } catch (Exception ex) {
                Logging.Log("Cannot remove legacy elevated startup task: " + ex.Message, 2);
            }
        }

        static string BuildCommand() {
            return "\"" + Assembly.GetExecutingAssembly().Location + "\"";
        }

        static string NormalizeCommand(string value) {
            return (value ?? String.Empty).Trim().Trim('"').Replace('/', '\\');
        }
    }
}
