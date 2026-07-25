# Chrome Smart Caps architecture decision

- Task: `AGZ-MAH-0006`
- Decision date: 2026-07-25
- Starting commit: `d7e0e90a149d8d792260011b5902b80770a055d7`
- Starting runtime line: `2.9.0.1-dev`
- Decision: `DIRECT-PATH-NOT-SAFE`
- Runtime implementation: none

## Scope

This investigation considered a desktop-only direct Smart Caps mutation path for ordinary Google Chrome controls:

- `input[type=text]`;
- `textarea`.

The following remain explicitly outside the supported boundary and must stay strict no-op:

- `contenteditable`;
- site editors and framework-specific editors;
- password/protected fields;
- Telegram, modern Notepad, and other Chromium applications;
- collapsed-caret `Insert` in Chrome.

No external site or user data is required. A self-contained local diagnostic page is retained at `docs/CHROME-SMART-CAPS-DIAGNOSTIC.html`.

## Evidence boundary

The exact starting head was inspected before this decision. Existing Mahou behavior is capability-gated:

- UI Automation is used only to read selection state/text;
- exact classic Win32 `Edit` uses bounded native messages for direct mutation;
- Microsoft Word uses a direct document `Range`;
- every other collapsed-caret target is a strict no-op.

This execution environment did not provide an interactive Windows desktop or a running local Chrome instance. Therefore no Chrome-version-specific UI Automation tree, DOM event log, or real undo trace is claimed here. This limitation does not permit a candidate adapter: the documented UI Automation client interfaces themselves do not expose a writable text range, so the mandatory mutation capability is absent even under the strongest plausible Chrome pattern set.

## UI Automation capability analysis

Microsoft documents `TextPattern`/`TextPattern2` as read-only access to a text store. `TextPattern2.GetCaretRange` can return a zero-length range at the caret, but that range is for navigation, text inspection, and geometry; it has no replace-text method.

Microsoft documents `ValuePattern.SetValue` as setting the value of the element. It does not address a substring or range. Microsoft also documents that multiline edit controls do not support `ValuePattern`; they use `TextPattern`, which does not set the control value.

References:

