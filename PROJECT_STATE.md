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
- Chrome desktop-only decision: `DIRECT-PATH-NOT-SAFE` under `AGZ-MAH-0006`
- Chrome browser-context editing-core result awaiting supervisor decision: `BROWSER-CONTEXT-MUTATION-NOT-SAFE` under `AGZ-MAH-0007`

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

## Task result awaiting supervisor decision

### AGZ-MAH-0007 — Chrome Manifest V3 editing-core prototype

- Investigated from exact starting commit `faaf170d12e5bbcbd49c0b66edf4bac75c1e3049`.
- Result: `BROWSER-CONTEXT-MUTATION-NOT-SAFE`.
- `setRangeText()` can express an exact range and caret movement but does not provide the required trusted normal editing-event plus single browser undo/redo transaction contract.
- Deprecated `execCommand('insertText')` may preserve browser undo but exact replacement requires a temporary programmatic selection, which violates the task boundary.
- Whole-field `.value` assignment and synthetic events remain rejected.
- A minimal action-activated Manifest V3 prototype uses only `activeTab` and `scripting`, is restricted by an exact page marker, performs strict preflight and delayed stale-state revalidation, and never mutates text.
- Added static and Node contract checks plus a dedicated read-only workflow.
- The available Chromium was `144.0.7559.96` on Debian 13, but managed policy blocked extension installation and all URLs, so no real unpacked-extension smoke is claimed. No prohibited workaround was used.
- No Native Messaging, Mahou runtime code, runtime version, counters, Backspace path, personal exceptions, Telegram, Notepad, merge, tag, signing, release, or publication changed.

## Completed project tasks

### AGZ-MAH-0006 — Chrome Smart Caps direct-path architecture decision

- Investigated from exact starting commit `d7e0e90a149d8d792260011b5902b80770a055d7`.
- Result: `DIRECT-PATH-NOT-SAFE`.
- UI Automation has no acceptable exact replace-range primitive; `ValuePattern.SetValue` is a rejected whole-field rewrite.
- Chrome remains strict no-op. No runtime or integration code changed.

### AGZ-MAH-0005 — selected-text conversion and clipboard verification

- Verified at exact source commit `3418d09de20ea327302a26858a7b752862bd429e` and tree `7920a089324fbd85d70b3669c7f86eb18ec5706a`.
- Automated checks passed: Modern Windows build run `30134239498` and Security regression run `30134239469`.
- The user confirmed the complete focused Windows selected-text and clipboard smoke.
- In browsers and messengers, no-selection `Insert` remains fail-closed.

### AGZ-MAH-0001 — Smart Caps verification

- Verified at exact source commit `0b43bb688115e0051114f38a745fce9e830452fd` and tree `bd0d80d350cea61cbdd2a9cecdfa6002f2c88ee9`.
- Automated checks passed: Modern Windows build run `30128168029` and Security regression run `30128168156`.
- The user confirmed the complete focused Windows smoke, including strict no-op behavior in unsupported controls.

### AGZ-MAH-0004 — immutable artifact provenance gate

- Completed and merged through PR #4 into `mixanizm-modern-v2.9.0.1`.
- Added immutable artifact identity, complete hashes, embedded commit verification, post-upload evidence, and fail-closed provenance tests.
- Did not change Mahou runtime behavior.

## Product task status

- `AGZ-MAH-0007` is a negative browser-context editing-core result awaiting supervisor acceptance. It does not add Chrome support.
- `AGZ-MAH-0006` established that a Chrome desktop-only direct adapter is not safe.
- `AGZ-MAH-0005` and `AGZ-MAH-0001` remain verified only for their exact recorded sources, artifacts, and Windows scenarios.
- `AGZ-MAH-0003` Notepad direct adapter remains deferred. PR #3 is not an accepted implementation.
- No product implementation task is currently active.

## Coordination model

- One active project supervisor chat maintains the overall state, chooses the next bounded task, prepares executor handoffs, and accepts results.
- One temporary executor chat handles one bounded task and does not begin the next task.
- GitHub, `PROJECT_STATE.md`, `ISSUES.md`, and `UNIVERSAL-CHAT-HANDOFF.md` remain the recoverable source of project state.

## Release state

- No public release is approved from the current modernization line.
- No tag or GitHub Release is authorized by this state document.
- Authenticode signing, final independent review, applicable CI, and remaining release-specific Windows checks remain release gates.
