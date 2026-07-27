# Project state — MIXANIZM Mahou

Snapshot date: 2026-07-28

## Authoritative development line

- Repository: `MIXANIZM/Mahou`
- Legacy default branch: `master`
- Current development branch: `mixanizm-modern-v2.9.0.1`
- Main development pull request: Draft PR #2, from `mixanizm-modern-v2.9.0.1` into `master`
- Current development head at the start of `AGZ-MAH-0011`: `779b50dcc27cbe58f69ddadd54d526a0394663df`
- Runtime line: `2.9.0.1-dev`
- Agatzub Development Ruleset: `v2.7.0`
- Rules content commit: `ba60623aec67c57d46bda7ce2b291a823de2d4ea`
- Last verified Smart Caps source commit: `0b43bb688115e0051114f38a745fce9e830452fd`
- Last verified selected-text conversion source commit: `3418d09de20ea327302a26858a7b752862bd429e`
- Chrome desktop-only decision: `DIRECT-PATH-NOT-SAFE` under `AGZ-MAH-0006`
- Chrome browser-context editing-core result: accepted `BROWSER-CONTEXT-MUTATION-NOT-SAFE` under `AGZ-MAH-0007`, merged through PR #9
- Telegram Desktop direct-path result: accepted `BLOCKED` under `AGZ-MAH-0008`
- Input-surface capability probe: `AGZ-MAH-0009: USER_PROBE_EVIDENCE_COMPLETE`
- Input-surface evidence record: `AGZ-MAH-0010`, accepted and merged through PR #12 at `779b50dcc27cbe58f69ddadd54d526a0394663df`
- Modern Notepad direct-path feasibility: `AGZ-MAH-0011: DIRECT-PATH-NOT-SAFE`

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
- Keep it Draft. Do not merge or mark Ready without explicit permission and completion of required runtime and release checks.

### Draft PR #3

- Open and unmerged.
- Based on `mixanizm-modern-v2.9.0.1`.
- Its current diff is transport and workflow history from the attempted Notepad-adapter task, not an accepted or integrated Notepad implementation.
- It must not be used as a source branch, cleaned up, closed, rebased, or modified without a separate decision.

## Current bounded task result

### AGZ-MAH-0011 — Modern Notepad RichEdit write feasibility

- Exact base: `779b50dcc27cbe58f69ddadd54d526a0394663df`.
- Task branch: `agz-mah-0011-notepad-richedit-feasibility`.
- Scope: documented Windows accessibility/RichEdit interoperability feasibility and read-only installed-build evidence.
- Result: `DIRECT-PATH-NOT-SAFE`.
- Mahou runtime source and runtime version `2.9.0.1-dev` remain unchanged.
- No text mutation, executable harness, runtime adapter, candidate artifact, merge, release, or PR #3 work is included.

The installed Notepad `11.2605.34.0` x64 control was identified as `RichEditD2DPT`. UIA exposed only `TextPattern` and `ValuePattern`. A read-only `AccessibleObjectFromWindow(OBJID_NATIVEOM)` call returned an object supporting `ITextDocument`, proving that the installed build marshalled that object in this observation, but not that Microsoft supports this RichEdit acquisition route across versions.

Microsoft documents RichEdit TOM acquisition through `EM_GETOLEINTERFACE`, whose pointer-bearing message contract is not an acceptable cross-process path here. The generic `OBJID_NATIVEOM` mechanism does not document RichEdit as a supported provider. Consequently independent `ITextRange` replacement and one normal Notepad Undo unit cannot be accepted through that acquisition path. The full evidence and gate decision are in `docs/NOTEPAD-RICHEDIT-FEASIBILITY.md`.

## Completed evidence result

### AGZ-MAH-0009 — Input surface capability map

Status:

```text
USER_PROBE_EVIDENCE_COMPLETE
```

