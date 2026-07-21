# Security

## Text safety

- Protected/password fields suppress manual conversion and Smart Caps.
- Collapsed-caret `Insert` and Smart Caps are fail-closed.
- Unsupported controls must not change text, selection, caret, layout, or clipboard.
- Synthetic selection and keyboard/clipboard fallbacks are forbidden in collapsed-caret mutation paths.
- Exact source, foreground window, focused control, caret, and freshness are revalidated before direct replacement where applicable.

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
