#!/usr/bin/env python3
from pathlib import Path
import shutil

root = Path(__file__).resolve().parents[2]
removed = []

for relative in [
    "Mahou/build-github-release+chocolatey-update.vbs",
    "Mahou/build-run.cmd",
    "Mahou/build.cmd",
    "Mahou/clean.cmd",
]:
    path = root / relative
    if path.exists():
        path.unlink()
        removed.append(relative)

chocolatey = root / "Chocolatey"
if chocolatey.exists():
    shutil.rmtree(chocolatey)
    removed.append("Chocolatey/")

security_path = root / ".github" / "scripts" / "security-regression.py"
security = security_path.read_text(encoding="utf-8")
marker = '''# The only source file allowed to use WebClient is the explicit opt-in translator.
for path in (ROOT / "Mahou").rglob("*.cs"):
'''
replacement = '''# Runtime/distribution scripting is intentionally absent from the application tree.
for pattern in ("*.cmd", "*.bat", "*.vbs", "*.ps1"):
    for script in (ROOT / "Mahou").rglob(pattern):
        errors.append("obsolete executable script remains in application tree: %s" % script.relative_to(ROOT))

# The only source file allowed to use WebClient is the explicit opt-in translator.
for path in (ROOT / "Mahou").rglob("*.cs"):
'''
if security.count(marker) != 1:
    raise SystemExit("Security regression script insertion marker not found exactly once")
security_path.write_text(security.replace(marker, replacement, 1), encoding="utf-8", newline="\n")

report_path = root / "SECURITY-AUDIT-MODERN.md"
report = report_path.read_text(encoding="utf-8")
report += "\n## Obsolete distribution automation removed\n\n"
report += "The MIXANIZM branch no longer contains legacy CMD/VBS release scripts or old Chocolatey download automation.\n"
if removed:
    report += "Removed: " + ", ".join("`%s`" % item for item in removed) + ".\n"
report_path.write_text(report, encoding="utf-8", newline="\n")

Path(__file__).unlink()
print("Round 9 removed obsolete distribution automation: " + ", ".join(removed))
