#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def replace_bytes(relative, old, new, expected=1):
    path = ROOT / relative
    data = path.read_bytes()
    old_b = old.encode('utf-8')
    new_b = new.encode('utf-8')
    count = data.count(old_b)
    if count != expected:
        raise RuntimeError('%s: expected %d occurrence(s), found %d' % (relative, expected, count))
    path.write_bytes(data.replace(old_b, new_b, expected))


replace_bytes(
    'Mahou/Classes/KMHook.cs',
    '"__keyboard", "__execute", "__cursorhere"',
    '"__keyboard", "__cursorhere"',
)
replace_bytes(
    'Mahou/Classes/KMHook.cs',
    '\t\t\t\tcase "__execute":\r\n\t\t\t\t\tExecute(args);\r\n\t\t\t\t\tbreak;\r\n',
    '',
)
start_marker = b'\t\tstatic void Execute(string args) {'
end_marker = b'\t\tpublic static List<Keys> strparsekey(string key, int times = 1) {'
path = ROOT / 'Mahou/Classes/KMHook.cs'
data = path.read_bytes()
start = data.find(start_marker)
end = data.find(end_marker, start)
if start < 0 or end < 0:
    raise RuntimeError('Execute method markers not found')
path.write_bytes(data[:start] + data[end:])
replace_bytes(
    'Mahou/Classes/Configs.cs',
    '\t\t\tCheckBool("Hidden", "AllowSnippetExecute", "false");\r\n',
    '',
)
Path(__file__).unlink()
print('Removed snippet __execute command execution path and its hidden configuration flag.')
