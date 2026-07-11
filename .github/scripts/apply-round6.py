#!/usr/bin/env python3
from pathlib import Path

root = Path(__file__).resolve().parents[2]
path = root / ".github" / "workflows" / "modern-windows-build.yml"
text = path.read_text(encoding="utf-8")


def replace_once(old, new, label):
    global text
    count = text.count(old)
    if count != 1:
        raise SystemExit("Expected %s exactly once, found %d" % (label, count))
    text = text.replace(old, new, 1)


replace_once(
'''      - name: Package source snapshot
        if: matrix.platform == 'x86'
''',
'''      - name: Verify security invariants
        shell: pwsh
        run: |
          python .github/scripts/security-regression.py | Tee-Object -FilePath security-regression.txt
          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

      - name: Package source snapshot
        if: matrix.platform == 'x86'
''',
    "pre-build security gate",
)

replace_once(
'''          Copy-Item (Join-Path $source 'AS_dict.txt') $dest -Force
          Copy-Item "deterministic-build-$platform.txt" $dest -Force

          $commit = (git rev-parse HEAD).Trim()
          $files = @('Mahou.exe','Mahou.exe.config','AS_dict.txt')
''',
'''          Copy-Item (Join-Path $source 'AS_dict.txt') $dest -Force
          Copy-Item "deterministic-build-$platform.txt" $dest -Force
          Copy-Item 'security-regression.txt' $dest -Force
          Copy-Item 'README-MIXANIZM.md' $dest -Force
          Copy-Item 'SECURITY-AUDIT-MODERN.md' $dest -Force
          Copy-Item 'KNOWN-LIMITATIONS.md' $dest -Force
          Copy-Item 'TEST-PLAN-WINDOWS11.md' $dest -Force

          $commit = (git rev-parse HEAD).Trim()
          $files = @(
            'Mahou.exe',
            'Mahou.exe.config',
            'AS_dict.txt',
            'deterministic-build-' + $platform + '.txt',
            'security-regression.txt',
            'README-MIXANIZM.md',
            'SECURITY-AUDIT-MODERN.md',
            'KNOWN-LIMITATIONS.md',
            'TEST-PLAN-WINDOWS11.md'
          )
''',
    "release documentation copy",
)

replace_once(
'''            public_sync = 'disabled'
            files = $manifestFiles
''',
'''            public_sync = 'disabled'
            security_regression = 'passed'
            runtime_test_status = 'required before release'
            files = $manifestFiles
''',
    "manifest security status",
)

path.write_text(text, encoding="utf-8", newline="\n")
Path(__file__).unlink()
print("Round 6 release documentation and security gate applied.")
