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
- Verified Smart Caps source commit: `0b43bb688115e0051114f38a745fce9e830452fd`
- Verified Smart Caps artifact: `Mahou-2.9.0.1-dev-win-x64-0b43bb6-run30128168029`

The old `master` branch is not the working line for modernized Mahou. Read `PROJECT_STATE.md` before choosing or assigning work.

## Coordination model

Mahou follows the central supervisor/executor workflow:

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
- Smart Caps is local, optional and disabled on a clean configuration.
- Smart Caps directly corrected the focused Russian test cases in Microsoft Word and the exact classic Win32 `Edit` adapter without visible selection.
- All-caps, already-correct, lowercase, digit-containing, mixed-script, email and URL test inputs remained unchanged as required.
- Immediate Backspace restored the original casing. The focused smoke produced the expected session deltas of `+8` Mahou corrections and `+2` Mahou reversions.
- Two explicit rejections created a local personal exception, the third occurrence remained unchanged, and the exception persisted after restart.
- With Mahou closed, the tested words were not corrected by Mahou.
- Modern Notepad, Chrome, Telegram and the tested password field remained fail-closed while Mahou and Smart Caps were running: no text, selection, caret, layout, clipboard or counter change.
- Draft PR #2 remains open, Draft, and unmerged.

## Completed tasks

### AGZ-MAH-0001 — Smart Caps verification

Verified on 2026-07-25 at exact source commit `0b43bb688115e0051114f38a745fce9e830452fd` and source tree `bd0d80d350cea61cbdd2a9cecdfa6002f2c88ee9`.

Verification evidence:

- Modern Windows build run `30128168029`: passed;
- Security regression run `30128168156`: passed;
- immutable x64 artifact `Mahou-2.9.0.1-dev-win-x64-0b43bb6-run30128168029`;
- artifact ZIP SHA-256 `e344355bfd51ca5ebfa5f0b0dd24a06af94e02497a3ce39e18ef8a59511c3a6c`;
- manifest, SHA-256 inventory, executable identity, embedded commit and retained evidence independently checked;
- full focused real-Windows smoke confirmed by the user.

The verified scope is the exact implementation and scenarios above. It does not authorize merge, marking PR #2 Ready, tagging, releasing, signing or publication.

### AGZ-MAH-0004 — immutable artifact provenance gate

Completed and merged through PR #4 into `mixanizm-modern-v2.9.0.1`.

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

## Deferred product task

### AGZ-MAH-0003 — modern Notepad adapter

The direct Notepad/RichEdit adapter remains deferred. Unsupported controls must continue to fail closed. PR #3 is not an accepted implementation and must not be used as one.

## Important prohibitions

- Do not restore UI Automation `.Select()`, `Shift+Left`, `Ctrl+Shift+Left`, generated-selection collapse/reselect, stale round-trip, or tracked-buffer `ConvertLast` fallbacks.
- Do not change PR #1 or PR #3 without a separate explicit task and permission.
- Do not merge or mark Ready PR #2, create a tag or Release, or publish a user build without explicit permission.
- Do not extend the verified Smart Caps scope beyond the exact source commit, direct adapters and fail-closed scenarios recorded above without a new bounded task and applicable verification.
- Do not enable paid GitHub features, paid runners, or paid CI capacity without explicit permission.
- Do not hand off an artifact unless it passes `ARTIFACT-PROVENANCE.md` against the exact expected commit and tree.

## Next management step

The project supervisor should review and accept the documentation-only AGZ-MAH-0001 verification result, keep PR #2 Draft and unmerged, then choose exactly one next bounded task and prepare a separate executor handoff.

No temporary executor should start Notepad work, Insert changes, release work or another product task without that handoff.
