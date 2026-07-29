# MIXANIZM Mahou

Privacy-hardened modernization of the latest preserved Mahou `2.9.0.1-dev` source line.

> **Draft status:** binaries from this branch are test builds, not a public release. The
> project still requires physical Windows 11 keyboard/clipboard testing, an independent
> repeat audit and Authenticode signing.

This branch is based on the latest preserved modern Mahou source lineage and keeps the
tabbed settings UI, an independent AutoSwitch dictionary, selection conversion,
translation panel, history and advanced layout controls. User-defined snippets are removed.

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
to the configured online translation service. User-defined snippets and their expression
commands are absent; legacy snippet files remain untouched inactive rollback data.

## Current status

This is still a draft test branch. It requires physical Windows 11 testing of keyboard
hooks, Insert word/selection conversion, independent AutoSwitch, modifier handling and the
full settings UI before merge or public release. See `SECURITY-AUDIT-MODERN.md`.

Original Mahou is GPL v2+ software. Original authorship remains credited in the source
history and license; MIXANIZM maintains this modernization branch.

## Build verification

GitHub Actions builds x86 and x64 twice in isolated directories and rejects differing
outputs. Each artifact contains SHA-256 sums, a build manifest, CycloneDX SBOM,
security-regression report, bundled AutoSwitch dictionary and Windows 11 test plan.

## Project history and license

This branch is based on GPL v2+ Mahou by BladeMight and later preserved contributors.
The original repository documentation is retained in `UPSTREAM-README.md`; attribution
is recorded in `NOTICE.md`. MIXANIZM modifications remain under the repository license.
