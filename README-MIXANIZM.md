# MIXANIZM Mahou — modernized 2.9.0.1 development line

This branch is based on the latest preserved modern Mahou source lineage and keeps the
full tabbed settings UI, AutoSwitch dictionary, snippets, selection conversion,
translation panel, history and advanced layout controls.

## MIXANIZM defaults

- Caps Lock behaves as normal Windows Caps Lock.
- Windows remains responsible for ordinary layout switching.
- Insert is the shared default action for the last word or selected text.
- AutoSwitch is opt-in and uses the dictionary shipped with the build.
- JKL is disabled by default until its native helpers receive a separate audit and are
  packaged intentionally.
- Settings and user data are stored in `%APPDATA%\MIXANIZM Mahou`.
- Logs are stored in `%LOCALAPPDATA%\MIXANIZM Mahou\Logs`.

## Network and privacy

The legacy self-updater and public sync/backup services are disabled. The dictionary
button restores the copy packaged with the verified build and does not download or run
an extraction script.

The translator remains an explicit opt-in feature. When enabled, selected text is sent
to the configured online translation service. Snippet `__execute` is blocked by default
and requires an explicit hidden setting to enable.

## Current status

This is still a draft test branch. It requires physical Windows 11 testing of keyboard
hooks, Insert word/selection conversion, AutoSwitch, snippets, modifier handling and the
full settings UI before merge or public release. See `SECURITY-AUDIT-MODERN.md`.

Original Mahou is GPL v2+ software. Original authorship remains credited in the source
history and license; MIXANIZM maintains this modernization branch.
