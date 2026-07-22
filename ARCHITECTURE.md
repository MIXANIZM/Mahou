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

## Smart Caps

Smart Caps is an optional service connected to the keyboard event stream. It tracks only a short fresh word and schedules a correction after a boundary key is committed. It lowercases accidental uppercase letters after the first letter of each apostrophe/hyphen-delimited word segment, while words typed entirely in capitals remain unchanged. Before mutation it revalidates age, foreground window, exclusion policy, exact source text, and direct-adapter availability. Unsupported or stale contexts are discarded.

Microsoft Word can independently correct two initial capitals before Mahou runs. Therefore the settings tab exposes session-only correction and reversion counters; they change only after a direct replacement performed by Mahou and contain no typed word content.

Intentional mixed-case names cannot be distinguished perfectly from accidental interior capitals. Immediate physical Backspace reverses Mahou's change, and two explicit reversions learn a local personal exception. Personal exceptions are stored in the existing INI configuration under `SmartTyping`. No typed word content is sent over the network or written to plaintext diagnostics.
