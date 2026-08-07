# Project state — MIXANIZM Mahou

Snapshot date: 2026-08-07

## Authoritative development line

- Repository: `MIXANIZM/Mahou`
- Legacy default branch: `master`
- Current development branch: `mixanizm-modern-v2.9.0.1`
- Main development pull request: Draft PR #2, from `mixanizm-modern-v2.9.0.1` into `master`
- Current development head at the start of `AGZ-MAH-0011`: `779b50dcc27cbe58f69ddadd54d526a0394663df`
- Current development head at the start of `AGZ-MAH-0012`: `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1`
- Current development head at the start of `AGZ-MAH-0013`: `1a930137f7254111f8ef200da3e86c54e697ab5e`
- Current development head at the start of `AGZ-MAH-0015`: `363a83b227cfa14e798e442960640e7caed03913`
- Current development head at the start of `AGZ-MAH-0016`: `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`
- Current development head at the start of `AGZ-MAH-0019`: `f02909611eb9a4502e9fe4d8fda9009922f352fd`
- Draft PR #18 head at the start of `AGZ-MAH-0020`: `5527f662cb844ba90d264f5b93cb27f8334bf295`
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
- Modern Notepad direct-path feasibility: `AGZ-MAH-0011: DIRECT-PATH-NOT-SAFE`, accepted and merged through PR #13 at merge commit `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1`
- Qt Windows editable-text feasibility: `AGZ-MAH-0012: DIRECT-PATH-NOT-SAFE`, accepted and merged through PR #14 at `1a930137f7254111f8ef200da3e86c54e697ab5e`
- AutoSwitch historical failure artifact: `AGZ-MAH-0014: AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT`
- AutoSwitch modern Notepad containment: `AGZ-MAH-0015: AUTOSWITCH_MODERN_NOTEPAD_CONTAINMENT / VERIFIED / ACCEPTED / MERGED`
- User snippets runtime failure: `AGZ-MAH-0018: SNIPPETS_TRIGGER_REPLACEMENT_FAIL`
- Active product decision: `USER_SNIPPETS_REMOVED` and `AUTOSWITCH_DECOUPLED` under `AGZ-MAH-0019`
- Active startup remediation: `AGZ-MAH-0020: AUTOSWITCH_DICTIONARY_STARTUP_REMEDIATION`

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

### Draft PR #18

- Open, unmerged, and Draft.
- Base: `mixanizm-modern-v2.9.0.1` at exact task base `f02909611eb9a4502e9fe4d8fda9009922f352fd`.
- Head branch: `agz-mah-0019-remove-snippets-decouple-autoswitch`.
- Scope is limited to removal of user snippets, independent literal AutoSwitch routing, regression coverage, documentation, and an exact-head smoke candidate.
- The candidate at `5527f662cb844ba90d264f5b93cb27f8334bf295` is rejected as `USER_SMOKE_FAILED / STARTUP_HANG_HIGH_CPU`; AGZ-MAH-0020 remediates startup on the same branch and PR.
- Do not merge or mark Ready. Physical-Windows smoke is still required.

### PR #16

- Closed and merged into `mixanizm-modern-v2.9.0.1` at `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`.
- Contains the accepted `AGZ-MAH-0015` AutoSwitch containment implementation.
- Adds no modern Notepad AutoSwitch support; exact `notepad.exe` + `RichEditD2DPT` is a strict safe no-op.

### PR #14

- Closed and merged into `mixanizm-modern-v2.9.0.1` at `1a930137f7254111f8ef200da3e86c54e697ab5e`.
- Records the documentation-only Qt Windows editable-text feasibility result `DIRECT-PATH-NOT-SAFE`.
- Added no runtime adapter or candidate artifact.

## Current bounded task result

### AGZ-MAH-0020 — AutoSwitch dictionary startup remediation

