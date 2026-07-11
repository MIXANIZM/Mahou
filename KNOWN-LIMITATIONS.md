# Known limitations of the current draft

- The binaries are unsigned test builds; Windows SmartScreen may warn.
- Physical keyboard-hook behavior still requires Windows 11 testing on real hardware.
- The online translator sends selected text to the configured service only after explicit opt-in.
- JKL native helpers are disabled by default and are not included in the portable artifact.
- `Mahou.mm` can intentionally open local programs, files and URLs from user-authored configuration.
- The legacy self-updater and public synchronization are disabled.
- Authenticode signing and a second independent security audit remain release blockers.
