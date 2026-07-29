# Chrome Manifest V3 editing-core decision

- Task: `AGZ-MAH-0007`
- Starting commit: `faaf170d12e5bbcbd49c0b66edf4bac75c1e3049`
- Runtime line: `2.9.0.1-dev`
- Decision: `BROWSER-CONTEXT-MUTATION-NOT-SAFE`
- Native Messaging: not implemented
- Mahou runtime integration: none
- User release: none

## Scope

The task isolated only the Chrome browser-context editing core for ordinary
`input[type=text]` and `textarea` controls on the autonomous local page
`docs/CHROME-SMART-CAPS-DIAGNOSTIC.html`.

It did not implement or change:

- Mahou runtime code, Smart Caps counters, Backspace reversal, personal exceptions,
  collapsed-caret Insert, AutoSwitch, snippets, or translator behavior;
- Native Messaging or a native host;
- Telegram, modern Notepad, Edge, external websites, Chrome Web Store publication,
  a local HTTP/WebSocket server, CDP, remote debugging, DLL injection, clipboard access,
  or broad host permissions.

## Browser and environment evidence

The available browser was:

```text
Chromium 144.0.7559.96 built on Debian GNU/Linux 13 (trixie)
```

A real unpacked-extension smoke could not be run in this execution environment because
managed Chromium policy contains both:

```text
ExtensionInstallBlocklist: ["*"]
URLBlocklist: ["*"]
```

That blocks extension installation and the local `file:` diagnostic page. No prohibited
local server, remote debugging, CDP, or policy bypass was used to work around the block.
This environment limitation is recorded explicitly and is not represented as runtime
Chrome evidence.

The negative decision does not depend on a visual failure in that managed browser. It
follows from the acceptance contract of the available standard editing methods: no
allowed method simultaneously supplies exact range-only mutation, no temporary
selection, one normal browser undo/redo unit, and the required normal trusted DOM edit
pipeline.

## Prototype shape

The retained test-only extension is under:

```text
integrations/chrome-smart-caps/prototype-extension/
```

Files:

- `manifest.json`
- `service-worker.js`
- `content-script.js`
- `editing-core.js`
- `README.md`

The extension is activated only by an explicit toolbar action for the current active tab.
It injects into the main frame only, checks the exact diagnostic marker, and performs
preflight plus delayed revalidation. It deliberately performs no text mutation.

## Manifest permissions

Exactly:

```text
activeTab
scripting
```

There are no `host_permissions`, `<all_urls>`, `nativeMessaging`, `storage`, `webRequest`,
`debugger`, downloads, clipboard permissions, background network access, remote scripts,
`eval`, or `new Function`.

Chrome documents `activeTab` as temporary access granted by an explicit user gesture and
supports `chrome.scripting.executeScript()` with `activeTab` plus `scripting` without a
persistent broad host grant:

- <https://developer.chrome.com/docs/extensions/develop/concepts/activeTab>
- <https://developer.chrome.com/docs/extensions/reference/api/scripting>

## Test-only candidate source

The service worker owns only these fixed pairs:

```text
окоРОчка -> окорочка
ПРИвет -> Привет
КуРиные -> Куриные
```

Each click creates version-1 requests with a random request ID, current creation time, and
`maxAgeMs` of 2500. This is not a second Smart Caps implementation.

## Safety and stale-state revalidation

The prototype rejects before any possible mutation when any of the following is true:

- wrong page marker, inactive/stale document, another frame, another active tab, or lost focus;
- detached or changed element;
- any control other than exact `input[type=text]` or `textarea`;
- contenteditable, password, readonly, disabled, hidden, or unknown input type;
- non-collapsed selection or unreadable/invalid caret;
- active composition;
- unsupported request version, malformed or duplicate request ID, invalid age, or expired request;
- exact expected word absent, missing delimiter, invalid word boundary, or changed prefix/suffix;
- value, caret, selection, document, element, focus, or composition changes during the
  75 ms candidate/revalidation window.

The pure editing core also contains post-mutation verification for automated negative and
contract tests: exact full value, unchanged prefix/suffix, expected collapsed caret, and
same document/element. It is not connected to a mutation API.

## Mutation-method evaluation

### `HTMLInputElement.setRangeText()` / `HTMLTextAreaElement.setRangeText()`

The standard API can replace an explicitly bounded range and lets script choose how the
selection is adjusted. It is therefore useful for exact range and caret calculations.
However, its documented contract does not provide the acceptance gate required here:

- one normal Chrome undo action and one redo action;
- a normal trusted `beforeinput`/`input` editing sequence equivalent to user editing;
- framework-controlled-state synchronization without synthetic events.

The documented textarea behavior includes selection-related events, not a guarantee of the
required trusted editing transaction. Therefore exact visual replacement alone is
insufficient.

References:

- <https://developer.mozilla.org/en-US/docs/Web/API/HTMLInputElement/setRangeText>
- <https://developer.mozilla.org/en-US/docs/Web/API/HTMLTextAreaElement/setRangeText>
- <https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#dom-textarea/input-setrangetext-dev>

**Decision:** rejected.

### `document.execCommand('insertText')`

`execCommand()` can preserve an undo buffer in some browsers, but it is deprecated,
non-standard, browser-dependent, and its input-event behavior may vary. More importantly,
replacing an exact word in a focused text control requires first setting a non-collapsed
programmatic selection over that word. `setSelectionRange()` updates selection state
immediately and paints a highlight while the control is focused. That directly violates
the no-temporary-selection gate.

References:

- <https://developer.mozilla.org/en-US/docs/Web/API/Document/execCommand>
- <https://developer.mozilla.org/en-US/docs/Web/API/HTMLInputElement/setSelectionRange>

**Decision:** rejected.

### Full `.value` assignment

Assigning the complete value is a whole-control rewrite, not a proved exact range mutation.
It also bypasses the required browser editing transaction and is explicitly forbidden by
the task.

**Decision:** rejected.

### Synthetic `beforeinput` / `input` events

Calling `dispatchEvent()` produces untrusted events. `Event.isTrusted` is false for events
dispatched by script. Synthetic events therefore cannot be presented as normal browser
editing, do not manufacture a native undo transaction, and cannot safely stand in for the
state transition expected by React/Vue-like controlled inputs.

References:

- <https://developer.mozilla.org/en-US/docs/Web/API/Event/isTrusted>
- <https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/dispatchEvent>

**Decision:** rejected.

## Acceptance-gate result

| Gate | Result |
| --- | --- |
| Exact target range | Calculable, but no accepted mutation primitive |
| Adjacent text preservation | Pre/post verification implemented, mutation disabled |
| Exact caret | Calculable, but no accepted undo/event transaction |
| No visible selection | Compatible with `setRangeText`, incompatible with `execCommand` exact replacement |
| One browser undo action | Not guaranteed by allowed non-selection method |
| One browser redo action | Not guaranteed by allowed non-selection method |
| Normal DOM edit trace | Not guaranteed by `setRangeText`; variable for deprecated `execCommand` |
| No forged trusted events | Preserved by refusing synthetic events |
| Controlled framework state | Not falsely simulated |
| Composition safety | Fail-closed |
| Focus/tab/document/value/caret races | Fail-closed |
| Clipboard untouched | No clipboard API or permission exists |

Because the gates must pass simultaneously, the editing core is not accepted.

## Diagnostic page

`docs/CHROME-SMART-CAPS-DIAGNOSTIC.html` now contains the exact marker:

```text
AGZ-MAH-0007-DIAGNOSTIC-V1
```

It records ordered focus, selection, keyboard, `beforeinput`, `input`, `change`,
composition, visibility, page lifecycle, `event.isTrusted`, value, and caret/selection
snapshots. Password values are never logged. It includes ordinary input, textarea,
contenteditable, password, readonly, disabled, and hidden controls plus a dedicated
extension-result panel. It sends no data and loads no external resources.

## Automated checks

Added:

```text
python .github/scripts/chrome-extension-editing-core-regression.py
node .github/tests/chrome-extension-editing-core.test.js
```

They verify Manifest V3, exact permission whitelist, absence of broad hosts/network/remote
code/eval/Native Messaging/clipboard and active mutation APIs, exact marker, fixed
candidate source, explicit action activation, main-frame-only injection, all required
protected/stale/composition/boundary rejections, adjacent-text and post-mutation contract
checks, and the final fail-closed decision.

A dedicated read-only `Chrome extension editing core` workflow runs both checks on the
task PR. The existing `Security regression` and `Modern Windows build` workflows remain
unchanged and must also stay green for the task head.

## Installation and removal

The unpacked installation procedure is documented in the prototype README. Because the
prototype contains no mutation path, the expected result after clicking its action is a
negative diagnostic result with unchanged value, caret, selection, undo/redo history, and
clipboard.

Disabling or removing the unpacked extension removes the service worker and injected
prototype. No Native Messaging registration, Mahou component, persistent setting, local
service, or registry entry exists.

## Known limitations

- No real unpacked-extension smoke was possible in the managed execution browser.
- No accepted mutation method remains to send to a user for a positive Windows smoke.
- Real IME composition was not exercised; code and automated tests still reject active
  composition.
- This decision is limited to the specified ordinary Chrome text controls and the strict
  acceptance gate. It does not claim that script cannot visually modify a field.

## Final decision

```text
BROWSER-CONTEXT-MUTATION-NOT-SAFE
```

The task succeeds as a negative architecture result. Chrome remains unsupported for this
Smart Caps path. Native Messaging and Mahou integration were not started.
