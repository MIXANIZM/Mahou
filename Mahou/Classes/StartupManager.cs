using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Mahou
{
    internal static class StartupManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "MIXANIZM Mahou";

        public static bool IsEnabled()
        {
            return IsRegistryRunEnabled() || LegacyShortcutExists();
        }

        public static void Enable()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (key == null)
                    throw new InvalidOperationException("Cannot open the current-user Run registry key.");

                key.SetValue(ValueName, Quote(Assembly.GetExecutingAssembly().Location), RegistryValueKind.String);
            }

            RemoveLegacyShortcutSafe();
        }

        public static void Disable()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (key != null)
                    key.DeleteValue(ValueName, false);
            }

            RemoveLegacyShortcutSafe();
        }

        public static void MigrateLegacyShortcutSafe()
        {
            try
            {
                if (LegacyShortcutExists())
                    Enable();
            }
            catch
            {
                // Startup migration must never block Mahou launch.
            }
        }

        private static bool IsRegistryRunEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                return key != null && key.GetValue(ValueName) != null;
        }

        private static bool LegacyShortcutExists()
        {
            return File.Exists(LegacyShortcutPath());
        }

        private static void RemoveLegacyShortcutSafe()
        {
            try
            {
                var shortcutPath = LegacyShortcutPath();
                if (File.Exists(shortcutPath))
                    File.Delete(shortcutPath);
            }
            catch
            {
                // If Windows temporarily locks the shortcut, registry startup is already enough.
            }
        }

        private static string LegacyShortcutPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Mahou.lnk");
        }

        private static string Quote(string path)
        {
            return "\"" + path + "\"";
        }
    }
}
