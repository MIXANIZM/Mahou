# Active

- No product implementation task is currently active under the supervisor/executor workflow.

# Awaiting supervisor decision

- [AGZ-MAH-0011] [DIRECT-PATH-NOT-SAFE / DOCUMENTATION_COMPLETE / DRAFT] 2026-07-28
  Investigated the documented Windows accessibility/RichEdit interoperability contract for an exact-range write in a disposable empty modern Notepad document from exact base `779b50dcc27cbe58f69ddadd54d526a0394663df`.
  The installed `RichEditD2DPT` control exposed UIA `TextPattern` and `ValuePattern`; a read-only `OBJID_NATIVEOM` probe also returned an object supporting `ITextDocument`. However, Microsoft documents RichEdit TOM acquisition through the pointer-bearing `EM_GETOLEINTERFACE` path, not a RichEdit-specific external `OBJID_NATIVEOM` contract. The generic object-model mechanism and one observed build do not establish a supported version-gated mutation contract.
  Exact independent `ITextRange` replacement is expressible after a valid TOM object is acquired, but cross-process acquisition support and a single ordinary Notepad Undo unit are not documented for this route. No text mutation, harness, adapter, runtime/version change, artifact, merge, release, or PR #3 work was performed. Modern Notepad remains strict no-op.

- [AGZ-MAH-0007] [BROWSER-CONTEXT-MUTATION-NOT-SAFE] 2026-07-25
  Investigated the isolated Manifest V3 Chrome editing core from exact starting commit `faaf170d12e5bbcbd49c0b66edf4bac75c1e3049` without Native Messaging or Mahou runtime integration.
  `setRangeText()` can express an exact range and caret adjustment but does not provide the required normal trusted editing-event plus single browser undo/redo transaction contract. Deprecated `execCommand('insertText')` can preserve browser undo in some configurations but exact replacement requires a temporary programmatic selection, which is forbidden. Whole-value assignment and synthetic events were also rejected.
  The retained prototype performs no mutation and reports `mutation-api-not-accepted`. No Chrome support, Mahou runtime change, merge, release or publication is authorized.

- [AGZ-MAH-0003] [SUPERSEDED BY AGZ-MAH-0011 / DIRECT-PATH-NOT-SAFE] 2026-07-25
  The documented-contract feasibility gate was completed by `AGZ-MAH-0011`; it did not establish a safe external direct adapter for modern Windows Notepad/RichEdit. Collapsed-caret `Insert` and Smart Caps remain strict no-op in those controls.
  Draft PR #3 remains open, but its current diff is transport and workflow history from the attempted task, not an accepted or integrated Notepad implementation. Do not use, modify, close, rebase, or clean up PR #3 without a separate decision.

# Accepted blocked results

- [AGZ-MAH-0008] [BLOCKED] [ACCEPTED] 2026-07-25
  The Telegram Desktop Smart Caps direct-path investigation was accepted and merged through PR #10 into `mixanizm-modern-v2.9.0.1`.
  The later real-Windows read-only probe identifies the tested Telegram Desktop 7.0.5 composer as `QT_CUSTOM`, UIA `Edit`, class `Ui::InputField::Inner`, with `TextPattern` and `ValuePattern` exposed. This is read evidence only and does not establish exact-range writing, application undo, draft/entity preservation, active-chat stability, composition safety or no-send behavior.
  No runtime adapter, diagnostic mutation, keyboard simulation, clipboard path, whole-field rewrite, version change, artifact or user mutation smoke package was added. Telegram remains strict no-op without a real user-created selection.

# Deferred product ideas

- Safe capitalization after sentence boundaries.
- Conservative typo correction.
- Optional local ghost-text completion.
- Chrome Smart Caps remains unsupported. `AGZ-MAH-0006` rejected the desktop-only path and `AGZ-MAH-0007` rejected the tested browser-context editing core under the strict range/caret/selection/undo/events gate. Native Messaging was not started.

# Completed

- [AGZ-MAH-0010] [ACCEPTED / MERGED] 2026-07-27
  Recorded the completed real-Windows read-only input-surface evidence without changing Mahou runtime behavior. PR #12 was accepted and merged into `mixanizm-modern-v2.9.0.1` at merge commit `779b50dcc27cbe58f69ddadd54d526a0394663df`.

