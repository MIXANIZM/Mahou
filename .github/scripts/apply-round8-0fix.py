#!/usr/bin/env python3
from pathlib import Path

root = Path(__file__).resolve().parents[2]
prepare_path = root / ".github" / "scripts" / "apply-round8-prepare.py"
queued_path = root / ".github" / "scripts" / "apply-round8.py"

prepare = prepare_path.read_text(encoding="utf-8")
old = '''if queued_script.exists():
    queued_script.unlink()
Path(__file__).unlink()
'''
new = '''if queued_script.exists():
    queued_script.write_text(
        "#!/usr/bin/env python3\\nfrom pathlib import Path\\n"
        "Path(__file__).unlink()\\n"
        "print('Privacy defaults were applied by the resilient preparation step.')\\n",
        encoding="utf-8",
        newline="\\n",
    )
Path(__file__).unlink()
'''
if prepare.count(old) != 1:
    raise SystemExit("Queued privacy script handoff marker not found exactly once")
prepare_path.write_text(prepare.replace(old, new, 1), encoding="utf-8", newline="\n")
Path(__file__).unlink()
print("Privacy migration queue handoff made safe.")
