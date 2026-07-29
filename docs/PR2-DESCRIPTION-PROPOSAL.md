# Proposed replacement body for Draft PR #2

This file is a proposal only. `AGZ-MAH-0016` does not update PR #2 metadata.

---

Modernization and hardening branch based on the latest preserved Mahou
development line rather than the obsolete `iamkarlson/Mahou` v1.4.3.0 source.

## Current baseline

- head branch: `mixanizm-modern-v2.9.0.1`;
- accepted development head: `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`;
- assembly line: `2.9.0.1-dev`;
- .NET Framework 4.8;
- modern tabbed settings UI, bundled standalone AutoSwitch dictionary, input
  history, selected-text conversion, translation panel and advanced layout
  controls retained;
- Release x86 and x64 are built independently twice and compared
  byte-for-byte.

`AGZ-MAH-0013` was accepted and merged through PR #15 at
`363a83b227cfa14e798e442960640e7caed03913`; it reconciled this description and
the release-readiness matrix without changing runtime behavior.

`AGZ-MAH-0015` was accepted and merged through PR #16 at
`a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`; it contains AutoSwitch before any
mutation on the exact modern Notepad surface. This is strict safe no-op
containment, not positive modern Notepad AutoSwitch support.

## Current text-mutation behavior

### Existing real user selection

An existing real user-created selection has priority. The selected-text
pipeline converts the selected text and preserves the complete OLE clipboard
object. If a safe clipboard snapshot cannot be obtained, the selected-text
operation fails closed.

Selected-text conversion and preservation of tested Unicode text, Word
formatting, images, Excel ranges and Explorer file-drop data were accepted
under `AGZ-MAH-0005`.

### Collapsed caret

Collapsed-caret `Insert` does not create a synthetic selection and does not use
keyboard selection, clipboard mutation, deletion/retyping, a tracked-word
fallback or a compatibility selection fallback.

The only supported direct collapsed-caret adapters are:

1. exact classic Win32 class `Edit`, using bounded native read/replacement with
   foreground, focus, caret and source revalidation;
2. Microsoft Word document `Range`, using direct range replacement without
   selecting the word.

The caret-word detector handles the caret before, inside or immediately after a
word and through a small adjacent horizontal whitespace gap. The target is
bounded to 256 characters.

If the selection state is unknown, the field is protected, the direct adapter
is unavailable, or any revalidation fails, the operation is a no-op.

### Unsupported collapsed-caret families

Without a real user-created selection, these families fail closed and do not
change text, selection, caret, layout or clipboard:

- modern Notepad / `RichEditD2DPT` and generic RichEdit variants;
- Chromium inputs, textareas and contenteditable surfaces;
- Qt / Telegram and other custom Qt editors;
- Electron / WebView;
- WhatsApp Desktop bridge surfaces;
- WPF;
- WinUI/UWP;
- unknown, custom-drawn and protected controls.

There is no generic RichEdit-compatible direct adapter.

## Smart Caps

Smart Caps is opt-in, local-only and disabled on a clean profile. It corrects
accidental interior capitals only in a fresh word and only through the exact
classic `Edit` or Microsoft Word `Range` direct adapter.

All-caps, mixed-script, numeric, URL/address-like, stale, excluded and protected
inputs are skipped. Immediate physical Backspace reverses a Mahou correction.
Two explicit reversions learn a local personal exception. Session counters
record only corrections and reversions performed by Mahou.

`AGZ-MAH-0001` accepted the focused real-Windows smoke for the positive direct
adapters, reversal and exception behavior, protected fields and strict no-op in
modern Notepad, Chrome and Telegram.

## User snippets removal and independent AutoSwitch

`AGZ-MAH-0018` recorded:

```text
SNIPPETS_TRIGGER_REPLACEMENT_FAIL
```

At exact source `f02909611eb9a4502e9fe4d8fda9009922f352fd`, simple replacement was corrupted, multiline left `agz1`, and delayed replacement left `agz`. The remaining snippets smoke was stopped.

Draft PR #18 implements:

```text
USER_SNIPPETS_REMOVED
AUTOSWITCH_DECOUPLED
```

