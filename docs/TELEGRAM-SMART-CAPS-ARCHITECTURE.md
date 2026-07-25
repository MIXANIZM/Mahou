# Telegram Desktop Smart Caps architecture investigation

Task: `AGZ-MAH-0008`

Status: `BLOCKED`

Starting commit: `076ee95325809bd0581c9e0f24e9dbb1022c6603`

Investigation branch: `agz-mah-0008-telegram-smart-caps-path`

## Decision summary

No Telegram runtime adapter or mutation candidate is added.

The acceptance gate requires evidence from the exact installed Telegram Desktop for Windows build and its ordinary new-message composer. The executor environment used for this task does not expose an interactive Windows desktop or a running Telegram process, so it cannot truthfully capture the executable identity, focused control, accessibility tree, caret, editable interfaces, active-chat stability, normal undo behavior, IME state, or message-send safety.

A positive result would therefore be unproven. A definitive `DIRECT-PATH-NOT-SAFE` result would also be unproven because the installed process might expose an interface that cannot be assessed without the real provider. The safe task result is `BLOCKED`, with Telegram remaining strict no-op.

## Existing Mahou boundary

At the starting commit:

- Smart Caps direct mutation is limited to the exact classic Win32 `Edit` adapter and Microsoft Word document `Range` adapter;
- existing user-created selection conversion is a separate verified path;
- Telegram without a user-created selection is strict no-op;
- Smart Caps itself does not use keyboard simulation, clipboard mutation, UI Automation selection, or whole-field replacement;
- unsupported or stale controls must remain unchanged.

No rejected Chrome method was transferred to Telegram.

## Runtime identity required but not available

The following values could not be measured in the executor environment and are deliberately not guessed:

- Telegram Desktop version;
- installer source: official installer, Microsoft Store, or portable;
- executable path and filename;
- file version and product name;
- digital signature;
- process architecture;
- top-level window class;
- focused window or control class;
- UI Automation tree around the ordinary new-message composer;
- UI Automation control type, automation ID, name, framework ID, class name, and supported patterns;
- MSAA role, state, and object identity;
- IAccessible2 and IAccessibleText availability;
- IAccessibleEditableText availability;
- full composer text read behavior;
- exact collapsed caret and selection offsets;
- active composition or IME state;
- normal Telegram undo behavior;
- active-chat identity and revalidation signal;
- distinction between ordinary new-message mode, edit-message mode, caption, forward comment, search, and other fields;
- message-send safety after an external accessibility mutation.

These values are mandatory evidence, not optional metadata.

## Official Telegram source reconnaissance

Only the official repository was inspected:

- repository: `telegramdesktop/tdesktop`;
- inspected source commit: `2a6fd2cb752f8b3caca9b3589b2e89d28b36f00d`;
- inspected file: `Telegram/SourceFiles/history/view/controls/history_view_compose_controls.cpp`;
- related field implementation: `Telegram/SourceFiles/chat_helpers/message_field.cpp` and `Telegram/SourceFiles/ui/widgets/fields/input_field.h`.

The inspected source identifies Telegram Desktop as a Qt application and the compose subsystem as using Telegram's `Ui::InputField` abstraction. This source observation does **not** establish the Windows accessibility provider or prove that the inspected commit matches the user's installed Telegram build. It is not used as a substitute for runtime inspection.

Official source:

- <https://github.com/telegramdesktop/tdesktop>
- <https://github.com/telegramdesktop/tdesktop/blob/2a6fd2cb752f8b3caca9b3589b2e89d28b36f00d/Telegram/SourceFiles/history/view/controls/history_view_compose_controls.cpp>
- <https://github.com/telegramdesktop/tdesktop/blob/2a6fd2cb752f8b3caca9b3589b2e89d28b36f00d/Telegram/SourceFiles/chat_helpers/message_field.cpp>

## External API assessment

### Bounded Win32 messages

No exact focused Telegram control class or documented Telegram-specific message contract was available to test. A class-name guess or reuse of the classic `Edit` adapter is forbidden. No Win32 mutation was attempted.

### UI Automation Text and TextRange

Microsoft documents the Text and TextRange control patterns as text retrieval, navigation, comparison, traversal, and selection interfaces. They do not provide a general exact range-replacement method.

`TextPatternRange.Select()` is expressly forbidden by this task because it creates or moves selection. UI Automation `ValuePattern.SetValue()` sets the element value as a whole and is also expressly forbidden.

The TextEdit control pattern exposes active and conversion composition ranges; it is not evidence of a general exact replacement primitive with Telegram undo and draft semantics.

Official references:

- <https://learn.microsoft.com/windows/win32/winauto/uiauto-about-text-and-textrange-patterns>
- <https://learn.microsoft.com/windows/win32/winauto/textedit-control-pattern>
- <https://learn.microsoft.com/windows/win32/api/uiautomationclient/nf-uiautomationclient-iuiautomationvaluepattern-setvalue>

