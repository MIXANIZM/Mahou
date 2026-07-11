#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "Mahou/Classes/Configs.cs"


def replace_eol(old, new, expected=1):
    data = PATH.read_bytes()
    old_lf = old.encode("utf-8")
    new_lf = new.encode("utf-8")
    old_crlf = old_lf.replace(b"\n", b"\r\n")
    new_crlf = new_lf.replace(b"\n", b"\r\n")
    count_lf = data.count(old_lf)
    count_crlf = data.count(old_crlf)
    count = count_lf + count_crlf
    if count != expected:
        raise RuntimeError(
            "Configs.cs: expected %d occurrence(s), found %d (LF=%d, CRLF=%d) for %r"
            % (expected, count, count_lf, count_crlf, old)
        )
    if count_crlf:
        data = data.replace(old_crlf, new_crlf, expected)
    else:
        data = data.replace(old_lf, new_lf, expected)
    PATH.write_bytes(data)


replace_eol(
    "    \t\t\tvar copy = File.Exists(filePath);\n"
    "    \t\t\tfilePath = Path.Combine(MahouUI.mahou_folder_appd, \"Mahou.ini\");\n"
    "    \t\t\tFile.Create(Path.Combine(MahouUI.mahou_folder_appd,\".force\"));\n",
    "\t\t\tfilePath = Path.Combine(MahouUI.mahou_folder_appd, \"Mahou.ini\");\n"
    "\t\t\tvar forceMarker = Path.Combine(MahouUI.mahou_folder_appd, \".force\");\n"
    "\t\t\tusing (File.Create(forceMarker)) { }\n",
)

Path(__file__).unlink()
print("Disposed the AppData force-marker stream immediately and removed the unused copy probe.")
