#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
UI = ROOT / "Mahou/MahouUI.cs"
TRANSLATE = ROOT / "Mahou/TranslatePanel.cs"


def replace_eol(path, old, new, expected=1):
    data = path.read_bytes()
    old_lf = old.encode("utf-8")
    new_lf = new.encode("utf-8")
    old_crlf = old_lf.replace(b"\n", b"\r\n")
    new_crlf = new_lf.replace(b"\n", b"\r\n")
    count_lf = data.count(old_lf)
    count_crlf = data.count(old_crlf)
    count = count_lf + count_crlf
    if count != expected:
        raise RuntimeError(
            "%s: expected %d occurrence(s), found %d (LF=%d, CRLF=%d) for %r"
            % (path.relative_to(ROOT), expected, count, count_lf, count_crlf, old)
        )
    if count_crlf:
        data = data.replace(old_crlf, new_crlf, expected)
    else:
        data = data.replace(old_lf, new_lf, expected)
    path.write_bytes(data)


replace_eol(
    UI,
    "\t\tstatic NotifyIcon[] Ticons;\n"
    "\t\tstatic Timer ttmr;\n",
    "\t\tstatic NotifyIcon[] Ticons;\n"
    "\t\tstatic Icon[] TOwnedIcons;\n"
    "\t\tstatic Timer ttmr;\n",
)
replace_eol(
    UI,
    "\t\tpublic static void NCS_tray() {\n",
    "\t\tstatic void SetNcsTrayIcon(int index, Bitmap source) {\n"
    "\t\t\tif (source == null || Ticons == null || Ticons[index] == null) return;\n"
    "\t\t\tIntPtr nativeHandle = IntPtr.Zero;\n"
    "\t\t\tIcon next = null;\n"
    "\t\t\ttry {\n"
    "\t\t\t\tnativeHandle = source.GetHicon();\n"
    "\t\t\t\tusing (var borrowed = Icon.FromHandle(nativeHandle))\n"
    "\t\t\t\t\tnext = (Icon)borrowed.Clone();\n"
    "\t\t\t\tTicons[index].Icon = next;\n"
    "\t\t\t\tvar previous = TOwnedIcons[index];\n"
    "\t\t\t\tTOwnedIcons[index] = next;\n"
    "\t\t\t\tnext = null;\n"
    "\t\t\t\tif (previous != null) previous.Dispose();\n"
    "\t\t\t} finally {\n"
    "\t\t\t\tif (next != null) next.Dispose();\n"
    "\t\t\t\tif (nativeHandle != IntPtr.Zero) WinAPI.DestroyIcon(nativeHandle);\n"
    "\t\t\t}\n"
    "\t\t}\n"
    "\t\tpublic static void NCS_tray() {\n",
)
replace_eol(
    UI,
    "\t\t\tif (Ticons == null)\n"
    "\t\t\t\tTicons = new NotifyIcon[3];\n",
    "\t\t\tif (Ticons == null)\n"
    "\t\t\t\tTicons = new NotifyIcon[3];\n"
    "\t\t\tif (TOwnedIcons == null)\n"
    "\t\t\t\tTOwnedIcons = new Icon[3];\n",
)
replace_eol(
    UI,
    "\t\t\t\tif (Tstates[v])\n"
    "\t\t\t\t\tt.Icon = Icon.FromHandle(icons_on[v].GetHicon());\n"
    "\t\t\t\telse\n"
    "\t\t\t\t\tt.Icon = Icon.FromHandle(icons[v].GetHicon());\n"
    "\t\t\t\tTicons[v] = t;\n",
    "\t\t\t\tTicons[v] = t;\n"
    "\t\t\t\tSetNcsTrayIcon(v, Tstates[v] ? icons_on[v] : icons[v]);\n",
)
replace_eol(
    UI,
    "\t\t\t\t\t\tif (tTstates[v])\n"
    "\t\t\t\t\t\t\tt.Icon = Icon.FromHandle(icons_on[v].GetHicon());\n"
    "\t\t\t\t\t\telse\n"
    "\t\t\t\t\t\t\tt.Icon = Icon.FromHandle(icons[v].GetHicon());\n",
    "\t\t\t\t\t\tSetNcsTrayIcon(v, tTstates[v] ? icons_on[v] : icons[v]);\n",
)
replace_eol(
    UI,
    "\t\t\t\t\tif (Ticons[v].Visible) Ticons[v].Visible = false;\n"
    "\t\t\t\t\tTicons[v].Dispose();\n"
    "\t\t\t\t}\n"
    "\t\t\t\tTicons = null;\n",
    "\t\t\t\t\tif (Ticons[v].Visible) Ticons[v].Visible = false;\n"
    "\t\t\t\t\tTicons[v].Icon = null;\n"
    "\t\t\t\t\tTicons[v].Dispose();\n"
    "\t\t\t\t\tif (TOwnedIcons != null && TOwnedIcons[v] != null) {\n"
    "\t\t\t\t\t\tTOwnedIcons[v].Dispose();\n"
    "\t\t\t\t\t\tTOwnedIcons[v] = null;\n"
    "\t\t\t\t\t}\n"
    "\t\t\t\t}\n"
    "\t\t\t\tTicons = null;\n"
    "\t\t\t\tTOwnedIcons = null;\n",
)

