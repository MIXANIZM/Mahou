# Universal chat handoff — MIXANIZM Mahou

## Project

- Repository: `MIXANIZM/Mahou`
- Legacy default branch: `master`
- Current development branch: `mixanizm-modern-v2.9.0.1`
- Main development pull request: Draft PR #2
- Runtime line: `2.9.0.1-dev`
- Agatzub Development Ruleset: `v2.7.0`
- Rules content commit: `ba60623aec67c57d46bda7ce2b291a823de2d4ea`
- Current development head at the `AGZ-MAH-0011` start: `779b50dcc27cbe58f69ddadd54d526a0394663df`
- Current development head at the `AGZ-MAH-0012` start: `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1`
- Current development head at the `AGZ-MAH-0013` start: `1a930137f7254111f8ef200da3e86c54e697ab5e`
- Current development head at the `AGZ-MAH-0015` start: `363a83b227cfa14e798e442960640e7caed03913`
- Current development head at the `AGZ-MAH-0016` start: `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`
- Draft PR #18 head at the `AGZ-MAH-0020` start: `5527f662cb844ba90d264f5b93cb27f8334bf295`
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
- Collapsed-caret Insert and Smart Caps remain no-op in modern Notepad/RichEdit, Chrome, Telegram, Discord, WhatsApp, WPF, WinUI/UWP, Electron/WebView, Qt/custom, unknown controls, protected fields, and failed probes without a supported direct adapter.
- `AGZ-MAH-0014` records the immutable historical defect result `AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT`: exact source `363a83b227cfa14e798e442960640e7caed03913` destructively produced `gпривет`, could delete the token, and split Undo in modern Notepad.
- `AGZ-MAH-0015` is `AUTOSWITCH_MODERN_NOTEPAD_CONTAINMENT / VERIFIED / ACCEPTED / MERGED`: exact `notepad.exe` + `RichEditD2DPT` now fails closed before AutoSwitch mutation while Chrome and Word keep their existing conversion routes.
- The accepted modern Notepad AutoSwitch behavior is strict safe no-op. No positive modern Notepad AutoSwitch support or direct adapter was added.
- Smart Caps is local, optional, disabled on a clean profile, and verified only for its exact recorded source/artifact/scenarios.
- Draft PR #2 remains open, Draft, and unmerged.

## Current bounded task

## AGZ-MAH-0020 — AutoSwitch dictionary startup remediation

Repository: `MIXANIZM/Mahou`
Existing branch: `agz-mah-0019-remove-snippets-decouple-autoswitch`
Existing Draft PR: #18
Exact starting head: `5527f662cb844ba90d264f5b93cb27f8334bf295`
Runtime: `2.9.0.1-dev`

The exact AGZ-MAH-0019 candidate is rejected and immutable:

```text
AGZ-MAH-0019: USER_SMOKE_FAILED / STARTUP_HANG_HIGH_CPU
build: 30469006963
x64 artifact: 8730807263
ZIP SHA-256: 4627c32bffe76ed3df71b8f35cfaa922783c31ee65770ea034207b78b2695214
Mahou.exe SHA-256: 87d86a215f0ee57fa95567a2a74550b4e1190b495bb4d8127601fb24a1407abb
```

Do not ask the user to run that candidate again. It hung during startup with sustained CPU before the UI/tray became usable.

The bundled dictionary has exactly 5,188,519 characters, 151,429 complete rules/aliases and 18 comment lines. AGZ-MAH-0020 replaces the suffix-copy parser with a monotonic bounded-index parser, publishes no partial data on malformed input, derives the UI count from the same parse, suppresses programmatic `TextChanged` parsing during configuration load, and clears active data without touching `AS_dict.txt` when disabled.

`AutoSwitchDictionaryStartupRegression` covers the real dictionary, exact first/middle/final mappings, aliases, comments, duplicates/order, LF/CRLF, malformed trailing input, literal snippet-like values, repeated parses, disabled clearing, a 150,000-rule multi-megabyte dictionary, 12-second x86/x64 bounds, and exactly one parser invocation through the configuration reload path. Local x86 and x64 executable regressions pass below 100 ms for both large inputs. Exact-head deterministic zero-warning CI and a replacement physical-Windows two-stage smoke remain pending. No physical verification is claimed.

The remediation does not restore snippets, alter AutoSwitch mutation semantics, weaken source-context revalidation, expand modern Notepad support, change Chrome/Word routing, modify legacy snippet files, or change the runtime version.

