# Release readiness

Task: `AGZ-MAH-0013`

Result:

```text
RELEASE_READINESS_RECONCILED
```

## Scope and evidence boundary

This record reconciles Draft PR #2 with the accepted development line at exact
commit `1a930137f7254111f8ef200da3e86c54e697ab5e`, where `AGZ-MAH-0012`
was accepted and merged through PR #14.

The Mahou runtime subtree at that commit is
`0fb3ede030b025f11c163997557d9f49ee88bf2d`. The runtime version file
`Mahou/Properties/AssemblyInfo.cs` is blob
`de35d9ae6dce4a0c5b98e9ce6496b90c9efb8ebe`. Both are unchanged by
`AGZ-MAH-0013`.

Only the following readiness statuses are used:

- `VERIFIED` — the applicable automated and real-Windows evidence exists;
- `AUTOMATED_ONLY` — exact-source automated evidence exists, but it is not
  represented as user verification;
- `USER_SMOKE_REQUIRED` — the retained feature still needs a focused physical
  Windows check;
- `BLOCKED` — a required gate has not been completed;
- `NOT_APPLICABLE` — the old requirement no longer applies to the accepted
  architecture.

## Evidence matrix

| Capability or gate | Status | Exact evidence commit | CI or artifact evidence | Real-Windows evidence | Remaining action | Blocks PR #2 merge | Blocks only public release |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Selected-text conversion | VERIFIED | `3418d09de20ea327302a26858a7b752862bd429e` (runtime tree is identical at `1a930137f7254111f8ef200da3e86c54e697ab5e`) | Modern Windows build `30134239498`; Security regression `30134239469`; x64 artifact ZIP SHA-256 `c0f0643e251319bc20d9f528f4b199a13f780af7292a45c84dcacd210d328d28`; `Mahou.exe` SHA-256 `b3821d0a3116728db91cd46bf6091a578db19214cea7c3e7b465393b8876a95a` | Accepted `AGZ-MAH-0005` forward/reverse selection smoke, including selection priority and repeated operation | None for PR #2; repeat only if runtime changes | No | No |
| Clipboard format preservation | VERIFIED | `3418d09de20ea327302a26858a7b752862bd429e` | Same `AGZ-MAH-0005` runs and artifact as selected-text conversion | Accepted preservation smoke for Unicode text, Word formatting, images, Excel ranges and Explorer file-drop data | None for PR #2; repeat only if clipboard code changes | No | No |
| Collapsed-caret `Insert` safety and direct routing | VERIFIED | `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`, `3418d09de20ea327302a26858a7b752862bd429e`, and unchanged runtime tree at `1a930137f7254111f8ef200da3e86c54e697ab5e` | Current Security regression `30318062370`; Modern Windows build `30318062373`; `InsertSafetyRegression` runs in both x86/x64 jobs | Accepted no-synthetic-selection/no-op evidence from `AGZ-MAH-0002`; accepted Word caret-word and unsupported no-selection evidence from `AGZ-MAH-0005` | None while the runtime tree remains unchanged | No | No |
| Exact classic Win32 `Edit` direct adapter | VERIFIED | `0b43bb688115e0051114f38a745fce9e830452fd` (same runtime tree at current head) | Modern Windows build `30128168029`; Security regression `30128168156`; x64 artifact ZIP SHA-256 `e344355bfd51ca5ebfa5f0b0dd24a06af94e02497a3ce39e18ef8a59511c3a6c` | Accepted `AGZ-MAH-0001` direct replacement without visible selection; `AGZ-MAH-0009/0010` separately identified the exact `Edit` family | None while the adapter remains restricted to exact class `Edit` | No | No |
| Microsoft Word document `Range` direct adapter | VERIFIED | `0b43bb688115e0051114f38a745fce9e830452fd` and `3418d09de20ea327302a26858a7b752862bd429e` | Runs `30128168029`, `30128168156`, `30134239498`, and `30134239469` | Accepted Smart Caps replacement without visible selection and accepted collapsed-caret Word behavior | None while runtime remains unchanged | No | No |
| Smart Caps | VERIFIED | `0b43bb688115e0051114f38a745fce9e830452fd` | Modern Windows build `30128168029`; Security regression `30128168156`; x64 artifact ZIP SHA-256 `e344355bfd51ca5ebfa5f0b0dd24a06af94e02497a3ce39e18ef8a59511c3a6c` | Accepted `AGZ-MAH-0001` focused Windows smoke, including positive direct adapters and strict no-op families | None while runtime remains unchanged | No | No |
| Personal Smart Caps exception and immediate reversion behavior | VERIFIED | `0b43bb688115e0051114f38a745fce9e830452fd` | Same `AGZ-MAH-0001` runs and artifact | Accepted Backspace restoration, `+8` corrections / `+2` reversions, exception learning after two rejections, and persistence after restart | None while runtime remains unchanged | No | No |
| Protected/password fields | VERIFIED | `0b43bb688115e0051114f38a745fce9e830452fd` and `3418d09de20ea327302a26858a7b752862bd429e` | Same Smart Caps and selected-text runs | Accepted no-op smoke with no text, selection, caret, layout, clipboard or counter change | None while protection logic remains unchanged | No | No |
| Modern Notepad / `RichEditD2DPT` strict no-op | VERIFIED | Safety checkpoint `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`; accepted feasibility merge `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1` | `AGZ-MAH-0011` runs: Security `30314045312`, Modern Windows build `30314045291`, Input surface probe `30314045300` | Accepted safe no-op evidence in `AGZ-MAH-0001/0002`; read-only installed-build evidence in `AGZ-MAH-0011` | No positive mutation smoke; keep strict no-op. PR #3 is superseded evidence, not an adapter | No | No |
| Chromium strict no-op without a real selection | VERIFIED | Smart Caps evidence `0b43bb688115e0051114f38a745fce9e830452fd`; accepted browser decision merge `076ee95325809bd0581c9e0f24e9dbb1022c6603` | Accepted `AGZ-MAH-0006/0007` architecture and regression evidence; current Security regression `30318062370` | Accepted Chrome no-op evidence in `AGZ-MAH-0001/0005`; read-only surface evidence in `AGZ-MAH-0010` | No positive Chrome caret-word test. Keep strict no-op | No | No |
| Qt / Telegram strict no-op without a real selection | VERIFIED | Smart Caps evidence `0b43bb688115e0051114f38a745fce9e830452fd`; accepted Qt merge `1a930137f7254111f8ef200da3e86c54e697ab5e` | Telegram task head `0e5dd3c58a785aad3c19fc447d1451f89f4e12fa` passed Security `30160130485` and Modern Windows build `30160130474`; current base passed `30318062370` and `30318062373` | Accepted Telegram no-op evidence in `AGZ-MAH-0001/0005`; read-only Telegram 7.0.5 evidence in `AGZ-MAH-0010`; accepted `AGZ-MAH-0012: DIRECT-PATH-NOT-SAFE` | No positive Telegram mutation test. Keep strict no-op | No | No |
| Positive collapsed-caret support smoke in Chrome or Telegram | NOT_APPLICABLE | Architecture decisions `076ee95325809bd0581c9e0f24e9dbb1022c6603` and `1a930137f7254111f8ef200da3e86c54e697ab5e` | Negative architecture gates and current source regression | Strict no-op has already been observed; there is no accepted writer to smoke | Remove this obsolete requirement from PR #2 | No | No |
| Generic RichEdit-compatible direct adapter | NOT_APPLICABLE | `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1` | `AGZ-MAH-0011` exact-head runs listed above | Installed-build evidence did not establish a supported external write contract | Do not claim or add a generic RichEdit adapter | No | No |
| AutoSwitch and bundled dictionary | USER_SMOKE_REQUIRED | Source and current exact head `1a930137f7254111f8ef200da3e86c54e697ab5e` | Current Security regression `30318062370`; Modern Windows build `30318062373` proves build/static gates only | No accepted focused runtime record located | Run the `TEST-PLAN-WINDOWS11.md` AutoSwitch/bundled-dictionary scenarios | Yes | No |
| Snippets | USER_SMOKE_REQUIRED | `1a930137f7254111f8ef200da3e86c54e697ab5e` | Current Security regression and Modern Windows build; bounded-source invariants are automated only | No accepted focused runtime record located | Run snippet expansion, cancellation, exclusions, delays and atomic-save scenarios | Yes | No |
| Translator and bounded network behavior | USER_SMOKE_REQUIRED | `1a930137f7254111f8ef200da3e86c54e697ab5e` | Security regression checks the 8-second timeout, 5000-character limit, disposable clients and maximum three redirects | No accepted live-network Windows record located | Test opt-in translation success, timeout/failure messaging, redirects and absence of sensitive diagnostic output | Yes | No |
| Input history | USER_SMOKE_REQUIRED | `1a930137f7254111f8ef200da3e86c54e697ab5e` | Builds and source gates pass; high-frequency history writes are intentionally outside `AtomicFile` | No accepted focused runtime record located | Test opt-in recording, Backspace modes, date/hour files and long-session behavior | Yes | No |
| Autorun | USER_SMOKE_REQUIRED | `1a930137f7254111f8ef200da3e86c54e697ab5e` | Security regression enforces only `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` creation and legacy cleanup | No accepted focused runtime record located | Enable/disable, sign out/in, confirm exact executable command and legacy shortcut/task removal | Yes | No |
| Restart | USER_SMOKE_REQUIRED | `1a930137f7254111f8ef200da3e86c54e697ab5e` | Security regression forbids CMD/VBS/`taskkill` restart paths and requires the parent-wait path | No accepted focused runtime record located | Test UI/hotkey restart, single-instance handoff and failure message | Yes | No |
| Settings and user-data migration | USER_SMOKE_REQUIRED | `1a930137f7254111f8ef200da3e86c54e697ab5e` | Security regression checks normalized configuration and application-data path markers | No accepted migration matrix located | Test clean profile, supported old profile, incompatible legacy quarantine, bundled dictionary copy and custom `/C` path | Yes | No |
| Atomic configuration writes and backups | USER_SMOKE_REQUIRED | `1a930137f7254111f8ef200da3e86c54e697ab5e` | Security regression checks write-through temp files, flush, same-directory replace, `.bak` and cleanup | No accepted physical filesystem/failure-injection record located | Test config, snippets and dictionary replacement plus `.bak` recovery; record the intentional non-atomic input-history exception | Yes | No |
| UI scaling | USER_SMOKE_REQUIRED | `1a930137f7254111f8ef200da3e86c54e697ab5e` | UI resource regression and Modern Windows build pass | No accepted multi-scale visual record located | Inspect primary and advanced settings at applicable Windows scale values | Yes | No |
| Deterministic Release x86/x64 builds | AUTOMATED_ONLY | `1a930137f7254111f8ef200da3e86c54e697ab5e` | Modern Windows build `30318062373`; two isolated builds per platform matched. x86 artifact ID `8672915631`, digest `sha256:0042a3c86d4a322ce47b1760503ebe8e1c331fde4e976ddd37fa793e1cabc80b`; x64 artifact ID `8672919479`, digest `sha256:0d972d637c5b3967a5a82ba979776031c24535d502023be664eaecaf7d29bb97` | Not applicable to the byte-comparison property | No merge action while exact-head CI remains green; rerun for any later runtime/build-input commit | No | Yes, because a release needs an exact release-candidate build |
| Artifact provenance | AUTOMATED_ONLY | Provenance implementation `42d6768fb505108381f78e3c6d986d6e8804d7ae`; current evidence `1a930137f7254111f8ef200da3e86c54e697ab5e` | Current build/evidence artifacts: x86 evidence ID `8672916167`, digest `sha256:08aa2d97a1def0ad91d2fac99824b1f616e32a20bd3d8e9e1fc4f70582aec35b`; x64 evidence ID `8672919965`, digest `sha256:3dbd2db7d898058d0ff45e4d02f6b8858c0db7b7d3e018462887c74bb039adfe` | No user runtime behavior is claimed | For public release, build and independently verify an exact accepted release candidate; do not relabel PR-head artifacts after merge | No | Yes |
| Security regression | AUTOMATED_ONLY | `1a930137f7254111f8ef200da3e86c54e697ab5e` | Security regression run `30318062370` — success; the same gate also ran inside both jobs of `30318062373` | Automated source audit only | Require green exact-head Security regression for the final PR #2 head and any release candidate | No at the recorded head | Yes for a later release head |
| Authenticode signing | BLOCKED | No signed release commit or artifact | No signing evidence exists | No signed-build installation check exists | Configure approved signing, sign the exact release artifact, and verify signature/chain before publication | No | Yes |
| Final independent release audit | BLOCKED | No completed final-audit commit | Earlier source regression and `SECURITY-AUDIT-MODERN.md` are not a final independent release audit | None | Perform an independent audit against the exact release candidate and record accepted residual risk | No | Yes |

