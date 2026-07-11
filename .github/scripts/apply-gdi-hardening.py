#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def replace_eol(relative, old, new, expected=1):
    path = ROOT / relative
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
            "%s: expected %d occurrence(s), found %d (LF=%d, CRLF=%d)"
            % (relative, expected, count, count_lf, count_crlf)
        )
    if count_crlf:
        data = data.replace(old_crlf, new_crlf, expected)
    else:
        data = data.replace(old_lf, new_lf, expected)
    path.write_bytes(data)


replace_eol(
    "Mahou/LangDisplay.cs",
    "\t\t\t\tBackgroundImage = new Bitmap(MahouUI.FLAG);\n",
    "\t\t\t\tvar previousBackground = BackgroundImage;\n"
    "\t\t\t\tBackgroundImage = new Bitmap(MahouUI.FLAG);\n"
    "\t\t\t\tif (previousBackground != null) previousBackground.Dispose();\n",
)
replace_eol(
    "Mahou/LangDisplay.cs",
    "\t\t\te.Graphics.DrawString(lbLang.Text, lbLang.Font, new SolidBrush(lbLang.ForeColor), 0, 0);\n",
    "\t\t\tusing (var brush = new SolidBrush(lbLang.ForeColor)) {\n"
    "\t\t\t\te.Graphics.DrawString(lbLang.Text, lbLang.Font, brush, 0, 0);\n"
    "\t\t\t}\n",
)

replace_eol(
    "Mahou/LangPanel.cs",
    "\t\t\tpct_Flag.BackgroundImage = new Bitmap(flag);\n",
    "\t\t\tvar previousFlag = pct_Flag.BackgroundImage;\n"
    "\t\t\tpct_Flag.BackgroundImage = new Bitmap(flag);\n"
    "\t\t\tif (previousFlag != null) previousFlag.Dispose();\n",
)
replace_eol(
    "Mahou/LangPanel.cs",
    "\t\tprotected override void OnPaint(PaintEventArgs e) {\n"
    "\t\t\tif (MMain.mahou == null) { base.OnPaint(e); return; }\n"
    "\t\t\tGraphics g = CreateGraphics();\n"
    "\t\t\tvar pn = new Pen(Color.Black);\n"
    "\t\t\tif (AeroEnabled && MahouUI.LangPanelBorderAero)\n"
    "\t\t\t\tpn = new Pen(CurrentAeroColor());\n"
    "\t\t\telse\n"
    "\t\t\t\tpn.Color = MMain.mahou.LangPanelBorderColor;\n"
    "\t\t\tg.DrawRectangle(pn, new Rectangle(0, 0, Size.Width - 1, Size.Height - 1));\n"
    "\t\t\tg.Dispose();\n"
    "\t\t\tpn.Dispose();\n"
    "\t\t\tbase.OnPaint(e);\n"
    "\t\t}\n",
    "\t\tprotected override void OnPaint(PaintEventArgs e) {\n"
    "\t\t\tif (MMain.mahou == null) { base.OnPaint(e); return; }\n"
    "\t\t\tvar borderColor = AeroEnabled && MahouUI.LangPanelBorderAero\n"
    "\t\t\t\t? CurrentAeroColor()\n"
    "\t\t\t\t: MMain.mahou.LangPanelBorderColor;\n"
    "\t\t\tusing (var pen = new Pen(borderColor)) {\n"
    "\t\t\t\te.Graphics.DrawRectangle(pen, new Rectangle(0, 0, Size.Width - 1, Size.Height - 1));\n"
    "\t\t\t}\n"
    "\t\t\tbase.OnPaint(e);\n"
    "\t\t}\n",
)

replace_eol(
    "Mahou/MahouUI.cs",
    "\t\t                if(Enabled)\n"
    "\t\t                    myBuffer.Graphics.FillRectangle(new SolidBrush(BG), r);\n",
    "\t\t                if(Enabled)\n"
    "\t\t                    using (var backgroundBrush = new SolidBrush(BG))\n"
    "\t\t                        myBuffer.Graphics.FillRectangle(backgroundBrush, r);\n",
)
replace_eol(
    "Mahou/MahouUI.cs",
    "\t\t                if(Enabled)\n"
    "\t\t                    myBuffer.Graphics.DrawRectangle(new Pen(Color.FromArgb(255, 133, 158, 191), 1), r);\n",
    "\t\t                if(Enabled)\n"
    "\t\t                    using (var borderPen = new Pen(Color.FromArgb(255, 133, 158, 191), 1))\n"
    "\t\t                        myBuffer.Graphics.DrawRectangle(borderPen, r);\n",
)
replace_eol(
    "Mahou/MahouUI.cs",
    "\t\t\t\t\t\t\tmyBuffer.Graphics.DrawRectangle(new Pen(TAB_BORDERS), i.Bounds.X, i.Bounds.Y, i.Bounds.Width, i.Bounds.Height);\n",
    "\t\t\t\t\t\t\tusing (var tabBorderPen = new Pen(TAB_BORDERS))\n"
    "\t\t\t\t\t\t\t\tmyBuffer.Graphics.DrawRectangle(tabBorderPen, i.Bounds.X, i.Bounds.Y, i.Bounds.Width, i.Bounds.Height);\n",
)
replace_eol(
    "Mahou/MahouUI.cs",
    "\t\t\t\t\t\t\t\tmyBuffer.Graphics.FillRectangle(new SolidBrush(TAB_FOCUS_BG), i.Bounds.X+1, i.Bounds.Y+1, i.Bounds.Width-2, i.Bounds.Height-2);\n",
    "\t\t\t\t\t\t\t\tusing (var focusBrush = new SolidBrush(TAB_FOCUS_BG))\n"
    "\t\t\t\t\t\t\t\t\tmyBuffer.Graphics.FillRectangle(focusBrush, i.Bounds.X+1, i.Bounds.Y+1, i.Bounds.Width-2, i.Bounds.Height-2);\n",
)
replace_eol(
    "Mahou/MahouUI.cs",
    "\t\t\t\t\t\t\tmyBuffer.Graphics.DrawString(t, i.Font, new SolidBrush(FG), xal, yal);\n",
    "\t\t\t\t\t\t\tusing (var textBrush = new SolidBrush(FG))\n"
    "\t\t\t\t\t\t\t\tmyBuffer.Graphics.DrawString(t, i.Font, textBrush, xal, yal);\n",
)

Path(__file__).unlink()
print("GDI hardening applied: disposable brushes, pens and replaced bitmaps are now released.")