- Exact starting head: `5527f662cb844ba90d264f5b93cb27f8334bf295`.
- Existing task branch and Draft PR remain `agz-mah-0019-remove-snippets-decouple-autoswitch` and #18.
- The rejected AGZ-MAH-0019 artifact remains immutable: build `30469006963`, x64 artifact `8730807263`, ZIP SHA-256 `4627c32bffe76ed3df71b8f35cfaa922783c31ee65770ea034207b78b2695214`, `Mahou.exe` SHA-256 `87d86a215f0ee57fa95567a2a74550b4e1190b495bb4d8127601fb24a1407abb`.
- Accepted failure: `USER_SMOKE_FAILED / STARTUP_HANG_HIGH_CPU`. The user must not be asked to run that candidate again.
- Exact bundled dictionary: 5,188,519 characters, 151,429 complete rules/aliases, 18 comment lines.
- The parser advances monotonically with bounded indexes, creates strings only for complete aliases/replacements, preserves order/duplicates/first-match behavior and LF/CRLF input, and publishes active arrays only after a complete successful parse.
- Configuration loading reads and parses the dictionary once, derives the displayed count from that result, suppresses programmatic `TextChanged` parsing, and clears active data when AutoSwitch is disabled without touching `AS_dict.txt`.
- Local x86 and x64 builds and executable regressions pass, including the actual bundled dictionary and a 150,000-rule synthetic dictionary. Local parser timings were under 100 ms; exact-head deterministic zero-warning CI remains pending because the local machine lacks the .NET Framework 4.8 reference pack.
- Runtime remains `2.9.0.1-dev`. No physical verification is claimed for the remediation.

### AGZ-MAH-0019 — remove user snippets and decouple AutoSwitch

- Exact base: `f02909611eb9a4502e9fe4d8fda9009922f352fd`.
- Task branch: `agz-mah-0019-remove-snippets-decouple-autoswitch`.
- Draft PR: #18.
- Runtime remains `2.9.0.1-dev`.
- Accepted input evidence: `AGZ-MAH-0018: SNIPPETS_TRIGGER_REPLACEMENT_FAIL` from build `30410253117` and x64 artifact `8708105691`.
- Product decisions: `USER_SNIPPETS_REMOVED` and `AUTOSWITCH_DECOUPLED`.
- User snippets UI, trigger collection, parser, expressions, hotkeys, exclusions, sounds, persistence, reload and save paths are removed from the active product.
- AutoSwitch uses a standalone source buffer and dictionary loader. Selected dictionary values are emitted literally by a dedicated AutoSwitch primitive; snippet expressions are unavailable.
- Legacy `snippets.txt`, `snippets.txt.bak` and obsolete INI values are neither read nor deleted. They remain inactive rollback data.
- Exact modern Notepad `notepad.exe` + `RichEditD2DPT` remains blocked before mutation, with source-context revalidation retained before each immediate and deferred mutation stage.
- The exact candidate at `5527f662cb844ba90d264f5b93cb27f8334bf295` failed physical Windows startup and is rejected as `USER_SMOKE_FAILED / STARTUP_HANG_HIGH_CPU`. AGZ-MAH-0020 supplies the replacement startup remediation; do not represent it as physically verified until replacement exact-head CI and the two-stage Windows smoke are accepted.

## Previous bounded task result

### AGZ-MAH-0015 — contain AutoSwitch in modern Notepad

