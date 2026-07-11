#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
UI = ROOT / "Mahou/MahouUI.cs"
DISPLAY = ROOT / "Mahou/LangDisplay.cs"


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
    "\t\tpublic TrayIcon icon;\n",
    "\t\tpublic TrayIcon icon;\n"
    "\t\tIcon generatedTrayIcon;\n",
)
replace_eol(
    UI,
    "\t\tpublic static void IfDispose(ref Bitmap disposable) {\n"
    "\t\t\tif (disposable != null) disposable.Dispose();\n"
    "\t\t\tdisposable = null;\n"
    "\t\t}\n",
    "\t\tpublic static void IfDispose(ref Bitmap disposable) {\n"
    "\t\t\tif (disposable != null) disposable.Dispose();\n"
    "\t\t\tdisposable = null;\n"
    "\t\t}\n"
    "\t\tpublic void SetTrayIconFromBitmap(Bitmap source) {\n"
    "\t\t\tif (source == null || icon == null || icon.trIcon == null) return;\n"
    "\t\t\tIntPtr nativeHandle = IntPtr.Zero;\n"
    "\t\t\tIcon next = null;\n"
    "\t\t\ttry {\n"
    "\t\t\t\tnativeHandle = source.GetHicon();\n"
    "\t\t\t\tusing (var borrowed = Icon.FromHandle(nativeHandle))\n"
    "\t\t\t\t\tnext = (Icon)borrowed.Clone();\n"
    "\t\t\t\ticon.trIcon.Icon = next;\n"
    "\t\t\t\tvar previous = generatedTrayIcon;\n"
    "\t\t\t\tgeneratedTrayIcon = next;\n"
    "\t\t\t\tnext = null;\n"
    "\t\t\t\tif (previous != null) previous.Dispose();\n"
    "\t\t\t} finally {\n"
    "\t\t\t\tif (next != null) next.Dispose();\n"
    "\t\t\t\tif (nativeHandle != IntPtr.Zero) WinAPI.DestroyIcon(nativeHandle);\n"
    "\t\t\t}\n"
    "\t\t}\n"
    "\t\tvoid SetStaticTrayIcon(Icon source) {\n"
    "\t\t\tif (icon == null || icon.trIcon == null) return;\n"
    "\t\t\ticon.trIcon.Icon = source;\n"
    "\t\t\tvar previous = generatedTrayIcon;\n"
    "\t\t\tgeneratedTrayIcon = null;\n"
    "\t\t\tif (previous != null) previous.Dispose();\n"
    "\t\t}\n",
)
replace_eol(
    DISPLAY,
    "\t\t\t\t\tvar fi = Icon.FromHandle((\n"
    "\t\t\t\t\t\t\t(MahouUI.TrayText && MahouUI.ITEXT != null) ? MahouUI.ITEXT : MahouUI.FLAG)\n"
    "\t\t\t\t\t\t.GetHicon());\n"
    "\t\t\t\t\tMMain.mahou.icon.trIcon.Icon = fi;\n"
    "\t\t\t\t\tWinAPI.DestroyIcon(fi.Handle);\n",
    "\t\t\t\t\tvar trayBitmap = (MahouUI.TrayText && MahouUI.ITEXT != null) ? MahouUI.ITEXT : MahouUI.FLAG;\n"
    "\t\t\t\t\tMMain.mahou.SetTrayIconFromBitmap(trayBitmap);\n",
)
replace_eol(
    UI,
    "\t\t\t\t\tif (File.Exists(flagpth)) {\n"
    "\t\t\t\t\t\tFLAG = new Bitmap(Image.FromFile(flagpth));\n"
    "\t\t\t\t\t}\n",
    "\t\t\t\t\tif (File.Exists(flagpth)) {\n"
    "\t\t\t\t\t\tusing (var loadedFlag = Image.FromFile(flagpth))\n"
    "\t\t\t\t\t\t\tFLAG = new Bitmap(loadedFlag);\n"
    "\t\t\t\t\t}\n",
)
replace_eol(
    UI,
    "\t\t\t\t\tBitmap b = null;\n"
    "\t\t\t\t\tif (FLAG != null) b = new Bitmap(FLAG);\n"
    "\t\t\t\t\tif (TrayText && ITEXT != null) b = new Bitmap(ITEXT);\n"
    "\t\t\t\t\tIcon flagicon;\n"
    "\t\t\t\t\tif (b != null)\n"
    "\t\t\t\t\t\tflagicon = Icon.FromHandle(b.GetHicon());\n"
    "\t\t\t\t\telse \n"
    "\t\t\t\t\t\tflagicon = Mahou.Properties.Resources.MahouTrayHD;\n"
    "\t\t\t\t\ticon.trIcon.Icon = flagicon;\n"
    "\t\t\t\t\tWinAPI.DestroyIcon(flagicon.Handle);\n"
    "\t\t\t\t\tlastTrayFlagLayout = lcid;\n",
    "\t\t\t\t\tBitmap b = null;\n"
    "\t\t\t\t\ttry {\n"
    "\t\t\t\t\t\tif (FLAG != null) b = new Bitmap(FLAG);\n"
    "\t\t\t\t\t\tif (TrayText && ITEXT != null) {\n"
    "\t\t\t\t\t\t\tif (b != null) b.Dispose();\n"
    "\t\t\t\t\t\t\tb = new Bitmap(ITEXT);\n"
    "\t\t\t\t\t\t}\n"
    "\t\t\t\t\t\tif (b != null) SetTrayIconFromBitmap(b);\n"
    "\t\t\t\t\t\telse SetStaticTrayIcon(Mahou.Properties.Resources.MahouTrayHD);\n"
    "\t\t\t\t\t} finally {\n"
    "\t\t\t\t\t\tif (b != null) b.Dispose();\n"
    "\t\t\t\t\t}\n"
    "\t\t\t\t\tlastTrayFlagLayout = lcid;\n",
)

Path(__file__).unlink()
print("Tray icon ownership hardened: native handles, cloned icons, prior icons and loaded images are released.")
