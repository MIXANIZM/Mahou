# Testing

## Fast gates

```text
python .github/scripts/security-regression.py
python .github/scripts/ui-resource-regression.py
python .github/scripts/artifact-provenance-regression.py
python .github/scripts/autoswitch-containment-regression.py
python .github/scripts/autoswitch-independence-regression.py
python .github/scripts/chrome-extension-editing-core-regression.py
node .github/tests/chrome-extension-editing-core.test.js
python .github/scripts/input-surface-probe-regression.py
```

The AutoSwitch independence gate proves that snippets UI/runtime/persistence is absent, legacy files are not accessed or deleted, AutoSwitch routing is outside legacy snippets state, and dictionary values are literal. The AutoSwitch containment gate locks the exact `notepad.exe` + `RichEditD2DPT` rejection, source-context capture, immediate/deferred revalidation call sites, workflow integration, and unchanged runtime version. The other source gates check Mahou safety invariants, common Russian/English Smart Caps localization markers, UI resource consistency, and artifact provenance. The Chrome editing-core gates separately check the test-only MV3 manifest, permission boundary, absence of active mutation/network/remote-code/Native Messaging paths, exact diagnostic marker, fixed candidates, fail-closed control and stale-state rules, adjacent-text preservation contracts, and post-mutation verification logic. The input-surface probe gate checks that the standalone executable source contains no mutation, selection, keyboard, clipboard, hook, injection, actual-text or window-title APIs and retains the required schema, embedded commit, redaction and password-suppression markers. None of these source tests replace runtime testing.

## AGZ-MAH-0019 automated and physical checks

The Modern Windows build compiles and runs both `AutoSwitchContainmentRegression.exe` and `AutoSwitchIndependenceRegression.exe` against each x86 and x64 `Mahou.exe`. The independence executable inspects the built binary, reflects the dedicated AutoSwitch methods, and sends a value containing `__delay(...)` and `__execute(...)` through the literal-input builder. The output must contain exactly the same characters with no parsing, process launch, delay or command expansion.

Source and executable coverage proves:

1. AutoSwitch routing is not nested under or dependent on `SnippetsEnabled`;
2. no active snippets UI, trigger hook, parser, expression or persistence path remains;
3. no runtime source reads, writes, creates or deletes `snippets.txt` or `snippets.txt.bak`;
4. both obsolete legacy enable values are inert because no runtime binding remains;
5. dictionary replacement text is literal;
6. exact modern Notepad containment and source-context revalidation remain mandatory;
7. runtime remains `2.9.0.1-dev`;
8. Release x86 and x64 are each built twice with controlled byte-for-byte output comparison and warnings-as-errors.

The focused physical-Windows smoke for the exact candidate is:

1. Snippets tab and controls are absent;
2. a clean profile generates no `snippets.txt`;
3. an existing legacy `snippets.txt` remains byte-identical and inactive;
4. AutoSwitch can be enabled with no snippets data;
5. Chrome converts `ghbdtn + space` to `привет`;
6. Microsoft Word converts `ghbdtn + space` to `привет`;
7. modern Notepad leaves `ghbdtn + space` unchanged;
8. rapid focus switching causes no deferred mutation;
9. AutoSwitch still works after Mahou restart;
10. manual Insert, Smart Caps and clipboard behavior show no intentional regression.

Do not test removed snippets. Until this focused smoke is accepted, the new-source AutoSwitch status is candidate-only rather than verified.

## Completed AGZ-MAH-0015 verification

The Modern Windows build compiles and runs `AutoSwitchContainmentRegression.exe` for both x86 and x64 against the built `Mahou.exe`. Deterministic cases prove:

1. modern Notepad `RichEditD2DPT` is rejected before deletion;
2. no Backspace/Delete or replacement event is emitted and `ghbdtn` remains unchanged;
3. the full token cannot be deleted;
4. changed foreground or focused-control identity cancels delayed work;
5. Chrome and Microsoft Word routing remains allowed;
6. protected classic Edit and unknown contexts fail closed;
7. the block remains limited to the exact modern Notepad surface;
8. runtime version remains `2.9.0.1-dev`.

