# MIXANIZM Mahou modern-base security audit

Baseline: preserved Mahou `2.9.0.1-dev` lineage, upstream commit `7dac9b588f71b004489034056fcc8ccb929c3ebb`.

This document records the first-pass static audit and the hardening applied on the
`mixanizm-modern-v2.9.0.1` branch. It is not a substitute for Windows 11 runtime testing.

## Critical and high-risk findings in the upstream baseline

1. **Legacy self-update trusted and executed an unsigned downloaded archive.**
   The updater downloaded release files, generated CMD/VBS extraction scripts, killed
   the running process, replaced the executable and launched the result without a
   cryptographic signature or pinned SHA-256 verification.
2. **Public synchronization could upload sensitive local data.**
   Settings, snippets, history, translation dictionaries and `Mahou.mm` could be sent to
   public third-party paste/file services. The proxy group could also be included.
3. **Proxy passwords were reversible Base64 text.**
4. **Selected-text conversion could destroy non-text clipboard formats.**
   The baseline manually copied selected clipboard formats, used unbounded waits and had
   incorrect global-memory ownership/unlock behavior.
5. **Configuration and user files could be written beside the executable.**
6. **Autostart used legacy shortcuts or elevated scheduled tasks.**
7. **Restart and exit generated scripts and used `taskkill /F`.**
8. **Snippet expression `__execute` could start arbitrary programs by default.**
9. **Several timing/range values were accepted without bounds.**
10. **Logging used blocking queue consumption and stored logs beside the executable.**

## Hardening applied in the first security pass

- Legacy updater download/install code removed from reachable methods; update UI is
  disabled and describes manual verified releases.
- Public backup/sync upload and restore UI is disabled; network methods are inert.
- Dictionary update restores the dictionary shipped in the verified build instead of
  downloading and executing extraction scripts.
- Proxy password storage uses Windows DPAPI (`CurrentUser`) with migration from legacy
  Base64/plain text.
- Default data directory is `%APPDATA%\MIXANIZM Mahou`; logs use
  `%LOCALAPPDATA%\MIXANIZM Mahou\Logs`.
- Legacy portable data is copied once when the destination is absent. Incompatible
  v1.4 settings are quarantined instead of parsed as modern settings.
- Configuration writes use a temp file, replace/copy fallback and a backup.
- Critical timing values are range-checked.
- Autostart uses the current-user `Run` registry value; old shortcut/task entries are
  removed but elevated tasks are never created.
- Restart uses a bounded parent-process wait argument; exit no longer invokes `taskkill`.
- Logging is non-blocking, bounded and size-rotated.
- Clipboard preservation uses an OLE `IDataObject` snapshot without enumerating or
  re-serializing HTML, RTF, Excel, image and file-drop formats. If preservation cannot
  be guaranteed, selected-text conversion fails open without clearing the clipboard.
- Clipboard waits are bounded.
- User-defined snippets, including `__execute` and every expression command path, are removed from the active product. Legacy snippet files are left untouched and inactive.
- Caps Lock remapping and one-key layout switching default to off.
- Untouched upstream word/selection hotkeys migrate to a shared `Insert` action.

## Still requiring work or explicit product decisions

- Translation is an explicit opt-in network feature and sends selected text to a remote
  translation endpoint. The UI must clearly disclose that behavior before release.
- JKL native helper lifecycle and binaries need a dedicated architecture/runtime audit.
- The custom `Mahou.mm` menu intentionally supports launching local programs and URLs;
  this must remain opt-in and be documented as executable local configuration.
- Keyboard hook latency and all selected-text conversion paths require physical Windows
  11 testing in Notepad, Chrome, Telegram/Discord, Word and Excel.
- Authenticode signing is not configured.
- Compiler warnings inherited from the modern baseline should be reduced after the
  security-critical behavior is stable.

## Release gate

Do not merge or publish a release until x86/x64 builds pass, Windows 11 runtime tests are
completed, a second independent audit is performed, and the resulting binaries are tied
to a source commit with SHA-256 provenance.

## Obsolete distribution automation removed

The MIXANIZM branch no longer contains legacy CMD/VBS release scripts or old Chocolatey download automation.
Removed: `Mahou/build-github-release+chocolatey-update.vbs`, `Mahou/build-run.cmd`, `Mahou/build.cmd`, `Mahou/clean.cmd`, `Chocolatey/`.
