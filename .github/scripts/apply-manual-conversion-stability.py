#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
HOOK = ROOT / "Mahou/Classes/KMHook.cs"
SECURITY = ROOT / ".github/scripts/security-regression.py"


def replace_bytes(path, old, new, expected=1):
    data = path.read_bytes()
    old_lf = old.encode("utf-8")
    new_lf = new.encode("utf-8")
    old_crlf = old_lf.replace(b"\n", b"\r\n")
    new_crlf = new_lf.replace(b"\n", b"\r\n")
    count = data.count(old_lf) + data.count(old_crlf)
    if count != expected:
        raise RuntimeError(
            f"{path.relative_to(ROOT)}: expected {expected}, found {count}: {old!r}"
        )
    if data.count(old_crlf):
        data = data.replace(old_crlf, new_crlf, expected)
    else:
        data = data.replace(old_lf, new_lf, expected)
    path.write_bytes(data)


replace_bytes(
    HOOK,
    '''\t\tstatic readonly object clipboardBackupSync = new object();\n\t\tstatic bool clipboardBackupPending;\n''',
    '''\t\tstatic readonly object clipboardBackupSync = new object();\n\t\tstatic bool clipboardBackupPending;\n\t\tstatic int manualConversionInProgress;\n\t\tstatic int manualConversionCooldownUntil;\n\t\tconst int ManualConversionCooldownMs = 120;\n\t\tconst int WordManualConversionCooldownMs = 300;\n'''
)

replace_bytes(
    HOOK,
    '''\t\tstatic bool selectionConversionSucceeded;\n\t\tpublic static void ConvertSelectionOrLastWord() {\n\t\t\tvar selectionState = SelectionProbe.GetState();\n\t\t\tif (selectionState == SelectionProbe.State.Sensitive) {\n\t\t\t\tLogging.Log("Insert conversion suppressed in a protected text field.", 2);\n\t\t\t\treturn;\n\t\t\t}\n\t\t\tif (selectionState == SelectionProbe.State.None) {\n\t\t\t\tConvertLast(MMain.c_word);\n\t\t\t\treturn;\n\t\t\t}\n\t\t\tselectionConversionSucceeded = false;\n\t\t\tConvertSelection();\n\t\t\tif (!selectionConversionSucceeded && selectionState != SelectionProbe.State.Selected)\n\t\t\t\tConvertLast(MMain.c_word);\n\t\t}\n''',
    '''\t\tstatic bool selectionConversionSucceeded;\n\t\tstatic bool TryBeginManualConversion() {\n\t\t\tif (Interlocked.CompareExchange(ref manualConversionInProgress, 1, 0) != 0) {\n\t\t\t\tLogging.Log("Manual conversion ignored because another conversion is still running.", 2);\n\t\t\t\treturn false;\n\t\t\t}\n\t\t\tvar now = Environment.TickCount;\n\t\t\tvar blockedUntil = Volatile.Read(ref manualConversionCooldownUntil);\n\t\t\tif (unchecked(now - blockedUntil) < 0) {\n\t\t\t\tInterlocked.Exchange(ref manualConversionInProgress, 0);\n\t\t\t\tLogging.Log("Manual conversion ignored during the short post-conversion cooldown.", 2);\n\t\t\t\treturn false;\n\t\t\t}\n\t\t\treturn true;\n\t\t}\n\t\tstatic void EndManualConversion() {\n\t\t\tvar cooldown = ManualConversionCooldownMs;\n\t\t\ttry {\n\t\t\t\tvar process = Locales.ActiveWindowProcess();\n\t\t\t\tif (process != null && String.Equals(process.ProcessName, "WINWORD", StringComparison.OrdinalIgnoreCase))\n\t\t\t\t\tcooldown = WordManualConversionCooldownMs;\n\t\t\t} catch (Exception e) {\n\t\t\t\tLogging.Log("Could not resolve foreground process for conversion cooldown: " + e.Message, 2);\n\t\t\t}\n\t\t\tVolatile.Write(ref manualConversionCooldownUntil, unchecked(Environment.TickCount + cooldown));\n\t\t\tInterlocked.Exchange(ref manualConversionInProgress, 0);\n\t\t}\n\t\tpublic static void ConvertSelectionOrLastWord() {\n\t\t\tif (!TryBeginManualConversion()) return;\n\t\t\ttry {\n\t\t\t\tvar selectionState = SelectionProbe.GetState();\n\t\t\t\tif (selectionState == SelectionProbe.State.Sensitive) {\n\t\t\t\t\tLogging.Log("Insert conversion suppressed in a protected text field.", 2);\n\t\t\t\t\treturn;\n\t\t\t\t}\n\t\t\t\tif (selectionState == SelectionProbe.State.None) {\n\t\t\t\t\tvar wordSnapshot = MMain.c_word == null ? new List<YuKey>() : new List<YuKey>(MMain.c_word);\n\t\t\t\t\tConvertLast(wordSnapshot);\n\t\t\t\t\treturn;\n\t\t\t\t}\n\t\t\t\tselectionConversionSucceeded = false;\n\t\t\t\tConvertSelection();\n\t\t\t\tif (!selectionConversionSucceeded && selectionState != SelectionProbe.State.Selected) {\n\t\t\t\t\tvar wordSnapshot = MMain.c_word == null ? new List<YuKey>() : new List<YuKey>(MMain.c_word);\n\t\t\t\t\tConvertLast(wordSnapshot);\n\t\t\t\t}\n\t\t\t} finally {\n\t\t\t\tEndManualConversion();\n\t\t\t}\n\t\t}\n'''
)