Implementation evidence:

```text
Task: AGZ-MAH-0015
Exact base: 363a83b227cfa14e798e442960640e7caed03913
Task commit: 99e712aa3d913d37e1268ccf65a0cf52aca65ab7
Implementation PR: #16
Merge commit: a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d
Result: AUTOSWITCH_MODERN_NOTEPAD_CONTAINMENT / VERIFIED / ACCEPTED / MERGED
```

Exact-head CI:

- Security regression `30403716646` — success;
- Input surface probe `30403716647` — success;
- Modern Windows build `30403716677` — success.

Merge-head CI:

- Security regression `30408387819` — success;
- Modern Windows build `30408387854` — success.

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

This verification is strict safe no-op containment for exact `notepad.exe` + `RichEditD2DPT`. It is not positive modern Notepad AutoSwitch support and does not establish a generic RichEdit writer. The historical failing source and artifact remain recorded as `AGZ-MAH-0014: AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT`.

## AGZ-MAH-0016 checks

This documentation-only verification-record task requires:

1. consistency across `ISSUES.md`, `PROJECT_STATE.md`, `UNIVERSAL-CHAT-HANDOFF.md`, `TESTING.md`, `docs/RELEASE-READINESS.md`, and `docs/PR2-DESCRIPTION-PROPOSAL.md`;
2. verification in GitHub of PR #16, task commit `99e712aa3d913d37e1268ccf65a0cf52aca65ab7`, merge commit `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`, and the recorded workflow run conclusions;
3. changed-file review proving no Mahou runtime source, workflow, runtime-version, PR #1, PR #2 metadata, or PR #3 change;
4. explicit distinction between historical defect evidence and current accepted containment behavior;
5. removal of AutoSwitch containment from the remaining Draft PR #2 smoke gates while retaining every other uncompleted retained-feature test.

`AGZ-MAH-0016` creates no runtime candidate and does not authorize Ready, merge, signing, tag, Release, or publication.

## Input surface probe workflow

The `Input surface probe` workflow runs on applicable task PRs and can also be dispatched manually. It:

- runs the source regression;
- stamps the exact workflow commit into the probe;
- builds the separate .NET Framework 4.8 Release x64 executable;
- builds and runs dependency-free C# contract tests;
- verifies `--version-json` against the exact commit;
- packages a separately named immutable x64 ZIP with `probe-manifest.json` and `SHA256SUMS.txt`;
- uploads the ZIP, its SHA-256 file and evidence JSON.

Contract tests cover path/URL/email/UIA-name redaction, password suppression, deterministic classification and fingerprinting, `CUSTOM_UNKNOWN` retention, schema version and embedded probe commit. The workflow does not build a Mahou runtime candidate and does not change runtime version `2.9.0.1-dev`.

## Completed probe-commit verification

The immutable probe used for the user capture was built from:

```text
cef38006dfe6093ee1b233703ad79215bb2a9758
```

Associated GitHub Actions results:

- `Security regression` run `30163258945` — success;
- `Input surface probe` run `30163258946` — success;
- `Modern Windows build` run `30163258943` — success.

These runs verify the probe source/build boundary and existing Mahou build/security gates at the probe commit. They do not convert any read capability into a write capability.

## Completed real-Windows read-only capture

`AGZ-MAH-0010` records the user-completed one-shot capture and marks:

```text
AGZ-MAH-0009: USER_PROBE_EVIDENCE_COMPLETE
```

Observed results:

| Surface | Normalized read evidence |
| --- | --- |
| Word | `WORD_OBJECT_MODEL` |
| Modern Notepad | `RICHEDIT`, `RichEditD2DPT` |
| AnyDesk | `CLASSIC_WIN32_EDIT`, exact `Edit` |
| Chrome input and textarea | identical `CHROMIUM_BROWSER / Edit / TextPattern + ValuePattern` |
| Chrome contenteditable | separate `CHROMIUM_BROWSER / Group / TextPattern` |
| Telegram Desktop 7.0.5 | `QT_CUSTOM / Ui::InputField::Inner / TextPattern + ValuePattern` |
| WhatsApp Web in Opera | Chromium Edit family |
| WhatsApp Desktop | `CUSTOM_UNKNOWN / DesktopChildSiteBridge`; internal editor not reached |
| Obsidian title | Electron Group/TextPattern |
| Obsidian CodeMirror body | Electron Edit/TextPattern + ValuePattern |

Manual capture rules were followed as a read-only evidence collection activity: focus the target field during the countdown, do not type/select/copy/paste/send during capture, and retain only redacted JSON.

The capture establishes surface distinctions, not mutation support:

- Chrome input/textarea and contenteditable require separate future contracts;
- Obsidian title and CodeMirror body require separate future contracts;
- WhatsApp Desktop remains blocked because only the host bridge was reached;
- UIA TextPattern/ValuePattern in Chromium and Qt is not proof of exact-range writing or application undo;
- only Word Range and exact classic Win32 Edit remain verified direct collapsed-caret adapters.

## Evidence-file review

Reviewed sanitized reports are committed under `docs/evidence/input-surface-probe/`:

- `chrome-input.json`
- `chrome-textarea.json`
- `chrome-contenteditable.json`
- `telegram-desktop-7.0.5-compose.json`
- `whatsapp-web-opera-compose.json`
- `whatsapp-desktop-bridge.json`
- `obsidian-title.json`
- `obsidian-codemirror-body.json`

Review criteria:

- `schema_version` is `1`;
- `probe_commit` is exactly `cef38006dfe6093ee1b233703ad79215bb2a9758`;
- `actual_text_collected` is `false`;
- no actual text, titles, URLs, usernames, clipboard content, executable paths or personal paths are present;
- automation IDs are absent or redacted;
- every report retains `no-write-capability-verified-by-this-probe`;
- duplicate and unsuccessful captures are excluded.

Raw JSON for Word, modern Notepad and AnyDesk was not available in the supplied files. Their normalized evidence is therefore recorded in documentation only; no fabricated report file is added.

## AGZ-MAH-0010 checks

This documentation-only task requires:

1. documentation consistency across the capability map, probe guide, project state, issues, handoff, architecture and testing documents;
2. changed-file review proving there are no Mahou runtime source or version changes;
3. Security regression on the exact task head;
4. Modern Windows build on the exact task head.

The task does not create or hand off a candidate artifact. The dedicated `Input surface probe` workflow is not required to rebuild the already immutable user probe merely because documentation and archived reports were recorded.

## AGZ-MAH-0011 checks

This documentation-only Windows accessibility/RichEdit interoperability task requires:

1. primary Microsoft documentation review for UIA, `OBJID_NATIVEOM`, RichEdit TOM acquisition, independent `ITextRange` behavior and Undo;
2. a read-only installed-build capture of Notepad/package/binary identity, focused control identity, supported UIA patterns and COM interface availability;
3. no text mutation before the documented-contract gate passes;
4. changed-file review proving that only documentation changed and Mahou runtime source/version remain untouched;
5. static security and input-surface-probe regression gates;
6. Security regression and Modern Windows build on the exact task head.

The documented RichEdit-specific external acquisition contract was not established, so the result is `DIRECT-PATH-NOT-SAFE`. No mutation smoke, executable harness or candidate artifact was created.

## AGZ-MAH-0012 checks

This documentation-only Qt Windows editable-text feasibility task requires:

1. exact source mapping from official Telegram Desktop 7.0.5 to Qt 5.15.19 and its Windows accessibility provider;
2. primary Qt and Microsoft contract review for UIA Text/Text2, Value, TextEdit, ObjectModel, MSAA, IAccessible2 and Qt's internal editable-text interface;
3. read-only identity checks for the official Telegram 7.0.5 x64 release and explicit recording that the formerly installed 7.0.5 runtime was no longer available for re-probe;
4. no text mutation before a documented external exact-range write contract is established;
5. changed-file review proving that only documentation changed and Mahou runtime source/version remain untouched;
6. static rejection review for UIA selection, whole-value writes, keyboard/clipboard input, hooks, injection, process memory and private in-process Qt access;
7. local security and input-surface-probe regression gates;
8. GitHub Security regression, Input surface probe and Modern Windows build on the exact task head.

The Qt Windows provider does not project the internal `QAccessibleEditableTextInterface` as an external range writer, so the result is `DIRECT-PATH-NOT-SAFE`. No mutation smoke, executable harness, feasibility workflow or candidate artifact was created.

## AGZ-MAH-0013 checks

This documentation-only release-readiness reconciliation requires:

1. consistency across `docs/RELEASE-READINESS.md`, the PR #2 body proposal, project state, issues, handoff, testing, architecture and README;
2. a stale-claim review against the actual runtime control flow and accepted evidence;
3. changed-file and tree review proving the `Mahou/` runtime subtree, runtime version and `.github/workflows/` are unchanged from exact base `1a930137f7254111f8ef200da3e86c54e697ab5e`;
4. all local fast gates above;
5. GitHub Security regression, Input surface probe and Modern Windows build on the exact task head.

The task creates no runtime candidate and does not authorize a merge or release.

## Historical research order

The recorded research order was:

1. modern RichEdit / Notepad;
2. Telegram Qt input;
3. Chromium Edit;
4. Chromium contenteditable;
5. Electron CodeMirror;
6. WhatsApp Desktop bridge remains blocked.

This order is retained as historical context. `AGZ-MAH-0011` and `AGZ-MAH-0012` rejected the documented external direct paths for modern Notepad and Qt. UIA TextPattern/ValuePattern alone is insufficient for Chromium and Qt.

## Chrome editing-core workflow

The read-only `Chrome extension editing core` workflow runs the static Python gate and dependency-free Node contract tests for changes to the prototype, diagnostic page, decision document, or their tests.

The retained prototype intentionally performs no mutation and must report:

```text
BROWSER-CONTEXT-MUTATION-NOT-SAFE
mutation-api-not-accepted
```

A successful negative test means the field value, caret, selection, undo/redo history, and clipboard remain unchanged. It is not proof of Chrome Smart Caps support.

## Windows CI

The `Modern Windows build` workflow:

- builds Release x86 twice and compares controlled files byte-for-byte;
- builds Release x64 twice and compares controlled files byte-for-byte;
- runs `InsertSafetyRegression` against the built executable;
- runs `SmartCapsRegression`, including third-initial, interior-capital, hyphenated-word, all-caps, mixed-script, numeric, URL and email cases;
- runs `AutoSwitchContainmentRegression` and `AutoSwitchIndependenceRegression` against x86 and x64 `Mahou.exe`;
- packages manifests, SHA-256 sums, security report, containment report, and test documentation;
- names every build/log/evidence artifact with the runtime version, platform, short source commit, and workflow run ID;
- verifies the generated archive with the exact full commit and tree, rejects an intentionally wrong expected commit, and rejects a legacy-manifest fixture before upload;
- creates a separate post-upload evidence JSON containing artifact ID/digest, run URL, source commit/tree, ZIP SHA-256, and executable SHA-256.

Documentation-only `AGZ-MAH-0016` must leave the existing runtime source, workflows and runtime version untouched. Any workflow runs caused by its PR are documentation-head health evidence only and do not replace the accepted `AGZ-MAH-0015` candidate and physical-Windows smoke.

## Artifact handoff

Follow `ARTIFACT-PROVENANCE.md` for Mahou runtime builds. Before giving a Mahou ZIP to a user, run `.github/scripts/verify-artifact-provenance.ps1` with the expected full commit, tree, platform, repository, and runtime version. When the retained incident archive is available, `.github/scripts/test-artifact-provenance.ps1 -LegacyZipPath <path>` must reject the real `0f9b75c...` ZIP for the current expected source.

