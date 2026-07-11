#!/usr/bin/env python3
from pathlib import Path
import shutil

root = Path(__file__).resolve().parents[2]
workflow_path = root / ".github" / "workflows" / "modern-windows-build.yml"
text = workflow_path.read_text(encoding="utf-8")

if "permissions:\n  contents: write\n" not in text:
    raise SystemExit("Expected temporary write permission was not found")
text = text.replace("permissions:\n  contents: write\n", "permissions:\n  contents: read\n", 1)

start = text.find("  apply-migrations:\n")
end = text.find("  build:\n", start)
if start < 0 or end < 0:
    raise SystemExit("Temporary migration job boundaries were not found")
text = text[:start] + text[end:]
text = text.replace("    needs: apply-migrations\n", "", 1)

workflow_path.write_text(text, encoding="utf-8", newline="\n")

scripts_dir = root / ".github" / "scripts"
for script in scripts_dir.glob("apply-round*.py"):
    if script.name != Path(__file__).name:
        script.unlink()

migrations_dir = root / ".github" / "migrations"
if migrations_dir.exists():
    shutil.rmtree(migrations_dir)

Path(__file__).unlink()
print("Temporary self-modifying CI removed; workflows now use read-only repository permissions.")
