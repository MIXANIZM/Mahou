from pathlib import Path

BOM = b"\xef\xbb\xbf"


def transform(path_name, transform_fn):
    path = Path(path_name)
    raw = path.read_bytes()
    has_bom = raw.startswith(BOM)
    if has_bom:
        raw = raw[len(BOM):]
    crlf = b"\r\n" in raw
    text = raw.decode("utf-8").replace("\r\n", "\n")
    text = transform_fn(text)
    encoded = (text.replace("\n", "\r\n") if crlf else text).encode("utf-8")
    if has_bom:
        encoded = BOM + encoded
    path.write_bytes(encoded)


def replace_once(text, old, new, label):
    count = text.count(old)
    if count != 1:
        raise SystemExit(f"{label}: expected one anchor, found {count}")
    return text.replace(old, new, 1)


def patch_hook(text):
    helper_anchor = "\t\tstatic bool ActiveProcessIs(string processName) {\n"
    helper = '''\t\tstatic void SwitchLayoutAfterManualConversion(uint targetLayout, string context, IntPtr expectedForeground) {
\t\t\tif (targetLayout == 0) {
\t\t\t\tLogging.Log("Post-conversion layout switch skipped because target layout is 0; context=" + context + ".", 2);
\t\t\t\treturn;
\t\t\t}
\t\t\tcs_layout_last = targetLayout;
\t\t\ttry {
\t\t\t\tvar foreground = WinAPI.GetForegroundWindow();
\t\t\t\tif (expectedForeground != IntPtr.Zero && foreground != expectedForeground) {
\t\t\t\t\tLogging.Log("Post-conversion layout switch cancelled because the foreground window changed; context=" + context + ".", 2);
\t\t\t\t\treturn;
\t\t\t\t}
\t\t\t\tvar activeWindow = Locales.ActiveWindow();
\t\t\t\tif (activeWindow == IntPtr.Zero) {
\t\t\t\t\tLogging.Log("Post-conversion layout switch skipped because there is no active window; context=" + context + ".", 2);
\t\t\t\t\treturn;
\t\t\t\t}
\t\t\t\tChangeToLayout(activeWindow, targetLayout);
\t\t\t\tLogging.Log("Keyboard layout synchronized with converted text; target=" + targetLayout + ", context=" + context + ".");
\t\t\t} catch (Exception e) {
\t\t\t\t// Text replacement has already succeeded. A layout-switch failure must not roll it back or crash Mahou.
\t\t\t\tLogging.Log("Could not synchronize keyboard layout after manual conversion; context=" + context + ", error=" + e.Message, 1);
\t\t\t}
\t\t}
'''
    if "static void SwitchLayoutAfterManualConversion" not in text:
        text = replace_once(text, helper_anchor, helper + helper_anchor, "layout helper")

    text = replace_once(text,
        "\t\tstatic bool TryConvertWordWithoutVisibleSelection() {\n\t\t\t// Layout-switching selection mode intentionally keeps the established compatibility path.\n",
        "\t\tstatic bool TryConvertWordWithoutVisibleSelection() {\n\t\t\tvar conversionForeground = WinAPI.GetForegroundWindow();\n\t\t\t// Layout-switching selection mode intentionally keeps the established compatibility path.\n",
        "direct conversion foreground")
    text = replace_once(text,
        "\t\t\t\tif (SelectionProbe.TryReplaceStandardEditWord(standardWord, replacement)) {\n\t\t\t\t\tcs_layout_last = targetLayout;\n",
        "\t\t\t\tif (SelectionProbe.TryReplaceStandardEditWord(standardWord, replacement)) {\n\t\t\t\t\tSwitchLayoutAfterManualConversion(targetLayout, \"standard-edit-caret-word\", conversionForeground);\n",
        "standard edit synchronization")
    text = replace_once(text,
        "\t\t\t\tif (wordResult == SelectionProbe.DirectWordResult.Replaced) {\n\t\t\t\t\tcs_layout_last = targetLayout;\n",
        "\t\t\t\tif (wordResult == SelectionProbe.DirectWordResult.Replaced) {\n\t\t\t\t\tSwitchLayoutAfterManualConversion(targetLayout, \"word-caret-range\", conversionForeground);\n",
        "Word synchronization")
    text = replace_once(text,
        "\t\tpublic static void ConvertSelection() {\n\t\t\tselectionConversionSucceeded = false;\n\t\t\tDebug.WriteLine(\"Start CS\");\n\t\t\ttry { //Used to catch errors\n",
        "\t\tpublic static void ConvertSelection() {\n\t\t\tselectionConversionSucceeded = false;\n\t\t\tDebug.WriteLine(\"Start CS\");\n\t\t\tvar conversionForeground = WinAPI.GetForegroundWindow();\n\t\t\ttry { //Used to catch errors\n",
        "selection foreground")
    text = replace_once(text,
        "\t\t\t\t\t\tvar result = \"\";\n\t\t\t\t\t\tint items = 0;\n",
        "\t\t\t\t\t\tvar result = \"\";\n\t\t\t\t\t\tint items = 0;\n\t\t\t\t\t\tuint convertedTargetLayout = 0;\n",
        "selection target state")
    text = replace_once(text,
        "\t\t\t\t\t\t\tChangeLayout(true);\n\t\t\t\t\t\t\tvar index = 0;\n",
        "\t\t\t\t\t\t\tChangeLayout(true);\n\t\t\t\t\t\t\tconvertedTargetLayout = nowLocale;\n\t\t\t\t\t\t\tvar index = 0;\n",
        "selection switch target")
    text = replace_once(text,
        "\t\t\t\t\t\t\tresult = ConvertText(ClipStr, l1, l2);\n\t\t\t\t\t\t\tcs_layout_last = l2;\n",
        "\t\t\t\t\t\t\tresult = ConvertText(ClipStr, l1, l2);\n\t\t\t\t\t\t\tconvertedTargetLayout = l2;\n",
        "selection converted target")
    text = replace_once(text,
        "\t\t\t\t\t\t\titems = result.Length;\n\t\t\t\t\t\t}\n\t\t\t\t\t\tReSelect(items, \"N\");\n",
        "\t\t\t\t\t\t\titems = result.Length;\n\t\t\t\t\t\t}\n\t\t\t\t\t\tSwitchLayoutAfterManualConversion(convertedTargetLayout, \"selection\", conversionForeground);\n\t\t\t\t\t\tReSelect(items, \"N\");\n",
        "selection final synchronization")
    return text


def patch_gate(text):
    return replace_once(text,
        '             "TryConvertWordWithoutVisibleSelection",\n             "TryConvertWordAroundCaret",\n',
        '             "TryConvertWordWithoutVisibleSelection",\n             "TryConvertWordAroundCaret",\n             "SwitchLayoutAfterManualConversion",\n             "Keyboard layout synchronized with converted text",\n',
        "security gate markers")


transform("Mahou/Classes/KMHook.cs", patch_hook)
transform(".github/scripts/security-regression.py", patch_gate)
