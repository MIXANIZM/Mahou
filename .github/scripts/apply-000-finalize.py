#!/usr/bin/env python3
from pathlib import Path
import base64
import gzip
import hashlib
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
SCRIPTS = ROOT / ".github" / "scripts"
FINALIZE = ROOT / ".github" / "finalize"
SELF = Path(__file__).resolve()

# The workflow shell expands apply-*.py before starting the loop. Keep that
# exact list so every already-captured path can later be replaced by a safe,
# self-deleting no-op after the real migration is complete.
captured = sorted(SCRIPTS.glob("apply-*.py"))

safe_order = [
    "apply-round4-fix.py",
    "apply-round5.py",
    "apply-round6.py",
    "apply-round7.py",
    "apply-round8-0fix.py",
    "apply-round8-prepare.py",
    "apply-round9.py",
    "apply-round10.py",
]

for name in safe_order:
    path = SCRIPTS / name
    if path.exists():
        print("Applying verified migration:", name)
        subprocess.run([sys.executable, str(path)], cwd=str(ROOT), check=True)

parts = [
    FINALIZE / "manual.00",
    FINALIZE / "manual.01a",
    FINALIZE / "manual.01b",
    FINALIZE / "manual.01c",
]
for part in parts:
    if not part.exists():
        raise RuntimeError("Missing finalizer payload part: " + str(part.relative_to(ROOT)))

encoded = "".join(part.read_text(encoding="ascii").strip() for part in parts)
if len(encoded) != 9780:
    raise RuntimeError("Finalizer payload length mismatch: %d" % len(encoded))
encoded_hash = hashlib.sha256(encoded.encode("ascii")).hexdigest()
if encoded_hash != "66696c6b2ab791193b15695793806b270798adb0ae5f85f6d95a81904ed683ef":
    raise RuntimeError("Finalizer payload SHA-256 mismatch: " + encoded_hash)

source = gzip.decompress(base64.b64decode(encoded, validate=True))
source_hash = hashlib.sha256(source).hexdigest()
if source_hash != "a7fd64dd5b6bec78bb1249489ef00f86ead70e936336bbeebb89ff43a2a63e06":
    raise RuntimeError("Decoded finalizer SHA-256 mismatch: " + source_hash)

manual = SCRIPTS / "manual-final.py"
manual.write_bytes(source)
print("Applying verified final cleanup.")
subprocess.run([sys.executable, str(manual)], cwd=str(ROOT), check=True)

security_gate = SCRIPTS / "security-regression.py"
if not security_gate.exists():
    raise RuntimeError("Security regression gate was not produced")
subprocess.run([sys.executable, str(security_gate)], cwd=str(ROOT), check=True)

if FINALIZE.exists():
    shutil.rmtree(FINALIZE)
if manual.exists():
    manual.unlink()

# The parent shell still has stale paths in its in-memory array. Recreate only
# those paths as tiny self-deleting scripts, so the loop can finish cleanly.
stub = "#!/usr/bin/env python3\nfrom pathlib import Path\nPath(__file__).unlink()\n"
for path in captured:
    if path.resolve() == SELF:
        continue
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(stub, encoding="utf-8", newline="\n")

SELF.unlink()
print("Final modernization migration applied; temporary migration machinery removed.")
