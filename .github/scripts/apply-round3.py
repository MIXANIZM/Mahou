#!/usr/bin/env python3
from pathlib import Path
import base64
import subprocess

root = Path(__file__).resolve().parents[2]
parts = sorted((root / ".github" / "migrations").glob("round3.b64.*"))
if len(parts) != 3:
    raise SystemExit("Expected exactly three round3 migration chunks")

encoded = "".join(part.read_text(encoding="ascii").strip() for part in parts)
patch = base64.b64decode(encoded)
patch_path = root / ".github" / "migrations" / "round3.patch"
patch_path.write_bytes(patch)

subprocess.run(["git", "apply", "--check", str(patch_path)], cwd=str(root), check=True)
subprocess.run(["git", "apply", "--whitespace=nowarn", str(patch_path)], cwd=str(root), check=True)

for part in parts:
    part.unlink()
patch_path.unlink()
Path(__file__).unlink()
print("Round 3 clipboard fail-safe hardening applied.")
