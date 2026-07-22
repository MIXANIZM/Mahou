# MIXANIZM Mahou — modernized 2.9.0.1 development line

- Project profile: `standard`
- Agatzub Development Ruleset: `v2.6.3`
- Rules content commit: `2f312c4adbb54ffd533d2877d49cfd39633460ab`

This branch is based on the latest preserved modern Mahou source lineage and keeps the
full tabbed settings UI, AutoSwitch dictionary, snippets, selection conversion,
translation panel, history and advanced layout controls.

## MIXANIZM defaults

- Caps Lock behaves as normal Windows Caps Lock.
- Windows remains responsible for ordinary layout switching.
- Insert is the shared default action for the last word or selected text.
- AutoSwitch is opt-in and uses the dictionary shipped with the build.
- Smart Caps is opt-in, local-only and corrects accidental uppercase letters inside freshly typed words only through verified direct text adapters. All-caps words are skipped; intentional mixed-case names can be kept through personal exceptions.
- The Smart Caps tab uses Mahou's common Russian/English localization and shows a session counter for corrections performed by Mahou itself.
- JKL is disabled by default until its native helpers receive a separate audit and are
  packaged intentionally.
- Settings and user data are stored in `%APPDATA%\MIXANIZM Mahou`.
- Logs are stored in `%LOCALAPPDATA%\MIXANIZM Mahou\Logs`.

## Network and privacy

The legacy self-updater and public sync/backup services are disabled. The dictionary
button restores the copy packaged with the verified build and does not download or run
an extraction script. Snippet `__execute` and its external-process launch path are
physically removed.

The translator remains an explicit opt-in feature. When enabled, selected text is sent
to the configured online translation service. Translation and speech requests use a
bounded per-operation client with an eight-second timeout and at most three redirects;
input is limited to 5000 characters and raw request URLs or responses are not written to
diagnostic output.

## User data reliability

Configuration is written through its existing temporary-file replacement path. Snippets,
the AutoSwitch dictionary, imported user files and generated default dictionaries are
also flushed to a same-directory temporary file and replaced with a `.bak` recovery copy.
High-frequency input-history updates intentionally keep their existing lightweight write
path until physical keyboard-hook performance testing is available.

## Current status

This is still a draft test branch. It requires physical Windows 11 testing of keyboard
hooks, Insert word/selection conversion, Smart Caps, AutoSwitch, snippets, modifier handling,
translator behavior and the full settings UI before merge or public release. See
`SECURITY-AUDIT-MODERN.md`.

Original Mahou is GPL v2+ software. Original authorship remains credited in the source
history and license; MIXANIZM maintains this modernization branch.
