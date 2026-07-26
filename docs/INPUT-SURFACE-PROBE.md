# Read-only input surface probe

- Task: `AGZ-MAH-0009`
- Tool: `tools/input-surface-probe/`
- Report schema: `1`
- Build: standalone Windows x64, .NET Framework 4.8
- Result: `PROBE_READY`

## What it does

`Mahou.InputSurfaceProbe.exe` performs one capture after a short countdown so the user can focus a target text field. It inspects only the foreground process and focused control, writes a redacted JSON report, and exits.

The probe is separate from Mahou. It is not referenced by the Mahou solution, does not install anything, requests `asInvoker` with `uiAccess=false`, requires no administrator rights, creates no hooks, and performs no continuous monitoring.

## What it records

Where available, the report contains:

- foreground process filename only, architecture, product/file version, and signing-certificate presence/name/thumbprint;
- top-level and focused HWND classes, without a window title;
- UIA control type, framework ID, class name, a redacted automation-ID token, supported pattern names, and password/protected state;
- UIA Name only as `present` plus a coarse length bucket;
- MSAA role/state as normalized non-content values;
- IAccessible2, IAccessibleText and IAccessibleEditableText interface presence only;
- whether caret/selection state and text length can be read without retrieving actual text;
- cautious classification, confidence, evidence, limitations and a normalized SHA-256 capability fingerprint;
- exact report schema and embedded source commit.

## What it never records

The implementation and tests forbid collection of:

- actual typed/document/chat text;
- document, chat or contact names;
- window titles;
- UIA Name content;
- executable paths or paths containing usernames;
- browser URLs;
- clipboard contents;
- chat history;
- passwords.

Password/protected controls suppress caret, selection and length output even if a provider might expose such values.

## No-write contract

The probe does not contain or invoke:

- UIA whole-value writes or programmatic selection;
- accessible editable-text methods;
- Win32 text replacement messages;
- Word Range mutation;
- keyboard simulation;
- clipboard APIs;
- hooks, hotkeys, polling monitors or input listeners;
- process-memory access beyond limited process identity queries;
- code injection.

`write_capabilities_unverified` means only that a provider advertised metadata such as a UIA ValuePattern, Word object model, or IAccessibleEditableText IID. It is not a safe-write result.

## Run one snapshot

1. Extract the exact immutable x64 probe ZIP.
2. Double-click `Mahou.InputSurfaceProbe.exe` as the ordinary user.
3. During the five-second countdown, click the exact text field being studied. Do not type, select, copy, paste or press Enter during capture.
4. Wait for the console to confirm the JSON filename and exit.
5. Keep each JSON with a neutral filename such as `chrome-input.json`; do not put chat/contact/document names in the filename.

Optional:

```text
Mahou.InputSurfaceProbe.exe --countdown 8 --output telegram-compose.json
Mahou.InputSurfaceProbe.exe --version-json
```

The output path is never written into the report. The console prints only the final filename, not the full user path.

## Suggested capture set

Create exactly one snapshot per target. No mutation test belongs to this task.

1. **Classic Win32 Edit** — a known legacy application field whose focused HWND class is expected to be `Edit`.
2. **Microsoft Word** — an ordinary editable document with a collapsed caret.
3. **Modern Notepad** — its main editor, without selecting text.
4. **Chrome input** — a disposable local/test page ordinary `input[type=text]`.
5. **Chrome textarea** — an ordinary `textarea` on the same disposable page.
6. **Chrome contenteditable** — a disposable `contenteditable` region.
7. **Telegram Desktop** — ordinary new-message composer in Saved Messages or a dedicated private test chat; do not press Enter.
8. **WhatsApp Desktop** — ordinary message composer in a private test context; do not send.
9. **One WPF or WinUI application** — an ordinary editable field if such an application is available.

Do not use real passwords or sensitive content. A neutral placeholder can already be present, but the probe will not retrieve it.

## Report interpretation

Every report has the required top-level fields:

```text
classification
confidence
evidence
read_capabilities
write_capabilities_unverified
limitations
```

It also includes `schema_version`, `probe_commit` and `capability_fingerprint`.

Classification values are:

```text
CLASSIC_WIN32_EDIT
RICHEDIT
WORD_OBJECT_MODEL
WPF
WINUI_UWP
CHROMIUM_BROWSER
ELECTRON_WEBVIEW
QT_CUSTOM
CUSTOM_UNKNOWN
```

Unknown controls remain unknown. A process or framework match alone never authorizes mutation. Reports from different application versions may legitimately produce different fingerprints.

## Automated verification

The dedicated `Input surface probe` workflow:

- scans executable source for mutation, selection, keyboard, clipboard, hook, injection and text/title extraction APIs;
- builds the standalone probe as x64 Release;
- runs dependency-free C# contract tests for redaction, password suppression, deterministic classification/fingerprint, unknown behavior, schema and embedded commit;
- verifies `--version-json` against the exact workflow commit;
- packages an immutable x64 ZIP with `probe-manifest.json` and `SHA256SUMS.txt`;
- uploads the ZIP, its SHA-256 file and post-build evidence.

The existing `Security regression` and `Modern Windows build` workflows remain required and unchanged in purpose.

## Limitations

- Accessibility providers can differ by application version, packaging channel and runtime state.
- Virtual accessibility children may expose different metadata than their backing HWND.
- Cross-integrity or protected processes may return less evidence; the probe fails closed.
- A successful read report is not proof of exact-range writing, undo, event, draft or application-state behavior.
- This task includes no text mutation, runtime adapter, release, signing or user acceptance of a future write path.
