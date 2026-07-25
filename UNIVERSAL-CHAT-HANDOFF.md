# Universal chat handoff — MIXANIZM Mahou

## Project

- Repository: `MIXANIZM/Mahou`
- Legacy default branch: `master`
- Current development branch: `mixanizm-modern-v2.9.0.1`
- Main development pull request: Draft PR #2
- Runtime line: `2.9.0.1-dev`
- Agatzub Development Ruleset: `v2.7.0`
- Rules content commit: `ba60623aec67c57d46bda7ce2b291a823de2d4ea`
- Current development head at the `AGZ-MAH-0009` handoff start: `2ca9c1540dbf68428e8c48180c56e4af98d5e59d`
- Last user-verified Insert safety checkpoint: `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`
- Verified Smart Caps source commit: `0b43bb688115e0051114f38a745fce9e830452fd`
- Verified selected-text conversion source commit: `3418d09de20ea327302a26858a7b752862bd429e`
- Chrome desktop-only decision: `DIRECT-PATH-NOT-SAFE` under `AGZ-MAH-0006`
- Chrome browser-context editing-core result awaiting supervisor decision: `BROWSER-CONTEXT-MUTATION-NOT-SAFE` under `AGZ-MAH-0007`
- Telegram Desktop direct-path result: accepted `BLOCKED` under `AGZ-MAH-0008`, merged through PR #10
- Input-surface capability probe result awaiting supervisor decision: `PROBE_READY` under `AGZ-MAH-0009`

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
- Modern Notepad/RichEdit, Chrome, Telegram, Discord, WPF, WinUI/UWP, Electron/WebView, Qt/custom, unknown controls, protected fields, and failed probes remain no-op without a user-created selection.
- Smart Caps is local, optional, disabled on a clean profile, and verified only for its exact recorded source/artifact/scenarios.
- Draft PR #2 remains open, Draft, and unmerged.

## Result awaiting supervisor decision

### AGZ-MAH-0009 — Input surface capability map

Starting point:

```text
Base branch: mixanizm-modern-v2.9.0.1
Exact base: 2ca9c1540dbf68428e8c48180c56e4af98d5e59d
Task branch: agz-mah-0009-input-surface-capability-map
```

Result:

```text
PROBE_READY
```

Product result and boundary:

- a standalone .NET Framework 4.8 x64 console probe exists under `tools/input-surface-probe/` and is not referenced by Mahou runtime;
- it performs one capture after a short countdown, inspects only the foreground process and focused control, writes one redacted JSON report, and exits;
- it records process filename/architecture/version/signer, top/focused HWND classes, UIA control/framework/class/automation-ID summary/patterns/protection, MSAA role/state, IAccessible2 interface presence, caret/selection/length readability and a normalized capability fingerprint where available;
- it never records actual text, window-title content, UIA Name content, document/chat/contact names, user paths, browser URLs, clipboard contents, chat history or passwords;
- protected/password controls suppress caret, selection and length output;
- it contains no writable text method, programmatic selection, keyboard simulation, clipboard API, hook, continuous monitor, hotkey, process-memory write or injection path;
- UIA ValuePattern, Word object model or IAccessibleEditableText presence is reported only as `write_capabilities_unverified` metadata;
- classifications are `CLASSIC_WIN32_EDIT`, `RICHEDIT`, `WORD_OBJECT_MODEL`, `WPF`, `WINUI_UWP`, `CHROMIUM_BROWSER`, `ELECTRON_WEBVIEW`, `QT_CUSTOM` and `CUSTOM_UNKNOWN`;
- unknown controls remain unknown, and no classification/fingerprint/process/framework match can activate a mutation path;
- the only verified direct Mahou adapters remain exact classic `Edit` and Microsoft Word document `Range`;
- documentation explains Word vs Chrome, Telegram/Qt vs Chrome, application-specific Electron/WebView risk, and why UIA read evidence does not imply safe exact-range writing;
- tests cover forbidden APIs, redaction, password suppression, deterministic classification/fingerprint, unknown retention, schema version and embedded probe commit;
- a dedicated workflow builds/tests the standalone x64 executable and creates an immutable probe ZIP with manifest and SHA-256;
- existing Security regression and Modern Windows build remain required;
- Mahou runtime code and version `2.9.0.1-dev` are unchanged;
- no merge, release, signing, runtime adapter, Chrome, Telegram or Notepad implementation is included.

