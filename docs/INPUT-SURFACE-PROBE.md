# Read-only input surface probe

- Probe task: `AGZ-MAH-0009`
- Evidence-recording task: `AGZ-MAH-0010`
- Tool: `tools/input-surface-probe/`
- Report schema: `1`
- Build: standalone Windows x64, .NET Framework 4.8
- Probe source commit: `cef38006dfe6093ee1b233703ad79215bb2a9758`
- Result: `USER_PROBE_EVIDENCE_COMPLETE`

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

`write_capabilities_unverified` means only that a provider advertised metadata such as a UIA `ValuePattern`, Word object model, or IAccessibleEditableText IID. It is not a safe-write result.

## Completed real-Windows capture result

The user completed the read-only probe on Windows with the immutable probe built from `cef38006dfe6093ee1b233703ad79215bb2a9758`.

| Surface | Result |
| --- | --- |
| Microsoft Word | `WORD_OBJECT_MODEL` |
| Modern Notepad | `RICHEDIT`; focused class `RichEditD2DPT` |
| AnyDesk classic field | `CLASSIC_WIN32_EDIT`; focused class `Edit` |
| Chrome `input` | `CHROMIUM_BROWSER`; UIA `Edit`; `TextPattern` + `ValuePattern` |
| Chrome `textarea` | same normalized Chromium Edit surface and fingerprint as tested `input` |
| Chrome `contenteditable` | separate `CHROMIUM_BROWSER`; UIA `Group`; `TextPattern` only |
| Telegram Desktop 7.0.5 | `QT_CUSTOM`; UIA `Edit`; class `Ui::InputField::Inner`; `TextPattern` + `ValuePattern` |
| WhatsApp Web in Opera | Chromium UIA `Edit`; `TextPattern` + `ValuePattern` |
| WhatsApp Desktop | `CUSTOM_UNKNOWN`; `Microsoft.UI.Content.DesktopChildSiteBridge`; internal editor not reached |
| Obsidian title | `ELECTRON_WEBVIEW`; UIA `Group`; `TextPattern` |
| Obsidian CodeMirror body | separate `ELECTRON_WEBVIEW`; UIA `Edit`; `TextPattern` + `ValuePattern` |

This evidence proves only the exposed read surface. It does not prove an exact writable range, safe replacement, application undo, framework event behavior, composition safety, or post-state verification.

## Archived sanitized reports

Reviewed reports that contain no actual text, titles, URLs, usernames, clipboard content, or personal paths are stored in:

```text
docs/evidence/input-surface-probe/
```

Committed files:

- `chrome-input.json`
- `chrome-textarea.json`
- `chrome-contenteditable.json`
- `telegram-desktop-7.0.5-compose.json`
- `whatsapp-web-opera-compose.json`
- `whatsapp-desktop-bridge.json`
- `obsidian-title.json`
- `obsidian-codemirror-body.json`

Duplicate and unsuccessful captures were omitted. Raw JSON for Word, modern Notepad, and AnyDesk was not available in the supplied files, so only the user-confirmed normalized evidence for those three surfaces is recorded.

## Important distinctions established by the capture

- Read capability does not imply safe write capability.
- Chrome `input`/`textarea` and Chrome `contenteditable` are distinct surfaces.
- Obsidian title and Obsidian CodeMirror body are distinct Electron surfaces.
- WhatsApp Desktop remains blocked at the desktop bridge; no internal editor surface was reached.
- Existing verified write adapters remain only Word document `Range` and exact classic Win32 `Edit`.

## Recommended research order

1. modern RichEdit / Notepad;
2. Telegram Qt input;
3. Chromium Edit;
4. Chromium `contenteditable`;
5. Electron CodeMirror;
6. WhatsApp Desktop bridge remains blocked.

RichEdit is first because a dedicated adapter may be able to use native range, caret, and undo semantics. Chromium and Telegram already demonstrate that UIA `TextPattern`/`ValuePattern` exposure alone does not provide the accepted exact-range mutation contract.

## Run one snapshot

1. Extract the exact immutable x64 probe ZIP.
2. Double-click `Mahou.InputSurfaceProbe.exe` as the ordinary user.
3. During the five-second countdown, click the exact text field being studied. Do not type, select, copy, paste or press Enter during capture.
4. Wait for the console to confirm the JSON filename and exit.
5. Keep each JSON with a neutral filename; do not put chat/contact/document names in the filename.

Optional:

```text
Mahou.InputSurfaceProbe.exe --countdown 8 --output neutral-surface.json
Mahou.InputSurfaceProbe.exe --version-json
```

The output path is never written into the report. The console prints only the final filename, not the full user path.

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

Unknown controls remain unknown. A process or framework match alone never authorizes mutation. Reports from different application versions or distinct surfaces in one application may legitimately produce different fingerprints.

## Automated verification of the probe commit

For probe commit `cef38006dfe6093ee1b233703ad79215bb2a9758`, GitHub Actions completed successfully:

- `Security regression` — run `30163258945`;
- `Input surface probe` — run `30163258946`;
- `Modern Windows build` — run `30163258943`.

The dedicated probe workflow scans executable source for forbidden mutation/selection/keyboard/clipboard/hook/injection/text/title APIs, builds the standalone x64 probe, runs redaction and classification contract tests, verifies the embedded source commit, and packages an immutable evidence ZIP.

## Limitations

- Accessibility providers can differ by application version, packaging channel and runtime state.
- Virtual accessibility children may expose different metadata than their backing HWND.
- Cross-integrity or protected processes may return less evidence; the probe fails closed.
- A successful read report is not proof of exact-range writing, undo, event, draft or application-state behavior.
- `AGZ-MAH-0010` includes no mutation experiment, runtime adapter, version change, release artifact, signing, merge or release.