- Exact base: `363a83b227cfa14e798e442960640e7caed03913`.
- Task branch: `agz-mah-0015-autoswitch-notepad-containment`.
- Task commit: `99e712aa3d913d37e1268ccf65a0cf52aca65ab7`.
- Implementation PR: #16.
- Merge commit: `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`.
- Result: `AUTOSWITCH_MODERN_NOTEPAD_CONTAINMENT / VERIFIED / ACCEPTED / MERGED`.
- Exact blocked surface: foreground process executable `notepad.exe`, focused control class `RichEditD2DPT`.
- AutoSwitch captures foreground, focused control, process, executable, and control class before routing. It rejects the blocked surface before dictionary matching can schedule deletion, layout switching, or replacement.
- Every AutoSwitch-only immediate or deferred mutation revalidates the same source context. Unknown, stale, protected classic Edit, or changed focus/control state fails closed.
- Exact-head CI succeeded: Security regression `30403716646`, Input surface probe `30403716647`, and Modern Windows build `30403716677`.
- Merge-head CI succeeded: Security regression `30408387819` and Modern Windows build `30408387854`.
- Accepted x64 artifact ID: `8705704874`.
- Candidate ZIP SHA-256: `85179e55a256dab80ddfb79496aa0e9e3fec202d7f828f9ba7714605ed0a5e9d`.
- Candidate `Mahou.exe` SHA-256: `908252105fb0566322d0c25a70364860fbfe904c847d96dbae1b33b288cd17b0`.
- The accepted physical-Windows smoke confirmed repeated modern Notepad `ghbdtn + space` remained unchanged, with no `gпривет`, deletion, partial replacement, deferred mutation after rapid window switching, or hidden AutoSwitch Undo operation. Chrome and Microsoft Word continued converting `ghbdtn` to `привет`.
- This is strict safe no-op containment, not positive modern Notepad AutoSwitch support.
- The containment does not add a Notepad adapter, did not change manual Insert, Smart Caps, the then-existing snippets feature, selected-text conversion, clipboard behavior, Chrome or Word routing, and does not use PR #3.
- Runtime version remains `2.9.0.1-dev`.

### AGZ-MAH-0014 — historical AutoSwitch defect artifact

- Result: `AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT`.
- Exact source: `363a83b227cfa14e798e442960640e7caed03913`.
- Modern Windows build: `30365352449`.
- x64 artifact ID: `8690549591`.
- ZIP SHA-256: `4ea545ddbec793f870d69b128cc11758cb61c5d7c388cf2330270aa9f8934a54`.
- The accepted historical smoke produced `gпривет`, complete deletion in another attempt, and split Undo in modern Notepad while Chrome and Word passed.
- This immutable artifact classification remains historical evidence and is not the current accepted containment behavior.

## Previous bounded task result

### AGZ-MAH-0013 — main PR release-readiness reconciliation

- Exact base: `1a930137f7254111f8ef200da3e86c54e697ab5e`.
- Task branch: `agz-mah-0013-main-pr-readiness`.
- Result: `RELEASE_READINESS_RECONCILED`.
- Scope: reconcile the actual development head, Draft PR #2 claims, accepted evidence, CI/provenance anchors, and separate merge/release gates.
- Records: `docs/RELEASE-READINESS.md` and `docs/PR2-DESCRIPTION-PROPOSAL.md`.
- Mahou runtime subtree and runtime version `2.9.0.1-dev` remain unchanged by that task.
- No workflow, runtime behavior, PR #2 metadata, PR #3, merge, signing, tag, Release, or publication change is included.

The reconciliation distinguishes verified, automated-only, user-smoke-required, not-applicable and blocked items. Existing accepted evidence remains anchored to its immutable source commits. Draft PR #2 must remain Draft until the merge gates in `docs/RELEASE-READINESS.md` are explicitly satisfied.

## Previous bounded task result

### AGZ-MAH-0012 — Qt Windows editable-text write feasibility

- Exact base: `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1`.
- Task branch: `agz-mah-0012-qt-edit-feasibility`.
- Scope: documented external Windows accessibility/COM write contracts for Qt 5.15.19 editable text, with Telegram Desktop 7.0.5 as the reference application.
- Result: `DIRECT-PATH-NOT-SAFE`.
- Status: accepted and merged through PR #14 at merge commit `1a930137f7254111f8ef200da3e86c54e697ab5e`.
- Mahou runtime source and runtime version `2.9.0.1-dev` remain unchanged.
- No text mutation, executable harness, runtime adapter, candidate artifact, merge, release, or PR #3 work is included.

