#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "Mahou/Classes/KMHook.cs"


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
            "KMHook.cs: expected %d occurrence(s), found %d (LF=%d, CRLF=%d) for %r"
            % (expected, count, count_lf, count_crlf, old)
        )
    if count_crlf:
        data = data.replace(old_crlf, new_crlf, expected)
    else:
        data = data.replace(old_lf, new_lf, expected)
    PATH.write_bytes(data)


replace_eol(
    "\t\t\t\tcase \"__delay\":\n"
    "\t\t\t\t\tint d = 0;\n"
    "\t\t\t\t\tif (Int32.TryParse(args, out d))\n"
    "\t\t\t\t\t\tThread.Sleep(d);\n"
    "\t\t\t\t\tbreak;\n",
    "\t\t\t\tcase \"__delay\":\n"
    "\t\t\t\t\tint d = 0;\n"
    "\t\t\t\t\tif (Int32.TryParse(args, out d)) {\n"
    "\t\t\t\t\t\td = Math.Max(0, Math.Min(d, MaxSnippetDelayMs));\n"
    "\t\t\t\t\t\tThread.Sleep(d);\n"
    "\t\t\t\t\t}\n"
    "\t\t\t\t\tbreak;\n",
)
replace_eol(
    "\t\t\t\t\tif (args.Contains(\"|\")) {\n"
    "\t\t\t\t\t\tvar A = args.Split('|');\n"
    "\t\t\t\t\t\tt=A[0];\n"
    "\t\t\t\t\t\tInt32.TryParse(A[1], out upc);\n"
    "\t\t\t\t\t\tif (A[1] == \"*\")\n"
    "\t\t\t\t\t\t\tupc = t.Length;\n"
    "\t\t\t\t\t}\n"
    "\t\t\t\t\tvar subst = 0;\n"
    "\t\t\t\t\tvar res = \"\";\n"
    "\t\t\t\t\tfor (int i=0; i!=upc; i++) {\n",
    "\t\t\t\t\tif (args.Contains(\"|\")) {\n"
    "\t\t\t\t\t\tvar A = args.Split('|');\n"
    "\t\t\t\t\t\tt=A[0];\n"
    "\t\t\t\t\t\tint parsedUppercaseCount;\n"
    "\t\t\t\t\t\tif (A[1] == \"*\")\n"
    "\t\t\t\t\t\t\tupc = t.Length;\n"
    "\t\t\t\t\t\telse if (Int32.TryParse(A[1], out parsedUppercaseCount))\n"
    "\t\t\t\t\t\t\tupc = parsedUppercaseCount;\n"
    "\t\t\t\t\t}\n"
    "\t\t\t\t\tupc = Math.Max(0, Math.Min(upc, Math.Min(t.Length, MaxUppercaseCharacters)));\n"
    "\t\t\t\t\tvar subst = 0;\n"
    "\t\t\t\t\tvar res = \"\";\n"
    "\t\t\t\t\tfor (int i=0; i<upc; i++) {\n",
)
replace_eol(
    "\t\tpublic static List<Keys> strparsekey(string key, int times = 1) {\n"
    "\t\t\tkey = key.ToLower().Replace(\"capslock\", \"capital\");\n",
    "\t\tconst int MaxSnippetDelayMs = 5000;\n"
    "\t\tconst int MaxKeyboardStepDelayMs = 1000;\n"
    "\t\tconst int MaxSnippetKeyRepeat = 1000;\n"
    "\t\tconst int MaxUppercaseCharacters = 10000;\n"
    "\t\tpublic static List<Keys> strparsekey(string key, int times = 1) {\n"
    "\t\t\ttimes = Math.Max(0, Math.Min(times, MaxSnippetKeyRepeat));\n"
    "\t\t\tkey = key.ToLower().Replace(\"capslock\", \"capital\");\n",
)
replace_eol(
    "\t\t\t\t\tfor (int x = 0; x != times; x++) {\n",
    "\t\t\t\t\tfor (int x = 0; x < times; x++) {\n",
    expected=5,
)
replace_eol(
    "\t\t\t\tInt32.TryParse(axy[1], out delay);\n"
    "\t\t\t\tDebug.WriteLine(\"SimKeyboard set delay:\"+delay);\n",
    "\t\t\t\tint parsedDelay;\n"
    "\t\t\t\tif (Int32.TryParse(axy[1], out parsedDelay))\n"
    "\t\t\t\t\tdelay = Math.Max(0, Math.Min(parsedDelay, MaxKeyboardStepDelayMs));\n"
    "\t\t\t\tDebug.WriteLine(\"SimKeyboard set delay:\"+delay);\n",
)
replace_eol(
    "\t\t\t\t\tInt32.TryParse(rma[0].Groups[2].Value, out times);\n",
    "\t\t\t\t\tint parsedTimes;\n"
    "\t\t\t\t\tif (Int32.TryParse(rma[0].Groups[2].Value, out parsedTimes))\n"
    "\t\t\t\t\t\ttimes = Math.Max(0, Math.Min(parsedTimes, MaxSnippetKeyRepeat));\n",
)
replace_eol(
    "\t\tpublic static void DoLater(Action act, int timeout) {\n"
    "\t\t\tSystem.Threading.Tasks.Task.Factory.StartNew(() => {\n"
    "\t\t\t                                             \tThread.Sleep(timeout);\n",
    "\t\tpublic static void DoLater(Action act, int timeout) {\n"
    "\t\t\ttimeout = Math.Max(0, Math.Min(timeout, 600000));\n"
    "\t\t\tSystem.Threading.Tasks.Task.Factory.StartNew(() => {\n"
    "\t\t\t                                             \tThread.Sleep(timeout);\n",
)

Path(__file__).unlink()
print("Bounded snippet delays, key repetitions, uppercase counts and deferred timeouts.")
