#!/usr/bin/env python3
from pathlib import Path
import re

root = Path(__file__).resolve().parents[2]


def read(path):
    return (root / path).read_text(encoding="utf-8-sig")


def write(path, text):
    (root / path).write_text(text, encoding="utf-8-sig", newline="\r\n")


def replace_once(text, old, new, label):
    count = text.count(old)
    if count != 1:
        raise SystemExit("Expected %s exactly once, found %d" % (label, count))
    return text.replace(old, new, 1)


# Full clipboard transactions: temporary text may never be written without a snapshot.
path = "Mahou/Classes/KMHook.cs"
text = read(path)
old = '''\t\tpublic static bool BackupClipboard() {
\t\t\tlock (clipboardBackupSync) {
\t\t\t\tif (clipboardBackupPending) {
\t\t\t\t\tLogging.Log("Clipboard backup is already pending; nested operation cancelled.", 2);
\t\t\t\t\treturn false;
\t\t\t\t}
\t\t\t\tvar snapshot = NativeClipboard.CaptureOleSnapshot();
\t\t\t\tif (snapshot == null) return false;
\t\t\t\tlastClip = snapshot;
\t\t\t\tclipboardBackupPending = true;
\t\t\t\treturn true;
\t\t\t}
\t\t}
'''
new = '''\t\tstatic bool CaptureClipboardBackup(bool allowExisting) {
\t\t\tlock (clipboardBackupSync) {
\t\t\t\tif (clipboardBackupPending) {
\t\t\t\t\tif (allowExisting) return true;
\t\t\t\t\tLogging.Log("Clipboard backup is already pending; nested operation cancelled.", 2);
\t\t\t\t\treturn false;
\t\t\t\t}
\t\t\t\tvar snapshot = NativeClipboard.CaptureOleSnapshot();
\t\t\t\tif (snapshot == null) return false;
\t\t\t\tlastClip = snapshot;
\t\t\t\tclipboardBackupPending = true;
\t\t\t\treturn true;
\t\t\t}
\t\t}
\t\tpublic static bool BackupClipboard() {
\t\t\treturn CaptureClipboardBackup(false);
\t\t}
\t\tstatic bool EnsureClipboardBackup() {
\t\t\treturn CaptureClipboardBackup(true);
\t\t}
'''
text = replace_once(text, old, new, "clipboard backup method")

old = '''\t\t\tif (!String.IsNullOrEmpty(special)) {
\t\t\t\tLogging.Log("Temporarily replacing clipboard text.");
\t\t\t\tif (!WaitForClip2BeFree()) return false;
\t\t\t\treturn NativeClipboard.SetText(special);
\t\t\t}
'''
new = '''\t\t\tif (!String.IsNullOrEmpty(special)) {
\t\t\t\tlock (clipboardBackupSync) {
\t\t\t\t\tif (!clipboardBackupPending || lastClip == null) {
\t\t\t\t\t\tLogging.Log("Temporary clipboard replacement refused because no full backup exists.", 2);
\t\t\t\t\t\treturn false;
\t\t\t\t\t}
\t\t\t\t}
\t\t\t\tLogging.Log("Temporarily replacing clipboard text.");
\t\t\t\tif (!WaitForClip2BeFree()) return false;
\t\t\t\treturn NativeClipboard.SetText(special);
\t\t\t}
'''
text = replace_once(text, old, new, "temporary clipboard replacement")

old = '''\t\tpublic static void PasteText(string text, string info="") {
\t\t\tLogging.Log("Pasting ["+text+"]  as "+info);
\t\t\tRestoreClipBoard(text);
\t\t\tKInputs.MakeInput(KInputs.AddPress(Keys.V), (int)WinAPI.MOD_CONTROL);
\t\t\tRestoreClipBoard();
\t\t}
'''
new = '''\t\tpublic static void PasteText(string text, string info="") {
\t\t\tLogging.Log("Pasting temporary text as " + info + ".");
\t\t\tif (!EnsureClipboardBackup()) {
\t\t\t\tLogging.Log("Paste cancelled because the clipboard could not be preserved.", 2);
\t\t\t\treturn;
\t\t\t}
\t\t\ttry {
\t\t\t\tif (!RestoreClipBoard(text)) return;
\t\t\t\tKInputs.MakeInput(KInputs.AddPress(Keys.V), (int)WinAPI.MOD_CONTROL);
\t\t\t\t// SendInput completes before every target consumes WM_PASTE. Keep the
\t\t\t\t// temporary text available for a short bounded interval.
\t\t\t\tThread.Sleep(35);
\t\t\t} finally {
\t\t\t\tEnsureClipboardRestored();
\t\t\t}
\t\t}
'''
text = replace_once(text, old, new, "PasteText transaction")