The snippets tab, controls, trigger engine, expressions, hotkeys, exclusions, sounds, persistence and runtime `snippets.txt` access are removed. Existing legacy files and obsolete keys are left untouched and inactive. AutoSwitch uses only its own settings and `AS_dict.txt`, with literal dictionary output and source-context revalidation. A focused exact-candidate physical-Windows smoke remains required before this new source is accepted.

## AutoSwitch containment in modern Notepad

`AGZ-MAH-0014` remains the immutable historical result for exact source
`363a83b227cfa14e798e442960640e7caed03913` and x64 artifact `8690549591`:

```text
AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT
```

That artifact produced `gпривет`, complete deletion on another attempt, and
split Undo in modern Notepad `RichEditD2DPT`, while Chrome and Microsoft Word
passed the same focused test.

`AGZ-MAH-0015` added a narrow fail-closed guard. Before dictionary routing,
AutoSwitch captures foreground window, focused control, process, executable,
control class and protected state. Exact `notepad.exe` + `RichEditD2DPT` is
rejected before Backspace, deletion, layout switching, replacement or deferred
callbacks can be scheduled. Immediate and delayed AutoSwitch operations also
revalidate the same source identity; focus or control changes cancel pending
work.

The accepted physical-Windows smoke confirmed:

- repeated modern Notepad `ghbdtn + space` remained unchanged;
- no `gпривет`, deletion or partial replacement occurred;
- rapid window switching produced no deferred mutation;
- Undo contained no hidden AutoSwitch operation;
- Chrome continued converting `ghbdtn` to `привет`;
- Microsoft Word continued converting `ghbdtn` to `привет`.

This result does not add a Notepad adapter, a generic RichEdit writer, or
positive modern Notepad AutoSwitch support. It does not change manual Insert,
Smart Caps, selected-text conversion or clipboard behavior. User snippets are removed separately under AGZ-MAH-0019. PR #3 is
not used.

## Accepted input-surface decisions

- `AGZ-MAH-0006`: Chrome desktop-only path — `DIRECT-PATH-NOT-SAFE`;
- `AGZ-MAH-0007`: Chrome browser-context editing core —
  `BROWSER-CONTEXT-MUTATION-NOT-SAFE`, merged through PR #9 at
  `076ee95325809bd0581c9e0f24e9dbb1022c6603`;
- `AGZ-MAH-0008`: Telegram direct-path investigation — accepted `BLOCKED`;
- `AGZ-MAH-0009/0010`: real-Windows read-only capability evidence completed;
- `AGZ-MAH-0011`: modern Notepad RichEdit direct path —
  `DIRECT-PATH-NOT-SAFE`, merged through PR #13 at
  `d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1`;
- `AGZ-MAH-0012`: Qt Windows editable-text path —
  `DIRECT-PATH-NOT-SAFE`, merged through PR #14 at
  `1a930137f7254111f8ef200da3e86c54e697ab5e`;
- `AGZ-MAH-0014`: AutoSwitch failure —
  `AUTOSWITCH_FAIL / HISTORICAL DEFECT ARTIFACT`;
- `AGZ-MAH-0015`: exact modern Notepad containment —
  `AUTOSWITCH_MODERN_NOTEPAD_CONTAINMENT / VERIFIED / ACCEPTED / MERGED`,
  merged through PR #16 at
  `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`.

Readable UIA `TextPattern`, `TextPattern2`, `ValuePattern`, caret or text
metadata does not establish a safe write primitive. Whole-control value writes
and programmatic selection remain forbidden.

PR #3 is not an accepted Notepad implementation. Its attempted Notepad work is
superseded by `AGZ-MAH-0011: DIRECT-PATH-NOT-SAFE`; PR #3 remains untouched.

## Security and reliability hardening

- legacy self-updater, remote extraction and public sync/backup paths are
  disabled;
- configuration and user data use `%APPDATA%\MIXANIZM Mahou`; logs use
  `%LOCALAPPDATA%\MIXANIZM Mahou\Logs`;
- proxy password is protected with Windows DPAPI and masked in the UI;
- autorun uses only
  `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`;
- restart launches the same executable with a bounded parent-wait argument and
  does not create CMD/VBS files or invoke `taskkill`;
- user-defined snippets, their UI, parser, expression commands, hotkeys, persistence and execution paths are removed;
- AutoSwitch, Smart Caps, translator and input history remain opt-in;
- invalid INI values are normalized and reads use a synchronized in-memory
  index;
