#!/usr/bin/env python3
from pathlib import Path
import re
import sys


ROOT = Path(__file__).resolve().parents[2]
errors = []

modern_workflow = (ROOT / ".github/workflows/modern-windows-build.yml").read_text(encoding="utf-8")
security_workflow = (ROOT / ".github/workflows/security-regression.yml").read_text(encoding="utf-8")
packager = (ROOT / ".github/scripts/new-build-provenance.ps1").read_text(encoding="utf-8")
verifier = (ROOT / ".github/scripts/verify-artifact-provenance.ps1").read_text(encoding="utf-8")
tests = (ROOT / ".github/scripts/test-artifact-provenance.ps1").read_text(encoding="utf-8")
documentation = (ROOT / "ARTIFACT-PROVENANCE.md").read_text(encoding="utf-8")
legacy_fixture = (
    ROOT / ".github/tests/fixtures/legacy-build-manifest-v1.json"
).read_text(encoding="utf-8")

all_workflows = "\n".join(
    path.read_text(encoding="utf-8")
    for path in sorted((ROOT / ".github/workflows").glob("*.yml"))
)

for forbidden in (
    "contents: write",
    "git push",
    "git commit",
    "base64 -d",
    "fromBase64String",
    "one-shot",
):
    if forbidden.lower() in all_workflows.lower():
        errors.append("forbidden writable/transport workflow marker: %s" % forbidden)

for forbidden_name in (
    "name: Mahou-x86",
    "name: Mahou-modern-x86",
    "name: Mahou-x86-full",
    "name: MIXANIZM-Mahou-modern-x86",
    "name: MIXANIZM-Mahou-modern-x64",
):
    if forbidden_name in all_workflows:
        errors.append("generic artifact name remains in workflow: %s" % forbidden_name)

expected_action_pins = (
    "actions/checkout@11d5960a326750d5838078e36cf38b85af677262 # v4.4.0",
    "microsoft/setup-msbuild@6fb02220983dee41ce7ae257b6f4d8f9bf5ed4ce # v2",
    "actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2",
)
for action_pin in expected_action_pins:
    if action_pin not in all_workflows:
        errors.append("required immutable action pin is missing: %s" % action_pin)

for action_use in re.findall(r"^\s*uses:\s*([^#\s]+)", all_workflows, re.MULTILINE):
    if not re.fullmatch(r"[^@\s]+@[0-9a-f]{40}", action_use):
        errors.append("workflow action is not pinned to a full commit SHA: %s" % action_use)

for workflow_name, workflow in (
    ("modern-windows-build.yml", modern_workflow),
    ("security-regression.yml", security_workflow),
):
    if not re.search(r"permissions:\s*\n(?:\s+[a-z-]+:\s*read\s*\n)+", workflow):
        errors.append("workflow permissions are not explicitly read-only: %s" % workflow_name)
    if "persist-credentials: false" not in workflow:
        errors.append("checkout credentials are not disabled: %s" % workflow_name)
    if "ref: ${{ github.event.pull_request.head.sha || github.sha }}" not in workflow:
        errors.append("workflow does not check out the exact PR/dispatch head: %s" % workflow_name)
    if "mixanizm-modern-v2.9.0.1" not in workflow:
        errors.append("development PR trigger is missing: %s" % workflow_name)

if "ref: mixanizm-modern-v2.9.0.1" in all_workflows:
    errors.append("a workflow still hard-codes the development branch checkout")

for marker in (
    "Mahou-$runtimeVersion-win-$platform-$shortSha-run$env:GITHUB_RUN_ID",
    "artifact-id",
    "artifact-digest",
    "artifact-evidence-",
    "new-build-provenance.ps1",
    "test-artifact-provenance.ps1",
    "-ExpectedPlatform $platform",
    "-ExpectedRepository '${{ github.repository }}'",
    "-ExpectedRuntimeVersion $env:RUNTIME_VERSION",
    "legacy-build-manifest-v1.json",
):
    if marker not in modern_workflow:
        errors.append("modern build provenance marker missing: %s" % marker)

for field in (
    "product",
    "runtime_version",
    "source_repository",
    "source_commit",
    "source_tree",
    "source_ref",
    "workflow_name",
    "workflow_run_id",
    "workflow_run_attempt",
    "platform",
    "configuration",
    "build_timestamp_utc",
    "deterministic_build",
    "security_regression_passed",
):
    if field not in packager:
        errors.append("required build manifest field missing: %s" % field)

for marker in (
    "SHA256SUMS.txt",
    "Get-FileHash",
    "Issued file is not covered by SHA256SUMS.txt",
    "Expected commit is not embedded in Mahou.exe",
    "Executable version mismatch",
    "Source tree mismatch",
    "Platform mismatch",
    "Source repository mismatch",
    "Runtime version mismatch",
    "build-manifest.json must be at the archive root",
):
    if marker not in verifier:
        errors.append("handoff verifier invariant missing: %s" % marker)

for marker in (
    "intentionally wrong expected commit",
    "intentionally wrong expected tree",
    "intentionally wrong expected platform",
    "one-byte Mahou.exe.config modification",
    "nested manifest or sibling archive content",
    "legacy artifact",
    "positive and negative regression tests passed",
):
    if marker not in tests:
        errors.append("provenance negative test marker missing: %s" % marker)

for marker in (
    '"schema_version": 1',
    '"product": "MIXANIZM Mahou"',
    '"version": "2.9.0.1-mixanizm"',
    '"source_commit": "0f9b75c37413af986aa92170f44b2fd5b397d5a5"',
    '"deterministic_rebuild_verified": true',
    '"security_regression": "passed"',
):
    if marker not in legacy_fixture:
        errors.append("legacy schema-v1 fixture marker missing: %s" % marker)

for marker in (
    "Mahou-2.9.0.1-dev-win-x86-<short-sha>-run<run-id>.zip",
    "verify-artifact-provenance.ps1",
    "0f9b75c37413af986aa92170f44b2fd5b397d5a5",
    "8254770086",
    "workflow_dispatch",
    "exact merged commit",
):
    if marker not in documentation:
        errors.append("artifact handoff documentation marker missing: %s" % marker)

if errors:
    print("ARTIFACT PROVENANCE REGRESSION FAILED")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("Artifact provenance regression passed.")
