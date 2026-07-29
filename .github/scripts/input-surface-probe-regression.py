#!/usr/bin/env python3
"""Fail-closed source checks for the standalone read-only input surface probe."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[2]
PROBE = ROOT / "tools" / "input-surface-probe"
SOURCE_FILES = sorted(p for p in PROBE.glob("*.cs"))
REQUIRED_FILES = [
    PROBE / "InputSurfaceProbe.csproj",
    PROBE / "Program.cs",
    PROBE / "WindowsProbe.cs",
    PROBE / "ProbeCore.cs",
    PROBE / "probe-commit.txt",
    PROBE / "tests" / "Program.cs",
]

FORBIDDEN = {
    "UIA whole-value mutation": [r"\bSetValue\s*\("],
    "programmatic selection": [r"\.Select\s*\(", r"setSelectionRange\s*\("],
    "range replacement": [r"\breplaceText\s*\(", r"\bsetRangeText\s*\("],
    "Win32 replacement": [r"\bEM_REPLACESEL\b", r"\bWM_SETTEXT\b"],
    "keyboard simulation": [r"\bSendInput\b", r"\bSendKeys\b", r"\bkeybd_event\b", r"\bPostMessage\b"],
    "clipboard access": [r"\bOpenClipboard\b", r"\bGetClipboardData\b", r"\bSetClipboardData\b", r"\bEmptyClipboard\b", r"\bClipboard\."],
    "hooks or monitoring": [r"\bSetWindowsHookEx\b", r"\bUnhookWindowsHookEx\b", r"\bAddClipboardFormatListener\b", r"\bRegisterHotKey\b"],
    "process injection": [r"\bWriteProcessMemory\b", r"\bCreateRemoteThread\b", r"\bVirtualAllocEx\b", r"\bQueueUserAPC\b"],
    "actual text extraction": [r"\.GetText\s*\(", r"\bWM_GETTEXT\b"],
    "window title collection": [r"\bGetWindowText\b", r"\bMainWindowTitle\b"],
}

REQUIRED_MARKERS = {
    "schema version": (PROBE / "ProbeCore.cs", 'SchemaVersion = "1"'),
    "probe commit resource": (PROBE / "InputSurfaceProbe.csproj", 'EmbeddedResource Include="probe-commit.txt"'),
    "actual text false": (PROBE / "ProbeCore.cs", "actual_text_collected"),
    "classification unknown": (PROBE / "ProbeCore.cs", '"CUSTOM_UNKNOWN"'),
    "password suppression": (PROBE / "ProbeCore.cs", "password_content_suppressed"),
    "metadata-only write label": (PROBE / "ProbeCore.cs", "no-write-capability-verified-by-this-probe"),
    "one-shot countdown": (PROBE / "Program.cs", "CountdownSeconds"),
    "asInvoker": (PROBE / "app.manifest", 'level="asInvoker"'),
    "no uiAccess": (PROBE / "app.manifest", 'uiAccess="false"'),
}


def fail(message: str) -> None:
    print(f"ERROR: {message}", file=sys.stderr)
    raise SystemExit(1)


for path in REQUIRED_FILES:
    if not path.is_file():
        fail(f"required probe file is missing: {path.relative_to(ROOT)}")

source = "\n".join(path.read_text(encoding="utf-8-sig") for path in SOURCE_FILES)
for category, patterns in FORBIDDEN.items():
    for pattern in patterns:
        if re.search(pattern, source, flags=re.IGNORECASE):
            fail(f"forbidden {category} API found: {pattern}")

for name, (path, marker) in REQUIRED_MARKERS.items():
    text = path.read_text(encoding="utf-8-sig")
    if marker not in text:
        fail(f"missing {name} marker in {path.relative_to(ROOT)}")

models = (PROBE / "ProbeCore.cs").read_text(encoding="utf-8-sig")
for forbidden_field in ("actual_text", "window_title", "browser_url", "clipboard_content", "uia_name"):
    if re.search(rf"public\s+\w+\??\s+{forbidden_field}\b", models, flags=re.IGNORECASE):
        fail(f"forbidden report field declared: {forbidden_field}")

windows_probe = (PROBE / "WindowsProbe.cs").read_text(encoding="utf-8-sig")
if "ProcessQueryLimitedInformation = 0x1000" not in windows_probe:
    fail("OpenProcess must remain limited to PROCESS_QUERY_LIMITED_INFORMATION")
if re.search(r"PROCESS_VM_|PROCESS_ALL_ACCESS", windows_probe, flags=re.IGNORECASE):
    fail("probe requests unsafe process access")

print("Input surface probe source regression passed.")