Qt's Windows UI Automation provider exposes text through read/navigation/selection `TextPattern`/`TextPattern2` ranges and the whole control through `ValuePattern`. It does not project UIA `TextEditPattern`, UIA `ObjectModelPattern`, IAccessible2 editable text, or another documented external exact-range writer. Qt's editable-text operations are in-process C++ calls and are not marshalled by the Windows provider. The full decision is in `docs/QT-WINDOWS-EDIT-FEASIBILITY.md`.

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
- Modern Notepad/RichEdit, Chrome, Telegram Qt, WhatsApp, Electron, WPF, WinUI and unknown controls remain strict no-op without a user-created selection for the collapsed-caret direct path.

Historical research order:

1. modern RichEdit / Notepad;
2. Telegram Qt input;
3. Chromium Edit;
4. Chromium contenteditable;
5. Electron CodeMirror;
6. WhatsApp Desktop bridge remains blocked.

RichEdit was first because it might expose native range, caret and undo semantics. `AGZ-MAH-0011` and `AGZ-MAH-0012` rejected the documented external direct paths for modern Notepad and Qt.

## Other task results

### AGZ-MAH-0007 — Chrome browser-context editing core

- Result: accepted `BROWSER-CONTEXT-MUTATION-NOT-SAFE`.
- Accepted and merged through PR #9 into `mixanizm-modern-v2.9.0.1` at merge commit `076ee95325809bd0581c9e0f24e9dbb1022c6603`.
- `setRangeText()` does not provide the required trusted editing-event plus single browser undo/redo contract.
- `execCommand('insertText')` requires a temporary programmatic selection for exact replacement and was rejected.
- Whole-field assignment and synthetic events remain rejected.
- The retained prototype performs no mutation and does not add Chrome support.

### AGZ-MAH-0008 — Telegram Desktop direct path

- Result: accepted `BLOCKED`.
- No safe installed-build mutation contract was established.
- The real-Windows read-only probe identifies the tested Telegram 7.0.5 composer as Qt `Ui::InputField::Inner` with UIA Text/Value patterns, but that evidence does not satisfy any write gate.
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

- `AGZ-MAH-0014` remains recorded as `AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT` for its immutable source and candidate.
- `AGZ-MAH-0018` is recorded as `SNIPPETS_TRIGGER_REPLACEMENT_FAIL`; its remaining snippets smoke was stopped.
- `AGZ-MAH-0019` is the active Draft PR #18 implementation of `USER_SNIPPETS_REMOVED` and `AUTOSWITCH_DECOUPLED`; exact-head CI and user smoke remain pending.
- `AGZ-MAH-0015` is `AUTOSWITCH_MODERN_NOTEPAD_CONTAINMENT / VERIFIED / ACCEPTED / MERGED` through PR #16 at `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`; modern Notepad behavior is strict safe no-op, not support.
- `AGZ-MAH-0009` is `USER_PROBE_EVIDENCE_COMPLETE`.
- `AGZ-MAH-0010` was accepted and merged through PR #12 without changing Mahou runtime behavior.
- `AGZ-MAH-0011` is accepted and merged through PR #13 at merge commit `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1` with result `DIRECT-PATH-NOT-SAFE`; modern Notepad has no direct adapter.
- `AGZ-MAH-0012` is accepted and merged through PR #14 at merge commit `1a930137f7254111f8ef200da3e86c54e697ab5e` with result `DIRECT-PATH-NOT-SAFE`; Telegram and all other Qt/custom surfaces remain strict no-op and no adapter was added.
- `AGZ-MAH-0013` records `RELEASE_READINESS_RECONCILED`; it changes documentation only and does not authorize merge or release.
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
- `docs/RELEASE-READINESS.md` is the current gate matrix; `docs/PR2-DESCRIPTION-PROPOSAL.md` is a proposal only and does not alter PR #2.
- AutoSwitch modern Notepad containment smoke is complete and is no longer a pending Draft PR #2 merge gate.
- User snippets are removed and no snippets smoke remains. Remaining merge gates include the focused AGZ-MAH-0019 AutoSwitch/removal smoke, the other retained-feature Windows smoke, green final-head CI, an accurate PR #2 description, supervisor review, and explicit Ready/merge permission.
- Public-release gates additionally include exact-candidate provenance, signing, final independent review, release-specific Windows checks, and separate tag/Release/publication permission.
