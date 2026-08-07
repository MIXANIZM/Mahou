#!/usr/bin/env python3
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]
SAFETY = (ROOT / "Mahou" / "Classes" / "AutoSwitchSafety.cs").read_text(encoding="utf-8-sig")
HOOK = (ROOT / "Mahou" / "Classes" / "KMHook.cs").read_text(encoding="utf-8-sig")
PROJECT = (ROOT / "Mahou" / "Mahou.csproj").read_text(encoding="utf-8-sig")
WINDOWS_WORKFLOW = (ROOT / ".github" / "workflows" / "modern-windows-build.yml").read_text(encoding="utf-8-sig")
SECURITY_WORKFLOW = (ROOT / ".github" / "workflows" / "security-regression.yml").read_text(encoding="utf-8-sig")

errors = []

for marker in (
    'ModernNotepadExecutable = "notepad.exe"',
    'ModernNotepadControlClass = "RichEditD2DPT"',
    "IsModernNotepadSurface",
    "TryCaptureAllowedSource",
    "CanMutateNow",
    "!current.Protected",
    "expected.Foreground == current.Foreground",
    "expected.FocusedControl == current.FocusedControl",
    "expected.ProcessId == current.ProcessId",
    "String.Equals(expected.ProcessExecutable, current.ProcessExecutable",
    "String.Equals(expected.ControlClass, current.ControlClass",
):
    if marker not in SAFETY:
        errors.append("AutoSwitch safety invariant missing: " + marker)

if 'Compile Include="Classes\\AutoSwitchSafety.cs"' not in PROJECT:
    errors.append("AutoSwitchSafety.cs is not compiled into Mahou")

capture = HOOK.find("AutoSwitchSafety.TryCaptureAllowedSource(out autoSwitchContext)")
route = HOOK.find("CheckAutoSwitch(sourceText, currentWord, autoSwitchContext)")
if capture < 0 or route < 0 or capture > route:
    errors.append("AutoSwitch source is not captured before dictionary routing")

replacement_start = HOOK.find("static void PerformAutoSwitchLiteralReplacement(")
replacement_end = HOOK.find("static WinAPI.INPUT[] BuildAutoSwitchLiteralInputs", replacement_start)
replacement = HOOK[replacement_start:replacement_end]
if replacement_start < 0 or replacement_end < 0:
    errors.append("dedicated literal AutoSwitch replacement primitive is missing")
elif replacement.count("AutoSwitchSafety.CanMutateNow(autoSwitchContext)") < 7:
    errors.append("literal AutoSwitch replacement is not revalidated before every mutation stage")

for mutation in (
    "KInputs.MakeInput(KInputs.AddPress(Keys.Back))",
    "KInputs.MakeInput(KInputs.AddPress(Keys.Back, word.Count))",
    "KInputs.MakeInput(BuildAutoSwitchLiteralInputs(replacementText))",
    "ChangeToLayout(Locales.ActiveWindow(), targetLayout)",
    "KInputs.MakeInput(KInputs.AddPress(Keys.Space))",
):
    if mutation not in replacement and mutation not in HOOK[route:replacement_start]:
        errors.append("expected guarded AutoSwitch mutation missing: " + mutation)

if "AutoSwitchSafety.CanMutateNow(autoSwitchContext)" not in HOOK[route:replacement_start]:
    errors.append("AutoSwitch routing lacks source-context revalidation")

for forbidden in (
    "ExpandSnippet", "ExpandSnippetWithExpressions", "CheckSnippet", "__execute",
    "__delay", "__keyboard", "__paste", "__selection", "__setlayout",
    "snippets.txt", "SnippetsEnabled",
):
    if forbidden in HOOK:
        errors.append("removed snippet capability remains in AutoSwitch hook: " + forbidden)

for workflow_name, workflow in (
    ("Modern Windows build", WINDOWS_WORKFLOW),
    ("Security regression", SECURITY_WORKFLOW),
):
    if "autoswitch-containment-regression.py" not in workflow:
        errors.append(workflow_name + " does not run the AutoSwitch containment source regression")
    if "autoswitch-independence-regression.py" not in workflow:
        errors.append(workflow_name + " does not run the AutoSwitch independence source regression")

if "AutoSwitchContainmentRegression.cs" not in WINDOWS_WORKFLOW:
    errors.append("Modern Windows build does not run the deterministic containment regression")
if "AutoSwitchIndependenceRegression.cs" not in WINDOWS_WORKFLOW:
    errors.append("Modern Windows build does not run the deterministic independence regression")
if "RUNTIME_VERSION: 2.9.0.1-dev" not in WINDOWS_WORKFLOW:
    errors.append("runtime version changed from 2.9.0.1-dev")

if errors:
    print("AUTOSWITCH CONTAINMENT REGRESSION FAILED")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("AutoSwitch containment source regression passed.")