replace_eol(
    TRANSLATE,
    "\t\tprotected override void OnPaint(PaintEventArgs e) {\n"
    "//\t\t\tif (MMain.mahou == null) { base.OnPaint(e); return; }\n"
    "\t\t\tGraphics g = CreateGraphics();\n"
    "\t\t\tvar pn = new Pen(Color.Black);\n"
    "\t\t\tif (AeroEnabled && MahouUI.TrBorderAero)\n"
    "\t\t\t\tpn = new Pen(CurrentAeroColor());\n"
    "\t\t\telse\n"
    "\t\t\t\tpn.Color = MahouUI.TrBorder;\n"
    "\t\t\tg.DrawRectangle(pn, new Rectangle(0, 0, Size.Width - 1, Size.Height - 1));\n"
    "\t\t\tg.Dispose();\n"
    "\t\t\tpn.Dispose();\n"
    "\t\t\tbase.OnPaint(e);\n"
    "\t\t}\n",
    "\t\tprotected override void OnPaint(PaintEventArgs e) {\n"
    "\t\t\tvar borderColor = AeroEnabled && MahouUI.TrBorderAero\n"
    "\t\t\t\t? CurrentAeroColor()\n"
    "\t\t\t\t: MahouUI.TrBorder;\n"
    "\t\t\tusing (var pen = new Pen(borderColor))\n"
    "\t\t\t\te.Graphics.DrawRectangle(pen, new Rectangle(0, 0, Size.Width - 1, Size.Height - 1));\n"
    "\t\t\tbase.OnPaint(e);\n"
    "\t\t}\n",
)

replace_eol(
    UI,
    "\t\t\t\t\t\tvar b = GetPathIcon(ffd).ToBitmap(); //Icon.ExtractAssociatedIcon(ffd).ToBitmap();\n"
    "\t\t\t\t\t\timg = Image.FromHbitmap(b.GetHbitmap());\n"
    "\t\t\t\t\t\tb.Dispose();\n",
    "\t\t\t\t\t\tusing (var extractedIcon = GetPathIcon(ffd))\n"
    "\t\t\t\t\t\t\timg = extractedIcon.ToBitmap();\n",
)

# The original directory-icon block mixes tabs and spaces, so replace it by
# unique structural markers instead of matching its indentation byte-for-byte.
ui_data = UI.read_bytes()
start_marker = b'WinAPI.ExtractIconEx("shell32.dll", 3, out large, out small, 1);'
end_marker = b'file_icons_cache["<DIRECTORY>"] = img;'
start = ui_data.find(start_marker)
end = ui_data.find(end_marker, start)
if start < 0 or end < 0:
    raise RuntimeError("MahouUI.cs: directory icon extraction markers not found")
if ui_data.find(start_marker, start + 1) >= 0:
    raise RuntimeError("MahouUI.cs: directory icon extraction start marker is not unique")
line_start = ui_data.rfind(b"\n", 0, start) + 1
indent = ui_data[line_start:start]
eol = b"\r\n" if b"\r\n" in ui_data[start:end] else b"\n"
new_lines = [
    b'WinAPI.ExtractIconEx("shell32.dll", 3, out large, out small, 1);',
    b'try {',
    b'\tvar selectedIcon = large != IntPtr.Zero ? large : small;',
    b'\tif (selectedIcon != IntPtr.Zero)',
    b'\t\tusing (var borrowedIcon = Icon.FromHandle(selectedIcon))',
    b'\t\t\timg = borrowedIcon.ToBitmap();',
    b'}',
    b'catch (Exception ee) {',
    b'\tLogging.Log("Can\'t extract icon..." +ee.Message + ee.StackTrace, 1);',
    b'}',
    b'finally {',
    b'\tif (large != IntPtr.Zero) WinAPI.DestroyIcon(large);',
    b'\tif (small != IntPtr.Zero && small != large) WinAPI.DestroyIcon(small);',
    b'}',
]
new_block = eol.join(indent + line for line in new_lines) + eol
ui_data = ui_data[:line_start] + new_block + ui_data[end:]
UI.write_bytes(ui_data)

Path(__file__).unlink()
print("Second GDI pass applied to lock-state tray icons, translator border and file icon extraction.")