## Parent task: AGZ-MAH-0019 — remove user snippets and decouple AutoSwitch

Repository: `MIXANIZM/Mahou`  
Exact base: `f02909611eb9a4502e9fe4d8fda9009922f352fd`  
Task branch: `agz-mah-0019-remove-snippets-decouple-autoswitch`  
Draft PR: #18  
Runtime: `2.9.0.1-dev`

Accepted predecessor evidence:

```text
AGZ-MAH-0018: SNIPPETS_TRIGGER_REPLACEMENT_FAIL
build: 30410253117
x64 artifact: 8708105691
```

The simple snippet replacement was corrupted, multiline left `agz1`, and delayed replacement left `agz`. The product decision is not to repair snippets:

```text
USER_SNIPPETS_REMOVED
AUTOSWITCH_DECOUPLED
```

The active product has no snippets tab, enable control, editor, trigger parser, expressions, snippet hotkeys, snippet-only exclusions, snippet sounds, persistence, reload, or `snippets.txt` runtime path. Existing `snippets.txt`, `snippets.txt.bak`, and old INI values remain untouched inactive legacy data for rollback compatibility.

AutoSwitch is routed outside any legacy snippets condition. It uses its own source buffer, only `AS_dict.txt`, and a dedicated literal-input primitive. Values such as `__delay`, `__execute`, `__keyboard`, `__paste`, `__selection`, and `__setlayout` are plain text and have no command meaning. Source identity is revalidated before deletion, layout switching, literal insertion, trailing-space insertion, and deferred callbacks. Exact `notepad.exe` + `RichEditD2DPT` remains a strict no-op.

The first candidate failed startup and is rejected. After AGZ-MAH-0020 exact-head CI succeeds, replacement smoke must run in two stages: startup/usability/CPU/exit first, and only after that passes, absent snippets UI/data generation, inactive preserved legacy file, Chrome/Word positive AutoSwitch, modern Notepad no-op, rapid-focus cancellation and restart persistence. Do not ask the user to test removed snippets.

### AGZ-MAH-0016 — record verified AutoSwitch containment smoke

Starting point:

```text
Base branch: mixanizm-modern-v2.9.0.1
Exact base: a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d
Task branch: docs/AGZ-MAH-0016-record-autoswitch-containment
Result: AUTOSWITCH_CONTAINMENT_VERIFICATION_RECORDED
```

Scope and result:

- record the accepted exact-head CI, merge-head CI, candidate hashes, and focused physical-Windows smoke from `AGZ-MAH-0015`;
- keep `AGZ-MAH-0014` as the immutable historical defect artifact for its exact source and candidate;
- mark `AGZ-MAH-0015` as `VERIFIED / ACCEPTED / MERGED` through PR #16 at `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`;
- remove AutoSwitch containment from the remaining Draft PR #2 smoke gates while leaving the other retained-feature tests pending;
- update only applicable documentation and the PR #2 body proposal;
- make no runtime, workflow, version, PR #1, PR #2 metadata, or PR #3 change.

This task does not authorize Ready, merge, signing, tag, Release, or publication.

## Previous bounded task

### AGZ-MAH-0015 — contain AutoSwitch in modern Notepad

```text
Exact base: 363a83b227cfa14e798e442960640e7caed03913
Task branch: agz-mah-0015-autoswitch-notepad-containment
Task commit: 99e712aa3d913d37e1268ccf65a0cf52aca65ab7
Implementation PR: #16
Merge commit: a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d
Result: AUTOSWITCH_MODERN_NOTEPAD_CONTAINMENT / VERIFIED / ACCEPTED / MERGED
```

The AutoSwitch pipeline is a keyboard replay path, not an exact-range adapter. The containment captures foreground HWND, focused-control HWND, process ID, executable name, control class, and protected classic-Edit state before dictionary routing. It rejects exact `notepad.exe` + `RichEditD2DPT` before any deletion, layout switch, replacement, or deferred callback is scheduled. Every immediate or deferred AutoSwitch mutation revalidates the same source context; unknown, protected, stale, or changed identity fails closed.

Accepted automated evidence:

- exact-head Security regression `30403716646` — success;
- exact-head Input surface probe `30403716647` — success;
- exact-head Modern Windows build `30403716677` — success;
- merge-head Security regression `30408387819` — success;
- merge-head Modern Windows build `30408387854` — success.

