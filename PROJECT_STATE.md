# Project state — MIXANIZM Mahou

Snapshot date: 2026-07-25

## Authoritative development line

- Repository: `MIXANIZM/Mahou`
- Legacy default branch: `master`
- Current development branch: `mixanizm-modern-v2.9.0.1`
- Main development pull request: Draft PR #2, from `mixanizm-modern-v2.9.0.1` into `master`
- Current development head at the start of `AGZ-MAH-0009`: `2ca9c1540dbf68428e8c48180c56e4af98d5e59d`
- Runtime line: `2.9.0.1-dev`
- Agatzub Development Ruleset: `v2.7.0`
- Rules content commit: `ba60623aec67c57d46bda7ce2b291a823de2d4ea`
- Last verified Smart Caps source commit: `0b43bb688115e0051114f38a745fce9e830452fd`
- Last verified selected-text conversion source commit: `3418d09de20ea327302a26858a7b752862bd429e`
- Chrome desktop-only decision: `DIRECT-PATH-NOT-SAFE` under `AGZ-MAH-0006`
- Chrome browser-context editing-core result awaiting supervisor decision: `BROWSER-CONTEXT-MUTATION-NOT-SAFE` under `AGZ-MAH-0007`
- Telegram Desktop direct-path result: accepted `BLOCKED` under `AGZ-MAH-0008`
- Input-surface capability probe result awaiting supervisor decision: `PROBE_READY` under `AGZ-MAH-0009`

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

### AGZ-MAH-0009 — Input surface capability map

- Exact base: `2ca9c1540dbf68428e8c48180c56e4af98d5e59d`.
- Task branch: `agz-mah-0009-input-surface-capability-map`.
- Result: `PROBE_READY`.
- Added a separate one-shot Windows x64 read-only probe under `tools/input-surface-probe/`; Mahou runtime code and runtime version remain unchanged.
- The probe records only redacted process/control/accessibility capability metadata and never retrieves actual text, window titles, UIA Name content, paths with usernames, browser URLs, clipboard content or passwords.
- It contains no writable text methods, programmatic selection, keyboard simulation, clipboard APIs, hooks, continuous monitoring or injection.
- Classification covers classic Edit, RichEdit, Word, WPF, WinUI/UWP, Chromium browser, Electron/WebView, Qt/custom and unknown/custom-drawn families. Classification and interface presence never authorize mutation.
- The only existing verified direct collapsed-caret adapters remain exact classic Win32 `Edit` and Microsoft Word document `Range`.
- Added static and C# contract tests, a dedicated x64 build/package workflow, immutable ZIP manifest/SHA-256, capability-map documentation and read-only capture instructions.
- Suggested future captures cover classic Edit, Word, modern Notepad, Chrome input/textarea/contenteditable, Telegram, WhatsApp and one WPF/WinUI application. No mutation testing belongs to this task.

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

## Accepted blocked results

### AGZ-MAH-0008 — Telegram Desktop Smart Caps direct path

- Investigated from exact starting commit `076ee95325809bd0581c9e0f24e9dbb1022c6603` on branch `agz-mah-0008-telegram-smart-caps-path`.
- Result: accepted `BLOCKED`.
- PR #10 was merged into the development line at `2ca9c1540dbf68428e8c48180c56e4af98d5e59d` as a documentation/evidence result only.
- The executor environment had no interactive Windows desktop or running Telegram process, so exact installed Telegram identity, focused composer signature, UIA/MSAA/IAccessible2 interfaces, caret, composition, active-chat stability, one-step undo, draft/entity preservation and message-send safety could not be measured and were not guessed.
- UI Automation Text/TextRange is not an exact range-write primitive; `ValuePattern.SetValue()` remains a forbidden whole-field write.
- IAccessible2 specifies `IAccessibleEditableText::replaceText`, but actual Telegram exposure and all Telegram-specific safety properties remain unmeasured. Interface presence alone is not accepted.
- Official Telegram source reconnaissance confirms a Qt-based custom `Ui::InputField` compose subsystem, but that source is not claimed to match the installed user build and does not establish its Windows accessibility provider.
- No runtime adapter, mutation probe, version change or user mutation smoke package was created. Telegram remains strict no-op without a real user-created selection.
- Primary decision record: `docs/TELEGRAM-SMART-CAPS-ARCHITECTURE.md`.

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

- `AGZ-MAH-0009` is `PROBE_READY` for supervisor review. It adds evidence tooling only, not an adapter.
- `AGZ-MAH-0008` is accepted `BLOCKED`; the read-only probe can gather initial real-Windows evidence but cannot convert that result into write support.
- `AGZ-MAH-0007` is a negative browser-context editing-core result awaiting supervisor acceptance. It does not add Chrome support.
- `AGZ-MAH-0006` established that a Chrome desktop-only direct adapter is not safe.
- `AGZ-MAH-0005` and `AGZ-MAH-0001` remain verified only for their exact recorded sources, artifacts, and Windows scenarios.
- `AGZ-MAH-0003` Notepad direct adapter remains deferred. PR #3 is not an accepted implementation.

## Coordination model

- One active project supervisor chat maintains the overall state, chooses the next bounded task, prepares executor handoffs, and accepts results.
- One temporary executor chat handles one bounded task and does not begin the next task.
- GitHub, `PROJECT_STATE.md`, `ISSUES.md`, and `UNIVERSAL-CHAT-HANDOFF.md` remain the recoverable source of project state.

## Release state

- No public release is approved from the current modernization line.
- No tag or GitHub Release is authorized by this state document.
- The `AGZ-MAH-0009` probe ZIP is a separate evidence artifact, not a Mahou runtime release.
- Authenticode signing, final independent review, applicable CI, and remaining release-specific Windows checks remain release gates.
