# Active

- [AGZ-MAH-0019] [IMPLEMENTATION_IN_PROGRESS / DRAFT] 2026-07-29
  Remove user-defined snippets and decouple AutoSwitch from every snippet enablement, parser, persistence, expression, UI and hotkey path. Exact base: `f02909611eb9a4502e9fe4d8fda9009922f352fd`; task branch: `agz-mah-0019-remove-snippets-decouple-autoswitch`; Draft PR #18.
  Product decisions: `USER_SNIPPETS_REMOVED` and `AUTOSWITCH_DECOUPLED`. Existing `snippets.txt` and `snippets.txt.bak` are inactive legacy rollback data: the runtime must not read, parse, create, modify, execute or delete them.
  AutoSwitch now has its own source buffer, dictionary loader and literal replacement primitive outside `SnippetsEnabled`. Exact modern Notepad `notepad.exe` + `RichEditD2DPT` remains fail-closed. General AutoSwitch behavior at the new source is not accepted until exact-head CI and focused physical-Windows smoke complete.

# Awaiting supervisor decision

- [AGZ-MAH-0016] [AUTOSWITCH_CONTAINMENT_VERIFICATION_RECORDED / DOCUMENTATION_COMPLETE / DRAFT] 2026-07-29
  Records the accepted physical-Windows result from `AGZ-MAH-0015` without changing Mahou runtime code, workflows, runtime version, PR #1, PR #2 metadata, or PR #3.
  The record distinguishes `AGZ-MAH-0014: AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT` from `AGZ-MAH-0015: AUTOSWITCH_MODERN_NOTEPAD_CONTAINMENT / VERIFIED / ACCEPTED / MERGED`. Modern Notepad AutoSwitch support was not added; exact `notepad.exe` + `RichEditD2DPT` is a strict safe no-op.

- [AGZ-MAH-0013] [RELEASE_READINESS_RECONCILED / DOCUMENTATION_COMPLETE / DRAFT] 2026-07-28
  Reconciled Draft PR #2 against exact development head `1a930137f7254111f8ef200da3e86c54e697ab5e`, accepted evidence, current automated runs, artifact provenance, and the actual fail-closed adapter boundary.
  `docs/RELEASE-READINESS.md` now separates verified, automated-only, user-smoke-required, not-applicable and blocked items; `docs/PR2-DESCRIPTION-PROPOSAL.md` provides a complete replacement body proposal without changing PR #2 metadata.
  Mahou runtime, runtime version, workflows, PR #2, and PR #3 are unchanged. The task PR remains Draft pending supervisor review; this result does not authorize Ready, merge, signing, tag, Release or publication.

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

- [AGZ-MAH-0018] [SNIPPETS_TRIGGER_REPLACEMENT_FAIL / ACCEPTED PRODUCT DECISION INPUT] 2026-07-29
  Physical-Windows testing of exact source `f02909611eb9a4502e9fe4d8fda9009922f352fd`, Modern Windows build `30410253117`, x64 artifact `8708105691`, ZIP SHA-256 `d9fdfedf4014a970139c9ed06ec337642a5208ea9256c64bd53f6aeaa6e5f4ce`, and `Mahou.exe` SHA-256 `265afd189b87d15b47db31e3eceb3a24426770a2f8af94d607d9df0c59aa8f70` found that simple replacement was corrupted, multiline replacement left prefix `agz1`, and delayed replacement left prefix `agz`.
  Classification: `AGZ-MAH-0018: SNIPPETS_TRIGGER_REPLACEMENT_FAIL`. The remaining snippets smoke was stopped. The feature is not to be repaired or preserved; `AGZ-MAH-0019` removes it and retains AutoSwitch independently.

