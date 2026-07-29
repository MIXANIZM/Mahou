# Release readiness

Original reconciliation task: `AGZ-MAH-0013`

Verification-record update: `AGZ-MAH-0016`

Results:

```text
RELEASE_READINESS_RECONCILED
AUTOSWITCH_CONTAINMENT_VERIFICATION_RECORDED
```

## Scope and evidence boundary

This record reconciles Draft PR #2 with the accepted development line and now
includes the verified AutoSwitch containment merged at exact development head
`a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`.

`AGZ-MAH-0014` remains the immutable historical result for exact source
`363a83b227cfa14e798e442960640e7caed03913` and its candidate:

```text
AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT
```

That artifact destructively produced `gпривет`, could delete the complete token,
and produced split Undo behavior in modern Notepad `RichEditD2DPT`.

`AGZ-MAH-0015` was implemented at task commit
`99e712aa3d913d37e1268ccf65a0cf52aca65ab7` and merged through PR #16 at
`a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d` with the accepted result:

```text
AUTOSWITCH_MODERN_NOTEPAD_CONTAINMENT / VERIFIED / ACCEPTED / MERGED
```

Modern Notepad AutoSwitch support was not added. The verified behavior for exact
`notepad.exe` + `RichEditD2DPT` is strict safe no-op before any AutoSwitch
mutation is scheduled. Chrome and Microsoft Word retain their existing
AutoSwitch conversion routes.

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
| Selected-text conversion | VERIFIED | `3418d09de20ea327302a26858a7b752862bd429e` | Modern Windows build `30134239498`; Security regression `30134239469`; x64 ZIP SHA-256 `c0f0643e251319bc20d9f528f4b199a13f780af7292a45c84dcacd210d328d28`; `Mahou.exe` SHA-256 `b3821d0a3116728db91cd46bf6091a578db19214cea7c3e7b465393b8876a95a` | Accepted `AGZ-MAH-0005` forward/reverse selection smoke, including selection priority and repeated operation | None unless affected runtime changes | No | No |
| Clipboard format preservation | VERIFIED | `3418d09de20ea327302a26858a7b752862bd429e` | Same `AGZ-MAH-0005` runs and artifact | Accepted preservation smoke for Unicode text, Word formatting, images, Excel ranges and Explorer file-drop data | None unless clipboard code changes | No | No |
| Collapsed-caret `Insert` safety and direct routing | VERIFIED | `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda` and `3418d09de20ea327302a26858a7b752862bd429e` | `InsertSafetyRegression` remains part of Modern Windows build; merge-head build `30408387854` succeeded | Accepted no-synthetic-selection/no-op evidence from `AGZ-MAH-0002`; accepted Word and unsupported-surface evidence from `AGZ-MAH-0005` | None while the direct-path boundary remains unchanged | No | No |
| Exact classic Win32 `Edit` direct adapter | VERIFIED | `0b43bb688115e0051114f38a745fce9e830452fd` | Modern Windows build `30128168029`; Security regression `30128168156`; x64 ZIP SHA-256 `e344355bfd51ca5ebfa5f0b0dd24a06af94e02497a3ce39e18ef8a59511c3a6c` | Accepted `AGZ-MAH-0001` direct replacement without visible selection; `AGZ-MAH-0009/0010` separately identified the exact `Edit` family | None while the adapter remains restricted to exact class `Edit` | No | No |
| Microsoft Word document `Range` direct adapter | VERIFIED | `0b43bb688115e0051114f38a745fce9e830452fd` and `3418d09de20ea327302a26858a7b752862bd429e` | Runs `30128168029`, `30128168156`, `30134239498`, and `30134239469` | Accepted Smart Caps replacement without visible selection and accepted collapsed-caret Word behavior | None while runtime remains unchanged | No | No |
| Smart Caps | VERIFIED | `0b43bb688115e0051114f38a745fce9e830452fd` | Modern Windows build `30128168029`; Security regression `30128168156`; x64 ZIP SHA-256 `e344355bfd51ca5ebfa5f0b0dd24a06af94e02497a3ce39e18ef8a59511c3a6c` | Accepted `AGZ-MAH-0001` focused Windows smoke, including positive direct adapters and strict no-op families | None while affected runtime remains unchanged | No | No |
| Personal Smart Caps exception and immediate reversion behavior | VERIFIED | `0b43bb688115e0051114f38a745fce9e830452fd` | Same `AGZ-MAH-0001` runs and artifact | Accepted Backspace restoration, correction/reversion counters, exception learning and restart persistence | None while affected runtime remains unchanged | No | No |
| Protected/password fields | VERIFIED | `0b43bb688115e0051114f38a745fce9e830452fd` and `3418d09de20ea327302a26858a7b752862bd429e` | Same Smart Caps and selected-text runs | Accepted no-op smoke with no text, selection, caret, layout, clipboard or counter change | None while protection logic remains unchanged | No | No |
| Modern Notepad / `RichEditD2DPT` collapsed-caret Insert and Smart Caps no-op | VERIFIED | Safety checkpoint `d4b37a3dac8b4b35d682c82a9ced7b87eaf9adda`; accepted feasibility merge `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1` | `AGZ-MAH-0011` exact-head Security `30314045312`, Input surface probe `30314045300`, Modern Windows build `30314045291` | Accepted no-op evidence applies to the direct collapsed-caret/Smart Caps routes | Keep direct-path no-op; do not claim a Notepad adapter | No | No |
| AutoSwitch containment in modern Notepad `RichEditD2DPT` | VERIFIED | Task `99e712aa3d913d37e1268ccf65a0cf52aca65ab7`; merge `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d` | Exact-head Security `30403716646`, Input surface probe `30403716647`, Modern Windows build `30403716677`; merge-head Security `30408387819`, Modern Windows build `30408387854`; x64 artifact ID `8705704874`; ZIP SHA-256 `85179e55a256dab80ddfb79496aa0e9e3fec202d7f828f9ba7714605ed0a5e9d`; `Mahou.exe` SHA-256 `908252105fb0566322d0c25a70364860fbfe904c847d96dbae1b33b288cd17b0` | Repeated `ghbdtn + space` stayed unchanged; no partial/deferred/hidden-Undo mutation; Chrome and Word continued converting | None for PR #2 while containment and routes remain unchanged. This is strict safe no-op, not Notepad support | No | No |
| Chromium strict no-op without a real selection | VERIFIED | Smart Caps evidence `0b43bb688115e0051114f38a745fce9e830452fd`; accepted browser decision merge `076ee95325809bd0581c9e0f24e9dbb1022c6603` | Accepted `AGZ-MAH-0006/0007` architecture and regression evidence | Accepted Chrome no-op evidence in `AGZ-MAH-0001/0005`; read-only surface evidence in `AGZ-MAH-0010` | No positive Chrome caret-word test; keep strict no-op | No | No |
| Qt / Telegram strict no-op without a real selection | VERIFIED | Smart Caps evidence `0b43bb688115e0051114f38a745fce9e830452fd`; accepted Qt merge `1a930137f7254111f8ef200da3e86c54e697ab5e` | Telegram task head passed Security `30160130485` and Modern Windows build `30160130474` | Accepted Telegram no-op evidence in `AGZ-MAH-0001/0005`; read-only Telegram evidence in `AGZ-MAH-0010`; accepted `AGZ-MAH-0012: DIRECT-PATH-NOT-SAFE` | No positive Telegram mutation test; keep strict no-op | No | No |
| Positive collapsed-caret support smoke in Chrome or Telegram | NOT_APPLICABLE | Architecture decisions `076ee95325809bd0581c9e0f24e9dbb1022c6603` and `1a930137f7254111f8ef200da3e86c54e697ab5e` | Negative architecture gates and source regressions | Strict no-op has already been observed; there is no accepted writer to smoke | Keep obsolete positive-support requirement removed | No | No |
| Generic RichEdit-compatible direct adapter | NOT_APPLICABLE | `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1` | `AGZ-MAH-0011` exact-head runs | Installed-build evidence did not establish a supported external write contract | Do not claim or add a generic RichEdit adapter | No | No |
| Snippets | USER_SMOKE_REQUIRED | Current development line | Current Security regression and Modern Windows build; bounded-source invariants are automated only | No accepted focused runtime record located | Run snippet expansion, cancellation, exclusions, delays and atomic-save scenarios | Yes | No |
| Translator and bounded network behavior | USER_SMOKE_REQUIRED | Current development line | Security regression checks the eight-second timeout, 5000-character limit, disposable clients and maximum three redirects | No accepted live-network Windows record located | Test opt-in translation success, timeout/failure messaging, redirects and absence of sensitive diagnostic output | Yes | No |
| Input history | USER_SMOKE_REQUIRED | Current development line | Builds and source gates pass; high-frequency history writes are intentionally outside `AtomicFile` | No accepted focused runtime record located | Test opt-in recording, Backspace modes, date/hour files and long-session behavior | Yes | No |
| Autorun | USER_SMOKE_REQUIRED | Current development line | Security regression enforces only `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` creation and legacy cleanup | No accepted focused runtime record located | Enable/disable, sign out/in, confirm exact executable command and legacy shortcut/task removal | Yes | No |
| Restart | USER_SMOKE_REQUIRED | Current development line | Security regression forbids CMD/VBS/`taskkill` restart paths and requires the parent-wait path | No accepted focused runtime record located | Test UI/hotkey restart, single-instance handoff and failure message | Yes | No |
| Settings and user-data migration | USER_SMOKE_REQUIRED | Current development line | Security regression checks normalized configuration and application-data path markers | No accepted migration matrix located | Test clean profile, supported old profile, incompatible legacy quarantine, bundled dictionary copy and custom `/C` path | Yes | No |
| Atomic configuration writes and backups | USER_SMOKE_REQUIRED | Current development line | Security regression checks write-through temp files, flush, same-directory replace, `.bak` and cleanup | No accepted physical filesystem/failure-injection record located | Test config, snippets and dictionary replacement plus `.bak` recovery; record the intentional non-atomic input-history exception | Yes | No |
| UI scaling | USER_SMOKE_REQUIRED | Current development line | UI resource regression and Modern Windows build pass | No accepted multi-scale visual record located | Inspect primary and advanced settings at applicable Windows scale values | Yes | No |
| Deterministic Release x86/x64 builds | AUTOMATED_ONLY | Task `99e712aa3d913d37e1268ccf65a0cf52aca65ab7`; merged source `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d` | Exact-head Modern Windows build `30403716677`; merge-head Modern Windows build `30408387854` | Not applicable to byte-comparison property | Rerun for any later runtime/build-input commit and for an exact release candidate | No | Yes |
| Artifact provenance | AUTOMATED_ONLY | Task `99e712aa3d913d37e1268ccf65a0cf52aca65ab7` | Accepted x64 task-head artifact ID `8705704874`, ZIP and executable hashes above | User smoke was performed on this candidate, but it remains a task-head candidate rather than a public release | Build and independently verify an exact release candidate; never relabel the task-head ZIP as merge-head | No | Yes |
| Security regression | AUTOMATED_ONLY | Task `99e712aa3d913d37e1268ccf65a0cf52aca65ab7`; merge `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d` | Exact-head `30403716646` and merge-head `30408387819` — success | Automated source audit only | Require green Security regression at the final PR #2 head and any release candidate | No at the recorded head | Yes for a later release head |
| Authenticode signing | BLOCKED | No signed release commit or artifact | No signing evidence exists | No signed-build installation check exists | Configure approved signing, sign the exact release artifact, and verify signature/chain before publication | No | Yes |
| Final independent release audit | BLOCKED | No completed final-audit commit | Earlier source regressions and `SECURITY-AUDIT-MODERN.md` are not a final independent release audit | None | Perform an independent audit against the exact release candidate and record accepted residual risk | No | Yes |

