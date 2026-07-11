#!/usr/bin/env python3
from pathlib import Path
import re

root = Path(__file__).resolve().parents[2]
ui_path = root / "Mahou" / "MahouUI.cs"
configs_path = root / "Mahou" / "Classes" / "Configs.cs"
security_path = root / "Mahou" / "MahouUI.Security.cs"
queued_script = root / ".github" / "scripts" / "apply-round8.py"

ui = ui_path.read_text(encoding="utf-8-sig")
configs = configs_path.read_text(encoding="utf-8-sig")
security = security_path.read_text(encoding="utf-8-sig")

fallbacks = {
    "WriteInputHistory": ("Functions", "WriteInputHistory"),
    "TrEnabled": ("TranslatePanel", "Enabled"),
    "AutoSwitchEnabled": ("AutoSwitch", "Enabled"),
    "LoggingEnabled": ("Functions", "Logging"),
}
resolved = []
unresolved = []

for variable, fallback in fallbacks.items():
    match = re.search(
        r"\b" + re.escape(variable) +
        r"\s*=\s*(?:MMain\.MyConfs|Configs?|MMain\.MyConfs)\.ReadBool\(\s*\"([^\"]+)\"\s*,\s*\"([^\"]+)\"\s*\)",
        ui,
    )
    section, key = match.groups() if match else fallback
    check_pattern = re.compile(
        r'CheckBool\(\s*"' + re.escape(section) + r'"\s*,\s*"' + re.escape(key) +
        r'"\s*,\s*"(?:true|false)"\s*\);'
    )
    replacement = 'CheckBool("%s", "%s", "false");' % (section, key)
    configs, count = check_pattern.subn(replacement, configs, count=1)
    if count == 1:
        resolved.append((variable, section, key))
    else:
        # Last chance: discover an unambiguous CheckBool entry by key only.
        broad = re.compile(
            r'CheckBool\(\s*"([^\"]+)"\s*,\s*"' + re.escape(key) +
            r'"\s*,\s*"(?:true|false)"\s*\);'
        )
        matches = list(broad.finditer(configs))
        if len(matches) == 1:
            actual_section = matches[0].group(1)
            configs = broad.sub('CheckBool("%s", "%s", "false");' % (actual_section, key), configs, count=1)
            resolved.append((variable, actual_section, key))
        else:
            unresolved.append((variable, section, key))

proxy_marker = '''            ClipBackOnlyText = false;
            MMain.MyConfs.Write("Hidden", "ClipBackOnlyText", "false");
            cbb_AutostartType.SelectedIndex = 0;
'''
if proxy_marker in security:
    security = security.replace(
        proxy_marker,
        '''            ClipBackOnlyText = false;
            MMain.MyConfs.Write("Hidden", "ClipBackOnlyText", "false");
            txt_ProxyPassword.UseSystemPasswordChar = true;
            HelpMeUnderstand.SetToolTip(txt_ProxyPassword,
                "Stored for the current Windows user with DPAPI; hidden on screen.");
            cbb_AutostartType.SelectedIndex = 0;
''',
        1,
    )
elif "txt_ProxyPassword.UseSystemPasswordChar = true;" not in security:
    unresolved.append(("ProxyPasswordMask", "UI", "txt_ProxyPassword"))

report = [
    "# Privacy defaults",
    "",
    "Confirmed explicit opt-in defaults:",
    "",
]
for variable, section, key in resolved:
    report.append("- `%s` → `[%s] %s=false`" % (variable, section, key))
if unresolved:
    report.extend(["", "Not changed automatically; requires manual audit:", ""])
    for variable, section, key in unresolved:
        report.append("- `%s` (candidate `[%s] %s`)" % (variable, section, key))
report.extend([
    "",
    "Existing explicit user choices are preserved; defaults apply only when a key is absent.",
    "Proxy passwords are protected with Windows DPAPI and masked when the confirmed UI control is available.",
    "",
])

configs_path.write_text(configs, encoding="utf-8-sig", newline="\r\n")
security_path.write_text(security, encoding="utf-8-sig", newline="\r\n")
(root / "PRIVACY-DEFAULTS.md").write_text("\n".join(report), encoding="utf-8", newline="\n")
if queued_script.exists():
    queued_script.unlink()
Path(__file__).unlink()
print("Privacy defaults confirmed: %d; deferred for audit: %d" % (len(resolved), len(unresolved)))
