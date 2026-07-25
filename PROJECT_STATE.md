# Project state — MIXANIZM Mahou

Snapshot date: 2026-07-25

## Authoritative development line

- Repository: `MIXANIZM/Mahou`
- Legacy default branch: `master`
- Current development branch: `mixanizm-modern-v2.9.0.1`
- Main development pull request: Draft PR #2, from `mixanizm-modern-v2.9.0.1` into `master`
- Runtime line: `2.9.0.1-dev`
- Agatzub Development Ruleset: `v2.7.0`
- Rules content commit: `ba60623aec67c57d46bda7ce2b291a823de2d4ea`
- Last verified Smart Caps source commit: `0b43bb688115e0051114f38a745fce9e830452fd`
- Last verified selected-text conversion source commit: `3418d09de20ea327302a26858a7b752862bd429e`

The old `master` branch is not the current working line for modernized Mahou. New bounded tasks normally branch from `mixanizm-modern-v2.9.0.1` unless a task handoff explicitly states otherwise.

## Open pull requests

### Draft PR #1

- Open and unmerged.
- Based on `master` and represents an earlier security-stabilization line.
- It is not the current development line.
- Do not close, rebase, retarget, or modify it without a separate decision.

### Draft PR #2

- Open and unmerged.
- Head branch: `mixanizm-modern-v2.9.0.1`.
- This is the main modernization and hardening PR and the authoritative development line.
- Keep it Draft. Do not merge or mark Ready without explicit permission and completion of the required runtime and release checks.

### Draft PR #3

- Open and unmerged.
- Based on `mixanizm-modern-v2.9.0.1`.
- Its current diff is transport and workflow history from the attempted Notepad-adapter task, not an accepted or integrated Notepad implementation.
- It must not be used as a source branch, cleaned up, closed, rebased, or modified without a separate decision.

## Completed project tasks

### AGZ-MAH-0005 — selected-text conversion and clipboard verification

- Verified at exact source commit `3418d09de20ea327302a26858a7b752862bd429e` and tree `7920a089324fbd85d70b3669c7f86eb18ec5706a`.
- Automated checks passed: Modern Windows build run `30134239498` and Security regression run `30134239469`.
- Verified artifact: `Mahou-2.9.0.1-dev-win-x64-3418d09-run30134239498`; ZIP SHA-256 `c0f0643e251319bc20d9f528f4b199a13f780af7292a45c84dcacd210d328d28`; `Mahou.exe` SHA-256 `b3821d0a3116728db91cd46bf6091a578db19214cea7c3e7b465393b8876a95a`.
- The user confirmed existing user-created selection conversion worked forward and backward in Word, modern Notepad, downloaded local Chrome `textarea` and `contenteditable` controls, Telegram Desktop and the other applicable tested applications.
- The user confirmed real selection retained priority, ordinary insert mode remained usable, protected fields remained no-op, and repeated conversions did not corrupt text, caret, selection, layout or application stability.
- Unicode text, Word rich formatting, images, Excel cell ranges and Explorer file-drop clipboard data remained available after conversion.
- In browsers and messengers, no-selection `Insert` remains fail-closed. Microsoft Word's separately verified direct word-around-caret behavior remains supported.
- Verification does not authorize merge, tag, release, signing or publication.

### AGZ-MAH-0001 — Smart Caps verification

- Verified at exact source commit `0b43bb688115e0051114f38a745fce9e830452fd` and tree `bd0d80d350cea61cbdd2a9cecdfa6002f2c88ee9`.
- Automated checks passed: Modern Windows build run `30128168029` and Security regression run `30128168156`.
- Verified artifact: `Mahou-2.9.0.1-dev-win-x64-0b43bb6-run30128168029`; ZIP SHA-256 `e344355bfd51ca5ebfa5f0b0dd24a06af94e02497a3ce39e18ef8a59511c3a6c`.
- The user confirmed the complete focused Windows smoke passed, including localization, default-off behavior, Word and classic Win32 `Edit` direct correction without visible selection, expected no-op inputs, Backspace reversal, session counters, persistent personal exceptions, disabled/closed behavior, and fail-closed checks in modern Notepad, Chrome, Telegram and a password field.
- Counter deltas matched the test plan: `+8` Mahou corrections and `+2` Mahou reversions.
- Verified support remains Microsoft Word and the exact classic Win32 `Edit` class. Unsupported controls remain strict no-op.
- Verification does not authorize merge, tag, release, signing or publication.

### AGZ-MAH-0004 — immutable artifact provenance gate

- Completed and merged through PR #4 into `mixanizm-modern-v2.9.0.1`.
- Added immutable artifact names, full source commit/tree manifests, complete SHA-256 coverage, embedded executable identity checks, post-upload evidence, and fail-closed positive and negative provenance tests.
- Did not change Mahou runtime behavior, Insert, Smart Caps, Word, classic Edit, Notepad, selection, caret, or layout behavior.
- PR #4 is closed and merged.

## Product task status

- `AGZ-MAH-0005` selected-text conversion and full clipboard preservation are verified only for the exact source, artifact and Windows scenarios recorded above.
- `AGZ-MAH-0001` Smart Caps is verified only for the exact implementation, artifact, adapters and Windows scenarios recorded above.
- `AGZ-MAH-0003` Notepad direct adapter remains deferred. Unsupported modern Notepad/RichEdit controls stay fail-closed until a dedicated implementation and real Windows verification are completed.
- `AGZ-MAH-0002` collapsed-caret Insert safety is verified for the accepted checkpoint recorded in `ISSUES.md` and `UNIVERSAL-CHAT-HANDOFF.md`.

No product implementation task is currently active.

## Coordination model

- One active project supervisor chat maintains the overall state, chooses the next bounded task, prepares executor handoffs, and accepts results.
- One temporary executor chat handles one bounded task and does not begin the next task.
- The supervisor does not perform long implementation or debugging cycles by default.
- GitHub, `PROJECT_STATE.md`, `ISSUES.md`, and `UNIVERSAL-CHAT-HANDOFF.md` remain the recoverable source of project state.

## Release state

- No public release is approved from the current modernization line.
- No tag or GitHub Release is authorized by this state document.
- Authenticode signing, final independent review, applicable CI, and remaining release-specific Windows checks remain release gates.
