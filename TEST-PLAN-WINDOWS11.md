# Windows 11 runtime test plan

## Insert and keyboard hooks

- Test Insert with no selection: supported direct Edit/Word targets change without touching the clipboard; unsupported controls are a complete no-op.
- Test Insert with selected text in Notepad, Word, Chrome, Telegram and Discord.
- Confirm password fields suppress conversion.
- Test fast typing, held modifiers, key repeat, Backspace, Space, Enter and application switching.

### Emergency no-synthetic-selection regression

Use a fresh disposable document and record the full text, caret, selection, keyboard layout and clipboard before every step.

- In a classic Win32 control whose exact window class is `Edit`, test `one two three four` with the caret inside and after every word. Only the direct target word may change; the prefix, suffix and separators must remain byte-for-byte identical.
- In Word, repeat the same matrix in two documents and switch documents between Insert presses. Only the active, revalidated Word Range may change.
- In modern Windows Notepad and every RichEdit-derived class (`RichEditD2DPT`, `RichEdit20W`, `RICHEDIT50W`), no-selection Insert must be a complete no-op until a dedicated verified adapter exists.
- In Chrome textarea/contenteditable, Telegram and Discord, no-selection Insert must also be a complete no-op until a direct adapter exists: no text, caret, selection, layout or clipboard change.
- Repeat Insert 20 times on one word, then on different words with mouse clicks and Left/Right/Home/End between presses. No blue selection may appear at any point.
- Test held and rapidly repeated Insert. One hotkey event may run at most one mutating direct strategy.
- Force an unsupported control, `SelectionProbe.State.Unknown`, a protected field and a direct-adapter failure. Every case must be a complete no-op.
- After every return from no-selection Insert, verify that no new selection exists and that ordinary typing and Backspace affect only the current caret position.
- With an existing user-created selection, verify that Insert still converts that selection. Then clear the selection manually before testing the no-selection path again.

## Smart typing — accidental capitals inside words

- The feature is disabled on a clean profile. Enable it on the **Smart typing / Умный ввод** tab. With the Russian Mahou interface, the complete tab, description, labels and button must be Russian and the description must not be clipped at 100–200% scaling.
- Before typing, record the numeric Mahou session counter shown on the tab. Microsoft Word can independently correct two initial capitals, so `ПРивет → Привет` by itself is not proof that Mahou acted.
- In Microsoft Word, type `окоРОчка `, `ПРИвет `, `КуРиные ` and `САнкт-ПЕтербург `; the completed words must become `окорочка `, `Привет `, `Куриные ` and `Санкт-Петербург ` without visible selection. The Mahou correction counter must increase once per Mahou replacement.
- In an exact classic Win32 `Edit` control, repeat `окоРОчка ` and `ПРИвет `; the same direct replacement and counter behavior is required.
- Type `США `, `USA `, `Привет `, `привет `, `A1b `, a mixed Cyrillic/Latin word, an email address and a URL; every value must remain byte-for-byte unchanged and the Mahou counter must not increase.
- Intentional mixed-case names such as `СДЭКом `, `iPhone `, `eBay `, `GitHub ` or `PowerShell ` may be normalized. Immediately press physical Backspace: the delimiter must be removed by the editor, the exact original casing must be restored, and the Mahou reversion counter must increase without changing adjacent text.
- Reject the same correction twice with immediate Backspace. Confirm that the normalized word appears in personal exceptions and is no longer corrected.
- Edit and clear the exception list in settings, save, restart Mahou and confirm persistence.
- Fully exit Mahou and repeat `окоРОчка `. The word must no longer change. Word may still correct its own two-initial-capital pattern, but the Mahou-only counter is unavailable because Mahou is closed.
- In modern Windows Notepad/RichEdit, Chrome, Telegram, Discord, password fields and excluded applications, Smart Caps must be a complete no-op until a dedicated verified direct adapter exists.
- Type quickly across word boundaries, switch windows between key-down and deferred correction, click the mouse, and move the caret. Stale candidates must be discarded without text, selection, caret, layout or clipboard changes.
- Run together with AutoSwitch. If AutoSwitch or Word changes the word first, Smart Caps must fail closed rather than apply to a different range or increment its counter.

## Clipboard

Before every test copy a non-text object, run conversion, then paste the original object again.

- Unicode text, HTML, RTF and Word formatting.
- PNG/image data.
- Excel cells.
- File-drop data from Explorer.
- Clipboard temporarily locked by another application.

## Windows integration

- Autorun creates only `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\MIXANIZM Mahou`.
- Restart creates no CMD/VBS files and does not invoke `taskkill`.
- Settings at 100%, 125%, 150%, 175% and 200% scaling.
- Two monitors with different scaling.
- Verify `%APPDATA%\MIXANIZM Mahou` and `%LOCALAPPDATA%\MIXANIZM Mahou\Logs`.

## Privacy and network

- Input history, translator, AutoSwitch and detailed logging are off on a clean profile.
- Legacy update and public-sync controls remain disabled.
- Enabling the translator clearly discloses that selected text is sent online.