## Runtime source audit

The claims above were checked against the current runtime source, including:

- `Mahou/Classes/KMHook.cs`: real-selection priority, strict handling of
  `Unknown`, direct-only collapsed-caret routing, clipboard backup/restore,
  AutoSwitch, snippets and input history;
- `Mahou/Classes/SelectionProbe.cs`: exact `Edit` class restriction, native
  direct replacement, Word `Range`, read-only UIA selection-state probing and
  password suppression;
- `Mahou/Classes/SmartCaps.cs`: direct-adapter-only correction/reversion and
  personal exceptions;
- `Mahou/Classes/NativeClipboard.cs`: non-materializing OLE clipboard snapshot
  and bounded restore;
- `Mahou/Classes/StartupManager.cs`, `Mahou/Program.cs` and `Mahou/MahouUI.cs`:
  autorun and restart;
- `Mahou/Classes/UserDataPaths.cs`, `Mahou/Classes/Configs.cs` and
  `Mahou/Classes/AtomicFile.cs`: migration, normalization and atomic writes;
- `Mahou/TranslatePanel.cs`: bounded translator client behavior.

One stale source comment in `Mahou/Classes/SelectionProbe.cs` still says that
clipboard conversion is a compatibility fallback. The executable
`ConvertSelectionOrLastWord` path does not do that: `Unknown` is a no-op and a
collapsed caret reaches only the exact `Edit` and Word `Range` strategies.
`.github/scripts/security-regression.py` enforces that boundary. The comment is
recorded here but is not changed because this task must leave the runtime
subtree unchanged.