- [AGZ-MAH-0009] [USER_PROBE_EVIDENCE_COMPLETE] 2026-07-27
  The standalone read-only Windows x64 probe was merged through PR #11 into the development line at `f82a3b233d250f4bfb432327063c6832ab24ea5a`. Probe source commit `cef38006dfe6093ee1b233703ad79215bb2a9758` passed Security regression run `30163258945`, Input surface probe run `30163258946`, and Modern Windows build run `30163258943`.
  Completed user evidence:
  - Word -> `WORD_OBJECT_MODEL`;
  - modern Notepad `RichEditD2DPT` -> `RICHEDIT`;
  - AnyDesk exact classic `Edit` -> `CLASSIC_WIN32_EDIT`;
  - Chrome input/textarea -> identical `CHROMIUM_BROWSER / Edit / TextPattern + ValuePattern`;
  - Chrome contenteditable -> separate `CHROMIUM_BROWSER / Group / TextPattern`;
  - Telegram Desktop 7.0.5 -> `QT_CUSTOM / Ui::InputField::Inner / TextPattern + ValuePattern`;
  - WhatsApp Web in Opera -> Chromium Edit family;
  - WhatsApp Desktop -> `CUSTOM_UNKNOWN / DesktopChildSiteBridge`, internal editor not reached;
  - Obsidian title -> Electron Group/TextPattern;
  - Obsidian CodeMirror body -> Electron Edit/TextPattern + ValuePattern.
  Raw JSON for Word, modern Notepad and AnyDesk was unavailable, so their user-confirmed normalized evidence was recorded without inventing raw reports. Eight other unique sanitized reports were committed; duplicates and unsuccessful captures were omitted.
  The only verified direct collapsed-caret adapters remain exact classic Win32 `Edit` and Microsoft Word document `Range`.
  Recommended research order is modern RichEdit/Notepad, Telegram Qt input, Chromium Edit, Chromium contenteditable, Electron CodeMirror, while WhatsApp Desktop remains blocked. RichEdit is first because it may expose native range, caret and undo semantics; UIA TextPattern/ValuePattern alone is already insufficient for Chromium and Qt.

- [AGZ-MAH-0006] [DIRECT-PATH-NOT-SAFE] 2026-07-25
  Investigated a direct desktop Smart Caps path for ordinary Chrome `input[type=text]` and `textarea` controls. UI Automation exposes no acceptable exact replace-range operation and `ValuePattern.SetValue` is a rejected whole-field rewrite. Chrome remains strict no-op.

- [AGZ-MAH-0005] [VERIFIED] 2026-07-25
  Verified existing real-selection `Insert` conversion and full OLE clipboard preservation at exact source commit `3418d09de20ea327302a26858a7b752862bd429e`. Modern Windows build run `30134239498` and Security regression run `30134239469` passed; the user confirmed the complete focused Windows smoke.
  In browsers and messengers, no-selection `Insert` remains a safe no-op; Microsoft Word's separately verified direct word-around-caret path continues to work.

- [AGZ-MAH-0001] [VERIFIED] 2026-07-25
  Verified the opt-in local Smart Caps implementation at exact source commit `0b43bb688115e0051114f38a745fce9e830452fd`. The user confirmed the full focused Windows smoke, including strict no-op behavior in unsupported controls.

- [AGZ-MAH-0004] [COMPLETED] 2026-07-23
  Added the immutable artifact provenance gate and merged it through PR #4 into `mixanizm-modern-v2.9.0.1`. This task did not change Mahou runtime behavior.

- [AGZ-MAH-0002] [VERIFIED] 2026-07-21
  Removed synthetic-selection behavior from collapsed-caret `Insert` and restricted the native adapter to the exact classic Win32 `Edit` class. The user verified on Windows 11 that modern Notepad performs a safe no-op. Verified checkpoint: `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`.

# Pull request status notes

- Draft PR #1 is open, based on `master`, and belongs to an earlier stabilization line. It is not the current working line and must not be changed without a separate decision.
- Draft PR #2 is open and is the main modernization PR from `mixanizm-modern-v2.9.0.1` into `master`. Keep it Draft and unmerged.
- Draft PR #3 is open against `mixanizm-modern-v2.9.0.1` and remains preserved transport history, not an accepted Notepad adapter.
- PR #10 is closed and merged and records only the accepted `BLOCKED` Telegram investigation.
- PR #11 is closed and merged and adds the read-only input-surface capability probe, not a runtime adapter.
- PR #12 is closed and merged and records only the accepted sanitized input-surface evidence.
