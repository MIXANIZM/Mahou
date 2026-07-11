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
    "\t\t\t\tMemory.Flush;",
    "\t\t\t\tMemory.Flush();",
    "Memory.Flush invocation",
)

replace_once(
    "Mahou/MahouUI.cs",
    """\t\t\t} finally {\n\t\t\t\tif (ACT_Match < 1)\n\t\t\t\t\tKMHook.EnsureClipboardRestored();\n\t\t\t\telse\n\t\t\t\t\tACT_Match--;\n\t\t\t}\n""",
    """\t\t\t} finally {\n\t\t\t\tif (ACT_Match > 0) ACT_Match--;\n\t\t\t\tKMHook.EnsureClipboardRestored();\n\t\t\t}\n""",
    "translator clipboard finalizer",
)

Path(__file__).unlink()
print("Round 3 clipboard compile and finalizer fixes applied.")
