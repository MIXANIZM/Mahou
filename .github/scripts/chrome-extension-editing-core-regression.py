#!/usr/bin/env python3
"""Fail when the AGZ-MAH-0007 fail-closed Chrome prototype widens its boundary."""
from __future__ import annotations

import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]
PROTOTYPE = ROOT / "integrations/chrome-smart-caps/prototype-extension"
MARKER = "AGZ-MAH-0007-DIAGNOSTIC-V1"
errors: list[str] = []


def read(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8-sig")
    except FileNotFoundError:
        errors.append(f"missing required file: {path.relative_to(ROOT)}")
        return ""


required_files = [
    PROTOTYPE / "manifest.json",
    PROTOTYPE / "service-worker.js",
    PROTOTYPE / "content-script.js",
    PROTOTYPE / "editing-core.js",
    PROTOTYPE / "README.md",
    ROOT / "docs/CHROME-SMART-CAPS-DIAGNOSTIC.html",
    ROOT / "docs/CHROME-EXTENSION-EDITING-CORE.md",
    ROOT / ".github/tests/chrome-extension-editing-core.test.js",
]
for required_file in required_files:
    read(required_file)

try:
    manifest = json.loads(read(PROTOTYPE / "manifest.json"))
except json.JSONDecodeError as error:
    errors.append(f"invalid manifest JSON: {error}")
    manifest = {}

if manifest.get("manifest_version") != 3:
    errors.append("manifest_version must be exactly 3")
permissions = manifest.get("permissions")
if permissions != ["activeTab", "scripting"]:
    errors.append("permissions must be exactly activeTab and scripting in the reviewed order")
if "host_permissions" in manifest:
    errors.append("host_permissions must be absent")
if "content_scripts" in manifest:
    errors.append("declarative content_scripts must be absent; activation must be explicit")
if manifest.get("background", {}).get("service_worker") != "service-worker.js":
    errors.append("service worker entry point is missing")
if "action" not in manifest:
    errors.append("explicit extension action is missing")

javascript_paths = [
    PROTOTYPE / "service-worker.js",
    PROTOTYPE / "content-script.js",
    PROTOTYPE / "editing-core.js",
]
javascript = "\n".join(read(path) for path in javascript_paths)
lower_javascript = javascript.lower()

for token in [
    "<all_urls>", "host_permissions", "nativemessaging", "webrequest", "debugger",
    "downloads", "clipboardread", "clipboardwrite", "xmlhttprequest", "websocket",
    "eventsource", "sendbeacon", "eval(", "new function", "importscripts(",
]:
    if token in lower_javascript:
        errors.append(f"forbidden extension token: {token}")

for token in ["fetch(", "http://", "https://"]:
    if token in lower_javascript:
        errors.append(f"network or remote-code token: {token}")

for token in [
    ".setrangetext(", "execcommand(", ".setselectionrange(", ".dispatchevent(",
    "element.value =", "element.value=", ".value = fullreplacement", ".value=fullreplacement",
]:
    if token in lower_javascript:
        errors.append(f"active mutation token returned: {token}")

service_worker = read(PROTOTYPE / "service-worker.js")
for source, replacement in [
    ("окоРОчка", "окорочка"),
    ("ПРИвет", "Привет"),
    ("КуРиные", "Куриные"),
]:
    if source not in service_worker or replacement not in service_worker:
        errors.append(f"missing fixed candidate: {source} -> {replacement}")
if "chrome.action.onClicked" not in service_worker:
    errors.append("prototype is not explicitly action-activated")
if "active: true, currentWindow: true" not in service_worker:
    errors.append("active tab/window revalidation is missing")
if "allFrames" in service_worker or "frameIds" in service_worker:
    errors.append("prototype must inject into the main frame only")

content_script = read(PROTOTYPE / "content-script.js")
editing_core = read(PROTOTYPE / "editing-core.js")
diagnostic = read(ROOT / "docs/CHROME-SMART-CAPS-DIAGNOSTIC.html")
for path_name, source in [
    ("content-script.js", content_script),
    ("editing-core.js", editing_core),
    ("diagnostic page", diagnostic),
]:
    if MARKER not in source:
        errors.append(f"exact diagnostic marker missing from {path_name}")
if "event.isTrusted" not in diagnostic:
    errors.append("diagnostic page must record event.isTrusted")
if "compositionstart" not in diagnostic or "compositionend" not in diagnostic:
    errors.append("diagnostic page composition trace is incomplete")

required_rejections = [
    "unknown-page", "wrong-frame", "stale-document", "detached-control", "focus-changed",
    "contenteditable-control", "password-control", "unsupported-control", "readonly-control",
    "disabled-control", "hidden-control", "composition-active", "non-collapsed-selection",
    "stale-request", "duplicate-request", "stale-element", "stale-value", "stale-caret",
    "target-boundary-mismatch", "adjacent-text-mismatch", "post-mutation-mismatch",
    "mutation-api-not-accepted",
]
for reason in required_rejections:
    if reason not in editing_core and reason not in content_script:
        errors.append(f"missing fail-closed reason: {reason}")

if "BROWSER-CONTEXT-MUTATION-NOT-SAFE" not in editing_core:
    errors.append("final negative architecture status is missing")
if "setRangeText" not in editing_core or "execCommand-insertText" not in editing_core:
    errors.append("rejected browser mutation methods are not documented in the core")
if "setTimeout(resolve, 75)" not in content_script:
    errors.append("stale-state revalidation window is missing")
if "No text mutation was attempted." not in content_script:
    errors.append("non-mutation result marker is missing")

if errors:
    for error in errors:
        print(f"ERROR: {error}")
    sys.exit(1)

print("Chrome extension editing-core source regression passed.")
