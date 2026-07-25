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

The suggested one-shot capture set is documented in `docs/INPUT-SURFACE-PROBE.md`: classic Win32 Edit, Word, modern Notepad, Chrome input/textarea/contenteditable, Telegram Desktop, WhatsApp Desktop, and one WPF/WinUI application if available. No text mutation test is part of `AGZ-MAH-0009`.

## Chrome editing-core workflow

The read-only `Chrome extension editing core` workflow runs the static Python gate and the dependency-free Node contract tests for changes to the prototype, diagnostic page, decision document, or their tests.

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

`AGZ-MAH-0009` must leave both the existing `Security regression` and `Modern Windows build` green because it changes repository code/CI/docs while leaving Mahou runtime sources and runtime version untouched.

## Artifact handoff

Follow `ARTIFACT-PROVENANCE.md` for Mahou runtime builds. Before giving a Mahou ZIP to a user, run `.github/scripts/verify-artifact-provenance.ps1` with the expected full commit, tree, platform, repository, and runtime version. When the retained incident archive is available, `.github/scripts/test-artifact-provenance.ps1 -LegacyZipPath <path>` must reject the real `0f9b75c...` ZIP for the current expected source.

The probe is a separate non-runtime artifact. Its own ZIP must match the exact name, source commit/tree, embedded probe commit, `probe-manifest.json`, `SHA256SUMS.txt`, external `.zip.sha256`, and workflow evidence produced by `input-surface-probe.yml`.

After merge, never represent a PR-head artifact as a merge-head artifact. Run a new `workflow_dispatch` on `mixanizm-modern-v2.9.0.1` only after that branch points to the exact merged commit, then verify the new run and artifact against that commit.

## Manual Windows checks

Follow `TEST-PLAN-WINDOWS11.md`. For Mahou text mutation, use disposable documents and verify text, caret, selection, keyboard layout, clipboard and the Mahou-only session counter before and after each operation.

For `AGZ-MAH-0009`, manual captures are read-only evidence collection. Focus the target field during the countdown, do not type/select/copy/paste/send, and retain the redacted JSON. A report is useful for capability mapping but cannot verify a write path.

For `AGZ-MAH-0007`, no positive user Chrome mutation smoke is requested because no mutation API passed the acceptance gate and no active mutation code remains. The local diagnostic page can still be used to confirm the fail-closed prototype result and event logging if a supervisor requests that limited check.

## Telegram Desktop direct-path investigation

`AGZ-MAH-0008` is accepted as `BLOCKED` because no interactive Windows Telegram process was available to the executor. The architecture record is `docs/TELEGRAM-SMART-CAPS-ARCHITECTURE.md`; no Telegram adapter or mutation test exists.

A future continuation must use the exact installed Telegram Desktop for Windows build and only the ordinary new-message composer in Saved Messages or another dedicated private test chat. A read-only `AGZ-MAH-0009` snapshot may supply initial process/control/provider metadata, but it cannot satisfy any mutation acceptance gate.

Before any mutation a future task must record and revalidate:

- executable path, file and product version, digital signature, architecture, install source, foreground window, focused control, HWND classes, and process ID;
- UI Automation ancestry, control type, automation ID, name, framework ID, class name, password/protected state, and all available patterns;
- MSAA role/state and IAccessible2 interface availability;
- exact full fixed test text, exact collapsed caret, exact selection state, target word boundaries, delimiter, composition state, candidate age, and active-chat identity;
- ordinary new-message mode rather than search, caption, edit-message, forward comment, passcode, or another field.

A writable primitive is acceptable only after a real bounded test proves all of the following without programmatic selection, keyboard simulation, clipboard access, or whole-field replacement:

- only the exact target word changes;
- prefix and suffix remain identical;
- the caret is exact and selection remains collapsed;
- formatting entities and draft state remain intact;
- the active composer and active chat remain unchanged;
- no message is sent;
- the complete post-state can be re-read and verified;
- one normal Telegram undo action reverts exactly the correction;
- stale window, focus, chat, element, source, caret, composition, or candidate state causes a complete no-op.

Reading text or caret, capability classification, or merely detecting `IAccessibleEditableText` is not proof of a safe write path. If any required property is uncertain, the test must stop and Telegram must remain no-op.

A feature is `VERIFIED` only after its exact commit passes applicable CI and the user confirms its required real behavior. A negative architecture decision can be accepted when the rejected methods, automated checks, environment limitations, and strict no-op result are all recorded.
