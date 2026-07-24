# Active

- No product implementation task is currently active under the supervisor/executor workflow.

# Awaiting supervisor decision

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

- [AGZ-MAH-0001] [VERIFIED] 2026-07-25
  Verified the opt-in local Smart Caps implementation at exact source commit `0b43bb688115e0051114f38a745fce9e830452fd` with the immutable x64 artifact `Mahou-2.9.0.1-dev-win-x64-0b43bb6-run30128168029`.
  GitHub Actions passed: Modern Windows build run `30128168029` and Security regression run `30128168156`. The artifact ZIP SHA-256 was independently confirmed as `e344355bfd51ca5ebfa5f0b0dd24a06af94e02497a3ce39e18ef8a59511c3a6c`, and its manifest identified the expected source commit and tree `bd0d80d350cea61cbdd2a9cecdfa6002f2c88ee9`.
  The user confirmed the full focused Windows smoke passed: Russian localization and default-off state; direct correction in Microsoft Word and the exact classic Win32 `Edit` adapter without visible selection; expected no-op cases; immediate Backspace restoration; correction/reversion counter deltas of `+8` and `+2`; creation of a personal exception after two explicit rejections and persistence after restart; no correction while Mahou was closed; and fail-closed behavior in modern Notepad, Chrome, Telegram and a password field without changing text, selection, caret, layout, clipboard or counters.
  Verified scope remains limited to the implemented direct adapters and the tested fail-closed applications. This verification does not approve merge, release, signing or publication.

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
