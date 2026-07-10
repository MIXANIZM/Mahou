using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Mahou
{
    internal static class AdaptiveLayoutLearning
    {
        private const string RulesFileName = "layout-learning.tsv";
        private const string KeyFileName = "layout-learning.key";
        private const int MaxRules = 5000;
        private static readonly object Sync = new object();
        private static readonly Dictionary<string, Rule> Rules = new Dictionary<string, Rule>();
        private static readonly KMHook.LowLevelProc Proc = HookCallback;
        private static readonly MethodInfo ConvertLastMethod = typeof(KMHook).GetMethod("ConvertLast", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly string[] SensitiveProcesses =
        {
            "keepass", "keepassxc", "1password", "bitwarden", "lastpass", "dashlane", "enpass",
            "chrome", "msedge", "firefox", "brave", "opera", "vivaldi", "iexplore",
            "credentialui", "logonui", "mstsc", "rdpclip", "cmd", "powershell", "pwsh", "conhost", "windowsterminal", "wt"
        };

        private static IntPtr hookId = IntPtr.Zero;
        private static bool loaded;
        private static bool suppressCurrentWord;
        private static DateTime ignoreUntilUtc = DateTime.MinValue;
        private static byte[] hmacKey;

        // Immutable-for-hook settings snapshot. Refreshed only by Start().
        private static bool enabled;
        private static bool autoConvertOnSpace;
        private static bool perAppRules;
        private static bool convertHotkeyEnabled;
        private static bool blockControl;
        private static bool doubleKey;
        private static bool hotkeyUsesControl;
        private static int minWordLength;
        private static int confirmationsToEnable;
        private static Hotkey convertHotkey;

        public static void Start()
        {
            Stop();
            EnsureConfigDefaults();
            LoadSettingsSnapshot();
            if (!enabled)
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
                loaded = false;
                hmacKey = null;
                DeleteIfExists(RulesFilePath());
                DeleteIfExists(RulesFilePath() + ".tmp");
                DeleteIfExists(RulesFilePath() + ".bak");
                DeleteIfExists(KeyFilePath());
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return KMHook.CallNextHookEx(hookId, nCode, wParam, lParam);

            if (!KMHook.self)
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
                        {
                            var snapshot = CaptureCurrentWord();
                            if (snapshot != null)
                                ThreadPool.QueueUserWorkItem(delegate { RecordManualCorrection(snapshot); });
                        }

                        if (IsAutoTrigger(message, key) && !suppressCurrentWord && ShouldAutoConvertCurrentWord())
                            ConvertCurrentWordWithoutEatingTrigger();

                        if (IsKeyDown(message) && IsWordBoundary(key))
                            suppressCurrentWord = false;
                    }
                }
                catch
                {
                    // Never break the global input chain.
                }
            }

            return KMHook.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private static bool IsConvertLastKeyUp(KMHook.KMMessages message, int vkCode)
        {
            if (!convertHotkeyEnabled || convertHotkey == null || MMain.mahou == null || KMHook.csdoing)
                return false;

            if (message != KMHook.KMMessages.WM_KEYUP && message != KMHook.KMMessages.WM_SYSKEYUP)
                return false;

            if (MMain.mahou.Active || MMain.mahou.moreConfigs.Active)
                return false;

            if (blockControl && hotkeyUsesControl)
                return false;

            if (doubleKey && !KMHook.hklOK)
                return false;

            return new Hotkey(NormalizeVkCode(vkCode), CurrentModifiers()).Equals(convertHotkey);
        }

        private static bool IsAutoTrigger(KMHook.KMMessages message, Keys key)
        {
            if (!autoConvertOnSpace || MMain.mahou == null || MMain.mahou.Active || MMain.mahou.moreConfigs.Active)
                return false;
            if (Control.ModifierKeys != Keys.None || !IsKeyDown(message))
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

        private static void RecordManualCorrection(Snapshot snapshot)
        {
            lock (Sync)
            {
                EnsureLoaded();
                Rule rule;
                if (!Rules.TryGetValue(snapshot.Hash, out rule))
                {
                    rule = new Rule { Hash = snapshot.Hash, Hits = 0, AutoEnabled = false, LastUsedUtc = DateTime.UtcNow };
                    Rules[rule.Hash] = rule;
                }

                rule.Hits++;
                rule.LastUsedUtc = DateTime.UtcNow;
                rule.AutoEnabled = rule.Hits >= confirmationsToEnable;
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
                Rule rule;
                return Rules.TryGetValue(snapshot.Hash, out rule) && rule.AutoEnabled && rule.Hits >= confirmationsToEnable;
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
            if (MMain.c_word == null || MMain.c_word.Count < minWordLength || !IsLearnable(MMain.c_word))
                return null;

            string activeApp = ActiveProcessName();
            if (IsSensitiveProcess(activeApp))
                return null;

            string signature = WordSignature(MMain.c_word);
            string appScope = perAppRules ? activeApp : String.Empty;
            string material = Locales.GetCurrentLocale() + "|" + appScope + "|" + signature;
            return new Snapshot { Hash = ComputeHash(material) };
        }

        private static bool IsSensitiveProcess(string processName)
        {
            if (String.IsNullOrWhiteSpace(processName))
                return true;

            foreach (var item in SensitiveProcesses)
                if (processName.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }

        private static bool IsLearnable(IList<KMHook.YuKey> word)
        {
            foreach (var key in word)
                if (key.altnum || key.yukey == Keys.Space)
                    return false;
            return true;
        }

        private static string WordSignature(IList<KMHook.YuKey> word)
        {
            var result = new StringBuilder();
            foreach (var key in word)
                result.Append((int)key.yukey).Append(':').Append(key.upper ? '1' : '0').Append(';');
            return result.ToString();
        }

        private static string ComputeHash(string material)
        {
            EnsureLearningKey();
            using (var hmac = new HMACSHA256(hmacKey))
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(material)));
        }

        private static void EnsureLearningKey()
        {
            if (hmacKey != null)
                return;

            Directory.CreateDirectory(Configs.dataPath);
            var path = KeyFilePath();
            if (File.Exists(path))
            {
                try
                {
                    var protectedBytes = File.ReadAllBytes(path);
                    hmacKey = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                    if (hmacKey.Length >= 32)
                        return;
                }
                catch
                {
                }
            }

            hmacKey = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(hmacKey);
            File.WriteAllBytes(path, ProtectedData.Protect(hmacKey, null, DataProtectionScope.CurrentUser));
        }

        private static void LoadSettingsSnapshot()
        {
            enabled = SafeReadBool("LayoutLearning", "Enabled", false);
            autoConvertOnSpace = SafeReadBool("LayoutLearning", "AutoConvertOnSpace", false);
            perAppRules = SafeReadBool("LayoutLearning", "PerAppRules", false);
            minWordLength = Clamp(SafeReadInt("LayoutLearning", "MinWordLength", 4), 2, 64);
            confirmationsToEnable = Clamp(SafeReadInt("LayoutLearning", "ConfirmationsToEnable", 2), 1, 20);
            convertHotkeyEnabled = SafeReadBool("EnabledHotkeys", "HKCLEnabled", true);
            blockControl = SafeReadBool("Functions", "BlockCTRL", false);
            doubleKey = SafeReadBool("DoubleKey", "Use", false);
            var modsText = MMain.MyConfs.Read("Hotkeys", "HKCLMods");
            hotkeyUsesControl = modsText.IndexOf("Control", StringComparison.OrdinalIgnoreCase) >= 0;
            int keyCode = SafeReadInt("Hotkeys", "HKCLKey", 19);
            convertHotkey = new Hotkey(keyCode, Hotkey.GetMods(modsText));
        }

        private static void EnsureConfigDefaults()
        {
            EnsureBool("LayoutLearning", "Enabled", false);
            EnsureBool("LayoutLearning", "AutoConvertOnSpace", false);
            EnsureBool("LayoutLearning", "PerAppRules", false);
            EnsureInt("LayoutLearning", "MinWordLength", 4, 2, 64);
            EnsureInt("LayoutLearning", "ConfirmationsToEnable", 2, 1, 20);
        }

        private static void EnsureBool(string section, string key, bool defaultValue)
        {
            bool value;
            if (!Boolean.TryParse(MMain.MyConfs.Read(section, key), out value))
                MMain.MyConfs.Write(section, key, defaultValue.ToString());
        }

        private static void EnsureInt(string section, string key, int defaultValue, int minimum, int maximum)
        {
            int value;
            if (!Int32.TryParse(MMain.MyConfs.Read(section, key), out value) || value < minimum || value > maximum)
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

        private static int Clamp(int value, int minimum, int maximum)
        {
            return value < minimum ? minimum : value > maximum ? maximum : value;
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

        private static void EnsureLoaded()
        {
            lock (Sync)
            {
                if (loaded)
                    return;

                loaded = true;
                Rules.Clear();
                var file = RulesFilePath();
                if (!File.Exists(file))
                    return;

                var lines = File.ReadAllLines(file, Encoding.UTF8);
                if (lines.Length > 0 && !lines[0].StartsWith("# hash\t"))
                {
                    DeleteIfExists(file);
                    DeleteIfExists(file + ".tmp");
                    DeleteIfExists(file + ".bak");
                    return;
                }

                foreach (var line in lines)
                {
                    if (String.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var parts = line.Split('\t');
                    int hits;
                    bool autoEnabled;
                    DateTime lastUsedUtc;
                    if (parts.Length != 4 || !Int32.TryParse(parts[1], out hits) ||
                        !Boolean.TryParse(parts[2], out autoEnabled) || !DateTime.TryParse(parts[3], out lastUsedUtc))
                        continue;

                    Rules[parts[0]] = new Rule
                    {
                        Hash = parts[0], Hits = Math.Max(0, hits), AutoEnabled = autoEnabled, LastUsedUtc = lastUsedUtc
                    };
                }

                while (Rules.Count > MaxRules)
                    TrimRulesIfNeeded();
            }
        }

        private static void TrimRulesIfNeeded()
        {
            if (Rules.Count <= MaxRules)
                return;

            string oldestKey = null;
            DateTime oldestDate = DateTime.MaxValue;
            foreach (var pair in Rules)
                if (pair.Value.LastUsedUtc < oldestDate) { oldestKey = pair.Key; oldestDate = pair.Value.LastUsedUtc; }
            if (oldestKey != null)
                Rules.Remove(oldestKey);
        }

        private static void SaveRulesAtomically()
        {
            Directory.CreateDirectory(Configs.dataPath);
            var file = RulesFilePath();
            var tempFile = file + ".tmp";
            var backupFile = file + ".bak";
            var lines = new List<string> { "# hash\thits\tautoEnabled\tlastUsedUtc" };
            foreach (var rule in Rules.Values)
                lines.Add(String.Join("\t", rule.Hash, rule.Hits, rule.AutoEnabled, rule.LastUsedUtc.ToString("o")));

            File.WriteAllLines(tempFile, lines.ToArray(), Encoding.UTF8);
            if (!File.Exists(file)) { File.Move(tempFile, file); return; }
            File.Replace(tempFile, file, backupFile, true);
        }

        private static void DeleteIfExists(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        private static string RulesFilePath() { return Path.Combine(Configs.dataPath, RulesFileName); }
        private static string KeyFilePath() { return Path.Combine(Configs.dataPath, KeyFileName); }

        private static string ActiveProcessName()
        {
            try
            {
                uint processId;
                GetWindowThreadProcessId(GetForegroundWindow(), out processId);
                return Process.GetProcessById((int)processId).ProcessName.ToLowerInvariant();
            }
            catch { return String.Empty; }
        }

        private sealed class Snapshot { public string Hash; }
        private sealed class Rule
        {
            public string Hash;
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
