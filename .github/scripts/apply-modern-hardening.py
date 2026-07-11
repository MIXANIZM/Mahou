#!/usr/bin/env python3
from pathlib import Path
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
PARTS = ROOT / ".github" / "hardening-parts"
PATCH = Path("/tmp/modern-hardening.patch")
DIAGNOSTICS = ROOT / "hardening-diagnostics"


def fail(message: str) -> None:
    raise SystemExit(message)


def run(*args: str) -> None:
    subprocess.run(args, cwd=str(ROOT), check=True)


def replace_between(text: str, start_marker: str, end_marker: str, replacement: str) -> str:
    start = text.find(start_marker)
    if start < 0:
        fail("Start marker not found: " + start_marker)
    end = text.find(end_marker, start)
    if end < 0:
        fail("End marker not found: " + end_marker)
    return text[:start] + replacement + text[end:]


def build_sanitized_patch() -> None:
    parts = sorted(PARTS.glob("part-*.patch"))
    if not parts:
        print("No staged hardening patch remains.")
        sys.exit(0)

    text = "".join(part.read_text(encoding="utf-8") for part in parts)
    patterns = [
        re.compile(
            r"@@ -10,7 \+10,8 @@ using System\.Text\.RegularExpressions;\n.*?"
            r"(?=@@ -37,7 \+38,7 @@ namespace Mahou \{)",
            re.S,
        ),
        re.compile(
            r"@@ -3961,7 \+3980,8 @@ namespace Mahou \{\n.*?"
            r"(?=diff --git a/Mahou/Classes/Logging\.cs)",
            re.S,
        ),
        re.compile(
            r"diff --git a/Mahou/Classes/NativeClipboard\.cs .*?"
            r"(?=diff --git a/Mahou/Classes/SecretProtector\.cs)",
            re.S,
        ),
        re.compile(
            r"@@ -4242,207 \+4061,29 @@.*?"
            r"(?=^@@ -4459,56 \+4100,15 @@)",
            re.S | re.M,
        ),
    ]
    for index, pattern in enumerate(patterns, 1):
        text, count = pattern.subn("", text, count=1)
        if count != 1:
            fail("Expected normalized patch group %d was not found exactly once" % index)

    DIAGNOSTICS.mkdir(parents=True, exist_ok=True)
    PATCH.write_text(text, encoding="utf-8")
    (DIAGNOSTICS / "modern-hardening.patch").write_text(text, encoding="utf-8")


def apply_patch() -> None:
    flags = ["--recount", "--ignore-space-change", "--ignore-whitespace"]
    check = subprocess.run(
        ["git", "apply", *flags, "--check", str(PATCH)],
        cwd=str(ROOT),
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
    )
    (DIAGNOSTICS / "apply-check.txt").write_text(check.stdout, encoding="utf-8")
    if check.returncode != 0:
        print(check.stdout)
        fail("Hardening patch validation failed")
    run("git", "apply", *flags, "--whitespace=nowarn", str(PATCH))


def replace_legacy_network_block() -> None:
    path = ROOT / "Mahou" / "MahouUI.cs"
    raw = path.read_text(encoding="utf-8-sig")
    replacement = '''\t\tvoid wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
\t\t\t// Legacy self-update is intentionally disabled.
\t\t}

\t\tstring getASD_RemoteSize(bool InZip = false) {
\t\t\ttry {
\t\t\t\tvar source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AS_dict.txt");
\t\t\t\tif (!File.Exists(source)) return MMain.Lang[Languages.Element.Error];
\t\t\t\tvar length = new FileInfo(source).Length;
\t\t\t\treturn (length / 1024d / 1024d).ToString("0.00") + " MB";
\t\t\t} catch { return MMain.Lang[Languages.Element.Error]; }
\t\t}

\t\tstring getResponce(string url) {
\t\t\tLogging.Log("Blocked legacy network request: " + url, 2);
\t\t\treturn null;
\t\t}

\t\tvoid btn_UpdateAutoSwitchDictionary_Click(object sender, EventArgs e) {
\t\t\tvar ok = RestoreBundledAutoSwitchDictionary();
\t\t\tbtn_UpdateAutoSwitchDictionary.ForeColor = ok ? Color.BlueViolet : Color.OrangeRed;
\t\t\tbtn_UpdateAutoSwitchDictionary.Text = ok ? "Bundled dictionary restored" : "Bundled dictionary missing";
\t\t}

'''
    raw = replace_between(
        raw,
        "\t\tvoid wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {",
        "\t\tstatic Regex rx = new Regex",
        replacement,
    )
    path.write_text(raw, encoding="utf-8-sig", newline="")


def add_restart_wait_helper() -> None:
    path = ROOT / "Mahou" / "Program.cs"
    raw = path.read_text(encoding="utf-8-sig")
    if "static void WaitForRestartParent(string[] args)" in raw:
        return
    marker = "\t\tpublic static void RefreshLCnMID() {"
    helper = '''\t\tstatic void WaitForRestartParent(string[] args) {
\t\t\tconst string prefix = "--restart-wait=";
\t\t\tif (args == null) return;
\t\t\tforeach (var arg in args) {
\t\t\t\tif (String.IsNullOrEmpty(arg) || !arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
\t\t\t\tint processId;
\t\t\t\tif (!Int32.TryParse(arg.Substring(prefix.Length), out processId) || processId <= 0) return;
\t\t\t\ttry {
\t\t\t\t\tusing (var process = Process.GetProcessById(processId)) {
\t\t\t\t\t\tif (!process.HasExited) process.WaitForExit(5000);
\t\t\t\t\t}
\t\t\t\t} catch (ArgumentException) {
\t\t\t\t\t// The old process already exited.
\t\t\t\t} catch (Exception e) {
\t\t\t\t\tDebug.WriteLine("Restart wait failed: " + e.Message);
\t\t\t\t}
\t\t\t\treturn;
\t\t\t}
\t\t}
'''
    if marker not in raw:
        fail("Program helper insertion marker not found")
    path.write_text(raw.replace(marker, helper + marker, 1), encoding="utf-8-sig", newline="")


def cleanup_staging() -> None:
    shutil.rmtree(PARTS, ignore_errors=True)
    shutil.rmtree(ROOT / ".github" / "hardening-payload", ignore_errors=True)
    for relative in [
        ".github/hardening-trigger.txt",
        ".github/workflows/apply-modern-hardening.yml",
        ".github/workflows/apply-modern-hardening-v2.yml",
        ".github/scripts/apply-modern-hardening.py",
    ]:
        path = ROOT / relative
        if path.exists():
            path.unlink()


def main() -> None:
    build_sanitized_patch()
    apply_patch()
    replace_legacy_network_block()
    add_restart_wait_helper()
    cleanup_staging()
    run("git", "diff", "--check")
    print("Modern hardening applied successfully.")


if __name__ == "__main__":
    main()
