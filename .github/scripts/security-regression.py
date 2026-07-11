#!/usr/bin/env python3
"""Fail CI when previously removed high-risk Mahou behavior returns."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]


def text(relative: str) -> str:
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
}

errors = []

forbidden = {
    "ui": [
        "UpdateMahou.cmd",
        "ExtractASD.cmd",
        "DownloadFileAsync(",
        "DownloadFile(",
        "UploadData(",
        "https://hastebin.com",
        "https://0x0.st",
        "Shell.Application",
        "TASKKILL /IM",
    ],
    "startup": ["/Create /TN", "Startup\\Mahou.lnk"],
    "program": ["taskkill", "RestartMahou.cmd", "RestartMahou.vbs"],
    "clipboard": ["EnumClipboardFormats", "GetClipboardFormatName", "GlobalSize"],
}

for file_key, needles in forbidden.items():
    source = files[file_key]
    for needle in needles:
        if needle.lower() in source.lower():
            errors.append("forbidden token returned in %s: %s" % (file_key, needle))

required = {
    "security_ui": [
        "LegacyNetworkDisabledMessage",
        'ClipBackOnlyText = false;',
        'MMain.MyConfs.Write("Hidden", "ClipBackOnlyText", "false")',
    ],
    "configs": [
        'CheckBool("Hidden", "AllowSnippetExecute", "false")',
        'CheckBool("Functions", "UseJKL", "false")',
        'CheckBool("Functions", "RemapCapslockAsF18", "false")',
        'CheckBool("Layouts", "ChangeToSpecificLayoutByKey", "false")',
        'CheckBool("Migrations", "MixanizmDefaultsV1", "false")',
    ],
    "clipboard": [
        "OleGetClipboard",
        "OleSetClipboard",
        "CaptureOleSnapshot",
        "OpenWithRetry",
    ],
    "hook": [
        "CaptureClipboardBackup",
        "EnsureClipboardBackup",
        "EnsureClipboardRestored",
        "Temporary clipboard replacement refused because no full backup exists",
        "ConvertSelectionOrLastWord",
    ],
    "secrets": ["ProtectedData.Protect", "ProtectedData.Unprotect", "DataProtectionScope.CurrentUser"],
    "startup": ["CurrentVersion\\Run", "MIXANIZM Mahou", "/Delete /TN"],
    "paths": ["MIXANIZM Mahou", "Environment.SpecialFolder.ApplicationData"],
    "program": ["WaitForRestartParent(args)", "UserDataPaths.Initialize(args)"],
}

for file_key, needles in required.items():
    source = files[file_key]
    for needle in needles:
        if needle not in source:
            errors.append("required hardening marker missing in %s: %s" % (file_key, needle))

# The only source file allowed to use WebClient is the explicit opt-in translator.
for path in (ROOT / "Mahou").rglob("*.cs"):
    source = path.read_text(encoding="utf-8-sig")
    if "WebClient" in source and path.name != "TranslatePanel.cs":
        errors.append("unexpected WebClient use outside translator: %s" % path.relative_to(ROOT))

# A temporary clipboard replacement must always require a pending full snapshot.
restore_start = files["hook"].find('public static bool RestoreClipBoard(string special = "")')
restore_end = files["hook"].find("public static void EnsureClipboardRestored()", restore_start)
restore_body = files["hook"][restore_start:restore_end]
for marker in ["clipboardBackupPending", "lastClip == null", "NativeClipboard.SetText(special)"]:
    if marker not in restore_body:
        errors.append("clipboard replacement guard incomplete: %s" % marker)

if errors:
    print("SECURITY REGRESSION CHECK FAILED")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("Security regression check passed: %d source files inspected." % len(list((ROOT / "Mahou").rglob("*.cs"))))
