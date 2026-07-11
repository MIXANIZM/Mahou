#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / ".github/scripts/security-regression.py"
source = PATH.read_text(encoding="utf-8")


def replace_once(old, new):
    global source
    if source.count(old) != 1:
        raise RuntimeError("security-regression.py: expected exactly one occurrence of %r, found %d" % (old, source.count(old)))
    source = source.replace(old, new, 1)


replace_once(
    '    "configs": text("Mahou/Classes/Configs.cs"),\n',
    '    "configs": text("Mahou/Classes/Configs.cs"),\n'
    '    "atomic": text("Mahou/Classes/AtomicFile.cs"),\n',
)
replace_once(
    '           "Image.FromHbitmap(b.GetHbitmap())"],\n',
    '           "Image.FromHbitmap(b.GetHbitmap())",\n'
    '           "File.WriteAllText(snipfile, txt_Snippets.Text, Encoding.UTF8)",\n'
    '           "File.WriteAllText(AS_dictfile, AutoSwitchDictionaryRaw, Encoding.UTF8)",\n'
    '           "File.WriteAllText(f, d[ty])"],\n',
)
replace_once(
    '             "Int32.TryParse(rma[0].Groups[2].Value, out times)"],\n',
    '             "Int32.TryParse(rma[0].Groups[2].Value, out times)",\n'
    '             "System.IO.File.WriteAllText(PATH, DictToRaw(def))"],\n',
)
replace_once(
    '                \'using (File.Create(forceMarker)) { }\'],\n'
    '    "clipboard":',
    '                \'using (File.Create(forceMarker)) { }\'],\n'
    '    "atomic": ["static readonly object SyncRoot = new object();",\n'
    '               "static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);",\n'
    '               "Guid.NewGuid().ToString(\\"N\\") + \\".tmp\\"",\n'
    '               "FileOptions.WriteThrough", "stream.Flush(true);",\n'
    '               "File.Replace(temp, fullPath, backup, true);",\n'
    '               "ReplaceByCopy(temp, fullPath);",\n'
    '               "if (File.Exists(temp)) File.Delete(temp)"],\n'
    '    "clipboard":',
)
replace_once(
    '             "for (int x = 0; x < times; x++)"],\n',
    '             "for (int x = 0; x < times; x++)",\n'
    '             "AtomicFile.WriteAllText(PATH, DictToRaw(def));"],\n',
)
replace_once(
    '           "if (small != IntPtr.Zero && small != large) WinAPI.DestroyIcon(small);"],\n',
    '           "if (small != IntPtr.Zero && small != large) WinAPI.DestroyIcon(small);",\n'
    '           "AtomicFile.WriteAllText(snipfile, txt_Snippets.Text, Encoding.UTF8);",\n'
    '           "AtomicFile.WriteAllText(AS_dictfile, AutoSwitchDictionaryRaw, Encoding.UTF8);",\n'
    '           "AtomicFile.WriteAllText(f, d[ty]);"],\n',
)
replace_once(
    '                "<Deterministic>true</Deterministic>", \'<None Include="app.manifest" />\'],\n',
    '                "<Deterministic>true</Deterministic>", \'<None Include="app.manifest" />\',\n'
    '                \'<Compile Include="Classes\\\\AtomicFile.cs" />\'],\n',
)

PATH.write_text(source, encoding="utf-8")
Path(__file__).unlink()
print("Added permanent regression guards for atomic low-frequency user data writes.")