The probe is a separate non-runtime artifact. Its own ZIP must match the exact name, source commit/tree, embedded probe commit, `probe-manifest.json`, `SHA256SUMS.txt`, external `.zip.sha256`, and workflow evidence produced by `input-surface-probe.yml`.

After merge, never represent a PR-head artifact as a merge-head artifact. The accepted `AGZ-MAH-0015` x64 candidate remains a task-head artifact from `99e712aa3d913d37e1268ccf65a0cf52aca65ab7`; merge-head CI establishes merged-source health but does not relabel that ZIP.

## Manual Windows checks

Follow `TEST-PLAN-WINDOWS11.md`. For Mahou text mutation, use disposable documents and verify text, caret, selection, keyboard layout, clipboard and the Mahou-only session counter before and after each operation.

For the input-surface probe, captures are read-only evidence collection. Focus the target field during the countdown, do not type/select/copy/paste/send, and retain the redacted JSON. A report is useful for capability mapping but cannot verify a write path.

For `AGZ-MAH-0007`, no positive user Chrome mutation smoke is requested because no mutation API passed the acceptance gate and no active mutation code remains. The local diagnostic page can still be used to confirm the fail-closed prototype result and event logging if a supervisor requests that limited check.

No positive direct-adapter mutation smoke is requested for modern Notepad/RichEdit or Telegram/Qt: `AGZ-MAH-0011` and `AGZ-MAH-0012` rejected those direct paths before mutation. Their required collapsed-caret direct-path result is strict no-op without a user-created selection.

The separate AutoSwitch keyboard-replay path did require focused containment smoke after `AGZ-MAH-0014`. That smoke is now accepted under `AGZ-MAH-0015`: modern Notepad remains unchanged, rapid focus changes do not produce deferred mutation, Undo contains no hidden operation, and Chrome/Word continue converting. This closes only the AutoSwitch containment gate; it does not add modern Notepad support or close unrelated retained-feature tests.

## Telegram Desktop direct-path investigation

`AGZ-MAH-0008` is accepted as `BLOCKED`. `AGZ-MAH-0012` later completed the Qt 5.15.19 documented-contract gate with `DIRECT-PATH-NOT-SAFE`: the Windows provider offers no documented external exact-range writer. The read-only snapshot supplies process/control/provider metadata but cannot satisfy any mutation acceptance gate.

Before any future Telegram mutation, a separately authorized task must record and revalidate:

- executable version/signature/architecture/install source, foreground window, focused control, HWND classes and process ID;
- UI Automation ancestry, control type, automation ID, name, framework ID, class name, password/protected state and all available patterns;
- MSAA role/state and IAccessible2 interface availability;
- exact full fixed test text, exact collapsed caret, exact selection state, target word boundaries, delimiter, composition state, candidate age and active-chat identity;
- ordinary new-message mode rather than search, caption, edit-message, forward comment, passcode or another field.

A writable primitive is acceptable only after a real bounded private-chat test proves, without programmatic selection, keyboard simulation, clipboard access, or whole-field replacement:

- only the exact target word changes;
- prefix and suffix remain identical;
- the caret is exact and selection remains collapsed;
- formatting entities and draft state remain intact;
- the active composer and active chat remain unchanged;
- no message is sent;
- the complete post-state can be re-read and verified;
- one normal Telegram undo action reverts exactly the correction;
- stale window, focus, chat, element, source, caret, composition or candidate state causes a complete no-op.

Reading text or caret, capability classification, `ValuePattern`, or merely detecting an editable interface is not proof of a safe write path. If any required property is uncertain, the test must stop and Telegram must remain no-op.

A feature is `VERIFIED` only after its exact commit passes applicable CI and the user confirms required real behavior. A negative architecture decision can be accepted when rejected methods, automated checks, environment limitations and strict no-op result are all recorded.
