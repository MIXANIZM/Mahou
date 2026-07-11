#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "Mahou/TranslatePanel.cs"


def replace_eol(old, new, expected=1):
    data = PATH.read_bytes()
    old_lf = old.encode("utf-8")
    new_lf = new.encode("utf-8")
    old_crlf = old_lf.replace(b"\n", b"\r\n")
    new_crlf = new_lf.replace(b"\n", b"\r\n")
    count_lf = data.count(old_lf)
    count_crlf = data.count(old_crlf)
    count = count_lf + count_crlf
    if count != expected:
        raise RuntimeError(
            "TranslatePanel.cs: expected %d occurrence(s), found %d (LF=%d, CRLF=%d) for %r"
            % (expected, count, count_lf, count_crlf, old)
        )
    if count_crlf:
        data = data.replace(old_crlf, new_crlf, expected)
    else:
        data = data.replace(old_lf, new_lf, expected)
    PATH.write_bytes(data)


replace_eol(
    "\t\t\t\tslt.Text = (gtr.auto_detect ? \"\" : gtr.src_lang+\"/\")+gtr.targ_lang+\":\";\n"
    "\t\t\t\tusing (var g = CreateGraphics()) {\n",
    "\t\t\t\tslt.Text = (gtr.auto_detect ? \"\" : gtr.src_lang+\"/\")+gtr.targ_lang+\":\";\n"
    "\t\t\t\tvar g = CreateGraphics();\n",
)
replace_eol(
    "\t\t\t\t}\n"
    "\t\t\t\tbtn.Location = new Point(pan.Width-14-1, 1);\n",
    "\t\t\t\tg.Dispose();\n"
    "\t\t\t\tbtn.Location = new Point(pan.Width-14-1, 1);\n",
)
replace_eol(
    "\t\t\tusing (var g = CreateGraphics()) {\n"
    "\t\t\tSetAboveTitleWidth();\n",
    "\t\t\tvar g = CreateGraphics();\n"
    "\t\t\tSetAboveTitleWidth();\n",
)
replace_eol(
    "\t\t\t\tc++;\n"
    "\t\t\t}\n"
    "\t\t\t}\n"
    "\t\t\tpan_Translations.Width = Width-2;\n",
    "\t\t\t\tc++;\n"
    "\t\t\t}\n"
    "\t\t\tg.Dispose();\n"
    "\t\t\tpan_Translations.Width = Width-2;\n",
)
replace_eol(
    "\t\t\t    try {\n"
    "\t\t\t\tif (!File.Exists(speech_file))\n"
    "\t    \t\t\tclient.DownloadFile(gtr.speech_url, speech_file);\n"
    "\t\t\t    } catch (Exception x) {\n",
    "\t\t\t    try {\n"
    "\t\t\t\tif (!File.Exists(speech_file)) {\n"
    "\t\t\t\t\tusing (var client = CreateTranslationClient())\n"
    "\t    \t\t\t\tclient.DownloadFile(gtr.speech_url, speech_file);\n"
    "\t\t\t\t}\n"
    "\t\t\t    } catch (Exception x) {\n",
)

Path(__file__).unlink()
print("Corrected translator variable scopes and moved speech download to a bounded client.")
