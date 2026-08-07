# Architecture

## Runtime shape

Mahou is a .NET Framework 4.8 Windows Forms application with low-level keyboard and raw-input processing, local configuration, optional translation, standalone AutoSwitch, and manual layout conversion.

## User snippets removal and AutoSwitch independence

`AGZ-MAH-0018` established `SNIPPETS_TRIGGER_REPLACEMENT_FAIL` on physical Windows. The product does not retain or repair user-defined snippets. `AGZ-MAH-0019` removes the snippets tab, configuration surface, trigger collection, parser, regex/multiline expansion, expression commands, clipboard/process/keyboard/delay capabilities, hotkeys, sounds, exclusions, reload and persistence paths.

Legacy `snippets.txt`, `snippets.txt.bak` and old INI values are inactive rollback data. The runtime neither reads nor writes nor deletes them. A clean profile does not generate a snippets file.

AutoSwitch is a separate subsystem:

- it is enabled only by its own settings and uses only `AS_dict.txt`;
- keyboard routing and source buffering are outside every legacy snippets condition;
- dictionary values are passed to a dedicated literal Unicode input builder and are never parsed as commands or expressions;
- old `SnippetsEnabled=true` and `SnippetsEnabled=false` values have no runtime effect;
- foreground HWND, focused-control HWND, PID, executable, class and protected state are captured before routing and revalidated before every mutation stage;
- unknown, stale, protected or changed context fails closed;
- exact `notepad.exe` + `RichEditD2DPT` remains rejected before any mutation is scheduled.

This refactor does not establish positive modern Notepad support. Chrome and Microsoft Word remain the positive focused-smoke targets for the existing AutoSwitch conversion behavior.

### AutoSwitch dictionary startup boundary

The exact AGZ-MAH-0019 candidate at `5527f662cb844ba90d264f5b93cb27f8334bf295` is rejected as `USER_SMOKE_FAILED / STARTUP_HANG_HIGH_CPU`. Its loader copied the complete remaining dictionary suffix for every rule, searched each suffix again, separately reparsed the dictionary for the UI count, and allocated three character-sized Boolean arrays. The real 5,188,519-character dictionary contains exactly 151,429 complete rules/aliases and 18 comments, so that startup path became effectively quadratic on the UI thread.

`AGZ-MAH-0020` replaces that path with `AutoSwitchDictionaryParser`. The parser owns one monotonic cursor, uses bounded ordinal searches from explicit indexes, allocates completed aliases and replacements only, preserves rule/alias order and duplicate first-match behavior, normalizes CRLF replacements to the legacy LF representation, and accepts the embedded BOM between bundled dictionary sections. A malformed or incomplete rule returns an empty failed result; `KMHook` publishes no partial arrays.

`LoadConfigs` reads and parses `AS_dict.txt` once when AutoSwitch is enabled and derives the UI count from the same result. Programmatic dictionary textbox updates are suppressed while configuration is loading. Disabling AutoSwitch clears the active arrays without parsing, modifying, or deleting the dictionary file.

## Text-mutation safety boundary

Text controls are treated by capability, not by a permissive class-name guess.

- **User-created selection:** handled by the legacy selected-text pipeline and remains a separate hardening area.
- **Collapsed caret, classic Win32 `Edit`:** bounded native read and exact direct replacement with foreground, focus, caret, and source revalidation.
- **Collapsed caret, Microsoft Word:** bounded direct document `Range` replacement.
- **All other controls:** strict no-op until a dedicated direct adapter is proven.

The collapsed-caret path must never use synthetic selection, UI Automation `.Select()`, keyboard selection, clipboard mutation, tracked-word deletion/retyping, or compatibility fallback.

## Input-surface capability probe

`AGZ-MAH-0009` adds a separate executable under `tools/input-surface-probe/`. It is not referenced by the Mahou solution and does not participate in input processing.

The probe performs one read-only capture after a countdown and exits. It inspects only the foreground process and focused control. It records redacted process/window/UIA/MSAA/IAccessible2 capability metadata, protected state, possible caret/selection/length readability and a deterministic capability fingerprint. It never retrieves actual text, window titles, UIA Name content, URLs, clipboard content or passwords.

The normalized families are evidence labels only:

- `CLASSIC_WIN32_EDIT`;
- `RICHEDIT`;
- `WORD_OBJECT_MODEL`;
- `WPF`;
- `WINUI_UWP`;
- `CHROMIUM_BROWSER`;
- `ELECTRON_WEBVIEW`;
- `QT_CUSTOM`;
- `CUSTOM_UNKNOWN`.

No classification, process filename, framework, pattern, readable caret, readable length or editable-interface presence can activate a Mahou mutation path. A dedicated adapter remains mandatory for every family except the already verified exact classic `Edit` and Word adapters.

Detailed boundaries are in `docs/INPUT-SURFACE-CAPABILITY-MAP.md` and `docs/INPUT-SURFACE-PROBE.md`.

## Real-Windows surface evidence

`AGZ-MAH-0010` records the completed user probe from source commit `cef38006dfe6093ee1b233703ad79215bb2a9758` and marks:

```text
AGZ-MAH-0009: USER_PROBE_EVIDENCE_COMPLETE
```

Observed surfaces:

- Word -> `WORD_OBJECT_MODEL`;
- modern Notepad `RichEditD2DPT` -> `RICHEDIT`;
- AnyDesk exact `Edit` -> `CLASSIC_WIN32_EDIT`;
- Chrome input/textarea -> identical Chromium UIA `Edit` surfaces with `TextPattern` + `ValuePattern`;
- Chrome contenteditable -> separate Chromium UIA `Group` with `TextPattern` only;
- Telegram Desktop 7.0.5 -> Qt UIA `Edit`, class `Ui::InputField::Inner`, with `TextPattern` + `ValuePattern`;
- WhatsApp Web in Opera -> Chromium Edit family;
- WhatsApp Desktop -> `CUSTOM_UNKNOWN` at `Microsoft.UI.Content.DesktopChildSiteBridge`, with the internal editor not reached;
- Obsidian title -> Electron UIA `Group` with `TextPattern`;
- Obsidian CodeMirror body -> separate Electron UIA `Edit` with `TextPattern` + `ValuePattern`.

The architecture consequences are mandatory:

1. **Read is not write.** Readable caret, selection, length, `TextPattern`, `ValuePattern`, or accessible editable-interface presence does not prove a safe exact-range mutation primitive.
2. **Surface identity is narrower than application identity.** Chrome input/textarea differs from contenteditable; Obsidian title differs from CodeMirror body. An application-level allow-list is invalid.
3. **Bridge evidence is not editor evidence.** WhatsApp Desktop remains blocked because the probe reached only `DesktopChildSiteBridge`, not an internal text editor.
4. **Verified write inventory is unchanged.** Only Word document `Range` and exact classic Win32 `Edit` are accepted direct collapsed-caret adapters.
5. **Whole-field replacement remains forbidden.** UIA `ValuePattern.SetValue()` cannot satisfy target-only replacement, stale-state safety, caret/selection preservation, normal application undo, editing-event, composition and post-state contracts.

Sanitized reports are stored under `docs/evidence/input-surface-probe/`. Raw JSON for Word, modern Notepad and AnyDesk was not available; only user-confirmed normalized evidence is recorded for those three surfaces.

## Historical adapter research order

The recorded order for separately authorized architecture tasks was:

1. modern RichEdit / Notepad;
2. Telegram Qt input;
3. Chromium Edit;
4. Chromium contenteditable;
5. Electron CodeMirror;
6. WhatsApp Desktop bridge remains blocked.

This order is retained as historical context. `AGZ-MAH-0011` and
`AGZ-MAH-0012` have since rejected the documented external direct paths for
modern Notepad/RichEdit and Qt respectively. Additional input-surface
feasibility work is paused during `AGZ-MAH-0013`; UIA
TextPattern/ValuePattern alone is already insufficient for Chromium and Qt.

## Modern Notepad RichEdit feasibility boundary

`AGZ-MAH-0011` completed the documented-contract gate for modern Notepad and records:

```text
DIRECT-PATH-NOT-SAFE
```

On the observed Notepad `11.2605.34.0` x64 build, the focused `RichEditD2DPT` control exposed UIA `TextPattern` and `ValuePattern`; a read-only `AccessibleObjectFromWindow(OBJID_NATIVEOM)` call returned a cross-process object supporting `ITextDocument`. This observation does not establish a supported RichEdit contract. Microsoft documents RichEdit TOM acquisition through `EM_GETOLEINTERFACE`, while the generic `OBJID_NATIVEOM` documentation does not identify RichEdit as a supported provider.

TOM can create an independent `ITextRange` and `ITextRange::SetText` can replace that range after a valid document object is acquired. The missing supported external acquisition contract is therefore decisive. In addition, one normal application-level Notepad Undo unit is not documented for this route. Version checks cannot convert an undocumented acquisition mechanism into a supported adapter.

No mutation smoke or executable harness was authorized after that gate failed. Mahou must not add an `OBJID_NATIVEOM`/TOM Notepad adapter, pointer-bearing cross-process RichEdit messages, UIA selection, whole-value writes, keyboard/clipboard paths, hooks, injection, or process-memory operations. Full evidence is in `docs/NOTEPAD-RICHEDIT-FEASIBILITY.md`.

### AutoSwitch containment on modern Notepad

`AGZ-MAH-0014` proved that the older general AutoSwitch keyboard-replay path bypassed the collapsed-caret adapter boundary. For a matched dictionary token it removed the trigger with one `SendInput`, calculated deletion from the intercepted `YuKey` list rather than current editor text, removed that many characters with another `SendInput`, optionally slept, replayed virtual keys under the target layout, and emitted any trailing space separately. The JKL route stored the replacement in `jklXHidServ.ActionOnLayout` for later execution.

That path assumed the same caret, focus, control, and committed text state throughout, but retained no source snapshot and performed no revalidation. Modern Notepad `RichEditD2DPT` provides no contract making the packets an atomic range replacement or one Undo record. The independently queued deletion/replay packets therefore explain the accepted observations: a source character can survive before the converted token, the token can be deleted without replacement, and Undo can be split.