- low-frequency configuration/user-data writes use flushed same-directory
  temporary files and backups where applicable;
- AutoSwitch deferred timeouts remain bounded; removed snippet delay/keyboard/repetition paths no longer exist;
- translator requests use disposable clients, an eight-second timeout, a
  5000-character input limit and at most three redirects;
- Release builds are deterministic, no-PDB and warnings-as-errors, with an
  embedded `asInvoker`, `uiAccess=false` manifest;
- source regression gates prevent removed unsafe behavior from returning;
- AutoSwitch independence, literal-replacement, source-context and modern Notepad containment regressions run in Security regression and Modern Windows build.

## Accepted runtime evidence

### Smart Caps

- exact source: `0b43bb688115e0051114f38a745fce9e830452fd`;
- Modern Windows build: `30128168029`;
- Security regression: `30128168156`;
- x64 artifact ZIP SHA-256:
  `e344355bfd51ca5ebfa5f0b0dd24a06af94e02497a3ce39e18ef8a59511c3a6c`;
- focused real-Windows smoke: accepted.

### Selected text and clipboard

- exact source: `3418d09de20ea327302a26858a7b752862bd429e`;
- Modern Windows build: `30134239498`;
- Security regression: `30134239469`;
- x64 artifact ZIP SHA-256:
  `c0f0643e251319bc20d9f528f4b199a13f780af7292a45c84dcacd210d328d28`;
- `Mahou.exe` SHA-256:
  `b3821d0a3116728db91cd46bf6091a578db19214cea7c3e7b465393b8876a95a`;
- focused real-Windows smoke: accepted.

### AutoSwitch modern Notepad containment

- task commit: `99e712aa3d913d37e1268ccf65a0cf52aca65ab7`;
- implementation PR: #16;
- merge commit: `a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`;
- exact-head Security regression `30403716646`: success;
- exact-head Input surface probe `30403716647`: success;
- exact-head Modern Windows build `30403716677`: success;
- merge-head Security regression `30408387819`: success;
- merge-head Modern Windows build `30408387854`: success;
- x64 artifact ID: `8705704874`;
- x64 ZIP SHA-256:
  `85179e55a256dab80ddfb79496aa0e9e3fec202d7f828f9ba7714605ed0a5e9d`;
- x64 `Mahou.exe` SHA-256:
  `908252105fb0566322d0c25a70364860fbfe904c847d96dbae1b33b288cd17b0`;
- focused real-Windows containment smoke: accepted.

The x64 candidate is an immutable task-head artifact from
`99e712aa3d913d37e1268ccf65a0cf52aca65ab7`. Merge-head CI verifies the merged
source but does not relabel that ZIP as a merge-head or release artifact.

## Current exact automated evidence

Accepted development head:
`a10cb8fe4fb6e203fe24c47b53ed97f1df7a556d`.

- Security regression `30408387819`: success;
- Modern Windows build `30408387854`: success.

The accepted AutoSwitch candidate evidence is recorded above. These are not a
public release and do not represent user verification of every retained
feature.

## Remaining gates before PR #2 merge

The old requirements for positive collapsed-caret Chrome/Telegram mutation
testing, compatibility-fallback identification, a generic RichEdit adapter,
already accepted Smart Caps/selected-text/clipboard scenarios, and modern
Notepad AutoSwitch containment are closed or not applicable.

PR #2 still requires:

1. focused AGZ-MAH-0019 AutoSwitch/removal smoke, then retained translator/network, input-history, autorun, restart, settings-migration, atomic-save/backup and UI-scaling smoke;
2. green Security regression and Modern Windows build at the final PR #2 head;
3. supervisor review of the reconciled release-readiness record and PR
   description;
4. explicit permission before changing PR #2 from Draft or merging it.

## Additional gates before public release

Public release additionally requires:

1. an exact accepted release-candidate commit and fresh deterministic x86/x64
   artifacts;
2. independent provenance verification of the exact candidate and a fresh build
   if the release tree changes after merge;
3. Authenticode signing and signature verification;
4. a final independent security/release audit with documented residual risk;
5. separate explicit permission for tag, release ZIP, GitHub Release and
   publication.

Draft only. Do not mark Ready, merge, tag, sign, release or publish until the
applicable gates and permission-gates are complete.
