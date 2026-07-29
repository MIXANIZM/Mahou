# Input surface capability map

- Probe task: `AGZ-MAH-0009`
- Evidence-recording task: `AGZ-MAH-0010`
- Result status: `USER_PROBE_EVIDENCE_COMPLETE`
- Probe schema: `1`
- Probe source commit: `cef38006dfe6093ee1b233703ad79215bb2a9758`
- Evidence date: `2026-07-27`
- Runtime change: none
- Mahou runtime version change: none (`2.9.0.1-dev` remains unchanged)

## Purpose and boundary

This map replaces application-by-application guessing with a capability-first inventory. The standalone probe records evidence about the foreground process and focused control, but it never claims that a readable control is safely writable.

A classification is only a normalized family label. Process filename, UI framework, class name, accessibility pattern, readable caret, readable text length, or an editable-interface IID is never sufficient authorization for mutation. Every Mahou write adapter still requires its own exact activation signature, stale-state validation, target-only mutation proof, caret/selection proof, undo semantics, application-state preservation, and real Windows verification.

## Completed real-Windows evidence

| Tested surface | Normalized family | Focused surface and read evidence | Safe direct-write status |
| --- | --- | --- | --- |
| Microsoft Word | `WORD_OBJECT_MODEL` | Word object model | **Existing verified adapter only:** bounded Word document `Range` |
| Modern Notepad | `RICHEDIT` | focused HWND class `RichEditD2DPT` | Unsupported; no accepted RichEdit adapter |
| AnyDesk classic field | `CLASSIC_WIN32_EDIT` | exact focused HWND class `Edit` | **Existing verified adapter only:** exact classic Win32 `Edit` |
| Chrome `input` | `CHROMIUM_BROWSER` | UIA `Edit`; `TextPattern` + `ValuePattern` | Unsupported |
| Chrome `textarea` | `CHROMIUM_BROWSER` | UIA `Edit`; `TextPattern` + `ValuePattern`; same normalized fingerprint as the tested Chrome `input` | Unsupported |
| Chrome `contenteditable` | `CHROMIUM_BROWSER` | UIA `Group`; `TextPattern`; no `ValuePattern` | Unsupported and distinct from input/textarea |
| Telegram Desktop 7.0.5 composer | `QT_CUSTOM` | UIA `Edit`; class `Ui::InputField::Inner`; `TextPattern` + `ValuePattern` | Unsupported; interface presence is metadata only |
| WhatsApp Web in Opera | `CHROMIUM_BROWSER` | Chromium UIA `Edit`; `TextPattern` + `ValuePattern` | Unsupported; Chromium Edit family |
| WhatsApp Desktop | `CUSTOM_UNKNOWN` | UIA `Pane`; class `Microsoft.UI.Content.DesktopChildSiteBridge`; no text/value pattern reached | **Blocked:** internal editor was not reached |
| Obsidian title | `ELECTRON_WEBVIEW` | UIA `Group`; class `inline-title`; `TextPattern` | Unsupported and distinct from body |
| Obsidian CodeMirror body | `ELECTRON_WEBVIEW` | UIA `Edit`; class `cm-content cm-lineWrapping`; `TextPattern` + `ValuePattern` | Unsupported and distinct from title |

### Evidence conclusions

- Read capability does not imply safe write capability.
- Chrome `input` and `textarea` were the same tested Chromium Edit surface, while `contenteditable` was a separate Group/TextPattern surface.
- Obsidian title and CodeMirror body were separate Electron surfaces and must not share an inferred mutation contract.
- WhatsApp Desktop remained at `DesktopChildSiteBridge`; the internal editor was not reached and the path remains blocked.
- The only verified direct collapsed-caret write adapters remain Microsoft Word document `Range` and the exact classic Win32 `Edit` class.
- `ValuePattern` presence in Chromium, Telegram Qt, WhatsApp Web, or Electron is not acceptance of whole-field replacement.

## Sanitized report archive

The following user-generated reports were reviewed and committed under `docs/evidence/input-surface-probe/`:

- `chrome-input.json`
- `chrome-textarea.json`
- `chrome-contenteditable.json`
- `telegram-desktop-7.0.5-compose.json`
- `whatsapp-web-opera-compose.json`
- `whatsapp-desktop-bridge.json`
- `obsidian-title.json`
- `obsidian-codemirror-body.json`

They contain no actual text, titles, URLs, usernames, clipboard content, or personal paths. Duplicate and unsuccessful captures were not committed. Raw JSON reports for Word, modern Notepad, and AnyDesk were not available in the supplied files; their normalized results above are recorded from the completed user-confirmed probe.

