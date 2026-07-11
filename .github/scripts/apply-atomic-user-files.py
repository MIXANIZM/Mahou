#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PROJECT = ROOT / "Mahou/Mahou.csproj"
UI = ROOT / "Mahou/MahouUI.cs"
HOOK = ROOT / "Mahou/Classes/KMHook.cs"


def replace_eol(path, old, new, expected=1):
    data = path.read_bytes()
    old_lf = old.encode("utf-8")
    new_lf = new.encode("utf-8")
    old_crlf = old_lf.replace(b"\n", b"\r\n")
    new_crlf = new_lf.replace(b"\n", b"\r\n")
    count_lf = data.count(old_lf)
    count_crlf = data.count(old_crlf)
    count = count_lf + count_crlf
    if count != expected:
        raise RuntimeError(
            "%s: expected %d occurrence(s), found %d (LF=%d, CRLF=%d) for %r"
            % (path.relative_to(ROOT), expected, count, count_lf, count_crlf, old)
        )
    if count_crlf:
        data = data.replace(old_crlf, new_crlf, expected)
    else:
        data = data.replace(old_lf, new_lf, expected)
    path.write_bytes(data)


replace_eol(
    PROJECT,
    '    <Compile Include="Classes\\Auray.cs" />\n',
    '    <Compile Include="Classes\\AtomicFile.cs" />\n'
    '    <Compile Include="Classes\\Auray.cs" />\n',
)
replace_eol(
    UI,
    "File.WriteAllText(snipfile, txt_Snippets.Text, Encoding.UTF8);",
    "AtomicFile.WriteAllText(snipfile, txt_Snippets.Text, Encoding.UTF8);",
    expected=2,
)
replace_eol(
    UI,
    "File.WriteAllText(AS_dictfile, AutoSwitchDictionaryRaw, Encoding.UTF8);",
    "AtomicFile.WriteAllText(AS_dictfile, AutoSwitchDictionaryRaw, Encoding.UTF8);",
)
replace_eol(
    UI,
    "File.WriteAllText(f, d[ty]);",
    "AtomicFile.WriteAllText(f, d[ty]);",
)
replace_eol(
    HOOK,
    "System.IO.File.WriteAllText(PATH, DictToRaw(def));",
    "AtomicFile.WriteAllText(PATH, DictToRaw(def));",
)

Path(__file__).unlink()
print("Enabled flushed temp-and-replace writes for snippets, AutoSwitch and imported/default dictionaries.")
