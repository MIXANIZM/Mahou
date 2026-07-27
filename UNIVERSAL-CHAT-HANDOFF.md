# Universal chat handoff — MIXANIZM Mahou

## Project

- Repository: `MIXANIZM/Mahou`
- Legacy default branch: `master`
- Current development branch: `mixanizm-modern-v2.9.0.1`
- Main development pull request: Draft PR #2
- Runtime line: `2.9.0.1-dev`
- Agatzub Development Ruleset: `v2.7.0`
- Rules content commit: `ba60623aec67c57d46bda7ce2b291a823de2d4ea`
- Current development head at the `AGZ-MAH-0010` start: `f82a3b233d250f4bfb432327063c6832ab24ea5a`
- Last user-verified Insert safety checkpoint: `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`
- Verified Smart Caps source: `0b43bb688115e0051114f38a745fce9e830452fd`
- Verified selected-text conversion source: `3418d09de20ea327302a26858a7b752862bd429e`

The old `master` branch is not the working line for modernized Mahou. Read `PROJECT_STATE.md` and `ISSUES.md` before choosing or assigning work.

## Coordination model

Mahou follows the central supervisor/executor workflow:

- one active project supervisor chat maintains overall state, chooses the next bounded task, prepares executor handoffs, and accepts or rejects results;
- one temporary executor chat performs one bounded task, reports back, and does not begin the next task;
- GitHub, `PROJECT_STATE.md`, `ISSUES.md`, and this handoff are the recoverable source of project state.

## Project goal

Modernize and harden Mahou while preserving useful layout-switching behavior and making every automatic text mutation fail closed.

## Verified current behavior

- Existing user-created selection has priority over collapsed-caret word conversion.
- Selected text converts forward and backward in verified applications and preserves tested OLE clipboard formats.
- Protected/password fields suppress conversion.
- Collapsed-caret `Insert` never creates synthetic blue selection.
- Microsoft Word document `Range` and exact classic Win32 `Edit` remain the only supported direct collapsed-caret adapters.
- Modern Notepad/RichEdit, Chrome, Telegram, Discord, WhatsApp, WPF, WinUI/UWP, Electron/WebView, Qt/custom, unknown controls, protected fields, and failed probes remain no-op without a user-created selection.
- Smart Caps is local, optional, disabled on a clean profile, and verified only for its exact recorded source/artifact/scenarios.
- Draft PR #2 remains open, Draft, and unmerged.

## Current bounded task

### AGZ-MAH-0010 — Record Windows input-surface evidence

Starting point:

```text
Base branch: mixanizm-modern-v2.9.0.1
Exact base: f82a3b233d250f4bfb432327063c6832ab24ea5a
Task branch: agz-mah-0010-user-probe-evidence
Probe commit: cef38006dfe6093ee1b233703ad79215bb2a9758
```

Scope and result:

- documentation and sanitized read-only evidence only;
- no Mahou runtime source or runtime version change;
- no mutation experiment, adapter, candidate artifact, signing, merge, release, or PR #3 work;
- `AGZ-MAH-0009: USER_PROBE_EVIDENCE_COMPLETE`.

Recorded user evidence:

| Surface | Normalized result |
| --- | --- |
| Microsoft Word | `WORD_OBJECT_MODEL` |
| Modern Notepad | `RICHEDIT`, focused class `RichEditD2DPT` |
| AnyDesk classic field | `CLASSIC_WIN32_EDIT`, exact class `Edit` |
| Chrome input/textarea | identical `CHROMIUM_BROWSER / Edit / TextPattern + ValuePattern` |
| Chrome contenteditable | separate `CHROMIUM_BROWSER / Group / TextPattern` |
| Telegram Desktop 7.0.5 | `QT_CUSTOM / Ui::InputField::Inner / TextPattern + ValuePattern` |
| WhatsApp Web in Opera | Chromium Edit family |
| WhatsApp Desktop | `CUSTOM_UNKNOWN / DesktopChildSiteBridge`; internal editor not reached |
| Obsidian title | Electron Group/TextPattern |
| Obsidian CodeMirror body | Electron Edit/TextPattern + ValuePattern |