## Capability matrix

| Family | Typical evidence | Read evidence the probe may record | Safe exact-range write status in Mahou | Current classification |
| --- | --- | --- | --- | --- |
| Classic Win32 `Edit` | focused HWND class exactly `Edit`; native selection/length messages; password style | class, protected state, selection/caret availability, text length only | **Verified adapter exists** for the exact classic `Edit` class, with bounded native revalidation; this probe does not invoke it | `CLASSIC_WIN32_EDIT` |
| RichEdit variants | focused/UIA class contains `RichEdit`, including modern variants | class, UIA patterns, possible selection/caret and length metadata | **Unsupported** as a general family. Modern Notepad remains unsupported; PR #3 is not an accepted adapter | `RICHEDIT` |
| Microsoft Word | foreground `WINWORD.exe` plus read-only access to the active Word object model | file/version/signer metadata, selection/caret offsets and document length without document text/name | **Verified adapter exists** using a bounded Word document `Range`; probe presence alone does not activate it | `WORD_OBJECT_MODEL` |
| WPF | UIA FrameworkId `WPF` | UIA control type/class/automation ID summary, supported patterns, protected state, possible caret/selection/length metadata | **Unsupported** until a dedicated adapter is proven | `WPF` |
| WinUI/UWP/XAML | XAML/WinUI FrameworkId or modern Windows host signatures | UIA metadata and read capabilities where exposed | **Unsupported** until a dedicated adapter is proven | `WINUI_UWP` |
| Chromium browser controls | recognized browser process plus Chromium accessibility provider signature | UIA metadata, possible TextPattern/TextPattern2 read evidence | **Unsupported**. `AGZ-MAH-0006` rejected the desktop-only path and `AGZ-MAH-0007` rejected the tested browser-context mutation core | `CHROMIUM_BROWSER` |
| Electron/WebView applications | Chromium/WebView host signature without a recognized standalone browser identity | provider metadata and read evidence where exposed | **Unsupported**. Every host application and distinct editor surface needs its own behavior and safety verification | `ELECTRON_WEBVIEW` |
| Qt/custom native controls | Qt framework/window signature or application-specific custom control | UIA/MSAA/IA2 interface presence and read evidence where exposed | **Unsupported**. Telegram and all other Qt applications remain strict no-op without a user-created selection | `QT_CUSTOM` |
| Unknown/custom-drawn controls | no stable recognized family signature | only whatever bounded metadata the provider exposes | **Unsupported and unknown**; never upgraded by optimistic inference | `CUSTOM_UNKNOWN` |

## Why Word differs from Chromium and Qt

Word exposes an application document model with stable document positions and independently addressable `Range` objects. Mahou's existing Word adapter was designed and verified against that object model, including exact source/range checks and caret restoration.

Chromium and Telegram may expose readable text, caret/selection evidence, `TextPattern`, and `ValuePattern`, but UI Automation Text/TextRange does not provide an accepted exact substring replacement operation. Whole-control value replacement cannot prove target-only mutation, normal application undo, expected editing events, framework-controlled state, composition safety, or stale full-value overwrite protection.

## Recommended research order

1. Modern RichEdit / Notepad.
2. Telegram Qt input.
3. Chromium Edit (`input`, `textarea`, and Chromium Edit-family web composers).
4. Chromium `contenteditable`.
5. Electron CodeMirror.
6. WhatsApp Desktop bridge remains blocked until the internal editor can be reached read-only.

RichEdit is first because it may expose native range, caret, and undo semantics that can be tested as a bounded application adapter. UIA `TextPattern`/`ValuePattern` alone is already known to be insufficient for Chromium and Qt, so those families must not be prioritized merely because they expose readable metadata.

## Current Mahou adapter inventory

Verified direct collapsed-caret adapters:

1. exact classic Win32 `Edit`;
2. Microsoft Word document `Range`.

Unsupported direct collapsed-caret families:

- all RichEdit variants including modern Notepad;
- WPF;
- WinUI/UWP/XAML;
- Chrome and other browsers;
- Electron/WebView applications;
- Telegram and all other Qt/custom applications;
- unknown/custom-drawn controls.

Existing user-created selection conversion remains a separate already-verified path and is not changed by this task.

## Interpretation rule

A future adapter task may use these reports to choose what to investigate next. It must not translate any fingerprint, classification, process name, framework, control type, class name, read pattern, or interface presence directly into a write allow-list. Unknown evidence remains `CUSTOM_UNKNOWN`; conflicting or incomplete evidence reduces confidence and remains unsupported.
