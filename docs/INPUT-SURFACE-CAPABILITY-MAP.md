# Input surface capability map

- Task: `AGZ-MAH-0009`
- Result status: `PROBE_READY`
- Probe schema: `1`
- Runtime change: none
- Mahou runtime version change: none (`2.9.0.1-dev` remains unchanged)

## Purpose and boundary

This map replaces application-by-application guessing with a capability-first inventory. The standalone probe records evidence about the foreground process and focused control, but it never claims that the control is safely writable.

A classification is only a normalized family label. Process filename, UI framework, class name, accessibility pattern, readable caret, readable text length, or an editable-interface IID is never sufficient authorization for mutation. Every Mahou write adapter still requires its own exact activation signature, stale-state validation, target-only mutation proof, caret/selection proof, undo semantics, application-state preservation, and real Windows verification.

## Capability matrix

| Family | Typical evidence | Read evidence the probe may record | Safe exact-range write status in Mahou | Current classification |
| --- | --- | --- | --- | --- |
| Classic Win32 `Edit` | focused HWND class exactly `Edit`; native selection/length messages; password style | class, protected state, selection/caret availability, text length only | **Verified adapter exists** for the exact classic `Edit` class, with bounded native revalidation; this probe does not invoke it | `CLASSIC_WIN32_EDIT` |
| RichEdit variants | focused/UIA class contains `RichEdit`, including modern variants | class, UIA patterns, possible selection/caret and length metadata | **Unsupported** as a general family. Modern Notepad remains unsupported; PR #3 is not an accepted adapter | `RICHEDIT` |
| Microsoft Word | foreground `WINWORD.exe` plus read-only access to the active Word object model | file/version/signer metadata, selection/caret offsets and document length without document text/name | **Verified adapter exists** using a bounded Word document `Range`; probe presence alone does not activate it | `WORD_OBJECT_MODEL` |
| WPF | UIA FrameworkId `WPF` | UIA control type/class/automation ID summary, supported patterns, protected state, possible caret/selection/length metadata | **Unsupported** until a dedicated adapter is proven | `WPF` |
| WinUI/UWP/XAML | XAML/WinUI FrameworkId or modern Windows host signatures | UIA metadata and read capabilities where exposed | **Unsupported** until a dedicated adapter is proven | `WINUI_UWP` |
| Chromium browser controls | recognized browser process plus Chromium accessibility provider signature | UIA metadata, possible TextPattern/TextPattern2 read evidence | **Unsupported**. `AGZ-MAH-0006` rejected the desktop-only path and `AGZ-MAH-0007` rejected the tested browser-context mutation core | `CHROMIUM_BROWSER` |
| Electron/WebView applications | Chromium/WebView host signature without a recognized standalone browser identity | provider metadata and read evidence where exposed | **Unsupported**. Every host application needs its own behavior and safety verification | `ELECTRON_WEBVIEW` |
| Qt/custom native controls | Qt framework/window signature or application-specific custom control | UIA/MSAA/IA2 interface presence and read evidence where exposed | **Unsupported**. Telegram and all other Qt applications remain strict no-op without a user-created selection | `QT_CUSTOM` |
| Unknown/custom-drawn controls | no stable recognized family signature | only whatever bounded metadata the provider exposes | **Unsupported and unknown**; never upgraded by optimistic inference | `CUSTOM_UNKNOWN` |

## Why Word differs from Chrome

Word exposes an application document model with stable document positions and independently addressable `Range` objects. Mahou's existing Word adapter was designed and verified against that object model, including exact source/range checks and caret restoration.

Chrome's Windows accessibility provider may expose readable text and a caret range, but UI Automation Text/TextRange does not provide an accepted exact substring replacement operation. Whole-control value replacement is not equivalent: it cannot prove target-only mutation, normal browser undo, expected editing events, framework-controlled state, composition safety, or stale full-value overwrite protection. Therefore readable Chrome evidence remains classification metadata only.

## Why Telegram/Qt differs from Chrome

Telegram Desktop uses a Qt/custom input subsystem rather than an ordinary browser DOM control. A Chromium result cannot be transferred to Telegram, and a Qt class or framework label cannot distinguish the normal message composer from search, captions, edit-message fields, passcode fields, or controls in unrelated Qt applications.

`AGZ-MAH-0008` found no safe installed-build evidence in the executor environment and is accepted as `BLOCKED`. IAccessible2 interface presence, including `IAccessibleEditableText`, would still be metadata only until a real Telegram build proves exact target replacement, caret, collapsed selection, unchanged draft/entities/chat, one normal undo, no composition conflict, no send, and exact post-state verification.

## Why Electron/WebView still needs application-specific verification

Electron and WebView hosts can use Chromium internally, but the surrounding application controls focus, document lifecycle, undo integration, state synchronization, navigation, protected fields, and application-specific actions. A Chromium provider signature identifies an implementation family, not a safe mutation contract. WhatsApp Desktop, Discord, Teams, custom WebView hosts, and other applications must therefore remain unsupported until separately verified.

## Why UIA read support does not imply safe writing

The probe may determine that a selection is collapsed, a caret range is readable, or a document length can be counted without retrieving text. Those are read capabilities only. They do not establish:

- an exact writable range primitive;
- atomic source/caret/selection revalidation;
- unchanged prefix and suffix;
- one normal application undo transaction;
- preservation of formatting, draft and framework state;
- composition/IME safety;
- no unintended action such as sending a message.

`ValuePattern` and `IAccessibleEditableText` presence are deliberately reported under `write_capabilities_unverified`, never under verified capabilities.

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

A future adapter task may use probe reports to choose what to investigate next. It must not translate any fingerprint or classification directly into a write allow-list. Unknown evidence remains `CUSTOM_UNKNOWN`; conflicting or incomplete evidence reduces confidence and remains unsupported.
