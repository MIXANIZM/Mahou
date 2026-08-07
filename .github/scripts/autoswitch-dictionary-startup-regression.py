#!/usr/bin/env python3
"""Lock the linear, single-pass AutoSwitch dictionary startup path."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]


def text(path):
    return (ROOT / path).read_text(encoding="utf-8-sig")


parser = text("Mahou/Classes/AutoSwitchDictionaryParser.cs")
hook = text("Mahou/Classes/KMHook.cs")
ui = text("Mahou/MahouUI.cs")
project = text("Mahou/Mahou.csproj")
windows_workflow = text(".github/workflows/modern-windows-build.yml")
security_workflow = text(".github/workflows/security-regression.yml")
errors = []

for required in (
    "internal static class AutoSwitchDictionaryParser",
    "Interlocked.Increment(ref invocationCount)",
    "dictionary.IndexOf(ReplacementMarker, sourceStart,",
    "lineEnd - sourceStart, StringComparison.Ordinal)",
    "dictionary.IndexOf(EndMarker, replacementStart,",
    "dictionary.Length - replacementStart, StringComparison.Ordinal)",
    "cursor = replacementEnd + EndMarker.Length",
    "return new AutoSwitchDictionaryParseResult(true, sources.ToArray(), replacements.ToArray()",
):
    if required not in parser:
        errors.append("linear parser invariant missing: " + required)

for forbidden in (
    "Substring(index + 5)",
    "Substring(index+5)",
    'dictionary = dictionary.Replace("\\r", "")',
    "new bool[dictionary.Length]",
    "GetAutoSwitchDictionaryCount",
):
    if forbidden in parser or forbidden in hook or forbidden in ui:
        errors.append("quadratic or duplicate dictionary work remains: " + forbidden)

if 'Compile Include="Classes\\AutoSwitchDictionaryParser.cs"' not in project:
    errors.append("AutoSwitch dictionary parser is not compiled into Mahou")

load_start = ui.find("void LoadConfigs()")
load_end = ui.find("\n\t\tList<string[]> ParseSets", load_start)
load_body = ui[load_start:load_end]
if load_start < 0 or load_end < 0:
    errors.append("LoadConfigs body not found")
elif load_body.count("KMHook.ReloadAutoSwitchDictionary()") != 1:
    errors.append("LoadConfigs must invoke the AutoSwitch parser exactly once")
if "UpdateAutoSwitchCountLabel(AutoSwitchDictionaryRaw" in load_body:
    errors.append("LoadConfigs still reparses the dictionary to display the count")

text_changed_start = ui.find("void Txt_AutoSwitchDictionaryTextChanged")
text_changed_end = ui.find("void Chk_DownloadASD_InZipCheckedChanged", text_changed_start)
text_changed = ui[text_changed_start:text_changed_end]
if "configs_loading || autoSwitchDictionaryTextUpdating" not in text_changed:
    errors.append("configuration loading does not suppress TextChanged dictionary parsing")

for workflow_name, workflow in (
    ("Modern Windows build", windows_workflow),
    ("Security regression", security_workflow),
):
    if "autoswitch-dictionary-startup-regression.py" not in workflow:
        errors.append(workflow_name + " does not run the startup source regression")
if "AutoSwitchDictionaryStartupRegression.cs" not in windows_workflow:
    errors.append("Modern Windows build does not compile the startup executable regression")

if errors:
    print("AUTOSWITCH DICTIONARY STARTUP REGRESSION FAILED")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("AutoSwitch dictionary startup source regression passed.")
