#!/usr/bin/env python3
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[2]
SAFETY = (ROOT / "Mahou" / "Classes" / "AutoSwitchSafety.cs").read_text(encoding="utf-8")
HOOK = (ROOT / "Mahou" / "Classes" / "KMHook.cs").read_text(encoding="utf-8")
PROJECT = (ROOT / "Mahou" / "Mahou.csproj").read_text(encoding="utf-8")
WINDOWS_WORKFLOW = (ROOT / ".github" / "workflows" / "modern-windows-build.yml").read_text(encoding="utf-8")
SECURITY_WORKFLOW = (ROOT / ".github" / "workflows" / "security-regression.yml").read_text(encoding="utf-8")

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
):
    if marker not in SAFETY:
        errors.append("AutoSwitch safety invariant missing: " + marker)

if 'Compile Include="Classes\\AutoSwitchSafety.cs"' not in PROJECT:
    errors.append("AutoSwitchSafety.cs is not compiled into Mahou")

capture = HOOK.find("AutoSwitchSafety.TryCaptureAllowedSource(out autoSwitchContext)")
first_check = HOOK.find("CheckAutoSwitch(snip, CW, true, autoSwitchContext)")
if capture < 0 or first_check < 0 or capture > first_check:
    errors.append("AutoSwitch source is not captured before dictionary mutation routing")

if re.search(r"CheckAutoSwitch\s*\([^,\n]+,\s*[^,\n\)]+\)", HOOK):
    errors.append("an AutoSwitch call site bypasses the source-context argument")

for caller in ("jkl_autoswitch_back", "jkl_autoswitch_back2", "autoswitch_back", "autoswitch_back2"):
    index = HOOK.find('"' + caller + '"')
    if index < 0:
        errors.append("AutoSwitch mutation call site missing: " + caller)
        continue
    window = HOOK[max(0, index - 260):index]
    if "AutoSwitchSafety.CanMutateNow(autoSwitchContext)" not in window:
        errors.append(caller + " is not guarded immediately before Backspace")

start_convert = HOOK.find("public static void StartConvertWord(")
end_convert = HOOK.find("\n\t\t}", start_convert)
convert_body = HOOK[start_convert:end_convert]
if "AutoSwitchSourceContext autoSwitchContext = null" not in convert_body:
    errors.append("converted-word path does not carry AutoSwitch source context")
if convert_body.count("AutoSwitchSafety.CanMutateNow(autoSwitchContext)") < 3:
    errors.append("converted-word path is not revalidated before delete, delay, and insertion")

expand = HOOK.find("static void ExpandSnippet(")
expand_end = HOOK.find("\n\t\t#endregion", expand)
expand_body = HOOK[expand:expand_end]
if "AutoSwitchSourceContext autoSwitchContext = null" not in expand_body:
    errors.append("post-conversion expansion path does not carry AutoSwitch source context")
if expand_body.count("AutoSwitchSafety.CanMutateNow(autoSwitchContext)") < 3:
    errors.append("post-conversion layout/space/error path lacks source revalidation")

for workflow_name, workflow in (
    ("Modern Windows build", WINDOWS_WORKFLOW),
    ("Security regression", SECURITY_WORKFLOW),
):
    if "autoswitch-containment-regression.py" not in workflow:
        errors.append(workflow_name + " does not run the AutoSwitch source regression")

if "AutoSwitchContainmentRegression.cs" not in WINDOWS_WORKFLOW:
    errors.append("Modern Windows build does not run the deterministic AutoSwitch regression")

if "RUNTIME_VERSION: 2.9.0.1-dev" not in WINDOWS_WORKFLOW:
    errors.append("runtime version changed from 2.9.0.1-dev")

if errors:
    print("AUTOSWITCH CONTAINMENT REGRESSION FAILED")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("AutoSwitch containment source regression passed.")
