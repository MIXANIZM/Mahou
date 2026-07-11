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

path = ROOT / 'Mahou/Classes/KMHook.cs'
data = path.read_bytes()
case_lf = b'\t\t\t\tcase "__execute":\n\t\t\t\t\tExecute(args);\n\t\t\t\t\tbreak;\n'
case_crlf = case_lf.replace(b'\n', b'\r\n')
case_count = data.count(case_lf) + data.count(case_crlf)
if case_count != 1:
    raise RuntimeError('KMHook.cs: expected one __execute case, found %d' % case_count)
data = data.replace(case_crlf, b'', 1).replace(case_lf, b'', 1)
start_marker = b'\t\tstatic void Execute(string args) {'
end_marker = b'\t\tpublic static List<Keys> strparsekey(string key, int times = 1) {'
start = data.find(start_marker)
end = data.find(end_marker, start)
if start < 0 or end < 0:
    raise RuntimeError('Execute method markers not found')
path.write_bytes(data[:start] + data[end:])

config_path = ROOT / 'Mahou/Classes/Configs.cs'
config = config_path.read_bytes()
config_lf = b'\t\t\tCheckBool("Hidden", "AllowSnippetExecute", "false");\n'
config_crlf = config_lf.replace(b'\n', b'\r\n')
config_count = config.count(config_lf) + config.count(config_crlf)
if config_count != 1:
    raise RuntimeError('Configs.cs: expected one AllowSnippetExecute default, found %d' % config_count)
config_path.write_bytes(config.replace(config_crlf, b'', 1).replace(config_lf, b'', 1))

Path(__file__).unlink()
print('Removed snippet __execute command execution path and its hidden configuration flag.')