Eight reviewed sanitized JSON reports are stored in `docs/evidence/input-surface-probe/`. They contain no actual text, titles, URLs, usernames, clipboard content, or personal paths. Raw JSON for Word, modern Notepad, and AnyDesk was not available in the supplied files; only the user-confirmed normalized evidence is recorded for those surfaces.

## Evidence interpretation

- Read capability does not imply safe write capability.
- Chrome input/textarea and contenteditable are distinct Chromium surfaces.
- Obsidian title and CodeMirror body are distinct Electron surfaces.
- WhatsApp Desktop remains blocked because the probe reached only `Microsoft.UI.Content.DesktopChildSiteBridge`, not an internal editor.
- UIA `TextPattern` and `ValuePattern` do not provide the accepted exact-range mutation, application undo, event, composition, stale-state and post-state contract.
- The only verified direct write adapters remain Word document `Range` and exact classic Win32 `Edit`.

## Recommended research order

1. modern RichEdit / Notepad;
2. Telegram Qt input;
3. Chromium Edit;
4. Chromium contenteditable;
5. Electron CodeMirror;
6. WhatsApp Desktop bridge remains blocked.

RichEdit is first because it may expose native range, caret and undo semantics suitable for a bounded application-specific adapter. Chromium and Qt already demonstrate that UIA TextPattern/ValuePattern alone is insufficient.

## Probe verification history

The immutable probe source commit `cef38006dfe6093ee1b233703ad79215bb2a9758` passed:

- Security regression run `30163258945`;
- Input surface probe run `30163258946`;
- Modern Windows build run `30163258943`.

The probe performs one capture, collects only redacted capability metadata, and exits. It contains no writable text methods, programmatic selection, keyboard simulation, clipboard APIs, hooks, continuous monitoring, process-memory writes or injection.

Primary records:

```text
docs/INPUT-SURFACE-CAPABILITY-MAP.md
docs/INPUT-SURFACE-PROBE.md
docs/evidence/input-surface-probe/
```

## Other project results

### AGZ-MAH-0007 — Chrome browser-context editing core

Result:

```text
BROWSER-CONTEXT-MUTATION-NOT-SAFE
```

The retained test-only MV3 prototype performs no mutation. `setRangeText()` did not satisfy trusted events plus one browser undo transaction; `execCommand('insertText')` required forbidden temporary programmatic selection; whole-field assignment and synthetic events remain rejected. Native Messaging was not started.

### AGZ-MAH-0008 — Telegram Desktop direct path

Result:

```text
BLOCKED — ACCEPTED
```

The new read-only Telegram 7.0.5 evidence identifies the focused Qt composer surface but does not establish safe writing. Telegram remains strict no-op without a user-created selection.

### AGZ-MAH-0005 — selected-text and clipboard verification

Verified at source `3418d09de20ea327302a26858a7b752862bd429e`; the user confirmed the complete focused Windows smoke.

### AGZ-MAH-0001 — Smart Caps verification

Verified at source `0b43bb688115e0051114f38a745fce9e830452fd`; the user confirmed the complete focused Windows smoke.

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

- Do not restore UI Automation `.Select()`, keyboard selection, generated-selection fallback, Backspace/retype, clipboard mutation, or whole-field rewrite.
- Do not turn any probe classification, fingerprint, read capability, process name, framework ID, UIA pattern or interface presence into a mutation allow-list.
- Do not treat Qt ancestry, Telegram process name, UIA read access, caret access or ValuePattern presence as proof of a safe Telegram mutation path.
- Do not combine Chromium input/textarea and contenteditable into one inferred editor contract.
- Do not combine Obsidian title and CodeMirror body into one inferred editor contract.
- Do not start mutation work from the WhatsApp Desktop bridge evidence; the internal editor was not reached.
- Do not change PR #1 or PR #3 without a separate explicit task.
- Do not merge or mark Ready PR #2 or the AGZ-MAH-0010 task PR, create a tag or Release, or publish a Mahou user build without explicit permission.

## Next management step

The project supervisor should inspect the `AGZ-MAH-0010` Draft PR, confirm that only documentation and sanitized evidence changed, and review the applicable Security regression and Modern Windows build results. After acceptance, the supervisor—not this executor—may choose whether to open a separate research task in the documented priority order.
