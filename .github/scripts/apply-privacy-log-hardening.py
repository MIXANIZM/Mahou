#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def replace(relative_path, old, new, expected=1):
    path = ROOT / relative_path
    data = path.read_text(encoding="utf-8-sig")
    count = data.count(old)
    if count != expected:
        raise RuntimeError("%s: expected %d occurrence(s), found %d for %r" % (relative_path, expected, count, old))
    path.write_text(data.replace(old, new, expected), encoding="utf-8-sig", newline="")


p = "Mahou/Classes/KMHook.cs"
replacements = [
    ('Logging.Log("[NCR] > Rule: " + NCRule.rule + " for snippets ignored expansion of the snippet: " + snip);', 'Logging.Log("[NCR] > A snippet expansion was ignored by rule " + NCRule.rule + ".");'),
    ('Logging.Log("[NCR] > Rule: " + NCRule.rule + " for autoswitch ignored conversion of the word: " + snip);', 'Logging.Log("[NCR] > An AutoSwitch conversion was ignored by rule " + NCRule.rule + ".");'),
    ('Logging.Log("[AS] > Double-layout autoswitch rule: " +as_wrongs[i] +"<=>" +as_corrects[i]);', 'Logging.Log("[AS] > Double-layout AutoSwitch rule matched.");'),
    ('Logging.Log("[AS] > Leave as it was: "+snil);', 'Logging.Log("[AS] > AutoSwitch left the word unchanged.");'),
    ('Logging.Log("[AS] snl: " +snil + ", l:" +snl + "as_crI: " + as_corrects[i] + ", l: " +asl + "SKIP: " +skipLS);', 'Logging.Log("[AS] > Rule evaluation completed; source layout=" + snl + ", target layout=" + asl + ", skipped=" + skipLS + ".");'),
    ('Logging.Log("[AS] > word ["+snip+"] has no expansion, snippet is not finished or its expansion commented.", 1);', 'Logging.Log("[AS] > A word has no usable expansion or its expansion is commented.", 1);'),
    ('Logging.Log("[AS] > Changed last snippet to AS-ed, "+corr+", instead of ignorecase: "+ snil);', 'Logging.Log("[AS] > Last snippet state was updated after AutoSwitch.");'),
    ('Logging.Log("[SNI] > Current snippet is [" + snip + "].");', 'Logging.Log("[SNI] > Current snippet length: " + (snip == null ? 0 : snip.Length) + ".");'),
    ('Logging.Log("[REEX] > Replaced: "+repl);', 'Logging.Log("[REEX] > Replacement applied; result length=" + (repl == null ? 0 : repl.Length) + ".");'),
    ('Logging.Log("[SNI] > Current snippet [" + snip + "] matched with "+__ANY__+" existing snippet [" + exps[i] + "].");', 'Logging.Log("[SNI] > Current snippet matched an __ANY__ rule.");'),
    ('Logging.Log("[SNI] > Current snippet [" + snip + "] matched existing snippet [" + exps[i] + "].");', 'Logging.Log("[SNI] > Current snippet matched an existing rule.");'),
    ('Logging.Log("[SNI] > Snippet ["+snip+"] has no expansion, snippet is not finished or its expansion commented.", 1);', 'Logging.Log("[SNI] > A snippet has no usable expansion or its expansion is commented.", 1);'),
    ('Logging.Log("[GETSYM] > "+(ignore?"fake;":"true;")+" ToUnEx() => ["+c+"].");', 'Logging.Log("[GETSYM] > Symbol translation completed; ignored=" + ignore + ".");'),
    ('Logging.Log("[DICT] Empty entry, just | : " +line, 2);', 'Logging.Log("[DICT] Empty dictionary entry at line " + i + ".", 2);'),
    ('Logging.Log("[DICT] > Wrong Dictionary, line #"+i+", => " +line);', 'Logging.Log("[DICT] > Invalid dictionary syntax at line #" + i + ".");'),
    ('Logging.Log("[Ul_str] > pre:" + ul + center);', 'Logging.Log("[Ul_str] > Pre-transform length=" + ((ul == null ? 0 : ul.Length) + (center == null ? 0 : center.Length)) + ".");'),
    ('Logging.Log("[Ul_str] > aft:" + ul + center);', 'Logging.Log("[Ul_str] > Post-transform length=" + ((ul == null ? 0 : ul.Length) + (center == null ? 0 : center.Length)) + ".");'),
    ('Logging.Log("[REEX] > regex: /"+regex_raw+"/"+(ignorecase ? "i" : "")+", snip ["+input+"]");', 'Logging.Log("[REEX] > Regex replacement requested; pattern length=" + (regex_raw == null ? 0 : regex_raw.Length) + ", input length=" + (input == null ? 0 : input.Length) + ".");'),
    ('Logging.Log("[SNI] > Expanding snippet [" + snip + "] to [" + expand + "].");', 'Logging.Log("[SNI] > Expanding snippet; trigger length=" + (snip == null ? 0 : snip.Length) + ", expansion length=" + (expand == null ? 0 : expand.Length) + ".");'),
    ('Logging.Log("Layout can\'t be guessed for: ["+snip+"].", 2);', 'Logging.Log("Layout could not be guessed for the current snippet.", 2);'),
    ('Logging.Log("[SNI] > Changing to guess layout [" + guess + "] after snippet ["+ gn + "].");', 'Logging.Log("[SNI] > Changing to guessed layout [" + guess + "] after snippet expansion.");'),
    (r'Logging.Log("[EXPR] > Expression [" + ex +"] missing its end \"\)\", at positon: " + expr_start +" in: [" + expand + "].", 2);'.replace('\\"\\)\\"', '\\")\\"'), 'Logging.Log("[EXPR] > Expression is missing its closing parenthesis at position " + expr_start + "; expression length=" + ex.Length + ", snippet length=" + expand.Length + ".", 2);'),
    ('Logging.Log("[EXPR] > Executing expression: " + ex + " with args: [" + args + "]");', 'Logging.Log("[EXPR] > Executing expression " + ex + "; argument length=" + args.Length + ".");'),
    ('Logging.Log("[EXPR] > Ignored espaced expression: " + ex);', 'Logging.Log("[EXPR] > Ignored escaped expression; expression length=" + ex.Length + ".");'),
    ('Logging.Log("[__setsnip] Set snip to [" + args + "]");', 'Logging.Log("[__setsnip] Updated current snippet; length=" + (args == null ? 0 : args.Length) + ".");'),
    ('Logging.Log("[__setlsnip] Set last snip to [" + args + "]");', 'Logging.Log("[__setlsnip] Updated last snippet; length=" + (args == null ? 0 : args.Length) + ".");'),
    ('Logging.Log("[EXPR] > Executing: executable: ["+fil+"] with args: ["+arg+"].");', 'Logging.Log("[EXPR] > Executing an explicitly enabled external command; argument length=" + (arg == null ? 0 : arg.Length) + ".");'),
    ('Logging.Log("[CS] > Starting conversion of [" + ClipStr + "].");', 'Logging.Log("[CS] > Starting conversion; selected text length=" + ClipStr.Length + ".");'),
    ('Logging.Log("[CS] > Char 1 is [" + s + "] in locale +[" + wasLocale + "].");', 'Logging.Log("[CS] > Source character probe completed for locale [" + wasLocale + "].");'),
    ('Logging.Log("[CS] > Char 2 is [" + sb + "] in locale +[" + nowLocale + "].");', 'Logging.Log("[CS] > Target character probe completed for locale [" + nowLocale + "].");'),
    ('Logging.Log("[CS] > Key of char [" + c + "] = {" + key + "}, upper = +[" + state + "].");', 'Logging.Log("[CS] > Character mapped to key {" + key + "}, upper state [" + state + "].");'),
    ('Logging.Log("[CS] > Conversion of string [" + ClipStr + "] from locale [" + l1 + "] into locale [" + l2 + "] became [" + result + "].");', 'Logging.Log("[CS] > Converted selected text from locale [" + l1 + "] to [" + l2 + "]; input length=" + ClipStr.Length + ", output length=" + result.Length + ".");'),
    ('Logging.Log("[CS] > Making input of [" + result + "] as string");', 'Logging.Log("[CS] > Typing converted selection; length=" + result.Length + ".");'),
    ('Logging.Log("Inputting ["+output+"] as "+tn);', 'Logging.Log("Inputting transformed selection as " + tn + "; length=" + (output == null ? 0 : output.Length) + ".");'),
    ('Logging.Log("[CUSTOM] > Stopping, that one already replaced: " + repl);', 'Logging.Log("[CUSTOM] > Stopping because this replacement was already applied.");'),
    ('Logging.Log("German fix T:" + T +  "/ c: " + c);', 'Logging.Log("German character normalization applied.");'),
    ('Logging.Log("[SNI] Snip rewrite: " + rewr + " => " + new string(c_snip.ToArray()));', 'Logging.Log("[SNI] Snippet rewrite completed; source length=" + (rewr == null ? 0 : rewr.Length) + ", result length=" + c_snip.Count + ".");'),
]

for old, new in replacements:
    replace(p, old, new)

path = ROOT / p
lines = path.read_text(encoding="utf-8-sig").splitlines(True)
markers = [new.split('Logging.Log(', 1)[1].split('"', 2)[1] for _, new in replacements if 'Logging.Log(' in new]
for index, line in enumerate(lines):
    if not any(marker in line for marker in markers):
        continue
    prefix = line[:len(line) - len(line.lstrip(" \t"))]
    width = len(prefix.expandtabs(4))
    lines[index] = "\t" * (width // 4) + " " * (width % 4) + line.lstrip(" \t")
path.write_text("".join(lines), encoding="utf-8-sig", newline="")

Path(__file__).unlink()
print("Privacy-safe logging hardening applied: %d plaintext log sites replaced." % len(replacements))