## Historical defect and current behavior

`AGZ-MAH-0014` must remain attached to its exact source and artifact. It is not
removed, downgraded, or retroactively reclassified after containment.

`AGZ-MAH-0015` closes the specific modern Notepad AutoSwitch containment gate by
proving that the blocked surface emits no immediate or deferred AutoSwitch
mutation. It does not create an exact-range writer, a generic RichEdit adapter,
or positive AutoSwitch support for modern Notepad.

## Runtime source audit

The readiness boundary includes:

- `Mahou/Classes/KMHook.cs`: real-selection priority, direct-only collapsed-caret
  routing, clipboard backup/restore, AutoSwitch, snippets and input history;
- `Mahou/Classes/AutoSwitchSafety.cs`: exact modern Notepad rejection and
  foreground/focus/process/control source-context revalidation;
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

`AGZ-MAH-0016` changes none of those runtime files, no workflow, and no runtime
version marker.

## Current exact automated evidence

Accepted containment task head:

```text
99e712aa3d913d37e1268ccf65a0cf52aca65ab7
```

- Security regression `30403716646`: success;
- Input surface probe `30403716647`: success;
- Modern Windows build `30403716677`: success;
- x64 artifact ID `8705704874`;
- x64 ZIP SHA-256
  `85179e55a256dab80ddfb79496aa0e9e3fec202d7f828f9ba7714605ed0a5e9d`;
- x64 `Mahou.exe` SHA-256
  `908252105fb0566322d0c25a70364860fbfe904c847d96dbae1b33b288cd17b0`.

Accepted merged development head:

```text
a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d
```

- Security regression `30408387819`: success;
- Modern Windows build `30408387854`: success.

The accepted candidate remains an immutable task-head artifact from
`99e712aa3d913d37e1268ccf65a0cf52aca65ab7`. Merge-head CI verifies the merged
source but does not turn that ZIP into a merge-head or public-release artifact.

## Merge gates for Draft PR #2

Accepted Smart Caps, selected-text, clipboard, protected-field, Word, classic
`Edit`, modern Notepad direct-path no-op, AutoSwitch modern Notepad containment,
Chromium no-op and Qt/Telegram no-op records close the corresponding old PR #2
blockers. Positive collapsed-caret Chrome and Telegram testing is not a gate
because the accepted architecture has no writer for those surfaces.

Before PR #2 can be considered for Ready/merge, the remaining merge gates are:

1. complete and record the focused real-Windows smoke for snippets,
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

No Ready transition, merge, tag, signing, Release or publication is authorized
by this record.
