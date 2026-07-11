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
    "\t\tpublic static bool running, useGS = true, useNA = false;\n"
    "\t\tpublic static readonly WebClient client = new WebClient();\n",
    "\t\tpublic static bool running, useGS = true, useNA = false;\n"
    "\t\tconst int TranslationTimeoutMs = 8000;\n"
    "\t\tconst int MaxTranslationInputCharacters = 5000;\n"
    "\t\tsealed class TimeoutWebClient : WebClient {\n"
    "\t\t\treadonly int timeoutMs;\n"
    "\t\t\tpublic TimeoutWebClient(int timeoutMs) {\n"
    "\t\t\t\tthis.timeoutMs = timeoutMs;\n"
    "\t\t\t}\n"
    "\t\t\tprotected override WebRequest GetWebRequest(Uri address) {\n"
    "\t\t\t\tvar request = base.GetWebRequest(address);\n"
    "\t\t\t\trequest.Timeout = timeoutMs;\n"
    "\t\t\t\tvar http = request as HttpWebRequest;\n"
    "\t\t\t\tif (http != null) {\n"
    "\t\t\t\t\thttp.ReadWriteTimeout = timeoutMs;\n"
    "\t\t\t\t\thttp.AllowAutoRedirect = true;\n"
    "\t\t\t\t\thttp.MaximumAutomaticRedirections = 3;\n"
    "\t\t\t\t}\n"
    "\t\t\t\treturn request;\n"
    "\t\t\t}\n"
    "\t\t}\n"
    "\t\tstatic TimeoutWebClient CreateTranslationClient() {\n"
    "\t\t\tvar client = new TimeoutWebClient(TranslationTimeoutMs);\n"
    "\t\t\tclient.Headers[HttpRequestHeader.UserAgent] =\n"
    "\t\t\t\t\"AndroidTranslate/5.3.0.RC02.130475354-53000263 5.1 phone TRANSLATE_OPM5_TEST_1\";\n"
    "\t\t\treturn client;\n"
    "\t\t}\n",
)
replace_eol(
    "\t\t\ttry {\n"
    "\t\t\t\tfor (int i=0; i!= tls.Length; i++) {\n",
    "\t\t\ttry {\n"
    "\t\t\t\tusing (var client = CreateTranslationClient()) {\n"
    "\t\t\t\tfor (int i=0; i!= tls.Length; i++) {\n",
)
replace_eol(
    "\t\t\t\t\t// corrects GTLink responce encoding.\n"
    "\t\t\t\t\tclient.Headers[\"User-Agent\"] = \"AndroidTranslate/5.3.0.RC02.130475354-53000263 5.1 phone TRANSLATE_OPM5_TEST_1\";\n",
    "\t\t\t\t\t// Corrects the direct Google Translate response encoding.\n",
)
replace_eol(
    "\t\t\t\t\tDebug.WriteLine(\"url: \" + url);\n"
    "\t\t\t\t\tvar raw_array = Encoding.UTF8.GetString(client.DownloadData(url));\n"
    "\t\t\t\t\tDebug.WriteLine(\"RAW:\" +raw_array);\n",
    "\t\t\t\t\tvar raw_array = Encoding.UTF8.GetString(client.DownloadData(url));\n",
)
replace_eol(
    "\t\t\t\t\tgtrlist.Add(gtresp);\n"
    "\t\t\t\t}\n"
    "\t\t\t} catch(Exception e) { MMain.mahou._TranslatePanel.GTRespError(e.Message/*+e.StackTrace*/); }\n",
    "\t\t\t\t\tgtrlist.Add(gtresp);\n"
    "\t\t\t\t}\n"
    "\t\t\t\t}\n"
    "\t\t\t} catch(Exception e) { MMain.mahou._TranslatePanel.GTRespError(NetworkErrorMessage(e)); }\n",
)
replace_eol(
    "\t\tpublic void ShowTranslation(string str, Point pos) {\n"
    "\t\t\tGTRs.Clear();\n"
    "\t\t\tpan_Translations.Controls.Clear();\n"
    "\t\t\trunning = true;\n",
    "\t\tstatic string NetworkErrorMessage(Exception error) {\n"
    "\t\t\tvar webError = error as WebException;\n"
    "\t\t\tif (webError != null && webError.Status == WebExceptionStatus.Timeout)\n"
    "\t\t\t\treturn \"Translation request timed out after \" + (TranslationTimeoutMs / 1000) + \" seconds.\";\n"
    "\t\t\treturn \"Translation request failed: \" + error.Message;\n"
    "\t\t}\n"
    "\t\tpublic void ShowTranslation(string str, Point pos) {\n"
    "\t\t\tif (running || String.IsNullOrEmpty(str)) return;\n"
    "\t\t\tif (str.Length > MaxTranslationInputCharacters) {\n"
    "\t\t\t\tGTRespError(\"Translation input is limited to \" + MaxTranslationInputCharacters + \" characters.\");\n"
    "\t\t\t\tLocation = pos;\n"
    "\t\t\t\tSpecialShow();\n"
    "\t\t\t\treturn;\n"
    "\t\t\t}\n"
    "\t\t\trunning = true;\n"
    "\t\t\ttry {\n"
    "\t\t\t\tShowTranslationCore(str, pos);\n"
    "\t\t\t} catch (Exception e) {\n"
    "\t\t\t\tGTRespError(NetworkErrorMessage(e));\n"
    "\t\t\t\tLocation = pos;\n"
    "\t\t\t\tSpecialShow();\n"
    "\t\t\t} finally {\n"
    "\t\t\t\trunning = false;\n"
    "\t\t\t}\n"
    "\t\t}\n"
    "\t\tvoid ShowTranslationCore(string str, Point pos) {\n"
    "\t\t\tGTRs.Clear();\n"
    "\t\t\tpan_Translations.Controls.Clear();\n",
)
replace_eol(
    "\t\t\t\tvar multi_resp = \"\";\n"
    "\t\t\t\ttry { multi_resp = Encoding.UTF8.GetString(Encoding.Default.GetBytes(client.DownloadString(TranslatePanel.GSLink+\"?multi=\"+multi)));\n"
    "\t\t\t\t\t} catch(Exception e) { GTRespError(e.Message); }\n"
    "\t\t\t\tDebug.WriteLine(multi);\n"
    "\t\t\t\tDebug.WriteLine(multi_resp);\n",
    "\t\t\t\tvar multi_resp = \"\";\n"
    "\t\t\t\ttry {\n"
    "\t\t\t\t\tusing (var client = CreateTranslationClient())\n"
    "\t\t\t\t\t\tmulti_resp = Encoding.UTF8.GetString(Encoding.Default.GetBytes(\n"
    "\t\t\t\t\t\t\tclient.DownloadString(TranslatePanel.GSLink+\"?multi=\"+multi)));\n"
    "\t\t\t\t} catch(Exception e) {\n"
    "\t\t\t\t\tGTRespError(NetworkErrorMessage(e));\n"
    "\t\t\t\t}\n",
)
replace_eol(
    "\t\t\tLocation = pos;\n"
    "\t\t\trunning = false;\n"
    "\t\t\tSpecialShow();\n",
    "\t\t\tLocation = pos;\n"
    "\t\t\tSpecialShow();\n",
)
replace_eol(
    "\t\t\t\tslt.Location = new Point(1, 0);\n"
    "\t\t\t\tslt.Text = (gtr.auto_detect ? \"\" : gtr.src_lang+\"/\")+gtr.targ_lang+\":\";\n"
    "\t\t\t\tvar g = CreateGraphics();\n"
    "\t\t\t\tvar size = g.MeasureString(slt.Text, slt.Font);\n"
    "\t\t\t\tslt.Width = (int)size.Width;\n"
    "\t\t\t\ttxt.Name = \"TR_TXT\"+gtr.targ_lang;\n",
    "\t\t\t\tslt.Location = new Point(1, 0);\n"
    "\t\t\t\tslt.Text = (gtr.auto_detect ? \"\" : gtr.src_lang+\"/\")+gtr.targ_lang+\":\";\n"
    "\t\t\t\tusing (var g = CreateGraphics()) {\n"
    "\t\t\t\tvar size = g.MeasureString(slt.Text, slt.Font);\n"
    "\t\t\t\tslt.Width = (int)size.Width;\n"
    "\t\t\t\ttxt.Name = \"TR_TXT\"+gtr.targ_lang;\n",
)
replace_eol(
    "\t\t\t\tg.Dispose();\n"
    "\t\t\t\tbtn.Location = new Point(pan.Width-14-1, 1);\n",
    "\t\t\t\t}\n"
    "\t\t\t\tbtn.Location = new Point(pan.Width-14-1, 1);\n",
)
replace_eol(
    "\t\t\t\tvar g = CreateGraphics();\n"
    "\t\t\t\tvar size = g.MeasureString(slt.Text, slt.Font);\n"
    "\t\t\t\tg.Dispose();\n"
    "\t\t\t\tslt.Width = (int)size.Width;\n",
    "\t\t\t\tSizeF size;\n"
    "\t\t\t\tusing (var g = CreateGraphics())\n"
    "\t\t\t\t\tsize = g.MeasureString(slt.Text, slt.Font);\n"
    "\t\t\t\tslt.Width = (int)size.Width;\n",
)
replace_eol(
    "\t\t\tvar g = CreateGraphics();\n"
    "\t\t\tSetAboveTitleWidth();\n",
    "\t\t\tusing (var g = CreateGraphics()) {\n"
    "\t\t\tSetAboveTitleWidth();\n",
)
replace_eol(
    "\t\t\tg.Dispose();\n"
    "\t\t\tpan_Translations.Width = Width-2;\n",
    "\t\t\t}\n"
    "\t\t\tpan_Translations.Width = Width-2;\n",
)
replace_eol(
    "\t\t\tvar g = CreateGraphics();\n"
    "\t\t\tvar size = g.MeasureString(TITLE.Text + \"  \", TITLE.Font);\n"
    "\t\t\tg.Dispose();\n",
    "\t\t\tSizeF size;\n"
    "\t\t\tusing (var g = CreateGraphics())\n"
    "\t\t\t\tsize = g.MeasureString(TITLE.Text + \"  \", TITLE.Font);\n",
)

Path(__file__).unlink()
print("Bounded translator network waits, guarded running state and made measurement graphics exception-safe.")
