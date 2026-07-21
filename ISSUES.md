# Active

- [AGZ-MAH-0001] [IMPLEMENTED] 2026-07-21
  Add opt-in local Smart Caps correction for two accidental initial capitals, for example `ПРивет → Привет`.
  Requirements: default off; direct verified adapters only; no synthetic selection, keyboard rewrite, clipboard access, or cloud processing; immediate Backspace restores the original casing; two explicit Backspace rejections create a local personal exception.
  Current target: Draft PR #2 branch `mixanizm-modern-v2.9.0.1`.

- [AGZ-MAH-0003] [DEFERRED] 2026-07-21
  Design dedicated verified direct adapters for modern Windows Notepad/RichEdit and supported browser/messenger editors. Until then, collapsed-caret `Insert` and Smart Caps remain strict no-op in those controls.

# Blocked

- None.

# Deferred

- Safe capitalization after sentence boundaries.
- Conservative typo correction.
- Optional local ghost-text completion.

# Recently verified

- [AGZ-MAH-0002] [VERIFIED] 2026-07-21
  Removed synthetic-selection behavior from collapsed-caret `Insert` and restricted the native adapter to the exact classic Win32 `Edit` class. User verified on Windows 11 that modern Notepad no longer highlights, adds, removes, or corrupts text and instead performs a safe no-op. Verified head: `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`.
