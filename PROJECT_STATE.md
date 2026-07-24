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

## Completed project task

### AGZ-MAH-0004 — immutable artifact provenance gate

- Completed and merged through PR #4 into `mixanizm-modern-v2.9.0.1`.
- Added immutable artifact names, full source commit/tree manifests, complete SHA-256 coverage, embedded executable identity checks, post-upload evidence, and fail-closed positive and negative provenance tests.
- Did not change Mahou runtime behavior, Insert, Smart Caps, Word, classic Edit, Notepad, selection, caret, or layout behavior.
- PR #4 is closed and merged.

## Product task status

- `AGZ-MAH-0001` Smart Caps remains unverified and deferred until a supervisor creates a new bounded executor handoff. Earlier apparent Word corrections were not proof of Mahou behavior.
- `AGZ-MAH-0003` Notepad direct adapter remains deferred. Unsupported modern Notepad/RichEdit controls stay fail-closed until a dedicated implementation and real Windows verification are completed.
- `AGZ-MAH-0002` collapsed-caret Insert safety is verified for the accepted checkpoint recorded in `ISSUES.md` and `UNIVERSAL-CHAT-HANDOFF.md`.

No product implementation task is active as part of the ruleset transition.

## Coordination model

- One active project supervisor chat maintains the overall state, chooses the next bounded task, prepares executor handoffs, and accepts results.
- One temporary executor chat handles one bounded task and does not begin the next task.
- The supervisor does not perform long implementation or debugging cycles by default.
- GitHub, `PROJECT_STATE.md`, `ISSUES.md`, and `UNIVERSAL-CHAT-HANDOFF.md` remain the recoverable source of project state.

## Release state

- No public release is approved from the current modernization line.
- No tag or GitHub Release is authorized by this state document.
- Authenticode signing, final independent review, applicable CI, and real Windows checks remain release gates.
