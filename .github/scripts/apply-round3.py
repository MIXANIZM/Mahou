#!/usr/bin/env python3
from pathlib import Path
import base64
import hashlib
import subprocess
import traceback

root = Path(__file__).resolve().parents[2]
migrations = root / ".github" / "migrations"
diagnostics = root / "migration-diagnostics"
diagnostics.mkdir(parents=True, exist_ok=True)

try:
    parts = sorted(migrations.glob("round3.b64.*"))
    lines = ["parts=" + str(len(parts))]
    for part in parts:
        data = part.read_bytes()
        lines.append(part.name + " size=" + str(len(data)) + " sha256=" + hashlib.sha256(data).hexdigest())
    if len(parts) != 3:
        raise RuntimeError("Expected exactly three round3 migration chunks")

    encoded = "".join(part.read_text(encoding="ascii").strip() for part in parts)
    lines.append("encoded_length=" + str(len(encoded)))
    patch = base64.b64decode(encoded, validate=True)
    lines.append("patch_size=" + str(len(patch)) + " patch_sha256=" + hashlib.sha256(patch).hexdigest())
    (diagnostics / "round3-summary.txt").write_text("\n".join(lines) + "\n", encoding="utf-8")

    patch_path = migrations / "round3.patch"
    patch_path.write_bytes(patch)

    check = subprocess.run(["git", "apply", "--check", str(patch_path)], cwd=str(root), text=True,
                           stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    (diagnostics / "git-apply-check.txt").write_text(check.stdout or "", encoding="utf-8")
    if check.returncode != 0:
        raise RuntimeError("git apply --check failed with code " + str(check.returncode))

    subprocess.run(["git", "apply", "--whitespace=nowarn", str(patch_path)], cwd=str(root), check=True)

    for part in parts:
        part.unlink()
    patch_path.unlink()
    Path(__file__).unlink()
    print("Round 3 clipboard fail-safe hardening applied.")
except BaseException:
    (diagnostics / "round3-error.txt").write_text(traceback.format_exc(), encoding="utf-8")
    raise