Accepted x64 candidate:

- artifact ID `8705704874`;
- ZIP SHA-256 `85179e55a256dab80ddfb79496aa0e9e3fec202d7f828f9ba7714605ed0a5e9d`;
- `Mahou.exe` SHA-256 `908252105fb0566322d0c25a70364860fbfe904c847d96dbae1b33b288cd17b0`.

Accepted physical-Windows smoke:

- repeated modern Notepad `ghbdtn + space` remained unchanged;
- no `gпривет`, deletion, or partial replacement occurred;
- rapid window switching produced no deferred mutation;
- Undo contained no hidden AutoSwitch operation;
- Chrome continued converting `ghbdtn` to `привет`;
- Microsoft Word continued converting `ghbdtn` to `привет`.

The result is containment, not support. It adds no Notepad adapter, did not change manual Insert, Smart Caps, the then-existing snippets feature, selected-text conversion, clipboard behavior, Chrome or Word routing, and does not use PR #3. Runtime version remains `2.9.0.1-dev`.

## Historical defect evidence

### AGZ-MAH-0014 — AutoSwitch modern Notepad failure

```text
Result: AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT
Exact source: 363a83b227cfa14e798e442960640e7caed03913
Modern Windows build: 30365352449
x64 artifact ID: 8690549591
ZIP SHA-256: 4ea545ddbec793f870d69b128cc11758cb61c5d7c388cf2330270aa9f8934a54
```

The accepted historical smoke produced `gпривет`, complete deletion on another attempt, and split Undo in modern Notepad while Chrome and Word passed. This result remains attached to that exact artifact and is not relabelled by the later containment.

## Previous bounded task

### AGZ-MAH-0013 — main PR release-readiness reconciliation

```text
Base branch: mixanizm-modern-v2.9.0.1
Exact base: 1a930137f7254111f8ef200da3e86c54e697ab5e
Task branch: agz-mah-0013-main-pr-readiness
Result: RELEASE_READINESS_RECONCILED
```

Scope and result:

- reconciles Draft PR #2 against the accepted evidence and current CI/provenance;
- records verified, automated-only, user-smoke-required, not-applicable and blocked readiness states in `docs/RELEASE-READINESS.md`;
- supplies a complete replacement body proposal in `docs/PR2-DESCRIPTION-PROPOSAL.md` without editing PR #2 metadata;
- keeps the exact supported direct adapters limited to Microsoft Word document `Range` and exact classic Win32 `Edit`;
- changes no Mahou runtime source/version, workflow, PR #2 metadata, PR #3 state, tag, Release or publication state.

Draft PR #2 remains Draft. Its merge gates and the additional public-release gates are separate and are authoritative in `docs/RELEASE-READINESS.md`.

## Previous bounded task

### AGZ-MAH-0012 — Qt Windows editable-text write feasibility

```text
Base branch: mixanizm-modern-v2.9.0.1
Exact base: d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1
Task branch: agz-mah-0012-qt-edit-feasibility
Decision: DIRECT-PATH-NOT-SAFE
Status: ACCEPTED / MERGED through PR #14
Merge commit: 1a930137f7254111f8ef200da3e86c54e697ab5e
```

Qt exposes external UIA Text/Text2 read/navigation/selection ranges and a whole-field Value provider, but no documented external exact-range writer. Qt's internal editable-text operations are in-process C++ calls and are not projected through UIA, MSAA or IAccessible2. No Mahou runtime source/version change, mutation, harness, adapter or candidate artifact was added.

## Recorded input-surface evidence

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

Eight reviewed sanitized JSON reports are stored in `docs/evidence/input-surface-probe/`. They contain no actual text, titles, URLs, usernames, clipboard content, or personal paths. Raw JSON for Word, modern Notepad, and AnyDesk was not available; only user-confirmed normalized evidence is recorded for those surfaces.

## Evidence interpretation

- Read capability does not imply safe write capability.
- Chrome input/textarea and contenteditable are distinct Chromium surfaces.
- Obsidian title and CodeMirror body are distinct Electron surfaces.
- WhatsApp Desktop remains blocked because the probe reached only `Microsoft.UI.Content.DesktopChildSiteBridge`, not an internal editor.
- UIA `TextPattern` and `ValuePattern` do not provide the accepted exact-range mutation, application undo, event, composition, stale-state and post-state contract.
- `AGZ-MAH-0011` found no documented RichEdit-specific external `OBJID_NATIVEOM` contract; observed COM availability on one Notepad build must not become a mutation allow-list.
- The only verified direct write adapters remain Word document `Range` and exact classic Win32 `Edit`.

