# Active

- [AGZ-MAH-0001] [IN PROGRESS] 2026-07-22
  Expand opt-in local Smart Caps from only two initial capitals to accidental capitals anywhere after the first letter of each word segment, for example `ПРИвет → Привет`, `окоРОчка → окорочка` and `САнкт-ПЕтербург → Санкт-Петербург`.
  Requirements: default off; Russian and English UI through the common localization dictionaries; direct verified adapters only; no synthetic selection, keyboard rewrite, clipboard access, or cloud processing; all-caps words remain unchanged; immediate Backspace restores the original casing; two explicit Backspace rejections create a local personal exception; the settings tab shows a session counter for Mahou corrections and reversions.
  User test of `e2c19110e56e6ed7ab30f4b8cb503431e32b7537` found that the new tab stayed in English because it checked `Russian` instead of Mahou's real `Русский` locale value. The apparent `ПРивет → Привет` behavior continued after Mahou was closed and was therefore Microsoft Word's own autocorrect, not verified Mahou behavior.
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
