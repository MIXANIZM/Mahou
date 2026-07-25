# Architecture

## Runtime shape

Mahou is a .NET Framework 4.8 Windows Forms application with low-level keyboard and raw-input processing, local configuration, optional translation, snippets, AutoSwitch, and manual layout conversion.

## Text-mutation safety boundary

Text controls are treated by capability, not by a permissive class-name guess.

- **User-created selection:** handled by the legacy selected-text pipeline and remains a separate hardening area.
- **Collapsed caret, classic Win32 `Edit`:** bounded native read and exact direct replacement with foreground, focus, caret, and source revalidation.
- **Collapsed caret, Microsoft Word:** bounded direct document `Range` replacement.
- **All other controls:** strict no-op until a dedicated direct adapter is proven.

The collapsed-caret path must never use synthetic selection, UI Automation `.Select()`, keyboard selection, clipboard mutation, tracked-word deletion/retyping, or compatibility fallback.

## Input-surface capability probe

`AGZ-MAH-0009` adds a separate executable under `tools/input-surface-probe/`. It is not referenced by the Mahou solution and does not participate in input processing.

The probe performs one read-only capture after a countdown and exits. It inspects only the foreground process and focused control. It records redacted process/window/UIA/MSAA/IAccessible2 capability metadata, protected state, possible caret/selection/length readability and a deterministic capability fingerprint. It never retrieves actual text, window titles, UIA Name content, URLs, clipboard content or passwords.

The normalized families are evidence labels only:

- `CLASSIC_WIN32_EDIT`;
- `RICHEDIT`;
- `WORD_OBJECT_MODEL`;
- `WPF`;
- `WINUI_UWP`;
- `CHROMIUM_BROWSER`;
- `ELECTRON_WEBVIEW`;
- `QT_CUSTOM`;
- `CUSTOM_UNKNOWN`.

No classification, process filename, framework, pattern, readable caret, readable length or editable-interface presence can activate a Mahou mutation path. A dedicated adapter remains mandatory for every family except the already verified exact classic `Edit` and Word adapters.

Detailed boundaries are in `docs/INPUT-SURFACE-CAPABILITY-MAP.md` and `docs/INPUT-SURFACE-PROBE.md`.

## Smart Caps

Smart Caps is an optional service connected to the keyboard event stream. It tracks only a short fresh word and schedules a correction after a boundary key is committed. It lowercases accidental uppercase letters after the first letter of each apostrophe/hyphen-delimited word segment, while words typed entirely in capitals remain unchanged. Before mutation it revalidates age, foreground window, exclusion policy, exact source text, and direct-adapter availability. Unsupported or stale contexts are discarded.

Microsoft Word can independently correct two initial capitals before Mahou runs. Therefore the settings tab exposes session-only correction and reversion counters; they change only after a direct replacement performed by Mahou and contain no typed word content.

Intentional mixed-case names cannot be distinguished perfectly from accidental interior capitals. Immediate physical Backspace reverses Mahou's change, and two explicit reversions learn a local personal exception. Personal exceptions are stored in the existing INI configuration under `SmartTyping`. No typed word content is sent over the network or written to plaintext diagnostics.

## Telegram Desktop boundary

`AGZ-MAH-0008` is accepted as `BLOCKED`: it did not establish a safe Telegram Desktop mutation primitive because the executor environment could not inspect the exact installed Windows process and ordinary new-message composer.

The official Telegram Desktop source confirms a Qt-based custom `Ui::InputField` compose subsystem, but source class ancestry is not an activation signature and does not prove the Windows accessibility provider or installed build behavior.

UI Automation Text/TextRange remains read-only for general text mutation; `.Select()` and whole-field `ValuePattern.SetValue()` are forbidden. IAccessible2 defines an addressable `IAccessibleEditableText::replaceText` operation, but it is not accepted unless the exact installed Telegram provider exposes it and a real Saved Messages test proves target-only mutation, exact caret, collapsed selection, unchanged neighbors, preserved formatting/draft state, unchanged active chat, one normal Telegram undo operation, no composition conflict, no send, and exact post-state verification.

Until that complete evidence exists:

- Telegram is not included in `SmartCaps.TryDirectReplace`;
- process name, Qt class, UIA read access, caret access, capability-probe classification or interface presence alone are insufficient;
- search, caption, edit-message, forward-comment, passcode, and every other Telegram field remain mandatory no-op;
- other Qt applications remain mandatory no-op.

The detailed investigation record is `docs/TELEGRAM-SMART-CAPS-ARCHITECTURE.md`.