- The standalone one-shot Windows x64 read-only probe was merged into the development line through PR #11 at `f82a3b233d250f4bfb432327063c6832ab24ea5a`.
- The immutable user probe came from commit `cef38006dfe6093ee1b233703ad79215bb2a9758`.
- Probe-commit Actions passed: Security regression `30163258945`, Input surface probe `30163258946`, and Modern Windows build `30163258943`.
- Real-Windows evidence now confirms that readable UIA patterns vary both across applications and between distinct surfaces in the same application.
- Read support does not establish a safe exact-range write primitive.
- The only existing verified direct collapsed-caret adapters remain exact classic Win32 `Edit` and Microsoft Word document `Range`.
- Modern Notepad/RichEdit, Chrome, Telegram Qt, WhatsApp, Electron, WPF, WinUI and unknown controls remain strict no-op without a user-created selection.

Recommended research order:

1. modern RichEdit / Notepad;
2. Telegram Qt input;
3. Chromium Edit;
4. Chromium contenteditable;
5. Electron CodeMirror;
6. WhatsApp Desktop bridge remains blocked.

RichEdit is first because it may expose native range, caret and undo semantics. UIA `TextPattern`/`ValuePattern` alone is already insufficient for Chromium and Qt.

## Other task results

### AGZ-MAH-0007 — Chrome Manifest V3 editing-core prototype

- Result: accepted `BROWSER-CONTEXT-MUTATION-NOT-SAFE`.
- Accepted and merged through PR #9 into `mixanizm-modern-v2.9.0.1` at merge commit `076ee95325809bd0581c9e0f24e9dbb1022c6603`.
- `setRangeText()` does not provide the required trusted editing-event plus single browser undo/redo contract.
- `execCommand('insertText')` requires a temporary programmatic selection for exact replacement and was rejected.
- Whole-field assignment and synthetic events remain rejected.
- The retained prototype performs no mutation and does not add Chrome support.

### AGZ-MAH-0008 — Telegram Desktop direct path

- Result: accepted `BLOCKED`.
- No safe installed-build mutation contract was established.
- The real-Windows read-only probe now identifies the tested Telegram 7.0.5 composer as Qt `Ui::InputField::Inner` with UIA Text/Value patterns, but that evidence does not satisfy any write gate.
- Telegram remains strict no-op without a user-created selection.

## Completed verified checkpoints

### AGZ-MAH-0005 — selected-text conversion and clipboard verification

- Verified at source `3418d09de20ea327302a26858a7b752862bd429e`.
- The user confirmed the complete focused Windows selected-text and clipboard smoke.
- In browsers and messengers, no-selection `Insert` remains fail-closed.

### AGZ-MAH-0001 — Smart Caps verification

- Verified at source `0b43bb688115e0051114f38a745fce9e830452fd`.
- The user confirmed the complete focused Windows smoke, including strict no-op behavior in unsupported controls.

### AGZ-MAH-0004 — immutable artifact provenance gate

- Completed and merged through PR #4 into `mixanizm-modern-v2.9.0.1`.
- It changed provenance/build evidence only, not runtime behavior.

## Product task status

- `AGZ-MAH-0009` is `USER_PROBE_EVIDENCE_COMPLETE`.
- `AGZ-MAH-0010` was accepted and merged through PR #12 without changing Mahou runtime behavior.
- `AGZ-MAH-0011` records `DIRECT-PATH-NOT-SAFE`; modern Notepad remains strict no-op and no adapter was added.
- `AGZ-MAH-0008` remains accepted `BLOCKED` for Telegram mutation.
- `AGZ-MAH-0007` is accepted with result `BROWSER-CONTEXT-MUTATION-NOT-SAFE` and merged through PR #9; it does not add Chrome support.
- `AGZ-MAH-0003` is superseded by the negative `AGZ-MAH-0011` feasibility result. PR #3 is not an accepted implementation.

## Coordination model

- One active project supervisor chat maintains overall state, chooses the next bounded task, prepares executor handoffs, and accepts results.
- One temporary executor chat handles one bounded task and does not begin the next task.
- GitHub, `PROJECT_STATE.md`, `ISSUES.md`, and `UNIVERSAL-CHAT-HANDOFF.md` remain the recoverable source of project state.

## Release state

- No public release is approved from the current modernization line.
- No tag or GitHub Release is authorized by this state document.
- The input-surface reports are evidence, not a Mahou runtime artifact or release.
- Authenticode signing, final independent review, applicable CI, and remaining release-specific Windows checks remain release gates.
