# Active

- No product implementation task is currently active under the supervisor/executor workflow.

# Awaiting supervisor decision

- [AGZ-MAH-0009] [PROBE_READY] 2026-07-25
  Created a standalone read-only Windows x64 input-surface probe on branch `agz-mah-0009-input-surface-capability-map` from exact base `2ca9c1540dbf68428e8c48180c56e4af98d5e59d`.
  The one-shot probe captures only redacted foreground-process and focused-control capability metadata: process identity/version/architecture/signer, HWND classes, UIA metadata/patterns/protected state, MSAA role/state, IAccessible2 interface presence, caret/selection/length readability, cautious classification and a normalized fingerprint.
  The source and contract gates prohibit text/window-title/UIA-Name-content/URL/clipboard/password collection, writable methods, programmatic selection, keyboard simulation, hooks, continuous monitoring and injection. Interface presence is metadata only and all write capability remains unverified.
  Added `docs/INPUT-SURFACE-CAPABILITY-MAP.md`, `docs/INPUT-SURFACE-PROBE.md`, a dedicated x64 build/test/package workflow, redaction/classification/password tests and immutable probe ZIP provenance. Mahou runtime code and runtime version `2.9.0.1-dev` are unchanged.
  The probe is ready for supervisor review and later read-only user snapshots. It does not add a Notepad, Chrome, Telegram, WhatsApp, WPF, WinUI, Electron, WebView, Qt or unknown-control adapter.

- [AGZ-MAH-0007] [BROWSER-CONTEXT-MUTATION-NOT-SAFE] 2026-07-25
  Investigated the isolated Manifest V3 Chrome editing core from exact starting commit `faaf170d12e5bbcbd49c0b66edf4bac75c1e3049` without Native Messaging or Mahou runtime integration.
  `setRangeText()` can express an exact range and caret adjustment but does not provide the required normal trusted editing-event plus single browser undo/redo transaction contract. Deprecated `execCommand('insertText')` can preserve browser undo in some configurations but exact replacement requires a temporary programmatic selection, which is forbidden. Whole-value assignment and synthetic events were also rejected.
  A minimal action-activated MV3 prototype with only `activeTab` and `scripting` remains fail-closed: it verifies the exact diagnostic marker, supported control, composition, request age/ID, focus, tab/frame/document/element/value/caret/selection, boundaries, prefix, and suffix, then reports `mutation-api-not-accepted` without changing text.
  Automated source and Node contract checks pass locally. A real unpacked-extension smoke was unavailable because the execution Chromium is managed with extension installation and all URLs blocked; no CDP, remote debugging, local server, or policy bypass was used.
  Draft PR is for supervisor review only. No merge, Ready state, tag, signing, release, publication, Native Messaging, or Mahou runtime change is authorized.

- [AGZ-MAH-0003] [DEFERRED] 2026-07-25
  Design a dedicated verified direct adapter for modern Windows Notepad/RichEdit. Until then, collapsed-caret `Insert` and Smart Caps remain strict no-op in those controls.
  Draft PR #3 remains open, but its current diff is transport and workflow history from the attempted task, not an accepted or integrated Notepad implementation. Do not use, modify, close, rebase, or clean up PR #3 without a separate decision.

# Accepted blocked results

- [AGZ-MAH-0008] [BLOCKED] [ACCEPTED] 2026-07-25
  The Telegram Desktop Smart Caps direct-path investigation from exact starting commit `076ee95325809bd0581c9e0f24e9dbb1022c6603` was accepted and merged through PR #10 into `mixanizm-modern-v2.9.0.1` at merge commit `2ca9c1540dbf68428e8c48180c56e4af98d5e59d`.
  The executor environment had no interactive Windows desktop or running Telegram process, so it could not capture the exact installed version, executable/signature, HWND and UI Automation identity, MSAA/IAccessible2 interfaces, exact caret, composition state, active-chat signal, Telegram undo behavior, formatting/draft preservation, or message-send safety required by the acceptance gate.
  Official API documentation confirms that UI Automation Text/TextRange is not an exact range-write primitive and that `ValuePattern.SetValue()` is whole-field replacement. IAccessible2 defines `IAccessibleEditableText::replaceText`, but runtime exposure and Telegram-specific atomicity, caret, draft/entity, active-chat, post-state and one-step undo behavior remain unmeasured; interface presence alone is insufficient.
  No runtime adapter, diagnostic mutation, keyboard simulation, clipboard path, whole-field rewrite, version change, artifact or user mutation smoke package was added. Telegram remains strict no-op without a real user-created selection.
  Decision record: `docs/TELEGRAM-SMART-CAPS-ARCHITECTURE.md`. `AGZ-MAH-0009` now supplies the separately scoped read-only capability probe requested as the next evidence tool; it still cannot authorize mutation.

# Deferred product ideas

- Safe capitalization after sentence boundaries.
- Conservative typo correction.
- Optional local ghost-text completion.
- Chrome Smart Caps remains unsupported. `AGZ-MAH-0006` rejected the desktop-only path and `AGZ-MAH-0007` rejected the tested browser-context editing core under the strict range/caret/selection/undo/events gate. Native Messaging was not started.

# Completed

