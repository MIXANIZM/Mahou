#!/usr/bin/env python3
from pathlib import Path

root = Path(__file__).resolve().parents[2]


def replace_once(path, old, new, label):
    file_path = root / path
    text = file_path.read_text(encoding="utf-8-sig")
    count = text.count(old)
    if count != 1:
        raise SystemExit("Expected %s exactly once, found %d" % (label, count))
    file_path.write_text(text.replace(old, new, 1), encoding="utf-8-sig", newline="\r\n")


replace_once(
    "Mahou/Classes/KMHook.cs",
    "\t\t\tif (!selectionConversionSucceeded) ConvertLast(MMain.c_word);",
    "\t\t\tif (!selectionConversionSucceeded) ConvertLast();",
    "modern ConvertLast fallback",
)

replace_once(
    "Mahou/MahouUI.cs",
    '''\t\tvoid GetUpdateInfo() {
\t\t\tUpdInfo = new [] {
\t\t\t\t"Manual verified updates only",
\t\t\t\tLegacyNetworkDisabledMessage,
\t\t\t\tApplication.ProductVersion,
\t\t\t\tString.Empty,
\t\t\t\tString.Empty
\t\t\t};
\t\t}
''',
    '''\t\tvoid GetUpdateInfo() {
\t\t\tLogging.Log("Legacy update check is disabled; verified releases are installed manually.");
\t\t}
''',
    "inert update info method",
)

Path(__file__).unlink()
print("Round 4 modern signature fixes applied.")
