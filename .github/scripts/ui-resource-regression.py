#!/usr/bin/env python3
"""Fail CI if recently fixed UI resource lifetime bugs return."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]


def text(relative):
    return (ROOT / relative).read_text(encoding="utf-8-sig")


ui = text("Mahou/MahouUI.cs")
translate = text("Mahou/TranslatePanel.cs")
errors = []

required = {
    "Mahou/MahouUI.cs": [
        "using (var g = cxx.CreateGraphics())",
        "using (var f = new Form())",
        "} finally {\n\t\t\t\tWinAPI.SetForegroundWindow(last);",
    ],
}

for path, markers in required.items():
    source = ui
    for marker in markers:
        if marker not in source:
            errors.append("required UI resource marker missing in %s: %s" % (path, marker))

if translate.count("using (var g = CreateGraphics())") < 2:
    errors.append("TranslatePanel must dispose both temporary Graphics instances with using")
if "size = g.MeasureString(slt.Text, slt.Font);" not in translate:
    errors.append("source-language measurement is missing from TranslatePanel")
if "si = g.MeasureString(txttrc.Text, txttrc.Font);" not in translate:
    errors.append("transcription measurement is missing from TranslatePanel")

forbidden = {
    "Mahou/MahouUI.cs": [
        "Graphics g = cxx.CreateGraphics();",
        "var f = new Form();\n\t\t\tf.FormBorderStyle",
        "f.Dispose();\n\t\t\tWinAPI.SetForegroundWindow(last);",
    ],
    "Mahou/TranslatePanel.cs": [
        "var g = CreateGraphics();\n\t\t\t\tvar size = g.MeasureString",
        "var si = g.MeasureString(txttrc.Text, txttrc.Font);",
        "g.Dispose();\n\t\t\t\tbtn.Location",
    ],
}

for path, markers in forbidden.items():
    source = ui if path.endswith("MahouUI.cs") else translate
    for marker in markers:
        if marker in source:
            errors.append("unsafe UI resource lifetime pattern returned in %s: %s" % (path, marker))

if errors:
    print("UI RESOURCE REGRESSION CHECK FAILED")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("UI resource regression check passed.")