## Other project results

### AGZ-MAH-0007 — Chrome browser-context editing core

Result: `BROWSER-CONTEXT-MUTATION-NOT-SAFE`.

The retained test-only MV3 prototype performs no mutation. `setRangeText()` did not satisfy trusted events plus one browser undo transaction; `execCommand('insertText')` required forbidden temporary programmatic selection; whole-field assignment and synthetic events remain rejected. Native Messaging was not started.

### AGZ-MAH-0010 — Input-surface evidence record

Accepted and merged through PR #12 into `mixanizm-modern-v2.9.0.1` at merge commit `779b50dcc27cbe58f69ddadd54d526a0394663df`. It changed documentation and sanitized evidence only.

### AGZ-MAH-0008 — Telegram Desktop direct path

Result: `BLOCKED — ACCEPTED`.

The read-only Telegram 7.0.5 evidence identifies the focused Qt composer surface but does not establish safe writing. Telegram remains strict no-op without a user-created selection.

### AGZ-MAH-0005 — selected-text and clipboard verification

Verified at source `3418d09de20ea327302a26858a7b752862bd429e`; the user confirmed the complete focused Windows smoke.

### AGZ-MAH-0001 — Smart Caps verification

Verified at source `0b43bb688115e0051114f38a745fce9e830452fd`; the user confirmed the complete focused Windows smoke.

### AGZ-MAH-0004 — immutable artifact provenance gate

Merged through PR #4 into `mixanizm-modern-v2.9.0.1`. It changed provenance/build evidence only, not runtime behavior.

## Pull request state

### Draft PR #1

Open and unmerged against `master`. It belongs to an earlier stabilization line. Do not modify, close, rebase, or retarget it without a separate decision.

### Draft PR #2

Open and unmerged from `mixanizm-modern-v2.9.0.1` into `master`. Keep it Draft; do not merge, mark Ready, tag, release, or publish without explicit permission and applicable checks.

### Draft PR #3

Open and unmerged against `mixanizm-modern-v2.9.0.1`. Its current diff is preserved transport/workflow history, not an accepted Notepad adapter. Do not use or modify it without a separate decision.

### PR #16

Closed and merged into `mixanizm-modern-v2.9.0.1` at `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`. It contains the accepted AutoSwitch containment and adds no positive modern Notepad support.

### PR #14

Closed and merged into `mixanizm-modern-v2.9.0.1` at `1a930137f7254111f8ef200da3e86c54e697ab5e`. It records the documentation-only Qt result `DIRECT-PATH-NOT-SAFE` and adds no runtime adapter or candidate artifact.

## Important prohibitions

- Do not restore UI Automation `.Select()`, keyboard selection, generated-selection fallback, Backspace/retype fallback, clipboard mutation, or whole-field rewrite to collapsed-caret direct paths.
- Do not turn any probe classification, fingerprint, read capability, process name, framework ID, UIA pattern or interface presence into a mutation allow-list.
- Do not represent AutoSwitch containment in modern Notepad as positive support; the accepted result is strict safe no-op.
- Do not treat Qt ancestry, Telegram process name, UIA read access, caret access or ValuePattern presence as proof of a safe Telegram mutation path.
- Do not combine Chromium input/textarea and contenteditable into one inferred editor contract.
- Do not combine Obsidian title and CodeMirror body into one inferred editor contract.
- Do not start mutation work from the WhatsApp Desktop bridge evidence; the internal editor was not reached.
- Do not change PR #1 or PR #3 without a separate explicit task.
- Do not merge or mark Ready PR #2, create a tag or Release, sign, or publish a Mahou user build without explicit permission.

## Next management step

The project supervisor should review Draft PR #18 and the exact AGZ-MAH-0020 replacement candidate evidence after CI. Stage A must establish startup usability, settled CPU and normal exit before any functional testing. Only after Stage A passes may Stage B check snippets removal, Chrome/Word AutoSwitch, modern Notepad strict no-op, rapid-focus cancellation and restart persistence. Before PR #2 can leave Draft, accept that smoke, complete the other remaining retained-feature Windows smoke, and satisfy every merge gate in `docs/RELEASE-READINESS.md`; signing, final independent review, exact-candidate provenance and separate publication permission remain additional public-release gates.
