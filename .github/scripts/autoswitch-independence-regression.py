#!/usr/bin/env python3
"""Prove that AutoSwitch is independent and the user snippets product surface is inactive."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[2]

def text(path):
    return (ROOT / path).read_text(encoding="utf-8-sig")

runtime_paths = sorted(
    str(path.relative_to(ROOT)).replace("\\", "/")
    for path in (ROOT / "Mahou").rglob("*")
    if path.suffix.lower() in {".cs", ".resx", ".config"}
)
runtime = {path: text(path) for path in runtime_paths}
hook = runtime["Mahou/Classes/KMHook.cs"]
ui = runtime["Mahou/MahouUI.cs"]
designer = runtime["Mahou/MahouUI.Designer.cs"]
configs = runtime["Mahou/Classes/Configs.cs"]
paths = runtime["Mahou/Classes/UserDataPaths.cs"]
workflow = text(".github/workflows/modern-windows-build.yml")
errors = []

# No active product/runtime snippet surface remains.
for path, source in runtime.items():
    lowered = source.lower()
    for forbidden in (
        "snippetsenabled", "snippets.txt", "snippetsexpandtype", "expandsnippet",
        "checksnippet", "txt_snippets", "tab_snippets", "snipfile", "snippetscount",
        "allowsnippetexecute", "__execute", "__delay", "__keyboard", "__paste",
        "__selection", "__setlayout", "__setsnip", "__setlsnip",
    ):
        if forbidden in lowered:
            errors.append(f"active snippets token remains in {path}: {forbidden}")

# AutoSwitch routing is standalone and cannot be gated by legacy snippets state.
auto_region_start = hook.find("#region AutoSwitch")
auto_region_end = hook.find("#endregion", auto_region_start)
auto_region = hook[auto_region_start:auto_region_end]
for required in (
    "if (!MahouUI.AutoSwitchEnabled)",
    "autoSwitchText.Add(sym)",
    "AutoSwitchSafety.TryCaptureAllowedSource(out autoSwitchContext)",
    "CheckAutoSwitch(sourceText, currentWord, autoSwitchContext)",
):
    if required not in auto_region:
        errors.append("standalone AutoSwitch routing marker missing: " + required)
if "Snippet" in auto_region or "snip" in auto_region.lower():
    errors.append("AutoSwitch routing still contains snippet dependencies")

for required in (
    "static readonly List<char> autoSwitchText",
    "static string lastAutoSwitchText",
    "static void PerformAutoSwitchLiteralReplacement(",
    "static WinAPI.INPUT[] BuildAutoSwitchLiteralInputs(string replacementText)",
    "KInputs.MakeInput(BuildAutoSwitchLiteralInputs(replacementText))",
    "public static void ReloadAutoSwitchDictionary()",
    "LoadAutoSwitchDictionary(MahouUI.AutoSwitchDictionaryRaw)",
):
    if required not in hook:
        errors.append("AutoSwitch independence marker missing: " + required)

literal_builder_start = hook.find("static WinAPI.INPUT[] BuildAutoSwitchLiteralInputs")
literal_builder_end = hook.find("static string CreateHFDir", literal_builder_start)
literal_builder = hook[literal_builder_start:literal_builder_end]
for forbidden in ("Regex", "Process", "Clipboard", "DoLater", "Thread.Sleep", "__"):
    if forbidden in literal_builder:
        errors.append("literal replacement builder interprets or mutates replacement text: " + forbidden)

# The runtime loads only AS_dict.txt and never creates or modifies legacy snippet files.
if '"AS_dict.txt"' not in ui or 'AS_dictfile' not in ui:
    errors.append("AutoSwitch dictionary path is missing")
if "KMHook.ReloadAutoSwitchDictionary();" not in ui:
    errors.append("AutoSwitch dictionary is not reloaded independently")
for source_name, source in (("ui", ui), ("paths", paths), ("hook", hook), ("configs", configs)):
    for forbidden in ("snippets.txt", "snippets.txt.bak"):
        if forbidden in source.lower():
            errors.append(f"{source_name} still accesses legacy {forbidden}")

# No snippets UI controls or legacy config bindings remain.
for forbidden in ("Snippets", "Snippet", "txt_Snip", "tab_Snip", "lnk_Snip"):
    if forbidden in designer:
        errors.append("user snippets UI remains: " + forbidden)
for forbidden in ('"Snippets"', "SnippetsEnabled", "SnippetExpand", "SnippetSpace", "SnippetSelection"):
    if forbidden in configs or forbidden in ui:
        errors.append("legacy snippets configuration remains active: " + forbidden)

# Existing legacy files are inactive data, not deletion targets.
combined = "\n".join(runtime.values())
if re.search(r"File\.Delete\s*\([^\n]*(?:snippet|snip)", combined, re.I):
    errors.append("runtime deletes legacy snippet data")

# Product packaging and active documentation no longer carry snippet examples or claims.
for obsolete in (
    "Notepad++/Mahou_snippets.xml",
    "snippet-bounds-diagnostics/result.txt",
    "snippet-removal-diagnostics/result.txt",
):
    if (ROOT / obsolete).exists():
        errors.append("obsolete snippets artifact remains: " + obsolete)
for active_doc in ("README.md", "README-MIXANIZM.md", "TEST-PLAN-WINDOWS11.md"):
    source = text(active_doc)
    if active_doc == "README.md" and "User-defined snippets are removed" not in source:
        errors.append("primary README does not record snippets removal")
    if "Run together with AutoSwitch and snippets" in source:
        errors.append("active test plan still asks for snippets testing")

# Exact containment and runtime line stay in the gate.
if '"notepad.exe"' not in text("Mahou/Classes/AutoSwitchSafety.cs") or \
   '"RichEditD2DPT"' not in text("Mahou/Classes/AutoSwitchSafety.cs"):
    errors.append("exact modern Notepad containment markers changed")
if "RUNTIME_VERSION: 2.9.0.1-dev" not in workflow:
    errors.append("runtime version changed")

if errors:
    print("AUTOSWITCH INDEPENDENCE REGRESSION FAILED")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("AutoSwitch independence source regression passed.")
