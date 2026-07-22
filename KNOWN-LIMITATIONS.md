# Known limitations of the current draft

- The binaries are unsigned test builds; Windows SmartScreen may warn.
- Physical keyboard-hook behavior still requires Windows 11 testing on real hardware.
- Smart Caps is opt-in and currently operates only through the verified classic Win32 `Edit` and Microsoft Word direct-range adapters; modern Notepad/RichEdit, browsers and messengers intentionally remain no-op without user selection.
- Microsoft Word has its own option for correcting two initial capitals. That behavior can continue after Mahou is closed and must not be treated as proof that Mahou worked; use an interior-capital test and the Mahou-only session counter.
- Intentional mixed-case names such as `СДЭКом`, `iPhone`, `eBay`, `GitHub` or `PowerShell` may resemble accidental capitals. Immediate Backspace reverses the change, and two reversions learn a personal exception.
- The online translator sends selected text to the configured service only after explicit opt-in.
- JKL native helpers are disabled by default and are not included in the portable artifact.
- `Mahou.mm` can intentionally open local programs, files and URLs from user-authored configuration.
- The legacy self-updater and public synchronization are disabled.
- Authenticode signing and a second independent security audit remain release blockers.
