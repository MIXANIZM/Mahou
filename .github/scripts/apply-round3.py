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

expected_encoded_length = 24680
expected_encoded_sha256 = "7da27a8c708c555517d2d22d03b50e9915da0912336c6ea7a09982852ee28f67"
expected_patch_size = 18510
expected_patch_sha256 = "ae0eb34751924e48b8bd948c89972af640366292e2952999191aea2ce705e97c"

try:
    parts = sorted(migrations.glob("r3small.*"))
    if len(parts) < 7:
        print("Round 3 waiting for all chunks: %d/7" % len(parts))
        raise SystemExit(0)
    if len(parts) != 7:
        raise RuntimeError("Expected exactly seven round3 migration chunks, found %d" % len(parts))

    lines = ["parts=" + str(len(parts))]
    for part in parts:
        data = part.read_bytes()
        lines.append(part.name + " size=" + str(len(data)) + " sha256=" + hashlib.sha256(data).hexdigest())

    encoded = "".join(part.read_text(encoding="ascii").strip() for part in parts)
    encoded_bytes = encoded.encode("ascii")
    encoded_hash = hashlib.sha256(encoded_bytes).hexdigest()
    lines.append("encoded_length=" + str(len(encoded)))
    lines.append("encoded_sha256=" + encoded_hash)
    (diagnostics / "round3-summary.txt").write_text("\n".join(lines) + "\n", encoding="utf-8")
    if len(encoded) != expected_encoded_length or encoded_hash != expected_encoded_sha256:
        raise RuntimeError("Round 3 encoded payload identity mismatch")

    patch = base64.b64decode(encoded, validate=True)
    patch_hash = hashlib.sha256(patch).hexdigest()
    lines.append("patch_size=" + str(len(patch)))
    lines.append("patch_sha256=" + patch_hash)
    (diagnostics / "round3-summary.txt").write_text("\n".join(lines) + "\n", encoding="utf-8")
    if len(patch) != expected_patch_size or patch_hash != expected_patch_sha256:
        raise RuntimeError("Round 3 decoded patch identity mismatch")

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
    for old_part in migrations.glob("round3.b64.*"):
        old_part.unlink()
    patch_path.unlink()
    Path(__file__).unlink()
    print("Round 3 clipboard fail-safe hardening applied.")
except SystemExit:
    raise
except BaseException:
    (diagnostics / "round3-error.txt").write_text(traceback.format_exc(), encoding="utf-8")
    raise
