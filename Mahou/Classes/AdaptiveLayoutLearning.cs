using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Mahou
{
    internal static class AdaptiveLayoutLearning
    {
        private const string DataDirectoryName = "MIXANIZM Mahou";
        private const string RulesFileName = "layout-learning.tsv";
        private const int MaxRules = 5000;
        private static readonly object Sync = new object();
        private static readonly Dictionary<string, Rule> Rules = new Dictionary<string, Rule>();
        private static readonly KMHook.LowLevelProc Proc = HookCallback;
        private static readonly MethodInfo ConvertLastMethod = typeof(KMHook).GetMethod("ConvertLast", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly string[] SensitiveProcesses =
        {
            "keepass", "keepassxc", "1password", "bitwarden", "lastpass", "dashlane", "enpass"
        };
        private static IntPtr hookId = IntPtr.Zero;
        private static bool loaded;
        private static bool suppressCurrentWord;
        private static DateTime ignoreUntilUtc = DateTime.MinValue;

        public static void Start()
        {
            EnsureConfigDefaults();
            if (!Enabled() || hookId != IntPtr.Zero)
                return;

            EnsureLoaded();
            hookId = KMHook.SetHook(Proc, (int)KMHook.KMMessages.WH_KEYBOARD_LL);
        }

        public static void Stop()
        {
            if (hookId == IntPtr.Zero)
                return;

            KMHook.UnhookWindowsHookEx(hookId);
            hookId = IntPtr.Zero;
        }

        public static void ClearRules()
        {
            lock (Sync)
            {
                Rules.Clear();
                loaded = true;
                DeleteIfExists(RulesFilePath());
                DeleteIfExists(RulesFilePath() + ".tmp");
                DeleteIfExists(RulesFilePath() + ".bak");
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && !KMHook.self)
            {
                try
                {
                    var message = (KMHook.KMMessages)(int)wParam;
                    int vkCode = Marshal.ReadInt32(lParam);
                    var key = (Keys)vkCode;

                    if (DateTime.UtcNow >= ignoreUntilUtc)
                    {
                        if (IsKeyDown(message) && key == Keys.Back)
                            suppressCurrentWord = true;

                        if (IsConvertLastKeyUp(message, vkCode) && !suppressCurrentWord)
                            RecordManualCorrection();

                        if (IsAutoTrigger(message, key) && !suppressCurrentWord && ShouldAutoConvertCurrentWord())
                            ConvertCurrentWordWithoutEatingTrigger();

                        if (IsKeyDown(message) && IsWordBoundary(key))
                            suppressCurrentWord = false;
                    }
                }
                catch
                {
                    // Adaptive learning must never break the original keyboard hook chain.
                }
            }

            return KMHook.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private static bool IsConvertLastKeyUp(KMHook.KMMessages message, int vkCode)
        {
            if (message != KMHook.KMMessages.WM_KEYUP && message != KMHook.KMMessages.WM_SYSKEYUP)
                return false;

            if (!SafeReadBool("EnabledHotkeys", "HKCLEnabled", true) || MMain.mahou == null || KMHook.csdoing)
                return false;

            if (MMain.mahou.Active || MMain.mahou.moreConfigs.Active)
                return false;

            if (SafeReadBool("Functions", "BlockCTRL", false) &&
                MMain.MyConfs.Read("Hotkeys", "HKCLMods").IndexOf("Control", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            if (SafeReadBool("DoubleKey", "Use", false) && !KMHook.hklOK)
                return false;

            var hotkey = new Hotkey(NormalizeVkCode(vkCode), CurrentModifiers());
            return hotkey.Equals(MMain.mahou.HKCLast);
        }

        private static bool IsAutoTrigger(KMHook.KMMessages message, Keys key)
        {
            if (!AutoConvertOnSpace())
                return false;

            if (MMain.mahou == null || MMain.mahou.Active || MMain.mahou.moreConfigs.Active)
                return false;

            if (Control.ModifierKeys != Keys.None)
                return false;

            if (!IsKeyDown(message))
                return false;

            return key == Keys.Space || key == Keys.Enter || key == Keys.Return;
        }

        private static bool IsKeyDown(KMHook.KMMessages message)
        {
            return message == KMHook.KMMessages.WM_KEYDOWN || message == KMHook.KMMessages.WM_SYSKEYDOWN;
        }

        private static bool IsWordBoundary(Keys key)
        {
            return key == Keys.Space || key == Keys.Enter || key == Keys.Return || key == Keys.Tab ||
                key == Keys.Home || key == Keys.End || key == Keys.Left || key == Keys.Right ||
                key == Keys.Up || key == Keys.Down || key == Keys.PageUp || key == Keys.PageDown;
        }

        private static void RecordManualCorrection()
        {
            var snapshot = CaptureCurrentWord();
            if (snapshot == null)
                return;

            lock (Sync)
            {
                EnsureLoaded();
                Rule rule;
                if (!Rules.TryGetValue(snapshot.Key, out rule))
                {
                    rule = new Rule
                    {
                        Key = snapshot.Key,
                        SourceLocale = snapshot.SourceLocale,
                        Signature = snapshot.Signature,
                        AppName = snapshot.AppName,
                        SourcePreview = snapshot.SourcePreview,
                        Hits = 0,
                        AutoEnabled = false,
                        LastUsedUtc = DateTime.UtcNow
                    };
                    Rules[rule.Key] = rule;
                }

                rule.Hits++;
                rule.SourcePreview = snapshot.SourcePreview;
                rule.LastUsedUtc = DateTime.UtcNow;
                rule.AutoEnabled = rule.Hits >= ConfirmationsToEnable();
                TrimRulesIfNeeded();
                SaveRulesAtomically();
            }
        }

        private static bool ShouldAutoConvertCurrentWord()
        {
            var snapshot = CaptureCurrentWord();
            if (snapshot == null)
                return false;

            lock (Sync)
            {
                EnsureLoaded();
                Rule rule;
                return Rules.TryGetValue(snapshot.Key, out rule) && rule.AutoEnabled && rule.Hits >= ConfirmationsToEnable();
            }
        }

        private static void ConvertCurrentWordWithoutEatingTrigger()
        {
            if (ConvertLastMethod == null || MMain.c_word == null || MMain.c_word.Count == 0)
                return;

            ignoreUntilUtc = DateTime.UtcNow.AddMilliseconds(250);
            ConvertLastMethod.Invoke(null, new object[] { MMain.c_word });
        }

        private static Snapshot CaptureCurrentWord()
        {
            if (MMain.c_word == null || MMain.c_word.Count < MinWordLength())
                return null;

            if (!IsLearnable(MMain.c_word))
                return null;

            string activeApp = ActiveProcessName();
            if (IsSensitiveProcess(activeApp))
                return null;

            uint locale = Locales.GetCurrentLocale();
            string signature = WordSignature(MMain.c_word);
            string appName = PerAppRules() ? activeApp : String.Empty;

            return new Snapshot
            {
                Key = MakeRuleKey(locale, signature, appName),
                SourceLocale = locale,
                Signature = signature,
                AppName = appName,
                SourcePreview = WordPreview(MMain.c_word)
            };
        }

        private static bool IsSensitiveProcess(string processName)
        {
            if (String.IsNullOrWhiteSpace(processName))
                return false;

            foreach (var item in SensitiveProcesses)
            {
                if (processName.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static bool IsLearnable(IList<KMHook.YuKey> word)
        {
            foreach (var key in word)
            {
                if (key.altnum || key.yukey == Keys.Space)
                    return false;
            }
            return true;
        }

        private static string WordSignature(IList<KMHook.YuKey> word)
        {
            var result = new StringBuilder();
            foreach (var key in word)
                result.Append((int)key.yukey).Append(':').Append(key.upper ? '1' : '0').Append(';');
            return result.ToString();
        }

        private static string WordPreview(IList<KMHook.YuKey> word)
        {
            var result = new StringBuilder();
            foreach (var key in word)
                result.Append(key.yukey).Append(' ');
            return result.ToString().Trim();
        }

        private static int NormalizeVkCode(int vkCode)
        {
            if (vkCode == 160 || vkCode == 161) return 16;
            if (vkCode == 162 || vkCode == 163) return 17;
            if (vkCode == 164 || vkCode == 165) return 18;
            if (vkCode == 240) return 20;
            return vkCode;
        }

        private static bool[] CurrentModifiers()
        {
            var modifiers = Control.ModifierKeys;
            return new[]
            {
                (modifiers & Keys.Control) == Keys.Control,
                (modifiers & Keys.Shift) == Keys.Shift,
                (modifiers & Keys.Alt) == Keys.Alt
            };
        }

        private static bool Enabled()
        {
            return SafeReadBool("LayoutLearning", "Enabled", true);
        }

        private static bool AutoConvertOnSpace()
        {
            return SafeReadBool("LayoutLearning", "AutoConvertOnSpace", false);
        }

        private static bool PerAppRules()
        {
            return SafeReadBool("LayoutLearning", "PerAppRules", false);
        }

        private static int MinWordLength()
        {
            return Math.Max(1, SafeReadInt("LayoutLearning", "MinWordLength", 4));
        }

        private static int ConfirmationsToEnable()
        {
            return Math.Max(1, SafeReadInt("LayoutLearning", "ConfirmationsToEnable", 2));
        }

        private static void EnsureConfigDefaults()
        {
            EnsureBool("LayoutLearning", "Enabled", true);
            EnsureBool("LayoutLearning", "AutoConvertOnSpace", false);
            EnsureBool("LayoutLearning", "PerAppRules", false);
            EnsureInt("LayoutLearning", "MinWordLength", 4);
            EnsureInt("LayoutLearning", "ConfirmationsToEnable", 2);
        }

        private static void EnsureBool(string section, string key, bool defaultValue)
        {
            bool value;
            if (!Boolean.TryParse(MMain.MyConfs.Read(section, key), out value))
                MMain.MyConfs.Write(section, key, defaultValue.ToString());
        }

        private static void EnsureInt(string section, string key, int defaultValue)
        {
            int value;
            if (!Int32.TryParse(MMain.MyConfs.Read(section, key), out value))
                MMain.MyConfs.Write(section, key, defaultValue.ToString());
        }

        private static bool SafeReadBool(string section, string key, bool defaultValue)
        {
            bool value;
            return Boolean.TryParse(MMain.MyConfs.Read(section, key), out value) ? value : defaultValue;
        }

        private static int SafeReadInt(string section, string key, int defaultValue)
        {
            int value;
            return Int32.TryParse(MMain.MyConfs.Read(section, key), out value) ? value : defaultValue;
        }

        private static void EnsureLoaded()
        {
            if (loaded)
                return;

            loaded = true;
            var file = RulesFilePath();
            if (!File.Exists(file))
                return;

            foreach (var line in File.ReadAllLines(file, Encoding.UTF8))
            {
                if (String.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('\t');
                uint locale;
                int hits;
                bool autoEnabled;
                DateTime lastUsedUtc;
                if (parts.Length != 7 || !UInt32.TryParse(parts[0], out locale) ||
                    !Int32.TryParse(parts[4], out hits) || !Boolean.TryParse(parts[5], out autoEnabled) ||
                    !DateTime.TryParse(parts[6], out lastUsedUtc))
                    continue;

                var key = MakeRuleKey(locale, parts[1], parts[2]);
                Rules[key] = new Rule
                {
                    Key = key,
                    SourceLocale = locale,
                    Signature = parts[1],
                    AppName = parts[2],
                    SourcePreview = Decode(parts[3]),
                    Hits = hits,
                    AutoEnabled = autoEnabled,
                    LastUsedUtc = lastUsedUtc
                };
            }

            while (Rules.Count > MaxRules)
                TrimRulesIfNeeded();
        }

        private static void TrimRulesIfNeeded()
        {
            if (Rules.Count <= MaxRules)
                return;

            var oldestKey = String.Empty;
            var oldestDate = DateTime.MaxValue;
            foreach (var pair in Rules)
            {
                if (pair.Value.LastUsedUtc < oldestDate)
                {
                    oldestKey = pair.Key;
                    oldestDate = pair.Value.LastUsedUtc;
                }
            }

            if (!String.IsNullOrEmpty(oldestKey))
                Rules.Remove(oldestKey);
        }

        private static void SaveRulesAtomically()
        {
            Directory.CreateDirectory(DataDirectoryPath());
            var file = RulesFilePath();
            var tempFile = file + ".tmp";
            var backupFile = file + ".bak";
            var lines = new List<string> { "# sourceLocale\tsignature\tappName\tsourcePreviewBase64\thits\tautoEnabled\tlastUsedUtc" };
            foreach (var rule in Rules.Values)
            {
                lines.Add(String.Join("\t", new[]
                {
                    rule.SourceLocale.ToString(),
                    rule.Signature,
                    rule.AppName ?? String.Empty,
                    Encode(rule.SourcePreview),
                    rule.Hits.ToString(),
                    rule.AutoEnabled.ToString(),
                    rule.LastUsedUtc.ToString("o")
                }));
            }

            File.WriteAllLines(tempFile, lines.ToArray(), Encoding.UTF8);
            if (!File.Exists(file))
            {
                File.Move(tempFile, file);
                return;
            }

            try
            {
                File.Replace(tempFile, file, backupFile, true);
            }
            catch
            {
                File.Copy(tempFile, file, true);
                File.Delete(tempFile);
            }
        }

        private static void DeleteIfExists(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }
        }

        private static string MakeRuleKey(uint locale, string signature, string appName)
        {
            return locale + "|" + appName + "|" + signature;
        }

        private static string DataDirectoryPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DataDirectoryName);
        }

        private static string RulesFilePath()
        {
            return Path.Combine(DataDirectoryPath(), RulesFileName);
        }

        private static string Encode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? String.Empty));
        }

        private static string Decode(string value)
        {
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(value)); }
            catch { return String.Empty; }
        }

        private static string ActiveProcessName()
        {
            try
            {
                uint processId;
                GetWindowThreadProcessId(GetForegroundWindow(), out processId);
                return Process.GetProcessById((int)processId).ProcessName.ToLowerInvariant();
            }
            catch
            {
                return String.Empty;
            }
        }

        private class Snapshot
        {
            public string Key;
            public uint SourceLocale;
            public string Signature;
            public string AppName;
            public string SourcePreview;
        }

        private class Rule : Snapshot
        {
            public int Hits;
            public bool AutoEnabled;
            public DateTime LastUsedUtc;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr windowHandle, out uint processId);
    }
}
