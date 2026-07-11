#!/usr/bin/env python3
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]


def read(path):
    return (ROOT / path).read_text(encoding="utf-8-sig")


def write(path, text, crlf=True):
    (ROOT / path).write_text(text, encoding="utf-8-sig", newline="\r\n" if crlf else "\n")


def replace_once(text, old, new, label):
    count = text.count(old)
    if count != 1:
        raise SystemExit("Expected %s exactly once, found %d" % (label, count))
    return text.replace(old, new, 1)


# Safe defaults must remain user-configurable after first launch.
path = "Mahou/Classes/Configs.cs"
text = read(path)
text = replace_once(text,
    '            CheckBool("Functions", "UseJKL", "true");',
    '            CheckBool("Functions", "UseJKL", "false");',
    "UseJKL default")
text = replace_once(text,
'''            // Caps Lock must remain the ordinary Windows Caps Lock unless the user
            // explicitly re-enables an advanced remapping later.
            _INI.SetValue("Functions", "RemapCapslockAsF18", "false");
            _INI.SetValue("Layouts", "ChangeToSpecificLayoutByKey", "false");

            // Migrate untouched upstream Pause/Scroll defaults to the requested
''',
'''            // Migrate untouched upstream Pause/Scroll defaults to the requested
''',
    "forced CapsLock policy")
write(path, text)

path = "Mahou/MahouUI.Security.cs"
text = read(path)
text = replace_once(text,
'''            chk_AppDataConfigs.Checked = true;
            chk_AppDataConfigs.Enabled = false;
            chk_RemapCapsLockAsF18.Checked = false;
            chk_SpecificLS.Checked = false;
            cbb_AutostartType.SelectedIndex = 0;
''',
'''            chk_AppDataConfigs.Checked = true;
            chk_AppDataConfigs.Enabled = false;
            cbb_AutostartType.SelectedIndex = 0;
''',
    "security UI forced CapsLock")
text = replace_once(text,
'''            chk_DownloadASD_InZip.Checked = false;
            chk_DownloadASD_InZip.Enabled = false;
            btn_UpdateAutoSwitchDictionary.Text = "Restore bundled dictionary";
''',
'''            chk_DownloadASD_InZip.Checked = false;
            chk_DownloadASD_InZip.Enabled = false;
            HelpMeUnderstand.SetToolTip(chk_TrEnable,
                "When enabled, selected text is sent to the configured online translation service.");
            HelpMeUnderstand.SetToolTip(chk_TrOnDoubleClick,
                "When enabled, double-clicked text may be sent to the configured online translation service.");
            btn_UpdateAutoSwitchDictionary.Text = "Restore bundled dictionary";
''',
    "translator disclosure")
write(path, text)

# Reject arbitrary plaintext that merely happens to be syntactically valid Base64.
path = "Mahou/Classes/SecretProtector.cs"
text = read(path)
old = '''        internal static bool TryDecodeLegacyBase64(string value, out string plainText) {
            plainText = String.Empty;
            if (String.IsNullOrEmpty(value)) return true;
            try {
                var bytes = Convert.FromBase64String(value);
                plainText = Encoding.Unicode.GetString(bytes);
                return true;
            } catch {
                return false;
            }
        }
'''
new = '''        internal static bool TryDecodeLegacyBase64(string value, out string plainText) {
            plainText = String.Empty;
            if (String.IsNullOrEmpty(value)) return true;
            try {
                var bytes = Convert.FromBase64String(value);
                if (bytes.Length == 0 || (bytes.Length & 1) != 0) return false;
                var decoded = Encoding.Unicode.GetString(bytes);
                if (Convert.ToBase64String(Encoding.Unicode.GetBytes(decoded)) != value) return false;
                foreach (var c in decoded) {
                    if (Char.IsControl(c) && c != '\\r' && c != '\\n' && c != '\\t') return false;
                }
                plainText = decoded;
                return true;
            } catch {
                return false;
            }
        }
'''
text = replace_once(text, old, new, "legacy Base64 migration")
write(path, text)

# Product metadata: preserve upstream credit while making support ownership clear.
path = "Mahou/Properties/AssemblyInfo.cs"
text = read(path)
replacements = {
    '[assembly: AssemblyTitle ("Mahou")]': '[assembly: AssemblyTitle ("MIXANIZM Mahou")]',
    '[assembly: AssemblyDescription ("A magic layout switcher.")]': '[assembly: AssemblyDescription ("Privacy-hardened keyboard layout assistant based on Mahou.")]',
    '[assembly: AssemblyCompany ("BladeMight")]': '[assembly: AssemblyCompany ("MIXANIZM")]',
    '[assembly: AssemblyProduct ("Mahou")]': '[assembly: AssemblyProduct ("MIXANIZM Mahou")]',
    '[assembly: AssemblyCopyright ("Copyright © BladeMight 2019")]': '[assembly: AssemblyCopyright ("Original Mahou © BladeMight; MIXANIZM modifications © 2026")]',
    '[assembly: AssemblyTrademark ("BM")]': '[assembly: AssemblyTrademark ("MIXANIZM")]',
    '[assembly: AssemblyDefaultAlias ("BladeMight")]': '[assembly: AssemblyDefaultAlias ("MIXANIZM Mahou")]',
}
for old, new in replacements.items():
    text = replace_once(text, old, new, old)
