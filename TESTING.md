# Testing

## Fast gates

```text
python .github/scripts/security-regression.py
python .github/scripts/ui-resource-regression.py
python .github/scripts/artifact-provenance-regression.py
python .github/scripts/chrome-extension-editing-core-regression.py
node .github/tests/chrome-extension-editing-core.test.js
python .github/scripts/input-surface-probe-regression.py
```

The first three gates check source-level Mahou safety invariants, common Russian/English Smart Caps localization markers, UI resource consistency, and artifact provenance. The Chrome editing-core gates separately check the test-only MV3 manifest, permission boundary, absence of active mutation/network/remote-code/Native Messaging paths, exact diagnostic marker, fixed candidates, fail-closed control and stale-state rules, adjacent-text preservation contracts, and post-mutation verification logic. The input-surface probe gate checks that the standalone executable source contains no mutation, selection, keyboard, clipboard, hook, injection, actual-text or window-title APIs and retains the required schema, embedded commit, redaction and password-suppression markers. None of these source tests replace runtime testing.

## Input surface probe workflow

The `Input surface probe` workflow runs on the task PR and can also be dispatched manually. It:

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

## Recommended research order

Future separately authorized tasks should investigate:

1. modern RichEdit / Notepad;
2. Telegram Qt input;
3. Chromium Edit;
4. Chromium contenteditable;
5. Electron CodeMirror;
6. WhatsApp Desktop bridge remains blocked.

RichEdit is first because it may expose native range, caret and undo semantics that can be tested directly. UIA TextPattern/ValuePattern alone is already insufficient for Chromium and Qt.

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
- packages manifests, SHA-256 sums, security report, and test documentation;
- names every build/log/evidence artifact with the runtime version, platform, short source commit, and workflow run ID;
- verifies the generated archive with the exact full commit and tree, rejects an intentionally wrong expected commit, and rejects a legacy-manifest fixture before upload;
- creates a separate post-upload evidence JSON containing artifact ID/digest, run URL, source commit/tree, ZIP SHA-256, and executable SHA-256.

`AGZ-MAH-0010` must leave both the existing `Security regression` and `Modern Windows build` green because it changes only documentation/evidence while leaving Mahou runtime sources and runtime version untouched.

## Artifact handoff

Follow `ARTIFACT-PROVENANCE.md` for Mahou runtime builds. Before giving a Mahou ZIP to a user, run `.github/scripts/verify-artifact-provenance.ps1` with the expected full commit, tree, platform, repository, and runtime version. When the retained incident archive is available, `.github/scripts/test-artifact-provenance.ps1 -LegacyZipPath <path>` must reject the real `0f9b75c...` ZIP for the current expected source.

The probe is a separate non-runtime artifact. Its own ZIP must match the exact name, source commit/tree, embedded probe commit, `probe-manifest.json`, `SHA256SUMS.txt`, external `.zip.sha256`, and workflow evidence produced by `input-surface-probe.yml`.

After merge, never represent a PR-head artifact as a merge-head artifact. Run a new workflow only when a new artifact is actually required and verify it against the exact source commit.

## Manual Windows checks

Follow `TEST-PLAN-WINDOWS11.md`. For Mahou text mutation, use disposable documents and verify text, caret, selection, keyboard layout, clipboard and the Mahou-only session counter before and after each operation.

For the input-surface probe, captures are read-only evidence collection. Focus the target field during the countdown, do not type/select/copy/paste/send, and retain the redacted JSON. A report is useful for capability mapping but cannot verify a write path.

For `AGZ-MAH-0007`, no positive user Chrome mutation smoke is requested because no mutation API passed the acceptance gate and no active mutation code remains. The local diagnostic page can still be used to confirm the fail-closed prototype result and event logging if a supervisor requests that limited check.

## Telegram Desktop direct-path investigation

`AGZ-MAH-0008` is accepted as `BLOCKED`. The later read-only snapshot supplies process/control/provider metadata but cannot satisfy any mutation acceptance gate.

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
