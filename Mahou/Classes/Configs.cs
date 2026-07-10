using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Mahou
{
    class Configs
    {
        public const string DataDirectoryName = "MIXANIZM Mahou";
        public static readonly string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DataDirectoryName);
        public static readonly string legacyFilePath = Path.Combine(Update.nPath, "Mahou.ini");
        public static readonly string filePath = Path.Combine(dataPath, "Mahou.ini");

        public Configs()
        {
            EnsureConfigLocation();

            EnsureInt("Hotkeys", "HKCLKey", 19, 0, 255);
            EnsureString("Hotkeys", "HKCLMods", "None");
            EnsureInt("Hotkeys", "HKCSKey", 145, 0, 255);
            EnsureString("Hotkeys", "HKCSMods", "None");
            EnsureInt("Hotkeys", "HKCLineKey", 19, 0, 255);
            EnsureString("Hotkeys", "HKCLineMods", "Shift");
            EnsureString("Hotkeys", "OnlyKeyLayoutSwicth", "CapsLock");
            EnsureInt("Hotkeys", "HKSymIgnKey", 122, 0, 255);
            EnsureString("Hotkeys", "HKSymIgnMods", "Shift + Control + Alt");
            EnsureInt("Hotkeys", "HKConvertMore", 122, 0, 255);
            EnsureString("Hotkeys", "HKConvertMoreMods", "Shift + Control");

            EnsureOptionalUInt("Locales", "locale1uId");
            EnsureOptionalUInt("Locales", "locale2uId");
            EnsureStringAllowEmpty("Locales", "locale1Lang", String.Empty);
            EnsureStringAllowEmpty("Locales", "locale2Lang", String.Empty);
            EnsureLanguage();

            EnsureBool("Functions", "IconVisibility", true);
            EnsureBool("Functions", "CycleMode", false);
            EnsureBool("Functions", "EmulateLayoutSwitch", false);
            EnsureInt("Functions", "ELSType", 0, 0, 2);
            EnsureBool("Functions", "CSSwitch", true);
            EnsureBool("Functions", "BlockCTRL", false);
            EnsureBool("Functions", "RePress", true);
            EnsureBool("Functions", "EatOneSpace", false);
            EnsureBool("Functions", "ReSelect", true);
            EnsureBool("Functions", "SymIgnModeEnabled", false);
            EnsureBool("Functions", "MoreTries", true);
            EnsureInt("Functions", "TriesCount", 5, 1, 50);
            EnsureBool("Functions", "DisplayLang", false);
            EnsureInt("Functions", "DLRefreshRate", 50, 10, 5000);
            EnsureColor("Functions", "DLForeColor", "#FFFFFF");
            EnsureColor("Functions", "DLBackColor", "#000000");
            EnsureBool("Functions", "ExperimentalCSSwitch", false);
            EnsureBool("Functions", "Snippets", false);
            EnsureBool("Functions", "DTTOnChange", false);
            EnsureBool("Functions", "ScrollTip", false);
            Write("Functions", "UpdatesEnabled", "false");

            EnsureBool("EnabledHotkeys", "HKCLEnabled", true);
            // Temporarily disabled: the legacy selection converter cannot preserve every clipboard format.
            Write("EnabledHotkeys", "HKCSEnabled", "false");
            EnsureBool("EnabledHotkeys", "HKCLineEnabled", true);
            EnsureBool("EnabledHotkeys", "HKSymIgnEnabled", true);

            EnsureBool("ExtCtrls", "UseExtCtrls", false);
            EnsureOptionalInt("ExtCtrls", "LCLocale");
            EnsureStringAllowEmpty("ExtCtrls", "LCLocaleName", String.Empty);
            EnsureOptionalInt("ExtCtrls", "RCLocale");
            EnsureStringAllowEmpty("ExtCtrls", "RCLocaleName", String.Empty);

            EnsureStringAllowEmpty("Proxy", "ServerPort", String.Empty);
            EnsureStringAllowEmpty("Proxy", "UserName", String.Empty);
            EnsureStringAllowEmpty("Proxy", "Password", String.Empty);

            EnsureInt("TTipUI", "Height", 14, 1, 500);
            EnsureInt("TTipUI", "Width", 16, 1, 500);
            EnsureFont("TTipUI", "Font", "Georgia; 8pt");
            EnsureInt("TTipUI", "xpos", 8, -10000, 10000);
            EnsureInt("TTipUI", "ypos", 0, -10000, 10000);
            EnsureBool("TTipUI", "TransparentBack", false);

            EnsureBool("DoubleKey", "Use", false);
            EnsureInt("DoubleKey", "Delay", 350, 50, 5000);
        }

        private static void EnsureConfigLocation()
        {
            Directory.CreateDirectory(dataPath);
            if (!File.Exists(filePath) && File.Exists(legacyFilePath))
            {
                try { File.Copy(legacyFilePath, filePath, false); }
                catch { }
            }

            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "!Unicode(✔), Mahou settings file", Encoding.Unicode);
        }

        public void Write(string section, string key, string value)
        {
            string storedValue = value ?? String.Empty;
            if (SecretProtector.IsProxyPassword(section, key))
                storedValue = SecretProtector.Protect(storedValue);

            if (!WritePrivateProfileString(section, key, storedValue, filePath))
                throw new IOException("Mahou could not save setting [" + section + "] " + key + ".");
        }

        public string Read(string section, string key)
        {
            var buffer = new StringBuilder(4096);
            GetPrivateProfileString(section, key, String.Empty, buffer, buffer.Capacity, filePath);
            string rawValue = buffer.ToString();

            if (!SecretProtector.IsProxyPassword(section, key))
                return rawValue;

            string plaintext;
            if (SecretProtector.TryUnprotect(rawValue, out plaintext))
                return plaintext;

            if (!String.IsNullOrEmpty(rawValue) && !SecretProtector.IsProtectedValue(rawValue))
            {
                Write(section, key, rawValue);
                ScrubLegacyProxyPassword();
                return rawValue;
            }

            return String.Empty;
        }

        public int ReadInt(string section, string key)
        {
            int value;
            return Int32.TryParse(Read(section, key), out value) ? value : 0;
        }

        public bool ReadBool(string section, string key)
        {
            bool value;
            return Boolean.TryParse(Read(section, key), out value) && value;
        }

        private void EnsureLanguage()
        {
            string language = Read("Locales", "LANGUAGE");
            if (!String.Equals(language, "RU", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(language, "EN", StringComparison.OrdinalIgnoreCase))
                Write("Locales", "LANGUAGE", "EN");
        }

        private void EnsureBool(string section, string key, bool defaultValue)
        {
            bool value;
            if (!Boolean.TryParse(Read(section, key), out value))
                Write(section, key, defaultValue.ToString());
        }

        private void EnsureInt(string section, string key, int defaultValue, int minimum, int maximum)
        {
            int value;
            if (!Int32.TryParse(Read(section, key), out value) || value < minimum || value > maximum)
                Write(section, key, defaultValue.ToString());
        }

        private void EnsureOptionalInt(string section, string key)
        {
            string raw = Read(section, key);
            int value;
            if (!String.IsNullOrEmpty(raw) && !Int32.TryParse(raw, out value))
                Write(section, key, String.Empty);
        }

        private void EnsureOptionalUInt(string section, string key)
        {
            string raw = Read(section, key);
            uint value;
            if (!String.IsNullOrEmpty(raw) && !UInt32.TryParse(raw, out value))
                Write(section, key, String.Empty);
        }

        private void EnsureString(string section, string key, string defaultValue)
        {
            if (String.IsNullOrWhiteSpace(Read(section, key)))
                Write(section, key, defaultValue);
        }

        private void EnsureStringAllowEmpty(string section, string key, string defaultValue)
        {
            if (Read(section, key) == null)
                Write(section, key, defaultValue);
        }

        private void EnsureColor(string section, string key, string defaultValue)
        {
            try { ColorTranslator.FromHtml(Read(section, key)); }
            catch { Write(section, key, defaultValue); }
        }

        private void EnsureFont(string section, string key, string defaultValue)
        {
            try
            {
                var converter = new FontConverter();
                var font = converter.ConvertFromInvariantString(Read(section, key)) as Font;
                if (font == null) throw new FormatException();
                font.Dispose();
            }
            catch { Write(section, key, defaultValue); }
        }

        private static void ScrubLegacyProxyPassword()
        {
            try
            {
                if (File.Exists(legacyFilePath))
                    WritePrivateProfileString("Proxy", "Password", String.Empty, legacyFilePath);
            }
            catch { }
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string path);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder result, int size, string path);
    }
}
