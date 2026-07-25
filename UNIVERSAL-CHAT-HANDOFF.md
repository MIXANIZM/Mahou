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
- Verified selected-text conversion source commit: `3418d09de20ea327302a26858a7b752862bd429e`
- Chrome desktop-only decision: `DIRECT-PATH-NOT-SAFE` under `AGZ-MAH-0006`
- Chrome browser-context editing-core result awaiting supervisor decision: `BROWSER-CONTEXT-MUTATION-NOT-SAFE` under `AGZ-MAH-0007`

The old `master` branch is not the working line for modernized Mahou. Read `PROJECT_STATE.md` before choosing or assigning work.

## Coordination model

Mahou follows the central supervisor/executor workflow:

- one active project supervisor chat maintains the overall state, chooses the next bounded task, prepares an executor handoff, and accepts or rejects the result;
- one temporary executor chat performs one bounded task, reports back, and does not begin the next task;
- GitHub, `PROJECT_STATE.md`, `ISSUES.md`, and this handoff are the recoverable source of project state.

## Project goal

Modernize and harden Mahou while preserving useful layout-switching behavior and making every automatic text mutation fail closed.

## Verified current behavior

- Existing user-created selection has priority over collapsed-caret word conversion.
- Selected text converts forward and backward in the verified applications and preserves the tested OLE clipboard formats.
- Protected/password fields suppress conversion.
- Collapsed-caret `Insert` never creates synthetic blue selection.
- Microsoft Word and the exact classic Win32 `Edit` class remain the only supported direct collapsed-caret adapters.
- Modern Notepad/RichEdit, Chrome, Telegram, Discord, unknown controls, protected fields, and failed probes remain no-op without a user-created selection.
- Smart Caps is local, optional, disabled on a clean profile, and verified only for its exact recorded source/artifact/scenarios.
- Draft PR #2 remains open, Draft, and unmerged.

## Result awaiting supervisor decision

### AGZ-MAH-0007 — Chrome Manifest V3 editing-core prototype

Starting point:

```text
faaf170d12e5bbcbd49c0b66edf4bac75c1e3049
```

Result:

```text
BROWSER-CONTEXT-MUTATION-NOT-SAFE
```

Evidence and boundary:

- created a minimal test-only MV3 prototype under `integrations/chrome-smart-caps/prototype-extension/`;
- permissions are exactly `activeTab` and `scripting`, with explicit toolbar-action activation and no host permissions;
- the content path is restricted by exact marker `AGZ-MAH-0007-DIAGNOSTIC-V1`, main frame, active document/tab, and focused exact control;
- fixed candidates are only `окоРОчка -> окорочка`, `ПРИвет -> Привет`, and `КуРиные -> Куриные`;
- strict rejection covers password, contenteditable, readonly, disabled, hidden, detached, unknown controls, non-collapsed selection, composition, invalid/duplicate/expired request, wrong frame/page/tab, stale document/element/value/caret/focus, invalid boundary, and changed neighbors;
- `setRangeText()` was rejected because its documented contract does not provide the required normal trusted edit-event plus one browser undo/redo transaction;
- deprecated `execCommand('insertText')` was rejected because exact replacement requires a temporary programmatic selection and its event behavior is browser/configuration dependent;
- full `.value` assignment and synthetic events remain forbidden;
- final code performs no mutation and reports `mutation-api-not-accepted`;
- static Python and dependency-free Node tests pass locally;
- a dedicated read-only PR workflow runs those checks;
- the available Chromium was `144.0.7559.96`, but managed policy blocked extension installation and all URLs, so no real unpacked-extension smoke is claimed and no forbidden workaround was used;
- no Native Messaging, Mahou runtime code, version, counters, Backspace reversal, personal exceptions, Telegram, Notepad, release, or publication work was performed.

Primary decision document: `docs/CHROME-EXTENSION-EDITING-CORE.md`.

## Completed tasks

### AGZ-MAH-0006 — Chrome desktop-only architecture decision

Result `DIRECT-PATH-NOT-SAFE`: UI Automation exposes no acceptable exact replace-range operation and whole-field `ValuePattern.SetValue` was rejected. Chrome remained strict no-op.

### AGZ-MAH-0005 — selected-text and clipboard verification

Verified at source `3418d09de20ea327302a26858a7b752862bd429e`; Modern Windows build `30134239498` and Security regression `30134239469` passed; the user confirmed the complete focused Windows smoke.

### AGZ-MAH-0001 — Smart Caps verification

Verified at source `0b43bb688115e0051114f38a745fce9e830452fd`; Modern Windows build `30128168029` and Security regression `30128168156` passed; the user confirmed the complete focused Windows smoke.

### AGZ-MAH-0004 — immutable artifact provenance gate

Merged through PR #4 into `mixanizm-modern-v2.9.0.1`. It changed provenance/build evidence only, not runtime behavior.

## Open pull requests

### Draft PR #1

Open and unmerged against `master`. It belongs to an earlier stabilization line. Do not modify, close, rebase, or retarget it without a separate decision.

### Draft PR #2

Open and unmerged from `mixanizm-modern-v2.9.0.1` into `master`. Keep it Draft; do not merge, mark Ready, tag, release, or publish without explicit permission and applicable checks.

### Draft PR #3

Open and unmerged against `mixanizm-modern-v2.9.0.1`. Its current diff is preserved transport/workflow history, not an accepted Notepad adapter. Do not use or modify it without a separate decision.

## Important prohibitions

- Do not restore UI Automation `.Select()`, keyboard selection, generated-selection fallback, Backspace/retype, clipboard mutation, or whole-field Chrome rewrite.
- Do not activate the AGZ-MAH-0007 prototype mutation: the retained code must remain strict no-op unless a future separately authorized architecture supplies a method that passes every gate.
- Do not start Native Messaging, Mahou integration, Telegram, Notepad, Edge, local server, CDP, remote debugging, DLL injection, Chrome Web Store, signing, or release work from this result.
- Do not change PR #1 or PR #3 without a separate explicit task.
- Do not merge or mark Ready PR #2, create a tag or Release, or publish a user build without explicit permission.
- Do not hand off an artifact unless it passes `ARTIFACT-PROVENANCE.md` against the exact expected commit and tree.

## Next management step

The project supervisor should inspect the AGZ-MAH-0007 Draft PR, confirm its exact head and CI, and accept or reject the negative architecture result. The executor must not begin another task.
