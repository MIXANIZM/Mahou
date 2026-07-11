#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
UI = ROOT / "Mahou/MahouUI.cs"
TR = ROOT / "Mahou/TranslatePanel.cs"


def replace(path, old, new, expected=1):
    data = path.read_text(encoding="utf-8-sig")
    count = data.count(old)
    if count != expected:
        raise RuntimeError(f"{path.relative_to(ROOT)}: expected {expected}, found {count}: {old!r}")
    path.write_text(data.replace(old, new, expected), encoding="utf-8")

replace(UI,
'''\t\tpublic static void DPISCALE(Control cxx, bool nox = false) {\n\t\t\tfloat dx, dy;\n\t\t\tGraphics g = cxx.CreateGraphics();\n\t\t\ttry { dx = g.DpiX; dy = g.DpiY; }\n\t\t\tfinally { g.Dispose(); }\n''',
'''\t\tpublic static void DPISCALE(Control cxx, bool nox = false) {\n\t\t\tfloat dx, dy;\n\t\t\tusing (var g = cxx.CreateGraphics()) {\n\t\t\t\tdx = g.DpiX;\n\t\t\t\tdy = g.DpiY;\n\t\t\t}\n''')

replace(UI,
'''\t\tpublic static void chrome_window_alt_fix() {\n\t\t\tvar last = Locales.ActiveWindow();\n\t\t\tvar f = new Form();\n\t\t\tf.FormBorderStyle = FormBorderStyle.None;\n\t\t\tf.MaximizeBox = f.MinimizeBox = false;\n\t\t\tf.TopMost = true;\n\t\t\tf.Width = f.Height = 1;\n\t\t\tf.Location = new Point(0, 0);\n\t\t\tf.Show();\n\t\t\tWinAPI.SetForegroundWindow(f.Handle);\n\t\t\tKInputs.MakeInput(new [] {\n\t\t                  \tKInputs.AddKey(Keys.LMenu, false),\n\t\t                  \tKInputs.AddKey(Keys.RMenu, false)});\n\t\t\tSystem.Threading.Thread.Sleep(1);\n\t\t\tf.Dispose();\n\t\t\tWinAPI.SetForegroundWindow(last);\n\t\t}\n''',
'''\t\tpublic static void chrome_window_alt_fix() {\n\t\t\tvar last = Locales.ActiveWindow();\n\t\t\ttry {\n\t\t\t\tusing (var f = new Form()) {\n\t\t\t\t\tf.FormBorderStyle = FormBorderStyle.None;\n\t\t\t\t\tf.MaximizeBox = f.MinimizeBox = false;\n\t\t\t\t\tf.TopMost = true;\n\t\t\t\t\tf.Width = f.Height = 1;\n\t\t\t\t\tf.Location = new Point(0, 0);\n\t\t\t\t\tf.Show();\n\t\t\t\t\tWinAPI.SetForegroundWindow(f.Handle);\n\t\t\t\t\tKInputs.MakeInput(new [] {\n\t\t\t\t\t\tKInputs.AddKey(Keys.LMenu, false),\n\t\t\t\t\t\tKInputs.AddKey(Keys.RMenu, false)});\n\t\t\t\t\tSystem.Threading.Thread.Sleep(1);\n\t\t\t\t}\n\t\t\t} finally {\n\t\t\t\tWinAPI.SetForegroundWindow(last);\n\t\t\t}\n\t\t}\n''')

replace(TR,
'''\t\t\t\tvar g = CreateGraphics();\n\t\t\t\tvar size = g.MeasureString(slt.Text, slt.Font);\n\t\t\t\tslt.Width = (int)size.Width;\n''',
'''\t\t\t\tSizeF size;\n\t\t\t\tusing (var g = CreateGraphics())\n\t\t\t\t\tsize = g.MeasureString(slt.Text, slt.Font);\n\t\t\t\tslt.Width = (int)size.Width;\n''', 1)
replace(TR,
'''\t\t\t\t\t\tvar si = g.MeasureString(txttrc.Text, txttrc.Font);\n''',
'''\t\t\t\t\t\tSizeF si;\n\t\t\t\t\t\tusing (var g = CreateGraphics())\n\t\t\t\t\t\t\tsi = g.MeasureString(txttrc.Text, txttrc.Font);\n''')
replace(TR, '''\t\t\t\tg.Dispose();\n\t\t\t\tbtn.Location = new Point(pan.Width-14-1, 1);\n''', '''\t\t\t\tbtn.Location = new Point(pan.Width-14-1, 1);\n''', 1)

Path(__file__).unlink()
print("Hardened UI Graphics/Form lifetime without changing measurement behavior.")
