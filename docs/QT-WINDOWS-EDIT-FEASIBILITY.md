# Qt Windows editable-text write feasibility

Task: `AGZ-MAH-0012`

Date: 2026-07-28

```text
Base branch: mixanizm-modern-v2.9.0.1
Exact base: d07682a0cdf5ce5ed87ee1ecd5a2ae82b73f2fd1
Task branch: agz-mah-0012-qt-edit-feasibility
Reference application: Telegram Desktop 7.0.5 x64
Result: DIRECT-PATH-NOT-SAFE
Variant: B
```

## Decision

Qt 5.15.19 on Windows does not project its internal
`QAccessibleEditableTextInterface` as a documented external exact-range write
contract. The Windows accessibility backend projects text through UI Automation
`TextPattern`/`TextPattern2`, whose ranges are read, navigation and selection
objects, and projects a whole-control value through `ValuePattern`. It does not
project UIA `TextEditPattern`, UIA `ObjectModelPattern`, MSAA/IAccessible2
editable text, or another documented COM range writer.

The only Qt operations that can delete, insert or replace an exact text range
are in-process C++ calls on `QAccessibleEditableTextInterface` (and ultimately
the widget's `QTextCursor`). They are not marshalled to an external process by
the examined Qt Windows provider. Reaching them would require a private,
in-process, injected or otherwise undocumented route forbidden by this task.

Therefore the documented-contract gate fails before mutation:

```text
DIRECT-PATH-NOT-SAFE
```

No mutation smoke, executable harness, Mahou runtime adapter or candidate
artifact is created. Telegram and every other Qt/custom surface remain strict
no-op without a real user-created selection.

## Scope and safety boundary

This is a Qt Windows editable-text family decision, with Telegram Desktop 7.0.5
as the reference application. It is not a Telegram-only allow-list and it does
not infer write capability from process name, window class, widget ancestry,
readable text, a caret, or the presence of any one accessibility pattern.

The investigation was read-only. It did not:

- call UIA `Select`, `AddToSelection` or `RemoveFromSelection`;
- create a programmatic or keyboard selection;
- call UIA `ValuePattern.SetValue` or MSAA `put_accValue`;
- replace a whole field;
- send keyboard input or use Backspace/retype;
- read or write the clipboard;
- install hooks, access process memory, inject code, load a plugin into
  Telegram, or use a modified Telegram build;
- use Telegram shortcuts, TDLib, Bot API, Web API or private undocumented
  interfaces;
- type, edit or send any Telegram message.

## Reference identity and source mapping

### Historical installed-build evidence

The accepted `AGZ-MAH-0010` read-only record for the then-installed Telegram
Desktop 7.0.5 x64 captured:

- valid Authenticode signer `Telegram FZ-LLC`;
- top-level HWND class `Qt51519QWindowIcon`;
- focused UIA control type `Edit`;
- framework ID `Qt`;
- class name `class Ui::InputField::Inner`;
- UIA `TextPattern` and `ValuePattern`;
- no observed `TextPattern2`, IAccessible2, IAccessibleText or
  IAccessibleEditableText.

That report collected no actual text and verified no write capability.

### Current-machine limitation

At this task's start the installed process had already advanced to Telegram
Desktop `7.0.6.0`. Its signed executable remained from `Telegram FZ-LLC`, but
there was no usable top-level Telegram window in the read-only process snapshot.
The exact formerly installed 7.0.5 process therefore could not be re-probed.
The official 7.0.5 portable release below was used only for binary/source
identity research and was never launched.

### Official Telegram Desktop 7.0.5

Official tag `v7.0.5` resolves to Telegram Desktop commit
[`56bd59a32a3b304326ec5dab79de2ba71a4c6101`](https://github.com/telegramdesktop/tdesktop/commit/56bd59a32a3b304326ec5dab79de2ba71a4c6101).
At that commit:

- `Telegram/build/qt_version.py` selects Qt `5.15.19` for Windows x64;
- `Telegram/SourceFiles/core/version.h` identifies application version
  `7.0.5`;
- the `lib_ui` submodule is
  [`1b34cf474465ba4a3e55b8964d6b092c4947a29e`](https://github.com/desktop-app/lib_ui/commit/1b34cf474465ba4a3e55b8964d6b092c4947a29e);
- the build helper links Qt's Windows platform and UI Automation support
  statically.

The official `tportable-x64.7.0.5.zip` was downloaded from the release only for
read-only inspection:

| Item | Identity |
| --- | --- |
| Release ZIP SHA-256 | `6DDC877CA0AAF9AA87E7D50555AF67D414FB94597A5BA54C9A8A8A3C400729FA` |
| `Telegram.exe` SHA-256 | `9AE9D41605EA30C557DBD340E365CA956EAC3209B14D638592EEBB307AF171AD` |
| `Telegram.exe` version | `7.0.5.0` |
| PE architecture | x64 (`0x8664`) |
| Authenticode | valid, `Telegram FZ-LLC` |
| Signer certificate thumbprint | `C8CB11E5352916312801039AEDC5F9E8C78E48D8` |

The archive contains `Telegram.exe` and the application D3D compiler payload,
not separate Qt or accessibility DLLs. The executable contains the string
`5.15.19`, consistent with the source build selection and the historical
`Qt51519QWindowIcon` class. The historical evidence did not record the installed
7.0.5 executable hash, so byte identity between that old installation and the
official portable executable cannot be asserted.

### Exact Qt and Telegram widget mapping

Qt tag `v5.15.19-lts-lgpl` maps the `qtbase` submodule to
[`c68297f66533200c9ff626b056d096c78b4b94db`](https://github.com/qt/qtbase/commit/c68297f66533200c9ff626b056d096c78b4b94db).
Telegram's exact build applies the official desktop-app patch set at
[`5c6549de54d24aa11bff0968b92fae06afe5b168`](https://github.com/desktop-app/patches/commit/5c6549de54d24aa11bff0968b92fae06afe5b168).

The relevant Telegram patch changes the UIA class name to the C++ RTTI name,
which explains `class Ui::InputField::Inner`. The other accessibility patches
add toggle, selection-container and attribute behavior; none projects editable
text as an external writer.

At the exact `lib_ui` commit, `Ui::InputField::Inner` is a `QTextEdit` subclass
backed by a custom `QTextDocument`. The generic `Ui::InputField` is reused in
multiple application contexts and supports single-line, no-newline and
multi-line modes. The class name alone therefore cannot prove that a focused
element is the ordinary new-message composer rather than search, caption,
edit-message, forward-comment, passcode or another field.

## External Windows contracts

### UI Automation Text and Text2

Microsoft documents the UIA text object model as read-only for insertion and
modification: a client must use another provider-specific pattern or keyboard
input to edit. `ITextProvider` and `ITextProvider2` return
`ITextRangeProvider` ranges. Those ranges can read, compare, navigate, scroll
and manipulate selection, but have no text setter.

Primary documentation:

- [UI Automation Text and TextRange patterns overview](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-about-text-and-textrange-patterns)
- [Understanding the UI Automation text object model](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-understandingtheuiautomationtextobjectmodel)

In the exact Qt source:

- `QWindowsUiaMainProvider::GetPatternProvider` maps an accessible text
  interface to `QWindowsUiaTextProvider`;
- `QWindowsUiaTextProvider` implements `ITextProvider2`;
- `QWindowsUiaTextRangeProvider` implements `ITextRangeProvider`;
- its callable surface includes range navigation, read and selection methods,
  but no insert, delete, replace or set-text method.

`TextPattern2` adds caret/annotation capabilities, not mutation. The historical
7.0.5 report did not observe it although the exact Qt source maps Text2 beside
Text. This source/runtime discrepancy cannot be resolved after the installed
binary changed to 7.0.6, but it cannot change the decision because neither
Text nor Text2 supplies an external write operation.

Selection methods are independently forbidden for Mahou because they would
create or alter user-visible selection and still require a separate input path.

### UI Automation Value

Qt's `QWindowsUiaValueProvider::SetValue` converts the supplied BSTR to a
`QString` and calls `QAccessibleInterface::setText(QAccessible::Value, value)`.
For a `QTextEdit`, Qt's accessible widget implementation calls
`QTextEdit::setText`, replacing the control value as a whole.

Microsoft likewise defines Value as the value of the entire control:

- [UI Automation Value control pattern](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-implementingvalue)

This cannot satisfy target-only replacement, independent stale-range
validation, unchanged prefix/suffix, exact caret/collapsed-selection
preservation, Telegram entity/draft preservation, or one normal editor Undo
unit. `ValuePattern.SetValue` is forbidden.

### UI Automation TextEdit

`TextEditPattern` provides active-composition and conversion-target ranges plus
text-edit events. It does not define insert, delete or replace:

- [UI Automation TextEdit control pattern](https://learn.microsoft.com/en-us/windows/win32/winauto/textedit-control-pattern)
- [IUIAutomationTextEditPattern](https://learn.microsoft.com/en-us/windows/win32/api/uiautomationclient/nn-uiautomationclient-iuiautomationtexteditpattern)

The exact Qt 5.15.19 Windows provider has no `UIA_TextEditPatternId` mapping.
Consequently it cannot serve as either a writer or a reliable composition
fail-closed signal for this surface.

### UI Automation ObjectModel

UIA `ObjectModelPattern` can expose a provider-specific COM object when a
provider deliberately implements the pattern:

- [Implementing the Object Model control pattern](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-implementingobjectmodel)

The exact Qt Windows main provider has no `UIA_ObjectModelPatternId` mapping and
does not marshal a `QTextDocument`, `QTextCursor` or
`QAccessibleEditableTextInterface` pointer. There is no documented
provider-specific Qt COM object to query.

### MSAA

MSAA `IAccessible::put_accValue` sets the accessible object's value rather than
an independent text range. MSAA selection is object/child selection, not a
documented character-range replacement contract for this Qt widget:

- [IAccessible::put_accValue](https://learn.microsoft.com/en-us/windows/win32/api/oleacc/nf-oleacc-iaccessible-put_accvalue)
- [How selection and focus work with accessible objects](https://learn.microsoft.com/en-us/windows/win32/winauto/how-selection-and-focus-properties-work-with-accessible-objects)

It therefore reduces to the rejected whole-field path and supplies no
acceptable external range writer.

### IAccessible2

IAccessible2 defines an addressable `IAccessibleEditableText` interface,
including `replaceText`, but that is a separate provider contract:

- [IAccessibleEditableText interface](https://accessibility.linuxfoundation.org/a11yspecs/ia2/docs/html/interface_i_accessible_editable_text.html)

The historical runtime query found no IAccessible2, IAccessibleText or
IAccessibleEditableText interface. More importantly, the exact Qt 5.15.19
Windows accessibility backend examined here projects UI Automation providers
and contains no IA2 editable-text projection. The mere existence of the IA2
specification cannot make an interface available on this surface.

### Internal Qt editable text

Qt documents the C++ `QAccessibleEditableTextInterface` methods
`deleteText`, `insertText` and `replaceText`:

- [QAccessibleEditableTextInterface](https://doc.qt.io/archives/qt-5.15/qaccessibleeditabletextinterface.html)
- [Accessibility in Qt](https://doc.qt.io/archives/qt-5.15/accessible.html)

For `QTextEdit`, Qt implements those calls with an internal `QTextCursor`.
`QAccessibleTextEdit::interface_cast` can return the editable-text interface to
code already running in the same process. The Windows UIA provider never calls
`editableTextInterface()` and exposes no COM wrapper for those methods.

An external Mahou process cannot safely call that C++ object. Hooks, injection,
process memory, loading a plugin into Telegram, private pointers and modified
Telegram builds are outside the documented accessibility contract and are
forbidden.

## Required semantic guarantees

Because no allowed external range-write primitive exists, none of the
application-level mutation guarantees can be established:

| Required property | Result |
| --- | --- |
| Independent external exact range | unavailable |
| External insert/delete/replace | unavailable |
| Target-only mutation | untestable after gate failure |
| Exact caret and collapsed selection | read may be possible; write preservation is unproven |
| Unchanged prefix and suffix | whole-value path cannot meet the contract |
| Telegram entities/formatting | not preserved by any accepted path |
| Draft and active-chat identity | no accepted write transaction or composer identity contract |
| One normal Telegram Undo unit | no accepted write primitive |
| No send | no mutation was attempted |
| IME/composition fail-closed | TextEdit pattern is not provided; no accepted composition contract |
| Ordinary composer vs other modes | generic Qt class is insufficient; exact old runtime cannot be re-probed |

Version pinning, process name, signature, class name and post-write re-reading
cannot turn an absent external operation into a safe contract.

## Rejected-path matrix

| Path | Why it is rejected |
| --- | --- |
| UIA Text/Text2 | read/navigation/selection only; no text setter |
| UIA range selection plus input | selection and simulated input are forbidden |
| UIA Value | whole-field replacement |
| UIA TextEdit | composition/events only; no mutation; not mapped by Qt |
| UIA ObjectModel | not implemented by the Qt provider |
| MSAA value | whole-object value, not an independent range |
| IAccessible2 editable text | not exposed by the runtime/provider |
| Internal Qt editable interface | in-process C++ only, not externally marshalled |
| `QTextDocument`/`QTextCursor` | private in-process application state |
| Keyboard, clipboard, Backspace/retype | explicitly forbidden and cannot meet atomic range/undo guarantees |
| Hook, injection, plugin, process memory | private invasive route outside the allowed contract |
| Telegram API/shortcut/web routes | not an edit of the focused native composer and explicitly out of scope |

## Verification and artifact policy

This task changes documentation only. It must pass:

- local security regression;
- local input-surface-probe regression;
- GitHub `Security regression`;
- GitHub `Modern Windows build`;
- changed-file and static forbidden-API review.

No feasibility executable is justified because there is no accepted operation
to exercise. No Mahou runtime candidate, ZIP, executable or user mutation smoke
is produced, so no task artifact hashes apply. The release ZIP and executable
hashes above identify research inputs only and must not be presented as Mahou
artifacts.

## Reopening condition

This result may be reopened only if a future Qt Windows provider exposes a
documented external operation that directly replaces an independent text range
without UI selection, keyboard simulation, clipboard use or whole-field
replacement. A separate authorized task would then still need to prove exact
surface/mode identity, immutable focus and source state, composition safety,
target-only mutation, exact post-state, preserved entities/draft/chat, no send
and one normal application Undo unit.

Until then, the only verified Mahou direct collapsed-caret adapters remain
Microsoft Word document `Range` and the exact classic Win32 `Edit` class.
