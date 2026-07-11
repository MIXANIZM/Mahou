#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def replace_text(relative, old, new, expected=1):
    path = ROOT / relative
    data = path.read_text(encoding='utf-8-sig')
    count = data.count(old)
    if count != expected:
        raise RuntimeError('%s: expected %d occurrence(s), found %d' % (relative, expected, count))
    path.write_text(data.replace(old, new, expected), encoding='utf-8-sig', newline='')


replace_text(
    'Mahou/Classes/KMHook.cs',
    '"__keyboard", "__execute", "__cursorhere"',
    '"__keyboard", "__cursorhere"',
)
replace_text(
    'Mahou/Classes/KMHook.cs',
    '\t\t\t\tcase "__execute":\n\t\t\t\t\tExecute(args);\n\t\t\t\t\tbreak;\n',
    '',
)
path = ROOT / 'Mahou/Classes/KMHook.cs'
data = path.read_text(encoding='utf-8-sig')
start_marker = '\t\tstatic void Execute(string args) {'
end_marker = '\t\tpublic static List<Keys> strparsekey(string key, int times = 1) {'
start = data.find(start_marker)
end = data.find(end_marker, start)
if start < 0 or end < 0:
    raise RuntimeError('Execute method markers not found')
path.write_text(data[:start] + data[end:], encoding='utf-8-sig', newline='')
replace_text(
    'Mahou/Classes/Configs.cs',
    '\t\t\tCheckBool("Hidden", "AllowSnippetExecute", "false");\n',
    '',
)
Path(__file__).unlink()
print('Removed snippet __execute command execution path and its hidden configuration flag.')
