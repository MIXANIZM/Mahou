# Active

- No product implementation task is currently active under the supervisor/executor workflow.

# Awaiting supervisor decision

- [AGZ-MAH-0001] [DEFERRED — NOT VERIFIED] 2026-07-25
  Expand opt-in local Smart Caps from only two initial capitals to accidental capitals anywhere after the first letter of each word segment, for example `ПРИвет → Привет`, `окоРОчка → окорочка` and `САнкт-ПЕтербург → Санкт-Петербург`.
  Requirements remain: default off; Russian and English UI through the common localization dictionaries; verified direct adapters only; no synthetic selection, keyboard rewrite, clipboard access, or cloud processing; all-caps words unchanged; immediate Backspace restores the original casing; two explicit Backspace rejections create a local personal exception; the settings tab shows a session counter for Mahou corrections and reversions.
  The user test of `e2c19110e56e6ed7ab30f4b8cb503431e32b7537` did not verify Mahou correction: the tab stayed in English, and the apparent `ПРивет → Привет` behavior continued after Mahou was closed, proving Microsoft Word autocorrect masked the test.
  Do not continue this task until the project supervisor creates a new bounded executor handoff.

- [AGZ-MAH-0003] [DEFERRED] 2026-07-25
  Design a dedicated verified direct adapter for modern Windows Notepad/RichEdit. Until then, collapsed-caret `Insert` and Smart Caps remain strict no-op in those controls.
  Draft PR #3 remains open, but its current diff is transport and workflow history from the attempted task, not an accepted or integrated Notepad implementation. Do not use, modify, close, rebase, or clean up PR #3 without a separate decision.

# Blocked

- None.

# Deferred product ideas

- Safe capitalization after sentence boundaries.
- Conservative typo correction.
- Optional local ghost-text completion.

# Completed

- [AGZ-MAH-0004] [COMPLETED] 2026-07-23
  Added the immutable artifact provenance gate and merged it through PR #4 into `mixanizm-modern-v2.9.0.1`.
  The accepted result includes immutable runtime/platform/commit/run artifact names, full commit/tree manifests, complete SHA-256 coverage, embedded executable identity verification, post-upload artifact evidence, and fail-closed positive and negative provenance checks.
  This task did not change Mahou runtime behavior or text-mutation behavior. PR #4 is closed and merged.

- [AGZ-MAH-0002] [VERIFIED] 2026-07-21
  Removed synthetic-selection behavior from collapsed-caret `Insert` and restricted the native adapter to the exact classic Win32 `Edit` class. The user verified on Windows 11 that modern Notepad no longer highlights, adds, removes, or corrupts text and instead performs a safe no-op. Verified checkpoint: `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`.

# Pull request status notes

- Draft PR #1 is open, based on `master`, and belongs to an earlier stabilization line. It is not the current working line and must not be changed without a separate decision.
- Draft PR #2 is open and is the main modernization PR from `mixanizm-modern-v2.9.0.1` into `master`. Keep it Draft and unmerged.
- Draft PR #3 is open against `mixanizm-modern-v2.9.0.1` and remains preserved transport history, not an accepted Notepad adapter.
