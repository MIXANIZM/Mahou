# AGZ-MAH-0007 Chrome editing-core prototype

Status: `BROWSER-CONTEXT-MUTATION-NOT-SAFE`

This is a test-only Manifest V3 prototype for the autonomous local page
`docs/CHROME-SMART-CAPS-DIAGNOSTIC.html`. It is not a Chrome Smart Caps release,
does not connect to Mahou, and does not implement Native Messaging.

## What remains active

- explicit activation through the extension action for the current active tab;
- exact diagnostic-page marker verification;
- main-frame, active-document, focused-element, control, protection, selection,
  composition, age, request-ID, source-value, caret, target-boundary, prefix, and
  suffix checks;
- a 75 ms revalidation window that rejects focus, document, element, value, caret,
  selection, and composition races;
- three fixed test-only candidate pairs;
- a result panel on the diagnostic page.

The prototype deliberately performs no text mutation. The final editing core retains
post-mutation verification as a pure testable function, but no browser mutation API
is wired to it.

## Why mutation is disabled

- `setRangeText()` can express a bounded replacement and caret mode, but a script call
  does not have a documented contract for one normal Chrome undo/redo transaction or
  the trusted `beforeinput`/`input` sequence expected from real editing.
- `document.execCommand('insertText')` is deprecated and requires a temporary
  programmatic selection of the target range, which violates the no-selection gate.
- assigning `element.value` is a forbidden whole-control rewrite.
- dispatching synthetic events cannot make them trusted and does not establish a
  browser editing transaction or safe synchronization with controlled framework state.

## Permissions

The manifest requests only:

- `activeTab`
- `scripting`

It has no `host_permissions`, `<all_urls>`, `nativeMessaging`, `storage`, `webRequest`,
`debugger`, downloads, clipboard, network, or remote-code permission.

## Local diagnostic use

1. Open `chrome://extensions`, enable Developer mode, and choose **Load unpacked**.
2. Select this `prototype-extension` directory.
3. In the extension details, enable **Allow access to file URLs** so the explicit
   action can run on the local diagnostic file. This does not add a manifest host permission.
4. Open `docs/CHROME-SMART-CAPS-DIAGNOSTIC.html` directly in Chrome.
5. Focus an ordinary text input or textarea containing one fixed candidate and place
   a collapsed caret after its delimiter.
6. Click the extension action.

Expected result: the page reports `BROWSER-CONTEXT-MUTATION-NOT-SAFE` and
`mutation-api-not-accepted`; the input value, caret, selection, undo history, redo
history, and clipboard remain untouched.

The extension refuses every other page by exact marker, and refuses contenteditable,
password, readonly, disabled, hidden, detached, unknown, non-collapsed-selection, and
composition-active controls.

## Removal

Remove or disable the unpacked extension in `chrome://extensions`. No host, service,
registry entry, local server, persistent storage, or Mahou component remains.
