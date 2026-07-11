#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CONFIGS = ROOT / "Mahou/Classes/Configs.cs"
UI = ROOT / "Mahou/MahouUI.cs"


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


replace_eol(CONFIGS, "using System;\n", "using System;\nusing System.Collections.Generic;\n")
replace_eol(
    CONFIGS,
    "\t\tpublic bool DEBUG;\n\t\t#endregion\n\t\t\n\t\tpublic INI(string ini, bool dbg = false) {\n"
    "\t\t\tthis.Raw = ini;\n\t\t\tthis.lines = Raw.Replace(\"\\r\", \"\").Split('\\n');\n"
    "\t\t\tthis.DEBUG = dbg;\n\t\t}\n",
    "\t\tpublic bool DEBUG;\n"
    "\t\treadonly object syncRoot = new object();\n"
    "\t\treadonly Dictionary<string, string> valueIndex = new Dictionary<string, string>(StringComparer.Ordinal);\n"
    "\t\t#endregion\n\t\t\n\t\tpublic INI(string ini, bool dbg = false) {\n"
    "\t\t\tthis.Raw = ini ?? String.Empty;\n"
    "\t\t\tthis.lines = Raw.Replace(\"\\r\", \"\").Split('\\n');\n"
    "\t\t\tthis.DEBUG = dbg;\n"
    "\t\t\tRebuildIndexUnlocked();\n"
    "\t\t}\n"
    "\t\tstring IndexKey(string section, string valueName) {\n"
    "\t\t\treturn section + \"\\u001f\" + valueName;\n"
    "\t\t}\n"
    "\t\tvoid RebuildIndexUnlocked() {\n"
    "\t\t\tvalueIndex.Clear();\n"
    "\t\t\tvar seenSections = new HashSet<string>(StringComparer.Ordinal);\n"
    "\t\t\tstring currentSection = null;\n"
    "\t\t\tforeach (var sourceLine in lines) {\n"
    "\t\t\t\tvar line = sourceLine ?? String.Empty;\n"
    "\t\t\t\tif (line.Length <= 1) { currentSection = null; continue; }\n"
    "\t\t\t\tif (line[0] == '!' || line[0] == ';') continue;\n"
    "\t\t\t\tif (line[0] == '[' && line[line.Length - 1] == ']') {\n"
    "\t\t\t\t\tvar candidate = line.Substring(1, line.Length - 2);\n"
    "\t\t\t\t\tcurrentSection = seenSections.Add(candidate) ? candidate : null;\n"
    "\t\t\t\t\tcontinue;\n"
    "\t\t\t\t}\n"
    "\t\t\t\tif (currentSection == null) continue;\n"
    "\t\t\t\tvar equals = line.IndexOf('=');\n"
    "\t\t\t\tif (equals <= 0) continue;\n"
    "\t\t\t\tvar key = IndexKey(currentSection, line.Substring(0, equals));\n"
    "\t\t\t\tif (!valueIndex.ContainsKey(key)) valueIndex.Add(key, line.Substring(equals + 1));\n"
    "\t\t\t}\n"
    "\t\t}\n"
    "\t\tpublic string GetRawSnapshot() {\n"
    "\t\t\tlock (syncRoot) return Raw;\n"
    "\t\t}\n"
    "\t\tpublic void ReplaceRaw(string raw) {\n"
    "\t\t\tlock (syncRoot) {\n"
    "\t\t\t\tRaw = raw ?? String.Empty;\n"
    "\t\t\t\tlines = Raw.Replace(\"\\r\", \"\").Split('\\n');\n"
    "\t\t\t\tRebuildIndexUnlocked();\n"
    "\t\t\t}\n"
    "\t\t}\n",
)
replace_eol(
    CONFIGS,
    "\t\tpublic void SetValue(string Section, string ValueName, string Value) {\n"
    "\t\t\tvar sect = HasSection(Section);\n"
    "\t\t\tvar val_line = HasValue(sect, ValueName);\n"
    "\t\t\tif (sect == -1) {\n"
    "\t\t\t\tlog(\"  NO SUCH SECT! \" + Section);\n"
    "\t\t\t\tlines = AddLine(\"[\"+Section+\"]\", sect, lines);\n"
    "\t\t\t\tsect = 0;\n"
    "\t\t\t\tval_line = -1;\n"
    "\t\t\t}\n"
    "\t\t\tif (val_line > -1) {\n"
    "\t\t\t\tlines[val_line] = ValueName + \"=\" + Value;\n"
    "\t\t\t}\n"
    "\t\t\tif (val_line == -1) {\n"
    "\t\t\t\tlog(\"   NO SUCH VALUE! \" + ValueName);\n"
    "\t\t\t\tlines = AddLine(ValueName + \"=\" + Value, sect, lines);\n"
    "\t\t\t}\n"
    "\t\t\tif (val_line == -1 || val_line > -1 || sect == -1) {\n"
    "\t\t\t\tRaw = string.Join(Environment.NewLine, lines);\n"
    "\t\t\t\tlines = Raw.Replace(\"\\r\", \"\").Split('\\n');\n"
    "\t\t\t}\n"
    "\t\t}\n",
    "\t\tpublic void SetValue(string Section, string ValueName, string Value) {\n"
    "\t\t\tlock (syncRoot) {\n"
    "\t\t\t\tvar sect = HasSection(Section);\n"
    "\t\t\t\tvar val_line = HasValue(sect, ValueName);\n"
    "\t\t\t\tif (sect == -1) {\n"
    "\t\t\t\t\tlog(\"  NO SUCH SECT! \" + Section);\n"
    "\t\t\t\t\tlines = AddLine(\"[\"+Section+\"]\", sect, lines);\n"
    "\t\t\t\t\tsect = 0;\n"
    "\t\t\t\t\tval_line = -1;\n"
    "\t\t\t\t}\n"
    "\t\t\t\tif (val_line > -1) {\n"
    "\t\t\t\t\tlines[val_line] = ValueName + \"=\" + Value;\n"
    "\t\t\t\t}\n"
    "\t\t\t\tif (val_line == -1) {\n"
    "\t\t\t\t\tlog(\"   NO SUCH VALUE! \" + ValueName);\n"
    "\t\t\t\t\tlines = AddLine(ValueName + \"=\" + Value, sect, lines);\n"
    "\t\t\t\t}\n"
    "\t\t\t\tRaw = string.Join(Environment.NewLine, lines);\n"
    "\t\t\t\tlines = Raw.Replace(\"\\r\", \"\").Split('\\n');\n"
    "\t\t\t\tRebuildIndexUnlocked();\n"
    "\t\t\t}\n"
    "\t\t}\n",
)
replace_eol(
    CONFIGS,
    "\t\tpublic string GetValue(string Section, string ValueName) {\n"
    "\t\t\tlog(\"Getting value :\"+ValueName+\": from section [\"+Section+\"].\");\n"
    "\t\t\tvar sect = HasSection(Section);\n"
    "\t\t\tvar val_line = HasValue(sect, ValueName);\n"
    "\t\t\tif (val_line < 0) return \"\";\n"
    "\t\t\treturn lines[val_line].Split(new []{'='}, 2)[1];\n"
    "\t\t}\n",
    "\t\tpublic string GetValue(string Section, string ValueName) {\n"
    "\t\t\tlock (syncRoot) {\n"
    "\t\t\t\tstring value;\n"
    "\t\t\t\treturn valueIndex.TryGetValue(IndexKey(Section, ValueName), out value) ? value : String.Empty;\n"
    "\t\t\t}\n"
    "\t\t}\n",
)
replace_eol(CONFIGS, "\t        \tvar inini = _INI.Raw;\n", "\t        \tvar inini = _INI.GetRawSnapshot();\n")
replace_eol(CONFIGS, "                File.WriteAllText(temp, _INI.Raw, Encoding.UTF8);\n", "                File.WriteAllText(temp, _INI.GetRawSnapshot(), Encoding.UTF8);\n")
replace_eol(
    UI,
    "\t\t\t\t\t\t\t\tif (!proxyg) \n"
    "\t\t\t\t\t\t\t\t\tMMain.MyConfs._INI.Raw = MMain.MyConfs.GetRawWithoutGroup(\"[Proxy]\", d[ty]);\n"
    "\t\t\t\t\t\t\t\telse\n"
    "\t\t\t\t\t\t\t\t\tMMain.MyConfs._INI.Raw = d[ty];\n",
    "\t\t\t\t\t\t\t\tif (!proxyg) \n"
    "\t\t\t\t\t\t\t\t\tMMain.MyConfs._INI.ReplaceRaw(MMain.MyConfs.GetRawWithoutGroup(\"[Proxy]\", d[ty]));\n"
    "\t\t\t\t\t\t\t\telse\n"
    "\t\t\t\t\t\t\t\t\tMMain.MyConfs._INI.ReplaceRaw(d[ty]);\n",
)

Path(__file__).unlink()
print("Added a thread-safe indexed INI read path while preserving raw file layout and first-key semantics.")
