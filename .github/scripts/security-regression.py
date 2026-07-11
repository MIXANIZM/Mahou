#!/usr/bin/env python3
"""Fail CI when previously removed high-risk Mahou behavior returns."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]


def text(relative):
    return (ROOT / relative).read_text(encoding="utf-8-sig")


files = {
    "ui": text("Mahou/MahouUI.cs"),
    "security_ui": text("Mahou/MahouUI.Security.cs"),
    "configs": text("Mahou/Classes/Configs.cs"),
    "clipboard": text("Mahou/Classes/NativeClipboard.cs"),
    "hook": text("Mahou/Classes/KMHook.cs"),
    "secrets": text("Mahou/Classes/SecretProtector.cs"),
    "startup": text("Mahou/Classes/StartupManager.cs"),
    "paths": text("Mahou/Classes/UserDataPaths.cs"),
    "program": text("Mahou/Program.cs"),
    "lang_display": text("Mahou/LangDisplay.cs"),
    "lang_panel": text("Mahou/LangPanel.cs"),
}
errors = []

forbidden = {
    "ui": ["UpdateMahou.cmd", "ExtractASD.cmd", "DownloadFileAsync(", "DownloadFile(",
           "UploadData(", "https://hastebin.com", "https://0x0.st", "Shell.Application", "TASKKILL /IM",
           "FillRectangle(new SolidBrush(BG)", "DrawRectangle(new Pen(TAB_BORDERS)",
           "FillRectangle(new SolidBrush(TAB_FOCUS_BG)", "DrawString(t, i.Font, new SolidBrush(FG)",
           "MMain.MyConfs._INI.Raw ="],
    "startup": ["/Create /TN", "Startup\\Mahou.lnk"],
    "program": ["taskkill", "RestartMahou.cmd", "RestartMahou.vbs"],
    "configs": ["AllowSnippetExecute", "var inini = _INI.Raw;", "File.WriteAllText(temp, _INI.Raw"],
    "hook": ["lastClipText", "MahouUI.ClipBackOnlyText", '"__execute"',
             "static void Execute(string args)", "Process.Start(",
             "for (int x = 0; x != times; x++)", "for (int i=0; i!=upc; i++)",
             "Int32.TryParse(axy[1], out delay)",
             "Int32.TryParse(rma[0].Groups[2].Value, out times)"],
    "lang_display": ["DrawString(lbLang.Text, lbLang.Font, new SolidBrush"],
    "lang_panel": ["Graphics g = CreateGraphics();", "var pn = new Pen(Color.Black);",
                   "pn = new Pen(CurrentAeroColor())"],
}
for key, needles in forbidden.items():
    source = files[key].lower()
    for needle in needles:
        if needle.lower() in source:
            errors.append("forbidden token returned in %s: %s" % (key, needle))

required = {
    "security_ui": ["LegacyNetworkDisabledMessage", "ClipBackOnlyText = false;",
                    'MMain.MyConfs.Write("Hidden", "ClipBackOnlyText", "false")',
                    "txt_ProxyPassword.UseSystemPasswordChar = true"],
    "configs": ['CheckBool("Functions", "UseJKL", "false")',
                'CheckBool("Functions", "RemapCapslockAsF18", "false")',
                'CheckBool("Layouts", "ChangeToSpecificLayoutByKey", "false")',
                'CheckBool("Migrations", "MixanizmDefaultsV1", "false")',
                'NormalizeInt("Hidden", "TrayHoverMahouMM", 0, 350000, 0)',
                'NormalizeInt("Hidden", "OverlayExcludedInterval", 250, 10000, 2500)',
                'NormalizeInt("Layouts", "SpecificKeysType", 0, 1, 0)',
                'NormalizeInt("Functions", "WriteInputHistoryBackSpaceType", 0, 1, 0)',
                'NormalizeInt("TranslatePanel", "Transparency", 1, 100, 90)',
                'NormalizeInt("Timings", "SelectedTextGetMoreTriesCount", 3, 20, 5)',
                'NormalizeInt("PersistentLayout", "Layout1CheckInterval", 1, 99999, 50)',
                'NormalizeInt("Appearence", "CaretLTWidth", 1, 1000, 26)',
                'NormalizeInt("Appearence", "CaretLTPositionX", -10000, 10000, 8)',
                'readonly object syncRoot = new object();',
                'readonly Dictionary<string, string> valueIndex',
                'void RebuildIndexUnlocked()',
                'lock (syncRoot)',
                'public string GetRawSnapshot()',
                'public void ReplaceRaw(string raw)',
                'valueIndex.TryGetValue(IndexKey(Section, ValueName)',
                '_INI.GetRawSnapshot()'],
    "clipboard": ["OleGetClipboard", "OleSetClipboard", "CaptureOleSnapshot", "OpenWithRetry",
                  "bounded retries"],
    "hook": ["CaptureClipboardBackup", "EnsureClipboardBackup", "EnsureClipboardRestored",
             "Temporary clipboard replacement refused because no full backup exists",
             "ConvertSelectionOrLastWord", "SelectionProbe.GetState",
             "selected text length=", "input length=",
             "Current snippet length:", "Snippet rewrite completed; source length=",
             "const int MaxSnippetDelayMs = 5000;",
             "const int MaxKeyboardStepDelayMs = 1000;",
             "const int MaxSnippetKeyRepeat = 1000;",
             "const int MaxUppercaseCharacters = 10000;",
             "d = Math.Max(0, Math.Min(d, MaxSnippetDelayMs));",
             "times = Math.Max(0, Math.Min(times, MaxSnippetKeyRepeat));",
             "delay = Math.Max(0, Math.Min(parsedDelay, MaxKeyboardStepDelayMs));",
             "timeout = Math.Max(0, Math.Min(timeout, 600000));",
             "for (int x = 0; x < times; x++)"],
    "secrets": ["ProtectedData.Protect", "ProtectedData.Unprotect", "DataProtectionScope.CurrentUser"],
    "startup": ["CurrentVersion\\Run", "MIXANIZM Mahou", "/Delete /TN"],
    "paths": ["MIXANIZM Mahou", "Environment.SpecialFolder.ApplicationData"],
    "program": ["WaitForRestartParent(args)", "UserDataPaths.Initialize(args)"],
    "lang_display": ["using (var brush = new SolidBrush(lbLang.ForeColor))",
                     "previousBackground.Dispose()"],
    "lang_panel": ["using (var pen = new Pen(borderColor))", "e.Graphics.DrawRectangle",
                   "previousFlag.Dispose()"],
    "ui": ["using (var backgroundBrush = new SolidBrush(BG))",
           "using (var borderPen = new Pen(Color.FromArgb(255, 133, 158, 191), 1))",
           "using (var tabBorderPen = new Pen(TAB_BORDERS))",
           "using (var focusBrush = new SolidBrush(TAB_FOCUS_BG))",
           "using (var textBrush = new SolidBrush(FG))",
           "MMain.MyConfs._INI.ReplaceRaw("],
}
for key, needles in required.items():
    source = files[key]
    for needle in needles:
        if needle not in source:
            errors.append("required hardening marker missing in %s: %s" % (key, needle))

privacy_forbidden = [
    'Starting conversion of [" + ClipStr',
    'Conversion of string [" + ClipStr',
    'Set snip to [" + args',
    'Set last snip to [" + args',
    'with args: [" + args',
    'Executing: executable: ["+fil',
    'Expanding snippet [" + snip',
    'Current snippet is [" + snip',
    'Inputting ["+output',
    'Snip rewrite: " + rewr',
    'Wrong Dictionary, line #"+i+", => " +line',
]
for needle in privacy_forbidden:
    if needle in files["hook"]:
        errors.append("plaintext diagnostic logging returned in hook: %s" % needle)

for pattern in ("*.cmd", "*.bat", "*.vbs", "*.ps1"):
    for script in (ROOT / "Mahou").rglob(pattern):
        errors.append("obsolete executable script remains in application tree: %s" % script.relative_to(ROOT))

for path in (ROOT / "Mahou").rglob("*.cs"):
    source = path.read_text(encoding="utf-8-sig")
    if "WebClient" in source and path.name != "TranslatePanel.cs":
        errors.append("unexpected WebClient use outside translator: %s" % path.relative_to(ROOT))

hook = files["hook"]
start = hook.find('public static bool RestoreClipBoard(string special = "")')
end = hook.find('public static void EnsureClipboardRestored()', start)
body = hook[start:end]
for marker in ("clipboardBackupPending", "lastClip == null", "NativeClipboard.SetText(special)"):
    if marker not in body:
        errors.append("clipboard replacement guard incomplete: %s" % marker)

if errors:
    print("SECURITY REGRESSION CHECK FAILED")
    for error in errors:
        print("- " + error)
    sys.exit(1)
print("Security regression check passed: %d source files inspected." % len(list((ROOT / "Mahou").rglob("*.cs"))))