replace_bytes(
    HOOK,
    '''\t\tpublic static void DoSelf(Action self_action, string caller="unknown") {\n\t\t\tvar pt = ">> DoSelf() ";\n\t\t\tif (self_action == null) { Logging.Log(pt+"null() action: from +" + caller); return; }\n\t\t\tvar mn = "?()"; \n\t\t\tif (self_action.Method != null)\n\t\t\t\t\tmn = self_action.Method.Name;\n\t\t\tmn += "+"+caller;\n\t\t\tif (selfie) {\n\t\t\t\tLogging.Log(pt+"Inside "+busy_on+" called: "+mn);\n\t\t\t\tself_action();\n\t\t\t} else {\n\t\t\t\tDebug.WriteLine(pt+ mn);\n//\t\t\t\tMMain.mahou.Invoke((MethodInvoker)delegate {\n\t\t\t\tif (LLHook._ACTIVE)  { LLHook.UnSet(); } \n\t\t\t\tif (MMain.mahou != null) { MMain.mahou.UnregisterHotkeys(); }\n//\t\t\t});\n\t\t\t\tif (MMain.rif != null)\n\t\t\t\t\tMMain.rif.RegisterRawInputDevices(IntPtr.Zero, WinAPI.RawInputDeviceFlags.Remove);\n\t\t\t\tselfie = true;\n\t\t\t\tbusy_on = mn;\n\t\t\t\tself_action();\n//\t\t\t\tMMain.mahou.Invoke((MethodInvoker)delegate {\n\t\t\t\tif (LLHook._ACTIVE) { LLHook.Set(); }\n\t\t\t\tif (MMain.mahou != null) { MMain.mahou.RegisterHotkeys(); }\n//\t\t\t\t                   });\n\t\t\t\tif (MMain.rif != null)\n\t\t\t\t\tMMain.rif.RegisterRawInputDevices(MMain.rif.Handle);\n\t\t\t\tselfie = false;\n\t\t\t\tDebug.WriteLine(pt+ "end " + mn);\n\t\t\t}\n\t\t}\n''',
    '''\t\tpublic static void DoSelf(Action self_action, string caller="unknown") {\n\t\t\tvar pt = ">> DoSelf() ";\n\t\t\tif (self_action == null) { Logging.Log(pt+"null() action: from +" + caller); return; }\n\t\t\tvar mn = "?()"; \n\t\t\tif (self_action.Method != null)\n\t\t\t\t\tmn = self_action.Method.Name;\n\t\t\tmn += "+"+caller;\n\t\t\tif (selfie) {\n\t\t\t\tLogging.Log(pt+"Inside "+busy_on+" called: "+mn);\n\t\t\t\tself_action();\n\t\t\t\treturn;\n\t\t\t}\n\t\t\tDebug.WriteLine(pt+ mn);\n\t\t\tvar llHookWasActive = LLHook._ACTIVE;\n\t\t\tvar hotkeysWereDisabled = false;\n\t\t\tvar rawInputWasRemoved = false;\n\t\t\ttry {\n\t\t\t\tif (llHookWasActive) LLHook.UnSet();\n\t\t\t\tif (MMain.mahou != null) {\n\t\t\t\t\thotkeysWereDisabled = true;\n\t\t\t\t\tMMain.mahou.UnregisterHotkeys();\n\t\t\t\t}\n\t\t\t\tif (MMain.rif != null) {\n\t\t\t\t\trawInputWasRemoved = true;\n\t\t\t\t\tMMain.rif.RegisterRawInputDevices(IntPtr.Zero, WinAPI.RawInputDeviceFlags.Remove);\n\t\t\t\t}\n\t\t\t\tselfie = true;\n\t\t\t\tbusy_on = mn;\n\t\t\t\tself_action();\n\t\t\t} finally {\n\t\t\t\ttry {\n\t\t\t\t\tif (llHookWasActive) LLHook.Set();\n\t\t\t\t} catch (Exception e) {\n\t\t\t\t\tLogging.Log("Low-level hook restore failed after " + mn + ": " + e.Message, 1);\n\t\t\t\t}\n\t\t\t\ttry {\n\t\t\t\t\tif (hotkeysWereDisabled && MMain.mahou != null) MMain.mahou.RegisterHotkeys();\n\t\t\t\t} catch (Exception e) {\n\t\t\t\t\tLogging.Log("Hotkey restore failed after " + mn + ": " + e.Message, 1);\n\t\t\t\t}\n\t\t\t\ttry {\n\t\t\t\t\tif (rawInputWasRemoved && MMain.rif != null) MMain.rif.RegisterRawInputDevices(MMain.rif.Handle);\n\t\t\t\t} catch (Exception e) {\n\t\t\t\t\tLogging.Log("Raw-input restore failed after " + mn + ": " + e.Message, 1);\n\t\t\t\t}\n\t\t\t\tselfie = false;\n\t\t\t\tbusy_on = "";\n\t\t\t\tDebug.WriteLine(pt+ "end " + mn);\n\t\t\t}\n\t\t}\n'''
)

