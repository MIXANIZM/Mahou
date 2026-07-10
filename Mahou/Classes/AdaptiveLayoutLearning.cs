using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Mahou
{
    internal static class AdaptiveLayoutLearning
    {
        private const string RulesFileName = "layout-learning.tsv";
        private const string KeyFileName = "layout-learning.key";
        private const string RulesHeader = "# MIXANIZM-LAYOUT-LEARNING-V2";
        private const int MaxRules = 5000;

        private static readonly object Sync = new object();
        private static readonly object SaveIoSync = new object();
        private static readonly Dictionary<string, Rule> Rules = new Dictionary<string, Rule>();
        private static readonly string[] SensitiveProcesses =
        {
            "keepass", "keepassxc", "1password", "bitwarden", "lastpass", "dashlane", "enpass",
            "chrome", "msedge", "firefox", "brave", "opera", "vivaldi", "iexplore",
            "credentialui", "logonui", "mstsc", "rdpclip", "cmd", "powershell", "pwsh", "conhost",
            "windowsterminal", "wt"
        };

        private static System.Threading.Timer foregroundTimer;
        private static byte[] hmacKey;
        private static bool loaded;
        private static bool suppressCurrentWord;
        private static int saveQueued;

        private static volatile bool enabled;
        private static volatile bool autoConvertOnSpace;
        private static volatile bool perAppRules;
        private static volatile bool sensitiveContext = true;
        private static volatile int minWordLength = 4;
        private static volatile int confirmationsToEnable = 2;
        private static string activeProcessName = String.Empty;

        public static void Start()
        {
            Stop();
            EnsureConfigDefaults();
            LoadSettingsSnapshot();
            suppressCurrentWord = false;

            if (!enabled)
                return;

            EnsureLearningKey();
            EnsureLoaded();
            UpdateForegroundContext(null);
            foregroundTimer = new System.Threading.Timer(UpdateForegroundContext, null, 500, 500);
        }

        public static void Stop()
        {
            var timer = foregroundTimer;
            foregroundTimer = null;
            if (timer != null)
                timer.Dispose();

            FlushPendingSave();
        }

        public static void OnBackspace()
        {
            if (enabled)
                suppressCurrentWord = true;
        }

        public static void OnBoundary()
        {
            suppressCurrentWord = false;
        }

        public static void RecordManualCorrection(IList<KMHook.YuKey> word)
        {
            if (!enabled || sensitiveContext || suppressCurrentWord)
                return;

            string hash = CaptureHash(word);
            if (String.IsNullOrEmpty(hash))
                return;

            lock (Sync)
            {
                Rule rule;
                if (!Rules.TryGetValue(hash, out rule))
                {
                    rule = new Rule
                    {
                        Hash = hash,
                        Hits = 0,
                        AutoEnabled = false,
                        LastUsedUtc = DateTime.UtcNow
                    };
                    Rules[hash] = rule;
                }

                rule.Hits++;
                rule.AutoEnabled = rule.Hits >= confirmationsToEnable;
                rule.LastUsedUtc = DateTime.UtcNow;
                TrimRulesIfNeeded();
            }

            QueueSave();
        }

        public static bool ShouldAutoConvert(IList<KMHook.YuKey> word)
        {
            if (!enabled || !autoConvertOnSpace || sensitiveContext || suppressCurrentWord)
                return false;

            string hash = CaptureHash(word);
            if (String.IsNullOrEmpty(hash))
                return false;

            lock (Sync)
            {
                Rule rule;
                return Rules.TryGetValue(hash, out rule) &&
                    rule.AutoEnabled && rule.Hits >= confirmationsToEnable;
            }
        }

        public static void ClearRules()
        {
            lock (Sync)
            {
                Rules.Clear();
                loaded = false;
                hmacKey = null;
            }

            Interlocked.Exchange(ref saveQueued, 0);
            lock (SaveIoSync)
            {
                DeleteIfExists(RulesFilePath());
                DeleteIfExists(RulesFilePath() + ".tmp");
                DeleteIfExists(RulesFilePath() + ".bak");
                DeleteIfExists(KeyFilePath());
            }

            if (enabled)
            {
                EnsureLearningKey();
                loaded = true;
            }
        }

        private static string CaptureHash(IList<KMHook.YuKey> word)
        {
            if (word == null || word.Count < minWordLength || hmacKey == null)
                return null;

            var material = new StringBuilder(64 + word.Count * 8);
            material.Append(Locales.GetCurrentLocale()).Append('|');
            if (perAppRules)
                material.Append(activeProcessName ?? String.Empty);
            material.Append('|');

            foreach (var key in word)
            {
                if (key.altnum || key.yukey == System.Windows.Forms.Keys.Space)
                    return null;

                material.Append((int)key.yukey)
                    .Append(':')
                    .Append(key.upper ? '1' : '0')
                    .Append(';');
            }

            byte[] keyCopy = hmacKey;
            using (var hmac = new HMACSHA256(keyCopy))
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(material.ToString())));
        }

        private static void UpdateForegroundContext(object state)
        {
            string processName = ActiveProcessName();
            activeProcessName = processName;
            sensitiveContext = IsSensitiveProcess(processName);
        }

        private static string ActiveProcessName()
        {
            try
            {
                uint processId;
                GetWindowThreadProcessId(GetForegroundWindow(), out processId);
                if (processId == 0)
                    return String.Empty;

                using (var process = Process.GetProcessById((int)processId))
                    return process.ProcessName.ToLowerInvariant();
            }
            catch
            {
                return String.Empty;
            }
        }

        private static bool IsSensitiveProcess(string processName)
        {
            if (String.IsNullOrWhiteSpace(processName))
                return true;

            foreach (string item in SensitiveProcesses)
                if (processName.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

            return false;
        }

        private static void LoadSettingsSnapshot()
        {
            enabled = SafeReadBool("LayoutLearning", "Enabled", false);
            autoConvertOnSpace = SafeReadBool("LayoutLearning", "AutoConvertOnSpace", false);
            perAppRules = SafeReadBool("LayoutLearning", "PerAppRules", false);
            minWordLength = Clamp(SafeReadInt("LayoutLearning", "MinWordLength", 4), 2, 64);
            confirmationsToEnable = Clamp(SafeReadInt("LayoutLearning", "ConfirmationsToEnable", 2), 1, 20);
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

        private static void EnsureLearningKey()
        {
            if (hmacKey != null)
                return;

            Directory.CreateDirectory(Configs.dataPath);
            string path = KeyFilePath();
            if (File.Exists(path))
            {
                try
                {
                    byte[] protectedBytes = File.ReadAllBytes(path);
                    byte[] candidate = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                    if (candidate != null && candidate.Length >= 32)
                    {
                        hmacKey = candidate;
                        return;
                    }
                }
                catch
                {
                }
            }

            var newKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(newKey);

            byte[] protectedKey = ProtectedData.Protect(newKey, null, DataProtectionScope.CurrentUser);
            WriteBytesAtomically(path, protectedKey);
            hmacKey = newKey;
        }

        private static void EnsureLoaded()
        {
            lock (Sync)
            {
                if (loaded)
                    return;

                loaded = true;
                Rules.Clear();
                string file = RulesFilePath();
                if (!File.Exists(file))
                    return;

                string[] lines;
                try
                {
                    lines = File.ReadAllLines(file, Encoding.UTF8);
                }
                catch
                {
                    return;
                }

                if (lines.Length == 0 || !String.Equals(lines[0], RulesHeader, StringComparison.Ordinal))
                {
                    DeleteIfExists(file);
                    DeleteIfExists(file + ".tmp");
                    DeleteIfExists(file + ".bak");
                    return;
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split('\t');
                    int hits;
                    bool autoEnabled;
                    DateTime lastUsedUtc;
                    if (parts.Length != 4 || String.IsNullOrWhiteSpace(parts[0]) ||
                        !Int32.TryParse(parts[1], out hits) ||
                        !Boolean.TryParse(parts[2], out autoEnabled) ||
                        !DateTime.TryParse(parts[3], out lastUsedUtc))
                        continue;

                    Rules[parts[0]] = new Rule
                    {
                        Hash = parts[0],
                        Hits = Math.Max(0, hits),
                        AutoEnabled = autoEnabled,
                        LastUsedUtc = lastUsedUtc.ToUniversalTime()
                    };
                }

                while (Rules.Count > MaxRules)
                    TrimRulesIfNeeded();
            }
        }

        private static void QueueSave()
        {
            if (Interlocked.Exchange(ref saveQueued, 1) != 0)
                return;

            ThreadPool.QueueUserWorkItem(delegate
            {
                while (Interlocked.Exchange(ref saveQueued, 0) != 0)
                    SaveRulesSnapshot();
            });
        }

        private static void FlushPendingSave()
        {
            if (Interlocked.Exchange(ref saveQueued, 0) != 0)
                SaveRulesSnapshot();
        }

        private static void SaveRulesSnapshot()
        {
            List<Rule> snapshot;
            lock (Sync)
            {
                snapshot = new List<Rule>(Rules.Count);
                foreach (Rule rule in Rules.Values)
                {
                    snapshot.Add(new Rule
                    {
                        Hash = rule.Hash,
                        Hits = rule.Hits,
                        AutoEnabled = rule.AutoEnabled,
                        LastUsedUtc = rule.LastUsedUtc
                    });
                }
            }

            var lines = new List<string>(snapshot.Count + 1) { RulesHeader };
            foreach (Rule rule in snapshot)
            {
                lines.Add(String.Join("\t", new[]
                {
                    rule.Hash,
                    rule.Hits.ToString(),
                    rule.AutoEnabled.ToString(),
                    rule.LastUsedUtc.ToString("o")
                }));
            }

            lock (SaveIoSync)
            {
                Directory.CreateDirectory(Configs.dataPath);
                string file = RulesFilePath();
                string temp = file + ".tmp";
                string backup = file + ".bak";
                File.WriteAllLines(temp, lines.ToArray(), Encoding.UTF8);

                if (!File.Exists(file))
                {
                    File.Move(temp, file);
                    return;
                }

                try
                {
                    File.Replace(temp, file, backup, true);
                }
                catch
                {
                    File.Copy(temp, file, true);
                    File.Delete(temp);
                }
            }
        }

        private static void TrimRulesIfNeeded()
        {
            if (Rules.Count <= MaxRules)
                return;

            string oldestKey = null;
            DateTime oldestDate = DateTime.MaxValue;
            foreach (KeyValuePair<string, Rule> pair in Rules)
            {
                if (pair.Value.LastUsedUtc < oldestDate)
                {
                    oldestDate = pair.Value.LastUsedUtc;
                    oldestKey = pair.Key;
                }
            }

            if (!String.IsNullOrEmpty(oldestKey))
                Rules.Remove(oldestKey);
        }

        private static void WriteBytesAtomically(string path, byte[] bytes)
        {
            string temp = path + ".tmp";
            File.WriteAllBytes(temp, bytes);
            if (File.Exists(path))
                File.Delete(path);
            File.Move(temp, path);
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

        private static string RulesFilePath()
        {
            return Path.Combine(Configs.dataPath, RulesFileName);
        }

        private static string KeyFilePath()
        {
            return Path.Combine(Configs.dataPath, KeyFileName);
        }

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
