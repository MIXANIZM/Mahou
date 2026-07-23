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
    "build-manifest.json must be at the archive root",
):
    if marker not in verifier:
        errors.append("handoff verifier invariant missing: %s" % marker)

for marker in (
    "intentionally wrong expected commit",
    "nested manifest or sibling archive content",
    "legacy artifact",
    "positive and negative regression tests passed",
):
    if marker not in tests:
        errors.append("provenance negative test marker missing: %s" % marker)

for marker in (
    "Mahou-2.9.0.1-dev-win-x86-<short-sha>-run<run-id>.zip",
    "verify-artifact-provenance.ps1",
    "0f9b75c37413af986aa92170f44b2fd5b397d5a5",
    "8254770086",
):
    if marker not in documentation:
        errors.append("artifact handoff documentation marker missing: %s" % marker)

if errors:
    print("ARTIFACT PROVENANCE REGRESSION FAILED")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("Artifact provenance regression passed.")
