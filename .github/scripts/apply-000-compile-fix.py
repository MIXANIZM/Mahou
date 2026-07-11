#!/usr/bin/env python3
from pathlib import Path

root = Path(__file__).resolve().parents[2]

compat = root / "Mahou" / "MahouUI.Compatibility.cs"
compat.write_text(
'''using System.Collections.Generic;

namespace Mahou {
    /// <summary>
    /// Non-network fields retained from the modern upstream UI. The legacy
    /// updater and public-sync endpoints stay removed; only shared state still
    /// referenced by keyboard, translation and local import/export code lives
    /// here.
    /// </summary>
    public partial class MahouUI {
        public static List<int> HKBlockAlt = new List<int>();
        public static bool BlockAltUpNOW = false;
        static bool isold = true, snip_checking, as_checking;
        public static Dictionary<string, string> TrSetsValues = new Dictionary<string, string>();
        static string latestSwitch = "null";

        const string SYNC_SEP = "#------>";
        readonly string[] SYNC_NAMES = { "Mahou.ini", "snippets.txt", "history.txt", "TSDict.txt", "Mahou.mm" };
        readonly string[] SYNC_TYPES = { "ini", "sni", "his", "tdi", "mm" };
    }
}
''',
    encoding="utf-8",
    newline="\n",
)

project = root / "Mahou" / "Mahou.csproj"
project_text = project.read_text(encoding="utf-8-sig")
entry = '    <Compile Include="MahouUI.Compatibility.cs">\n      <DependentUpon>MahouUI.cs</DependentUpon>\n    </Compile>\n'
marker = '    <Compile Include="MahouUI.Security.cs">\n      <DependentUpon>MahouUI.cs</DependentUpon>\n    </Compile>\n'
if 'Compile Include="MahouUI.Compatibility.cs"' not in project_text:
    if marker not in project_text:
        raise RuntimeError("MahouUI.Security.cs project marker not found")
    project_text = project_text.replace(marker, marker + entry, 1)
    project.write_text(project_text, encoding="utf-8-sig", newline="")

hook = root / "Mahou" / "Classes" / "KMHook.cs"
hook_text = hook.read_text(encoding="utf-8-sig")
count = hook_text.count("ConvertLast();")
if count != 2:
    raise RuntimeError("Expected exactly two parameterless ConvertLast calls, found %d" % count)
hook_text = hook_text.replace("ConvertLast();", "ConvertLast(MMain.c_word);")
hook.write_text(hook_text, encoding="utf-8-sig", newline="")

Path(__file__).unlink()
print("Compile compatibility declarations restored without restoring legacy network endpoints.")
