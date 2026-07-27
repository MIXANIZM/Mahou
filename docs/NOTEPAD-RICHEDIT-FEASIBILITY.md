# Modern Notepad RichEdit exact-range write feasibility

Task: `AGZ-MAH-0011`

Decision:

```text
DIRECT-PATH-NOT-SAFE
```

## Scope

This is a Windows accessibility/interoperability API feasibility record. It does not
add a Mahou runtime adapter.

The investigation was limited to a locally launched empty, unsaved Microsoft Notepad
document. No document text was mutated. No keyboard simulation, clipboard access,
programmatic selection, whole-document replacement, remote allocation, process-memory
write, hook, injection, save, release, signing, or work from PR #3 was used.

The task started from exact development commit:

```text
779b50dcc27cbe58f69ddadd54d526a0394663df
```

That commit is the merge commit of accepted PR #12 for `AGZ-MAH-0010`.

## Decision summary

The installed Notepad editor exposes safe cross-process read access through UI
Automation `TextPattern`. It also responds at runtime to the documented generic
`AccessibleObjectFromWindow(OBJID_NATIVEOM)` mechanism with a COM object that supports
`ITextDocument`.

That runtime response is not sufficient for an accepted write path:

1. Microsoft documents `OBJID_NATIVEOM` as a generic way for a server to publish a
   custom COM object model, but does not document Microsoft RichEdit as publishing TOM
   through that mechanism.
2. The RichEdit/TOM documentation identifies `EM_GETOLEINTERFACE` as the supported way
   to obtain `ITextDocument` from a RichEdit control.
3. `EM_GETOLEINTERFACE` takes an `IRichEditOle**` pointer. It is a RichEdit private
   message at or above `WM_USER`; Windows explicitly does not marshal such messages
   between processes.
4. The installed `RichEditD2DPT` UI Automation provider does not publish the
   `ObjectModel` pattern, which would have supplied an explicitly UIA-marshalled COM
   object-model path.
5. Without a documented RichEdit-to-`OBJID_NATIVEOM` contract, the observed
   `ITextDocument` response is an implementation detail that can disappear or change.
   Version gating cannot turn an undocumented acquisition path into a supported
   external API contract.
6. TOM documents independent ranges, exact `SetText`, and undo grouping, but Microsoft
   does not document that an externally acquired range on modern Notepad creates
   exactly one normal Notepad Undo unit while preserving its UI selection and all
   application state. That requires a mutation smoke, which is not permitted once the
   acquisition contract fails.

No mutating feasibility harness was created.

## Exact installed runtime evidence

Read-only evidence was collected on 2026-07-28 from OS build `26200.8894`, x64.
Handles and process IDs are ephemeral and identify only this capture.

### Package and binaries

| Item | Observed value |
| --- | --- |
| Package identity | `Microsoft.WindowsNotepad` |
| Package full name | `Microsoft.WindowsNotepad_11.2605.34.0_x64__8wekyb3d8bbwe` |
| Package version / architecture | `11.2605.34.0` / `x64` |
| Manifest publisher | `CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US` |
| Application entry point | `Notepad\Notepad.exe`, `Windows.FullTrustApplication` |
| Notepad file version | `11.2605.34.0` |
| Notepad SHA-256 | `A84360F31E78C4AAAB51A0E5C8981C599BE022B58C7A9776B2D89E8B2527D416` |
| Notepad Authenticode | valid; Microsoft Corporation; certificate thumbprint `1D77A9B9E8FE2075D9AD15123257FB90DB0DA4A1` |
| Loaded package RichEdit | package-local `riched20.dll` |
| RichEdit file version | `16.0.20128.42275` |
| RichEdit SHA-256 | `6389CACED000B90FC0344F7CD9D84A0A2F2CA10B54CFAECBB077DC98D6D2305A` |
| RichEdit Authenticode | valid; Microsoft Corporation; certificate thumbprint `1D77A9B9E8FE2075D9AD15123257FB90DB0DA4A1` |

The package manifest was read from the installed package. No Store account, network
source, user document path, title, or document content was collected.

### HWND and UI Automation identity

| Item | Observed value |
| --- | --- |
| Notepad process ID | `2248` |
| Top-level HWND / class | `0x1708DC` / `Notepad` |
| GUI-thread active HWND | `0x1708DC` |
| Focused HWND / class | `0x2708CC` / `RichEditD2DPT` |
| Focused HWND owner | Notepad PID `2248`, thread `30684` |
| UIA control type | `ControlType.Document` |
| UIA framework | `Win32` |
| UIA class | `RichEditD2DPT` |
| UIA password state | `false` |
| Published UIA patterns | `TextPattern`, `ValuePattern` |
| Not published | `TextPattern2`, `TextEdit`, `ObjectModel` |

