#!/usr/bin/env python3
from pathlib import Path

root = Path(__file__).resolve().parents[2]
readme_path = root / "README.md"
upstream_path = root / "UPSTREAM-README.md"
mixanizm_path = root / "README-MIXANIZM.md"

if not upstream_path.exists():
    upstream_path.write_text(readme_path.read_text(encoding="utf-8-sig"), encoding="utf-8", newline="\n")

base = mixanizm_path.read_text(encoding="utf-8")
front = '''# MIXANIZM Mahou

Privacy-hardened modernization of the latest preserved Mahou `2.9.0.1-dev` source line.

> **Draft status:** binaries from this branch are test builds, not a public release. The
> project still requires physical Windows 11 keyboard/clipboard testing, an independent
> repeat audit and Authenticode signing.

'''
# Avoid repeating the first heading from README-MIXANIZM.
body_lines = base.splitlines()
if body_lines and body_lines[0].startswith("# "):
    body_lines = body_lines[1:]
while body_lines and not body_lines[0].strip():
    body_lines.pop(0)

build = '''
## Build verification

GitHub Actions builds x86 and x64 twice in isolated directories and rejects differing
outputs. Each artifact contains SHA-256 sums, a build manifest, CycloneDX SBOM,
security-regression report, bundled AutoSwitch dictionary and Windows 11 test plan.

## Project history and license

This branch is based on GPL v2+ Mahou by BladeMight and later preserved contributors.
The original repository documentation is retained in `UPSTREAM-README.md`; attribution
is recorded in `NOTICE.md`. MIXANIZM modifications remain under the repository license.
'''
readme_path.write_text(front + "\n".join(body_lines).rstrip() + "\n" + build, encoding="utf-8", newline="\n")

notice = '''# Notice

MIXANIZM Mahou is a modernization of the GPL v2+ Mahou keyboard layout project.

Original Mahou authorship and copyright belong to BladeMight and the contributors visible
in the preserved Git history. The modern baseline used here follows the preserved
`2.9.0.1-dev` lineage at upstream commit
`7dac9b588f71b004489034056fcc8ccb929c3ebb`.

MIXANIZM is responsible for the security, privacy, build and product changes made on the
`mixanizm-modern-v2.9.0.1` branch. No endorsement by the original authors is implied.
'''
(root / "NOTICE.md").write_text(notice, encoding="utf-8", newline="\n")

trigger = root / ".github" / "security-regression-trigger.txt"
if trigger.exists():
    trigger.unlink()

Path(__file__).unlink()
print("Round 10 repository documentation and attribution applied.")