old = '''\t\t\t\tGetClipStr();
\t\t\t\ttry {
\t\t\t\t\tRestoreClipBoard(Regex.Replace(args, "\\r?\\n|\\r", Environment.NewLine));
\t\t\t\t\tKInputs.MakeInput(KInputs.AddPress(Keys.V), (int)WinAPI.MOD_CONTROL);
'''
new = '''\t\t\t\tGetClipStr();
\t\t\t\ttry {
\t\t\t\t\tif (!RestoreClipBoard(Regex.Replace(args, "\\r?\\n|\\r", Environment.NewLine))) {
\t\t\t\t\t\tLogging.Log("Snippet paste cancelled because a safe clipboard transaction was unavailable.", 2);
\t\t\t\t\t\tbreak;
\t\t\t\t\t}
\t\t\t\t\tKInputs.MakeInput(KInputs.AddPress(Keys.V), (int)WinAPI.MOD_CONTROL);
'''
text = replace_once(text, old, new, "snippet paste transaction")

# Remove inherited dead fields and unreachable statement so Release builds are warning-free.
text = replace_once(text, '\t\tstatic bool m_autoswitchcorrect;\n', '', "unused autoswitch field")
write(path, text)

path = "Mahou/MahouUI.cs"
text = read(path)
for old, label in [
    ('\t\tstatic string[] UpdInfo;\n', 'unused update info'),
    ('\t\tpublic static List<int> HKBlockAlt = new List<int>();\n', 'unused alt list'),
    ('\t\tpublic static bool BlockAltUpNOW = false;\n', 'unused alt flag'),
    ('\t\tstatic bool isold = true, snip_checking, as_checking;\n', 'old updater state'),
    ('\t\tstatic int progress = 0;\n', 'old updater progress'),
    ('\t\tpublic static Dictionary<string, string> TrSetsValues = new Dictionary<string, string>();\n', 'unused translation set cache'),
    ('\t\tstatic string latestSwitch = "null";\n', 'unused latest switch'),
    ('\t\tconst string SYNC_HOST = "https://hastebin.com";\n', 'unused sync host'),
    ('\t\tconst string SYNC_HOST2 = "https://0x0.st";\n', 'unused sync host 2'),
    ('\t\tconst string SYNC_SEP = "#------>";\n', 'unused sync separator'),
    ('\t\treadonly string[] SYNC_NAMES = { "Mahou.ini", "snippets.txt", "history.txt", "TSDict.txt", "Mahou.mm" };\n', 'unused sync names'),
    ('\t\treadonly string[] SYNC_TYPES = { "ini", "sni", "his", "tdi", "mm" };\n', 'unused sync types'),
    ('\t\tint tmpsni_i = -1;\n', 'unused snippet index'),
    ('\t\t\t\tcurrentLayout = GetCurrentLayout();\n', 'unreachable layout refresh'),
]:
    text = replace_once(text, old, '', label)
text = replace_once(text,
    '\t\tpublic string AutoCopyTranslation = "", onlySnippetsExcluded = "", onlyAutoSwitchExcluded = "", specSwtchsSet = "";\n',
    '\t\tpublic string AutoCopyTranslation = "", onlySnippetsExcluded = "", onlyAutoSwitchExcluded = "";\n',
    'unused specific switch set')
write(path, text)

# The text-only clipboard backup mode is not safe for images/HTML/RTF/Excel.
path = "Mahou/MahouUI.Security.cs"
text = read(path)
old = '''            chk_AppDataConfigs.Checked = true;
            chk_AppDataConfigs.Enabled = false;
            cbb_AutostartType.SelectedIndex = 0;
'''
new = '''            chk_AppDataConfigs.Checked = true;
            chk_AppDataConfigs.Enabled = false;
            ClipBackOnlyText = false;
            MMain.MyConfs.Write("Hidden", "ClipBackOnlyText", "false");
            cbb_AutostartType.SelectedIndex = 0;
'''
text = replace_once(text, old, new, "safe clipboard policy")
write(path, text)

Path(__file__).unlink()
print("Round 4 clipboard transaction and warning cleanup applied.")
