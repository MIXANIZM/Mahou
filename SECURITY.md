# Security

## Text safety

- Protected/password fields suppress manual conversion and Smart Caps.
- Collapsed-caret `Insert` and Smart Caps are fail-closed.
- Unsupported controls must not change text, selection, caret, layout, or clipboard.
- Synthetic selection and keyboard/clipboard fallbacks are forbidden in collapsed-caret mutation paths.
- Exact source, foreground window, focused control, caret, and freshness are revalidated before direct replacement where applicable.

## User snippets removal boundary

- User-defined snippets are absent from active runtime and UI code.
- No active source may read, parse, create, write, back up, reload, execute or delete `snippets.txt` or `snippets.txt.bak`.
- Legacy files and obsolete INI keys remain untouched inactive rollback data.
- Snippet expressions and command paths, including `__execute`, `__delay`, `__keyboard`, `__paste`, `__selection` and `__setlayout`, are unavailable.
- AutoSwitch dictionary values are emitted literally and cannot invoke process, clipboard, delayed-command or keyboard-script behavior.
- The Security regression and executable `AutoSwitchIndependenceRegression` fail if removed snippet tokens or methods return.

## AutoSwitch mutation boundary

- AutoSwitch is independent of legacy `SnippetsEnabled` values and requires no snippets file.
- It captures foreground HWND, focused-control HWND, PID, executable, control class and protected state before routing.
- The same context is revalidated before every Backspace/delete, layout change, literal insertion, trailing-space insertion and deferred callback.
- Exact `notepad.exe` + `RichEditD2DPT`, protected classic Edit, unknown context and changed context fail closed.

## AutoSwitch dictionary parsing boundary

- Dictionary parsing advances only forward and never copies the complete remaining suffix.
- Active source/replacement arrays are replaced only after a complete successful parse; malformed or incomplete input publishes no partial rules.
- The displayed rule count comes from the same parse result used by runtime matching, not from a second scan or character-sized tracking arrays.
- Disabling AutoSwitch clears active parsed data without modifying or deleting `AS_dict.txt`.

## Secrets and local data

- Proxy credentials use Windows DPAPI for the current user and are masked in the UI.
- Configuration and personal Smart Caps exceptions remain local under the Mahou application data directory.
- Raw typed words must not be written to diagnostic logs.

## Network

- Legacy updater and public synchronization are disabled.
- Translation remains explicit opt-in and sends selected text only to the configured translation service with bounded requests.
- Smart Caps has no network path.

## Release blockers

- Real Windows smoke testing of affected keyboard behavior.
- Green security regression and deterministic x86/x64 builds.
- Authenticode signing and final independent review remain required for a public release.