The interactive desktop kept another application as the global foreground window during
the automated capture. The Notepad GUI thread nevertheless reported its top-level
Notepad window as active and `RichEditD2DPT` as its focused child. A positive candidate
would have to re-establish global foreground identity immediately before every read and
write; this negative decision does not claim that gate passed.

### Read-only caret and source evidence

The empty disposable document was queried only through UI Automation:

| Evidence | Result |
| --- | --- |
| `TextPattern.DocumentRange.GetText(-1)` length | `0` |
| `TextPattern.GetSelection()` range count | `1` |
| Selection range | degenerate/collapsed |
| Caret/selection offset | `0` |
| Actual text logged | no |

Microsoft documents that a single degenerate range returned by
`IUIAutomationTextPattern::GetSelection` represents the insertion point when no text is
selected. The installed provider does not expose `TextPattern2`, so
`GetCaretRange` is unavailable; focus plus the degenerate selection supplies the
read-only caret evidence for this exact empty document.

## API investigation

### UI Automation `TextPattern`

Documented and actually reachable out of process:

- full source: `DocumentRange` plus `GetText`;
- exact-range read: clone/move endpoints plus `GetText`;
- current selection and collapsed caret: `GetSelection`;
- post-state re-read: the same operations.

The [UI Automation Text and TextRange documentation](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-about-text-and-textrange-patterns)
states that the Text pattern is read-only and does not insert or modify text.
`IUIAutomationTextRange` can change its own endpoints; its `Select` method would change
the visible selection and is forbidden.

Result: safe read primitive, no write primitive.

### UI Automation `ValuePattern`