source = HOOK.read_text(encoding="utf-8-sig")
start = source.index("\t\tpublic static void ConvertLast(List<YuKey> c_, bool line = false) {")
end = source.index("\n\t\tstatic bool SymbolIgnoreRules(char c)", start)
method = source[start:end]
original = method
method = method.replace(
    "\t\tpublic static void ConvertLast(List<YuKey> c_, bool line = false) {\n\t\t\ttry",
    "\t\tpublic static void ConvertLast(List<YuKey> c_, bool line = false) {\n\t\t\tvar sourceWord = c_ == null ? new List<YuKey>() : new List<YuKey>(c_);\n\t\t\ttry",
    1,
)
method = method.replace("c_.Count", "sourceWord.Count")
method = method.replace(
    "YuKey[] YuKeys = line ? c_.ToArray() : LayoutKeyReplace(c_,",
    "YuKey[] YuKeys = line ? sourceWord.ToArray() : LayoutKeyReplace(sourceWord,",
    1,
)
method = method.replace("ConvertLast(c_, line);", "ConvertLast(sourceWord, line);")
if method == original:
    raise RuntimeError("ConvertLast snapshot patch made no changes")
source = source[:start] + method + source[end:]
HOOK.write_text(source, encoding="utf-8-sig", newline="")

replace_bytes(
    SECURITY,
    '''             "System.IO.File.WriteAllText(PATH, DictToRaw(def))"],\n''',
    '''             "System.IO.File.WriteAllText(PATH, DictToRaw(def))",\n             "if (LLHook._ACTIVE) { LLHook.Set(); }",\n             "ConvertLast(MMain.c_word);"],\n'''
)
replace_bytes(
    SECURITY,
    '''             "for (int x = 0; x < times; x++)",\n             "AtomicFile.WriteAllText(PATH, DictToRaw(def));"],\n''',
    '''             "for (int x = 0; x < times; x++)",\n             "AtomicFile.WriteAllText(PATH, DictToRaw(def));",\n             "static int manualConversionInProgress;",\n             "Interlocked.CompareExchange(ref manualConversionInProgress, 1, 0)",\n             "new List<YuKey>(MMain.c_word)",\n             "var llHookWasActive = LLHook._ACTIVE;",\n             "if (llHookWasActive) LLHook.Set();",\n             "Hotkey restore failed after",\n             "Raw-input restore failed after"],\n'''
)

Path(ROOT / "manual-conversion-diagnostics").mkdir(exist_ok=True)
(ROOT / "manual-conversion-diagnostics/result.txt").write_text(
    "Serialized repeated manual conversions, snapshotted the tracked word, and guaranteed hook/hotkey/raw-input restoration through finally.\n",
    encoding="utf-8",
)
print("Applied manual conversion stability hardening.")