`AGZ-MAH-0015` is containment, not support. AutoSwitch captures the foreground HWND, focused-control HWND, process ID, executable name, and control class before matching. Exact `notepad.exe` + `RichEditD2DPT`, protected classic Edit, incomplete identity, and stale identity fail closed. The same snapshot is checked before immediate Backspace, layout change, word deletion, converted-key replay, trailing-space insertion, error restoration, and inside delayed JKL or configured-delay execution. A focus/control change makes the pending action a no-op.

This guard does not create an exact-range writer. `AGZ-MAH-0019` later removes user snippets and makes AutoSwitch independent without changing the manual Insert, Smart Caps, selected-text, or clipboard architecture. Chrome and Microsoft Word remain on their existing positive AutoSwitch routes. PR #3 remains outside the accepted implementation.

Each future adapter must independently prove:

- exact supported application/build/control identity;
- ordinary editable mode and absence of protected/composition state;
- immutable foreground/focus/control identity before write;
- exact source range, boundaries, caret and collapsed selection;
- target-only direct replacement without synthetic selection, keyboard or clipboard;
- identical prefix and suffix;
- exact post-state re-read;
- preserved formatting, draft/editor state and active context;
- one normal application undo unit;
- complete fail-closed behavior on every stale or uncertain state.

## Smart Caps

Smart Caps is an optional service connected to the keyboard event stream. It tracks only a short fresh word and schedules a correction after a boundary key is committed. It lowercases accidental uppercase letters after the first letter of each apostrophe/hyphen-delimited word segment, while words typed entirely in capitals remain unchanged. Before mutation it revalidates age, foreground window, exclusion policy, exact source text, and direct-adapter availability. Unsupported or stale contexts are discarded.

Microsoft Word can independently correct two initial capitals before Mahou runs. Therefore the settings tab exposes session-only correction and reversion counters; they change only after a direct replacement performed by Mahou and contain no typed word content.

Intentional mixed-case names cannot be distinguished perfectly from accidental interior capitals. Immediate physical Backspace reverses Mahou's change, and two explicit reversions learn a local personal exception. Personal exceptions are stored in the existing INI configuration under `SmartTyping`. No typed word content is sent over the network or written to plaintext diagnostics.

## Qt Windows editable-text boundary

`AGZ-MAH-0012` completed the documented external-contract gate for Qt 5.15.19
Windows editable text, with Telegram Desktop 7.0.5 as the reference
application, and records:

```text
DIRECT-PATH-NOT-SAFE
```

Qt's Windows provider maps its accessible text interface to UI Automation
`TextPattern`/`TextPattern2`. The resulting `ITextRangeProvider` objects support
read, navigation and selection but have no text setter. Qt also maps a
`ValuePattern` provider whose write replaces the control value as a whole.
Neither route provides an independent external exact-range replacement.

Qt internally implements `QAccessibleEditableTextInterface::deleteText`,
`insertText` and `replaceText` for `QTextEdit` with `QTextCursor`. Those are
in-process C++ calls. The Windows provider does not project them through UIA
`TextEditPattern`, UIA `ObjectModelPattern`, MSAA/IAccessible2 editable text or
another documented COM interface. Reaching the internal object would require
an injected, private or otherwise undocumented route.

The historical 7.0.5 capture observed UIA `TextPattern` and `ValuePattern` but
not `TextPattern2`, while the exact Qt source maps Text2 with Text. The old
installed binary is no longer available to resolve that discrepancy. It does
not affect the decision because Text2 adds caret/annotation behavior, not
mutation.

Consequences:

- Telegram and other Qt/custom controls are not included in
  `SmartCaps.TryDirectReplace`;
- UIA selection, whole-value writes, keyboard or clipboard input, hooks,
  injection, process memory, private Qt pointers and modified application
  builds remain forbidden;
- process name, signature, Qt class, UIA read access, caret access,
  `TextPattern`, `TextPattern2`, `ValuePattern`, or internal editable-interface
  existence cannot establish a write contract;
- generic `Ui::InputField::Inner` identity cannot distinguish an ordinary
  composer from search, caption, edit-message, forward-comment, passcode or
  another mode;
- no mutation smoke or harness exists because no primitive passed the
  pre-mutation gate.

Full evidence and the rejected-path matrix are in
`docs/QT-WINDOWS-EDIT-FEASIBILITY.md`. The earlier Telegram-specific
investigation remains recorded in `docs/TELEGRAM-SMART-CAPS-ARCHITECTURE.md`.

## Release-readiness boundary

`AGZ-MAH-0013` reconciles architecture claims, accepted evidence and release
gates without changing runtime behavior. `docs/RELEASE-READINESS.md` is the
current status matrix and separates PR #2 merge gates from additional public
release gates. `docs/PR2-DESCRIPTION-PROPOSAL.md` is a metadata proposal only;
it does not change PR #2 or authorize Ready, merge, signing, tag, Release or
publication.
