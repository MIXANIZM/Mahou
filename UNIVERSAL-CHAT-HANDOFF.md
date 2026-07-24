# Universal chat handoff — MIXANIZM Mahou

## Project

- Repository: `MIXANIZM/Mahou`
- Legacy default branch: `master`
- Current development branch: `mixanizm-modern-v2.9.0.1`
- Main development pull request: Draft PR #2
- Runtime line: `2.9.0.1-dev`
- Agatzub Development Ruleset: `v2.7.0`
- Rules content commit: `ba60623aec67c57d46bda7ce2b291a823de2d4ea`
- Last user-verified Insert safety checkpoint: `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`
- Last Smart Caps candidate tested by the user: `e2c19110e56e6ed7ab30f4b8cb503431e32b7537` — not verified

The old `master` branch is not the working line for modernized Mahou. Read `PROJECT_STATE.md` before choosing or assigning work.

## Coordination model

Mahou now follows the central supervisor/executor workflow:

- one active project supervisor chat maintains the overall state, chooses the next bounded task, prepares an executor handoff, and accepts or rejects the result;
- one temporary executor chat performs one bounded task, reports back, and does not begin the next task;
- the supervisor does not perform long implementation or debugging cycles by default;
- GitHub, `PROJECT_STATE.md`, `ISSUES.md`, and this handoff are the recoverable source of project state.

Detailed role rules come from `standards/CHAT-ROLES-AND-WORKFLOW.md` at the exact rules content commit pinned above. Project-specific instructions are in `AGENTS.md`.

## Project goal

Modernize and harden Mahou while preserving useful layout-switching behavior and making every automatic text mutation fail closed.

## Verified current behavior

- Collapsed-caret `Insert` never creates synthetic blue selection.
- Modern Windows Notepad/RichEdit, Chrome, Telegram, Discord, unknown controls, protected fields, and failed probes are no-op without a user-created selection.
- Modern Notepad was manually verified not to add, remove, highlight, or corrupt text.
- Microsoft Word and the exact classic Win32 `Edit` class remain the only supported direct collapsed-caret adapters.
- Draft PR #2 remains open, Draft, and unmerged.

## Completed task

`AGZ-MAH-0004` is completed and merged through PR #4 into `mixanizm-modern-v2.9.0.1`.

The accepted provenance gate provides:

- immutable runtime/platform/commit/run artifact names;
- full repository, commit, tree, ref, workflow, run, platform, configuration and timestamp identity;
- SHA-256 coverage and exact manifest inventory checks;
- executable runtime-version and embedded-commit verification;
- post-upload artifact ID, digest, run, ZIP and executable evidence;
- fail-closed positive and negative provenance regression tests;
- an explicit rule that a PR-head artifact cannot be represented as a merge-head artifact.

This task did not change Insert, Smart Caps, Word, classic Edit, Notepad, browser/messenger, selection, caret, layout, or other runtime behavior.

## Open pull requests

### Draft PR #1

Open and unmerged against `master`. It belongs to an earlier stabilization line and is not the current development line. Do not modify, close, rebase, or retarget it without a separate decision.

### Draft PR #2

Open and unmerged from `mixanizm-modern-v2.9.0.1` into `master`. This is the main modernization PR. Keep it Draft; do not merge, mark Ready, tag, release, or publish without explicit permission and completion of the applicable checks.

### Draft PR #3

Open and unmerged against `mixanizm-modern-v2.9.0.1`. Its present diff is transport and workflow history from the attempted Notepad-adapter task, not an accepted or integrated Notepad implementation. Do not use it as a source branch and do not modify, close, rebase, or clean it up without a separate decision.

## Deferred product tasks

### AGZ-MAH-0001 — Smart Caps

The revised optional Smart Caps behavior remains unverified. The last user test was invalid as proof of Mahou correction because Microsoft Word continued correcting after Mahou was closed. The English-only tab also exposed a localization mismatch.

Do not continue Smart Caps until the supervisor creates a new bounded executor handoff with exact acceptance criteria and a focused Windows smoke test.

### AGZ-MAH-0003 — modern Notepad adapter

The direct Notepad/RichEdit adapter remains deferred. Unsupported controls must continue to fail closed. PR #3 is not an accepted implementation and must not be used as one.

## Important prohibitions

- Do not restore UI Automation `.Select()`, `Shift+Left`, `Ctrl+Shift+Left`, generated-selection collapse/reselect, stale round-trip, or tracked-buffer `ConvertLast` fallbacks.
- Do not change PR #1 or PR #3 without a separate explicit task and permission.
- Do not merge or mark Ready PR #2, create a tag or Release, or publish a user build without explicit permission.
- Do not describe Smart Caps or a Notepad adapter as verified until the exact implementation passes applicable CI and the user completes the focused real Windows smoke test.
- Do not enable paid GitHub features, paid runners, or paid CI capacity without explicit permission.
- Do not hand off an artifact unless it passes `ARTIFACT-PROVENANCE.md` against the exact expected commit and tree.

## Next management step

Create or appoint one project supervisor chat. The supervisor must restore state from the current GitHub branch, `PROJECT_STATE.md`, `ISSUES.md`, and this handoff, then choose exactly one next bounded task and prepare a separate executor handoff.

No temporary executor should start Smart Caps, Notepad work, Insert changes, release work, or another product task without that handoff.