- [AGZ-MAH-0015] [AUTOSWITCH_MODERN_NOTEPAD_CONTAINMENT / VERIFIED / ACCEPTED / MERGED] 2026-07-29
  Implemented strict containment from exact base `363a83b227cfa14e798e442960640e7caed03913` on branch `agz-mah-0015-autoswitch-notepad-containment`. Task commit `99e712aa3d913d37e1268ccf65a0cf52aca65ab7` was merged through PR #16 at `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`.
  Exact-head CI succeeded: Security regression `30403716646`, Input surface probe `30403716647`, and Modern Windows build `30403716677`. Merge-head CI succeeded: Security regression `30408387819` and Modern Windows build `30408387854`.
  Accepted x64 candidate artifact ID `8705704874`; ZIP SHA-256 `85179e55a256dab80ddfb79496aa0e9e3fec202d7f828f9ba7714605ed0a5e9d`; `Mahou.exe` SHA-256 `908252105fb0566322d0c25a70364860fbfe904c847d96dbae1b33b288cd17b0`.
  The user confirmed repeated modern Notepad `ghbdtn + space` remained unchanged, with no `gпривет`, deletion, partial replacement, deferred mutation after rapid window switching, or hidden AutoSwitch Undo entry. Chrome and Microsoft Word continued converting `ghbdtn` to `привет`.
  This verification proves strict safe no-op for exact `notepad.exe` + `RichEditD2DPT`; it does not add modern Notepad AutoSwitch support. Runtime version remains `2.9.0.1-dev`.

- [AGZ-MAH-0014] [AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT] 2026-07-29
  Real-Windows testing of exact source `363a83b227cfa14e798e442960640e7caed03913`, Modern Windows build `30365352449`, x64 artifact `8690549591`, ZIP SHA-256 `4ea545ddbec793f870d69b128cc11758cb61c5d7c388cf2330270aa9f8934a54` found a destructive AutoSwitch failure in Microsoft Notepad `RichEditD2DPT`.
  Typing `ghbdtn` produced `gпривет`; another attempt deleted the token completely. The first Undo showed no visible restoration and the second restored the source. Chrome and Microsoft Word passed the same focused test.
  This classification remains the immutable historical result for that artifact. It is not the current accepted containment behavior and must not be erased or relabelled.

- [AGZ-MAH-0012] [DIRECT-PATH-NOT-SAFE / ACCEPTED / MERGED] 2026-07-28
  Investigated documented external Windows write contracts for Qt 5.15.19 editable text from exact base `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1`, using Telegram Desktop 7.0.5 as the reference application.
  No primitive passed the pre-mutation gate. The documentation-only result was accepted and merged through PR #14 at `1a930137f7254111f8ef200da3e86c54e697ab5e`; no runtime adapter, mutation, runtime/version change or candidate artifact was added. Telegram and other Qt/custom surfaces remain strict no-op without a real user-created selection.

- [AGZ-MAH-0011] [DIRECT-PATH-NOT-SAFE / ACCEPTED / MERGED] 2026-07-28
  Investigated the documented Windows accessibility/RichEdit interoperability contract for an exact-range write in a disposable empty modern Notepad document from exact base `779b50dcc27cbe58f69ddadd54d526a0394663df`.
  The installed `RichEditD2DPT` control exposed UIA `TextPattern` and `ValuePattern`; a read-only `OBJID_NATIVEOM` probe also returned an object supporting `ITextDocument`. However, Microsoft documents RichEdit TOM acquisition through the pointer-bearing `EM_GETOLEINTERFACE` path, not a RichEdit-specific external `OBJID_NATIVEOM` contract. The generic object-model mechanism and one observed build do not establish a supported version-gated mutation contract.
  Exact independent `ITextRange` replacement is expressible after a valid TOM object is acquired, but cross-process acquisition support and a single ordinary Notepad Undo unit are not documented for this route. The result was accepted and merged through PR #13 at merge commit `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1`. No text mutation, harness, adapter, runtime/version change, artifact, release, or PR #3 work was performed. Modern Notepad remains strict no-op.

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
  The historical research order was modern RichEdit/Notepad, Telegram Qt input, Chromium Edit, Chromium contenteditable, Electron CodeMirror, while WhatsApp Desktop remains blocked. Additional feasibility work is paused during `AGZ-MAH-0013` release-readiness reconciliation.

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
- PR #16 is closed and merged at `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`; it contains the accepted AutoSwitch modern Notepad containment implementation and does not add modern Notepad support.
- PR #14 is closed and merged at `1a930137f7254111f8ef200da3e86c54e697ab5e`; it records the documentation-only `AGZ-MAH-0012` Qt feasibility decision and adds no adapter.
- PR #10 is closed and merged and records only the accepted `BLOCKED` Telegram investigation.
- PR #11 is closed and merged and adds the read-only input-surface capability probe, not a runtime adapter.
- PR #12 is closed and merged and records only the accepted sanitized input-surface evidence.
- PR #13 is closed and merged and records the accepted `AGZ-MAH-0011` result `DIRECT-PATH-NOT-SAFE` at merge commit `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1`.