if 'AssemblyInformationalVersion' not in text:
    text += '\n[assembly: AssemblyInformationalVersion ("2.9.0.1-mixanizm") ]\n'
write(path, text)

# Release builds contain no absolute PDB path and are suitable for deterministic checks.
path = "Mahou/Mahou.csproj"
text = read(path)
marker = '''  <PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
    <Optimize>True</Optimize>
'''
replacement = '''  <PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
    <Optimize>True</Optimize>
    <DebugSymbols>false</DebugSymbols>
    <DebugType>None</DebugType>
    <Deterministic>true</Deterministic>
'''
text = replace_once(text, marker, replacement, "Release property group")
write(path, text)

# Remove obsolete updater state and two trivial unused exception variables.
path = "Mahou/MahouUI.cs"
text = read(path)
text = replace_once(text,
    'static bool updating, was, isold = true, checking, snip_checking, as_checking, check_ASD_size = true;',
    'static bool isold = true, snip_checking, as_checking;',
    "obsolete updater fields")
text = replace_once(text, '\t\tstatic int progress = 0, _progress = 0;\n', '', "obsolete updater progress")
text = text.replace('\t\t\tcheck_ASD_size = true;\n', '')
text = replace_once(text, '} catch(Exception e) {\n\t\t\t\tfong = true;', '} catch(Exception) {\n\t\t\t\tfong = true;', "unused explorer exception")
text = text.replace('http://github.com/BladeMight/Mahou/releases', 'https://github.com/MIXANIZM/Mahou/releases')
text = text.replace('http://github.com/BladeMight/Mahou/wiki', 'https://github.com/MIXANIZM/Mahou/wiki')
text = text.replace('http://github.com/BladeMight/Mahou', 'https://github.com/MIXANIZM/Mahou')
text = text.replace('http://blademight.github.io/Mahou/', 'https://github.com/MIXANIZM/Mahou')
text = text.replace('mailto:BladeMight@gmail.com', 'https://github.com/MIXANIZM/Mahou/issues')
write(path, text)

path = "Mahou/Classes/jklXHidServ.cs"
text = read(path)
text = replace_once(text, '} catch(Exception e) {\n\t\t\t\t\t\t\t\t\tmax_tries--;', '} catch(Exception) {\n\t\t\t\t\t\t\t\t\tmax_tries--;', "unused JKL exception")
write(path, text)

readme = '''# MIXANIZM Mahou — modernized 2.9.0.1 development line

This branch is based on the latest preserved modern Mahou source lineage and keeps the
full tabbed settings UI, AutoSwitch dictionary, snippets, selection conversion,
translation panel, history and advanced layout controls.

## MIXANIZM defaults

- Caps Lock behaves as normal Windows Caps Lock.
- Windows remains responsible for ordinary layout switching.
- Insert is the shared default action for the last word or selected text.
- AutoSwitch is opt-in and uses the dictionary shipped with the build.
- JKL is disabled by default until its native helpers receive a separate audit and are
  packaged intentionally.
- Settings and user data are stored in `%APPDATA%\\MIXANIZM Mahou`.
- Logs are stored in `%LOCALAPPDATA%\\MIXANIZM Mahou\\Logs`.

## Network and privacy

The legacy self-updater and public sync/backup services are disabled. The dictionary
button restores the copy packaged with the verified build and does not download or run
an extraction script.

The translator remains an explicit opt-in feature. When enabled, selected text is sent
to the configured online translation service. Snippet `__execute` is blocked by default
and requires an explicit hidden setting to enable.

## Current status

This is still a draft test branch. It requires physical Windows 11 testing of keyboard
hooks, Insert word/selection conversion, AutoSwitch, snippets, modifier handling and the
full settings UI before merge or public release. See `SECURITY-AUDIT-MODERN.md`.

Original Mahou is GPL v2+ software. Original authorship remains credited in the source
history and license; MIXANIZM maintains this modernization branch.
'''
(ROOT / "README-MIXANIZM.md").write_text(readme, encoding="utf-8", newline="\n")

# Delete this one-time migration script after applying it.
Path(__file__).unlink()
