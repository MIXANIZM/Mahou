# Windows 11 runtime test plan

## Insert and keyboard hooks

- Test Insert with no selection: the last word changes immediately and the clipboard is untouched.
- Test Insert with selected text in Notepad, Word, Chrome, Telegram and Discord.
- Confirm password fields suppress conversion.
- Test fast typing, held modifiers, key repeat, Backspace, Space, Enter and application switching.

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