## Merge gates for Draft PR #2

The accepted Smart Caps, selected-text, clipboard, protected-field, Word,
classic `Edit`, modern Notepad no-op, Chromium no-op and Qt/Telegram no-op
records close the corresponding old PR #2 blockers. Positive collapsed-caret
Chrome and Telegram testing is not a gate because the accepted architecture has
no writer for those surfaces.

Before PR #2 can be considered for Ready/merge, the remaining merge gates are:

1. complete and record the focused real-Windows smoke for AutoSwitch, snippets,
   translator/network behavior, input history, autorun, restart, settings
   migration, atomic save/backup behavior and UI scaling;
2. keep Security regression and Modern Windows build green at the final PR #2
   head;
3. apply an accepted, accurate PR #2 description and complete supervisor review;
4. obtain explicit permission before marking PR #2 Ready or merging it.

PR #2 remains Draft.

## Public release gates

Public release additionally requires:

1. an exact accepted release-candidate commit and fresh deterministic x86/x64
   build;
2. independent artifact-provenance verification for that exact candidate and a
   fresh build if the release tree changes after merge;
3. Authenticode signing and signature verification;
4. a final independent security/release audit with documented residual risk;
5. explicit, separate permission for tag, release ZIP, GitHub Release and any
   publication.

No tag, signing, Release or publication is authorized by this record.