- [AGZ-MAH-0006] [DIRECT-PATH-NOT-SAFE] 2026-07-25
  Investigated a direct desktop Smart Caps path for ordinary Chrome `input[type=text]` and `textarea` controls from exact starting commit `d7e0e90a149d8d792260011b5902b80770a055d7`.
  UI Automation can expose text and, where `TextPattern2` is available, a collapsed caret range, but it exposes no client-side text-range replacement. `ValuePattern.SetValue` is a whole-control rewrite and cannot prove target-only mutation, exact caret preservation, one normal browser undo unit, expected DOM events, composition safety, or protection against a stale full-value overwrite.
  No runtime adapter, full-field rewrite, synthetic selection, keyboard selection, clipboard mutation, tracked-word fallback, CDP/JavaScript injection, extension, or Native Messaging host was added. Chrome Smart Caps remains strict no-op. The architecture decision and local disposable event probe are recorded under `docs/`.
  This result is documentation-only. No runtime artifact exists, and no merge, Ready state, tag, signing, release, or publication is authorized.

- [AGZ-MAH-0005] [VERIFIED] 2026-07-25
  Verified existing real-selection `Insert` conversion and full OLE clipboard preservation at exact source commit `3418d09de20ea327302a26858a7b752862bd429e` and tree `7920a089324fbd85d70b3669c7f86eb18ec5706a`.
  GitHub Actions passed: Modern Windows build run `30134239498` and Security regression run `30134239469`. The immutable x64 artifact was `Mahou-2.9.0.1-dev-win-x64-3418d09-run30134239498`; ZIP SHA-256 `c0f0643e251319bc20d9f528f4b199a13f780af7292a45c84dcacd210d328d28`; `Mahou.exe` SHA-256 `b3821d0a3116728db91cd46bf6091a578db19214cea7c3e7b465393b8876a95a`.
  The user confirmed the complete focused Windows smoke passed. Existing selections converted forward and back in Microsoft Word, modern Windows Notepad, downloaded local Chrome `textarea` and `contenteditable` controls, Telegram Desktop and the other applicable tested applications. Selection priority, caret behavior, ordinary insert mode, protected-field no-op behavior and repeated-operation stability worked as required.
  Unicode text, Word rich formatting, images, Excel cell ranges and Explorer file-drop clipboard data remained usable after conversion. In browsers and messengers, no-selection `Insert` remains a safe no-op; Microsoft Word's separately verified direct word-around-caret path continues to work.
  This verification records runtime evidence only. It does not authorize merge, marking PR #2 Ready, tagging, signing, release or publication.

- [AGZ-MAH-0001] [VERIFIED] 2026-07-25
  Verified the opt-in local Smart Caps implementation at exact source commit `0b43bb688115e0051114f38a745fce9e830452fd` with the immutable x64 artifact `Mahou-2.9.0.1-dev-win-x64-0b43bb6-run30128168029`.
  GitHub Actions passed: Modern Windows build run `30128168029` and Security regression run `30128168156`. The artifact ZIP SHA-256 was independently confirmed as `e344355bfd51ca5ebfa5f0b0dd24a06af94e02497a3ce39e18ef8a59511c3a6c`, and its manifest identified the expected source commit and tree `bd0d80d350cea61cbdd2a9cecdfa6002f2c88ee9`.
  The user confirmed the full focused Windows smoke passed: Russian localization and default-off state; direct correction in Microsoft Word and the exact classic Win32 `Edit` adapter without visible selection; expected no-op cases; immediate Backspace restoration; correction/reversion counter deltas of `+8` and `+2`; creation of a personal exception after two explicit rejections and persistence after restart; no correction while Mahou was closed; and fail-closed behavior in modern Notepad, Chrome, Telegram and a password field without changing text, selection, caret, layout, clipboard or counters.
  Verified scope remains limited to the implemented direct adapters and the tested fail-closed applications. This verification does not approve merge, release, signing or publication.

- [AGZ-MAH-0004] [COMPLETED] 2026-07-23
  Added the immutable artifact provenance gate and merged it through PR #4 into `mixanizm-modern-v2.9.0.1`.
  The accepted result includes immutable runtime/platform/commit/run artifact names, full commit/tree manifests, complete SHA-256 coverage, embedded commit verification, post-upload artifact evidence, and fail-closed positive and negative provenance checks.
  This task did not change Mahou runtime behavior or text-mutation behavior. PR #4 is closed and merged.

- [AGZ-MAH-0002] [VERIFIED] 2026-07-21
  Removed synthetic-selection behavior from collapsed-caret `Insert` and restricted the native adapter to the exact classic Win32 `Edit` class. The user verified on Windows 11 that modern Notepad no longer highlights, adds, removes, or corrupts text and instead performs a safe no-op. Verified checkpoint: `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`.

# Pull request status notes

- Draft PR #1 is open, based on `master`, and belongs to an earlier stabilization line. It is not the current working line and must not be changed without a separate decision.
- Draft PR #2 is open and is the main modernization PR from `mixanizm-modern-v2.9.0.1` into `master`. Keep it Draft and unmerged.
- Draft PR #3 is open against `mixanizm-modern-v2.9.0.1` and remains preserved transport history, not an accepted Notepad adapter.
- PR #10 is closed and merged into `mixanizm-modern-v2.9.0.1`; it records only the accepted `BLOCKED` Telegram investigation.