### MSAA and IAccessible2

MSAA and IAccessible2 exposure is a runtime property of the exact focused accessible object and was not available for inspection.

The IAccessible2 specification separates read-only `IAccessibleText` from `IAccessibleEditableText`. `IAccessibleEditableText::replaceText(startOffset, endOffset, text)` is an addressable range operation, but the specification describes it as equivalent to delete followed by insert. Presence of the interface alone would not prove:

- one atomic Telegram edit transaction;
- one normal Telegram undo operation;
- preservation of formatting entities and draft state;
- exact collapsed-caret preservation;
- no selection side effect;
- active-chat stability;
- ordinary new-message mode;
- no message send;
- exact post-mutation source and neighbor verification.

Therefore the interface must be detected and behaviorally verified on the exact installed build before it can be accepted.

Official references:

- <https://accessibility.linuxfoundation.org/a11yspecs/ia2/docs/html/interface_i_accessible_text.html>
- <https://accessibility.linuxfoundation.org/a11yspecs/ia2/docs/html/interface_i_accessible_editable_text.html>

### Qt accessibility

Qt exposes accessible interfaces through `QAccessible`, including text and value interfaces, but the exact mapping to Windows UI Automation, MSAA, or IAccessible2 is platform- and provider-dependent. Qt class ancestry is not a safe activation signature for Telegram and cannot distinguish the ordinary composer from captions, search, edit-message fields, password fields, or other Qt applications.

Official references:

- <https://doc.qt.io/qt-6/qaccessible.html>
- <https://doc.qt.io/qt-6/qaccessibleinterface.html>

## Writable primitive gate

Safe writable exact range: `UNPROVEN`

Composer uniquely identifiable: `UNPROVEN`

A candidate must remain rejected until one real external API passes every item below on the exact tested Telegram version and accessibility signature:

1. Read the exact full composer text without logging it.
2. Obtain an exact collapsed caret and exact selection state.
3. Address only the intended word range.
4. Replace only that range without selecting it.
5. Avoid whole-field replacement.
6. Preserve prefix and suffix exactly.
7. Preserve the exact expected caret.
8. Produce one normal Telegram undo operation.
9. Preserve formatting entities and draft state.
10. Re-read and verify the complete result.
11. Revalidate process, executable, version, foreground window, focused element, composer signature, ordinary new-message mode, active chat, source text, caret, word boundary, delimiter, candidate age, and absence of composition immediately before mutation.
12. Verify the same composer and active chat after mutation.
13. Prove that no message was sent.
14. Fail closed on every mismatch or indeterminate state.

## Rejected methods

The following methods remain rejected without further experimentation:

- UI Automation `.Select()`;
- any programmatic selection;
- `Shift+Left` or `Ctrl+Shift+Left`;
- Backspace and retyping;
- `SendKeys`, `keybd_event`, or `SendInput`;
- clipboard mutation;
- whole-composer rewrite;
- `ValuePattern.SetValue()`;
- Telegram keyboard shortcuts as mutation transport;
- DLL or code injection;
- process-memory write;
- API hooking or UI patching;
- modified Telegram clients;
- TDLib, Bot API, Telegram Web, local servers, or remote services;
- reuse of the rejected Chrome prototypes.

## Required real-Windows investigation

A future continuation of this same task must run in Saved Messages or another dedicated private test chat with a disposable fixed test string and no Enter key.

It must capture, without logging message content:

- exact executable identity and signature;
- process and window architecture;
- foreground and focused HWND classes;
- UI Automation ancestry and children around the field;
- all supported patterns;
- MSAA and IAccessible2 interfaces;
- text length, exact caret offset, and collapsed selection;
- editable range availability;
- ordinary new-message versus edit/caption/search signatures;
- active composition state;
- active-chat identity or another reliable revalidation signal;
- mutation result, neighbors, caret, selection, undo, formatting entities, draft state, and send safety.

The investigation must stop immediately if mutation requires selection, keyboard simulation, clipboard, whole-field replacement, an unstable signature, uncertain active chat, uncertain undo, uncertain composition state, or unverifiable post-state.

## Current product behavior

Telegram Desktop Smart Caps remains unsupported and strict no-op without a real user-created selection.

No runtime version was changed. No adapter, artifact, user smoke package, merge, tag, Release, signing, or publication was produced.

## Containment

Until the required Windows evidence exists:

- do not add `TelegramSmartCapsAdapter` or equivalent runtime code;
- do not broaden the classic `Edit` adapter;
- do not activate on process name alone;
- do not treat Qt ancestry, UIA read access, caret access, or `IAccessibleEditableText` presence alone as proof of safety;
- keep Telegram outside `SmartCaps.TryDirectReplace`.