- [About the Text and TextRange Control Patterns](https://learn.microsoft.com/windows/win32/winauto/uiauto-about-text-and-textrange-patterns)
- [IUIAutomationTextPattern2::GetCaretRange](https://learn.microsoft.com/windows/win32/api/uiautomationclient/nf-uiautomationclient-iuiautomationtextpattern2-getcaretrange)
- [IUIAutomationValuePattern::SetValue](https://learn.microsoft.com/windows/win32/api/uiautomationclient/nf-uiautomationclient-iuiautomationvaluepattern-setvalue)
- [ValuePattern value remarks](https://learn.microsoft.com/dotnet/api/system.windows.automation.valuepattern.valuepatterninformation.value)

### Control matrix

| Control | Read exact text | Read collapsed caret | Replace exact range | Restore exact caret without selection | Decision |
| --- | --- | --- | --- | --- | --- |
| `input[type=text]` | Potentially, through `ValuePattern` and/or `TextPattern`, depending on the Chrome accessibility provider | Potentially, when `TextPattern2.GetCaretRange` is exposed and active | **No UI Automation client API** | Not provable after a permitted range write, because no permitted range write exists | Reject direct adapter |
| `textarea` | Potentially through `TextPattern` | Potentially through `TextPattern2.GetCaretRange` | **No UI Automation client API**; multiline controls do not provide `ValuePattern` as a write substitute | Not provable | Reject direct adapter |
| `contenteditable` | Provider-dependent | Provider-dependent | No supported writable range; outside task boundary | Not provable | Strict no-op |
| `input[type=password]` | Must not be read as ordinary text | Irrelevant | Must not write | Irrelevant | Protected strict no-op |

The word “potentially” is intentional: reading capability is provider- and version-dependent and is not evidence of safe writing capability.

## Why `ValuePattern.SetValue` is rejected

A full-value replacement cannot satisfy the task’s mandatory proof obligations:

1. **Target range:** it replaces the element value rather than a bounded word range.
2. **Adjacent text:** Mahou could compare a precomputed full string, but it cannot make the provider mutate only the target span or prove that no site-side normalization occurred.
3. **Caret:** UI Automation does not provide an atomic “replace this range and restore this collapsed caret” operation.
4. **Race safety:** a user edit between the last read and `SetValue` can be overwritten by a stale whole-field value.
5. **Selection:** restoring a caret through `TextPatternRange.Select()` would create selection state and is explicitly forbidden.
6. **Undo:** UI Automation does not promise that `SetValue` creates one normal browser editing undo unit.
7. **DOM events:** UI Automation does not promise browser `beforeinput`, `input`, or `change` events with the expected payload and ordering.
8. **Composition/IME:** a whole-field write can disrupt an active composition, and no atomic composition guard is available through these client patterns.
9. **Framework state:** a DOM value changed outside the site’s normal editing pipeline can diverge from React/Vue/other controlled state.

Because any one of these gaps is sufficient to fail the acceptance gate, `ValuePattern.SetValue` must not be used as an approximate workaround.

## Caret, undo, events, and composition findings

### Caret preservation

A readable collapsed caret may be available through `TextPattern2.GetCaretRange`, but the range is not writable. There is no permitted atomic operation that replaces a word and retains the exact caret. Using `.Select()` to move the caret is forbidden.

**Result:** not guaranteed; acceptance gate fails.

### Undo behavior

No permitted UI Automation mutation primitive can be tested for a normal Chrome undo unit. `ValuePattern.SetValue` has no documented browser undo contract.

**Result:** unproven; acceptance gate fails.

### DOM event behavior

No permitted UI Automation mutation primitive guarantees the required `beforeinput`, `input`, and `change` behavior. The diagnostic page records these events for any future browser-context implementation, but no unsafe mutation was performed merely to collect a favorable trace.

**Result:** unproven; acceptance gate fails.

### Composition and IME

`TextEditPattern` exposes information about active composition; it does not supply a client-side replace-range operation. A desktop-only adapter cannot atomically confirm and preserve the browser composition transaction.

**Result:** all composition/IME states remain fail-closed.

### Password fields

Chrome password fields must be rejected from the focused automation element’s protected/password state before any text read or mutation attempt. Unknown or inaccessible protection state is also a rejection.

**Result:** strict no-op.

## Decision

A safe direct desktop adapter for Chrome is not available through the allowed UI Automation and Win32 mechanisms.

Mahou must therefore retain the current behavior:

- Smart Caps remains no-op in Chrome;
- Chrome is not added to `SelectionProbe` direct mutation dispatch;
- no whole-field rewrite is introduced;
- no synthetic selection, keyboard selection, Backspace/retype, clipboard mutation, tracked-word fallback, CDP, JavaScript injection, extension, Native Messaging, local server, or DLL injection is introduced.

This is a successful architecture result, not an implementation candidate.

## Recommended future architecture: extension plus Native Messaging

A future separately authorized task may implement Chrome support in browser context through a narrowly scoped extension and a Native Messaging host. That architecture is recommended because only browser context can identify the actual active DOM control, observe selection/composition state, use the browser editing pipeline, and verify DOM events.

### Minimal extension components

1. A Manifest V3 extension with the minimum host permissions explicitly approved by the user.
2. A content script restricted to ordinary `input[type=text]` and `textarea` controls.
3. A service worker that brokers messages and enforces tab/frame identity.
4. A Native Messaging connection to Mahou with a small, versioned protocol.
5. No remote server, analytics, browsing-history collection, or page-content storage.

### Security model

- Default deny for every control other than exact ordinary text input/textarea.
- Reject password, protected, disabled, readonly, hidden, detached, cross-origin-inaccessible, unknown, and composition-active controls.
- Bind each candidate to exact tab, frame, document generation, element identity, source text, selection start/end, and timestamp.
- Revalidate all fields immediately before mutation.
- Require a collapsed selection and exact expected word before the delimiter.
- Apply only the target range through a browser-context editing operation that is separately proven to preserve undo and expected events.
- Verify the resulting full value, target range, selection, and adjacent text; fail closed on any mismatch.
- Exchange no surrounding page content beyond the minimum bounded context required to validate the candidate.

### Minimal message schema

Mahou to extension candidate request:

```json
{
  "version": 1,
  "requestId": "random-id",
  "createdAt": 0,
  "expectedWord": "ПРИвет",
  "replacement": "Привет",
  "maxAgeMs": 2500
}
```

Extension response:

```json
{
  "version": 1,
  "requestId": "random-id",
  "status": "applied-or-rejected",
  "reason": "bounded-enum"
}
```

The extension must derive tab, frame, document, element, full value, caret, selection, protection, and composition state locally. Those values should not be sent to Mahou unless a later threat model demonstrates a strict need.

### Installation and removal

- Installation must be an explicit user action from a reviewed package/source.
- Native Messaging registration must be per-user and removable without affecting Mahou core behavior.
- Removing or disabling either side must return Chrome Smart Caps to strict no-op.
- No background accessibility hooks or local listener services may remain after uninstall.

### Personal exceptions

Personal Smart Caps exceptions remain owned by Mahou in the existing local configuration. The extension receives only the already-approved bounded candidate/replacement pair; it does not maintain an independent exception database or sync exceptions to the cloud.

### Future test plan

A future implementation must independently prove, for both `input[type=text]` and `textarea`:

- exact active tab/frame/document/element binding;
- collapsed selection and exact caret preservation;
- exact target-only replacement with adjacent text unchanged;
- one normal browser undo action;
- expected `beforeinput`, `input`, and `change` behavior;
- safe cancellation on focus, tab, window, caret, value, document, or candidate age changes;
- strict no-op for `contenteditable`, password, readonly, disabled, unknown, composition-active, and unsupported controls;
- no activation in Edge, Electron, Telegram, modern Notepad, or another process merely because it uses Chromium;
- immediate Backspace reversal and Mahou counters exactly once;
- clean extension and Native Messaging uninstall.

### Exact boundary of the next implementation task

A later task may design and prototype only the extension/Native Messaging protocol and a local disposable test page. It must not silently modify Mahou’s current desktop mutation path, selected-text pipeline, Chrome collapsed-caret `Insert`, Telegram, Notepad, AutoSwitch, snippets, translator, runtime version, release state, or existing PRs.

## Local diagnostic page use

Open `docs/CHROME-SMART-CAPS-DIAGNOSTIC.html` directly in Chrome. It records focus, selection, `beforeinput`, `input`, `change`, and composition events without transmitting data. The page is evidence tooling only; it does not make UI Automation writable and does not authorize a runtime adapter.
