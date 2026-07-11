#!/usr/bin/env python3
from pathlib import Path
import re

root = Path(__file__).resolve().parents[2]
ui_path = root / "Mahou" / "MahouUI.cs"
configs_path = root / "Mahou" / "Classes" / "Configs.cs"
security_path = root / "Mahou" / "MahouUI.Security.cs"

ui = ui_path.read_text(encoding="utf-8-sig")
configs = configs_path.read_text(encoding="utf-8-sig")
security = security_path.read_text(encoding="utf-8-sig")

variables = ["WriteInputHistory", "TrEnabled", "AutoSwitchEnabled", "LoggingEnabled"]
resolved = []

for variable in variables:
    patterns = [
        re.compile(r"\b" + re.escape(variable) + r"\s*=\s*MMain\.MyConfs\.ReadBool\(\s*\"([^\"]+)\"\s*,\s*\"([^\"]+)\"\s*\)"),
        re.compile(r"\b" + re.escape(variable) + r"\s*=\s*Configs?\.ReadBool\(\s*\"([^\"]+)\"\s*,\s*\"([^\"]+)\"\s*\)"),
    ]
    match = None
    for pattern in patterns:
        match = pattern.search(ui)
        if match:
            break
    if not match:
        raise SystemExit("Could not resolve configuration key for " + variable)
    section, key = match.groups()
    resolved.append((variable, section, key))

    check_pattern = re.compile(
        r'CheckBool\(\s*"' + re.escape(section) + r'"\s*,\s*"' + re.escape(key) +
        r'"\s*,\s*"(?:true|false)"\s*\);'
    )
    replacement = 'CheckBool("%s", "%s", "false");' % (section, key)
    configs, count = check_pattern.subn(replacement, configs, count=1)
    if count != 1:
        raise SystemExit("Could not set opt-in default for %s [%s/%s]" % (variable, section, key))

proxy_marker = '''            ClipBackOnlyText = false;
            MMain.MyConfs.Write("Hidden", "ClipBackOnlyText", "false");
            cbb_AutostartType.SelectedIndex = 0;
'''
proxy_replacement = '''            ClipBackOnlyText = false;
            MMain.MyConfs.Write("Hidden", "ClipBackOnlyText", "false");
            txt_ProxyPassword.UseSystemPasswordChar = true;
            HelpMeUnderstand.SetToolTip(txt_ProxyPassword,
                "Stored for the current Windows user with DPAPI; hidden on screen.");
            cbb_AutostartType.SelectedIndex = 0;
'''
if security.count(proxy_marker) != 1:
    raise SystemExit("Proxy password UI insertion marker not found exactly once")
security = security.replace(proxy_marker, proxy_replacement, 1)

report = [
    "# Privacy defaults",
    "",
    "The following features are explicit opt-in on a clean MIXANIZM Mahou configuration:",
    "",
]
for variable, section, key in resolved:
    report.append("- `%s` → `[%s] %s=false`" % (variable, section, key))
report.extend([
    "",
    "Existing explicit user choices are preserved; these values are only used when a key is absent.",
    "Proxy passwords are protected with Windows DPAPI and masked in the settings interface.",
    "",
])

configs_path.write_text(configs, encoding="utf-8-sig", newline="\r\n")
security_path.write_text(security, encoding="utf-8-sig", newline="\r\n")
(root / "PRIVACY-DEFAULTS.md").write_text("\n".join(report), encoding="utf-8", newline="\n")
Path(__file__).unlink()
print("Round 8 explicit privacy defaults applied: " + ", ".join(v for v, _, _ in resolved))