The installed provider publishes `ValuePattern`, but
[`IUIAutomationValuePattern::SetValue`](https://learn.microsoft.com/en-us/windows/win32/api/uiautomationclient/nf-uiautomationclient-iuiautomationvaluepattern-setvalue)
sets the value of the element. It has no target range, prefix/suffix contract, or
independent caret contract.

Result: rejected whole-field replacement.

### UI Automation `TextPattern2`, `TextEdit`, and `ObjectModel`

- `TextPattern2` is not exposed, so there is no `GetCaretRange` or composition-aware
  range acquisition through that pattern.
- `TextEdit` is not exposed. In any case, its documented client-visible methods expose
  active composition and conversion-target ranges; they do not provide an exact-range
  replacement method.
- `ObjectModel` is not exposed. This matters because Microsoft documents that UI
  Automation marshals the COM pointer returned by
  [`IObjectModelProvider::GetUnderlyingObjectModel`](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-implementingobjectmodel)
  to the client process.

Result: no UIA-marshalled Notepad DOM/TOM write path.

### Direct RichEdit messages

Microsoft documents:

- `EM_GETSEL` for current selection/caret;
- `EM_GETTEXTRANGE` and `EM_GETTEXTEX` for text reads;
- `EM_GETOLEINTERFACE` for obtaining `IRichEditOle`, followed by
  `QueryInterface(ITextDocument)`.

The range and object-model messages contain caller pointers:

- [`EM_GETTEXTRANGE`](https://learn.microsoft.com/en-us/windows/win32/controls/em-gettextrange)
  takes a pointer to `TEXTRANGE`, which itself points to the output buffer;
- [`EM_GETTEXTEX`](https://learn.microsoft.com/en-us/windows/win32/controls/em-gettextex)
  takes a pointer to `GETTEXTEX` and a separate output buffer pointer;
- [`EM_GETOLEINTERFACE`](https://learn.microsoft.com/en-us/windows/win32/controls/em-getoleinterface)
  takes a pointer to an `IRichEditOle*`.

The [`SendMessage` contract](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendmessage)
states that Windows marshals only system messages below `WM_USER`; messages at or above
`WM_USER` require custom marshalling. RichEdit extended messages are not safely usable
with caller-process pointers. Remote allocation or injection to make those pointers
valid is forbidden.

`EM_SETSEL`, `EM_EXSETSEL`, and `EM_REPLACESEL` operate on the active UI selection and
are independently forbidden. `WM_SETTEXT`, `EM_SETTEXTEX`, and UIA `SetValue` rewrite
the field/document and are independently forbidden.

Result: no acceptable cross-process exact-range write path.

### TOM through `ITextDocument`

TOM itself contains the operations a hypothetical adapter would need:

- [`ITextDocument::Range`](https://learn.microsoft.com/en-us/windows/win32/api/tom/nf-tom-itextdocument-range)
  creates an `ITextRange` for specified content positions;
- `ITextSelection` is separately documented as a text range with selection
  highlighting, so a range obtained from `Range` is conceptually independent of the
  active selection;
- [`ITextRange::GetText`](https://learn.microsoft.com/en-us/windows/win32/api/tom/nf-tom-itextrange-gettext)
  reads that range;
- [`ITextRange::SetText`](https://learn.microsoft.com/en-us/windows/win32/api/tom/nf-tom-itextrange-settext)
  replaces only the range text;
- `BeginEditCollection` / `EndEditCollection` are documented as undo grouping, and
  [`ITextDocument::Undo`](https://learn.microsoft.com/en-us/windows/win32/api/tom/nf-tom-itextdocument-undo)
  operates on undo records.

The blocker is acquiring that TOM object through a documented external RichEdit
contract.

Official RichEdit documentation says to obtain `ITextDocument` through
`EM_GETOLEINTERFACE`. It does not say that RichEdit publishes TOM through
`OBJID_NATIVEOM`. The generic
[`OBJID_NATIVEOM` technique](https://learn.microsoft.com/en-us/windows/win32/winauto/using-objid-nativeom-to-expose-a-native-object-model-interface-for-a-window)
allows a server to return a custom COM object and notes that cross-process interfaces
may need COM registration. The
[`Object Identifiers` reference](https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers)
likewise permits any COM interface. Neither source identifies RichEdit or promises
`ITextDocument`.

Read-only runtime probe on the exact installed build:

```text
AccessibleObjectFromWindow(RichEditD2DPT, OBJID_NATIVEOM, IID_IUnknown)
  HRESULT: S_OK
QueryInterface(IID_ITextDocument)
  HRESULT: S_OK
```

Successful cross-process return and `QueryInterface` demonstrate that COM marshalling
works on this installed build. They do not establish a documented RichEdit contract.
The older [`AccessibleObjectFromWindow` reference](https://learn.microsoft.com/en-us/previous-versions/ms696137(v=vs.85))
documents specific Office native-object-model window classes, not RichEdit.

Result: technically present implementation behavior, but not an accepted supported
external RichEdit API.

### Text Services Framework

TSF can obtain read/write edit sessions and ranges, but the
[TSF architecture](https://learn.microsoft.com/en-us/windows/win32/tsf/architecture)
defines a text service as a COM in-process server registered with TSF. Turning Mahou
into a text service would load code into the target application process and is outside
this task and the no-injection boundary.

Result: rejected.

### WinUI/WinRT `RichEditBox.Document`

The [`RichEditBox` application API](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/rich-edit-box)
gives the owning application an `ITextDocument` through its `Document` property. It is
an in-application control API, not a documented external automation API for another
process, and modern Notepad does not expose that object through UIA `ObjectModel`.

Result: not externally reachable under the task constraints.

## Undo and atomicity conclusion

TOM's edit collection can group TOM edits, but:

- `EndEditCollection` is allowed to return `E_NOTIMPL`;
- no Microsoft Notepad contract maps an externally obtained TOM edit collection to one
  ordinary Notepad Undo command;
- no documented Notepad contract guarantees that `SetText` preserves its visible
  caret, UI selection, formatting mode, dirty state, auxiliary editor state, or IME
  state exactly as required;
- the installed provider exposes neither `TextPattern2` nor `TextEdit`, so active
  composition cannot be ruled out through the reachable UIA patterns.

The required one-step Undo, target-only atomic replacement, exact caret, collapsed
selection, no-composition state, and complete post-state verification therefore remain
unproven. A mutation was not justified.

## Acceptance-gate result

| Gate | Result |
| --- | --- |
| Exact supported Notepad package/version/signature | observed for installed build; no supported-version policy exists |
| Exact focused `RichEditD2DPT` editor | observed |
| Full source read | available through UIA `TextPattern` |
| Exact collapsed caret/selection read | available for the captured empty document |
| Independent range without UI selection | TOM defines it, but no documented external RichEdit acquisition path |
| Target-only atomic replacement | not proven |
| No whole-document rewrite | not proven by an accepted write primitive |
| Identical prefix and suffix | can be read, but no accepted write to verify |
| Exact caret after replacement | not proven |
| Selection remains collapsed | not proven |
| One normal Notepad Undo restores only replacement | not proven and not contractually guaranteed |
| Full post-state re-read | available through UIA, but no accepted write |
| Fail closed after every identity/state change | cannot make undocumented object acquisition a supported gate |
| No active composition/IME uncertainty | not provable through published patterns |
| No save or unrelated state change | maintained by this read-only investigation |

## Final state

```text
AGZ-MAH-0011: DIRECT-PATH-NOT-SAFE
```

Modern Notepad remains a strict no-op for Mahou collapsed-caret mutation. A future task
may reopen the decision only if Microsoft publishes a RichEdit-specific external
`OBJID_NATIVEOM`/TOM contract or another documented independent-range API with explicit
cross-process, selection, composition, caret, and undo semantics.