Suggested later read-only user captures are documented for classic Edit, Word, modern Notepad, Chrome input/textarea/contenteditable, Telegram Desktop, WhatsApp Desktop, and one WPF/WinUI application if available. No text mutation test belongs to this task.

Primary records:

```text
docs/INPUT-SURFACE-CAPABILITY-MAP.md
docs/INPUT-SURFACE-PROBE.md
```

The supervisor should verify the exact task head, all three applicable CI workflows, the immutable x64 probe ZIP and SHA-256, then accept or return specific findings. The executor must not begin a mutation continuation or another task.

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

## Accepted blocked result

### AGZ-MAH-0008 — Telegram Desktop Smart Caps direct path

Starting point and integration:

```text
Starting commit: 076ee95325809bd0581c9e0f24e9dbb1022c6603
Task branch: agz-mah-0008-telegram-smart-caps-path
PR #10: merged
Merge commit: 2ca9c1540dbf68428e8c48180c56e4af98d5e59d
```

Result:

```text
BLOCKED — ACCEPTED
```

Evidence and boundary:

- exact starting head and CI were confirmed before integration;
- central Agatzub rules remain stable at pinned `v2.7.0`; no newer stable version was found;
- the executor environment had no interactive Windows desktop or running Telegram process;
- exact installed Telegram version, install source, executable/signature, architecture, HWND classes, UIA tree and patterns, MSAA/IAccessible2 interfaces, exact caret, composition, active-chat signal, one-step undo, draft/entity preservation, and message-send safety could not be measured and were not guessed;
- Microsoft UI Automation Text/TextRange is not an exact range-write primitive; `.Select()` and whole-field `ValuePattern.SetValue()` remain forbidden;
- IAccessible2 defines `IAccessibleEditableText::replaceText`, but actual Telegram exposure and all required Telegram-specific safety properties remain unmeasured; interface presence alone is insufficient;
- official `telegramdesktop/tdesktop` source reconnaissance confirms a Qt-based custom `Ui::InputField` compose subsystem but is not claimed to match the installed user build or establish its Windows accessibility provider;
- no runtime adapter, mutation probe, keyboard simulation, clipboard path, whole-field rewrite, runtime version change or user mutation smoke package was added;
- Telegram remains strict no-op without a real user-created selection;
- primary record: `docs/TELEGRAM-SMART-CAPS-ARCHITECTURE.md`.

`AGZ-MAH-0009` is the separately scoped read-only evidence tool requested after this accepted blocked result. It cannot itself satisfy or bypass the Telegram mutation gate.

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

- Do not restore UI Automation `.Select()`, keyboard selection, generated-selection fallback, Backspace/retype, clipboard mutation, or whole-field rewrite.
- Do not turn any `AGZ-MAH-0009` classification, fingerprint, read capability, process name, framework ID, UIA pattern or IAccessible2 interface presence into a mutation allow-list.
- Do not treat Qt ancestry, Telegram process name, UIA read access, caret access, or `IAccessibleEditableText` presence alone as proof of a safe Telegram mutation path.
- Do not activate the AGZ-MAH-0007 prototype mutation: the retained code must remain strict no-op unless a future separately authorized architecture supplies a method that passes every gate.
- Do not start Native Messaging, Mahou integration, Telegram mutation, Notepad, Edge, local server, CDP, remote debugging, DLL injection, signing, or release work from these results.
- Do not change PR #1 or PR #3 without a separate explicit task.
- Do not merge or mark Ready PR #2 or the AGZ-MAH-0009 task PR, create a tag or Release, or publish a Mahou user build without explicit permission.
- Do not represent the standalone probe ZIP as a Mahou runtime release.
- Do not hand off an artifact unless its exact commit/tree, embedded commit, manifest and SHA-256 match the applicable workflow evidence.

## Next management step

The project supervisor should inspect the AGZ-MAH-0009 Draft PR, exact final head, `Input surface probe`, `Security regression` and `Modern Windows build` runs, and the immutable x64 probe ZIP/SHA-256. The supervisor should then accept `PROBE_READY` or return specific corrections. The executor must not begin another task.
