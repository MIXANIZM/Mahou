# MIXANIZM Mahou — modernized 2.9.0.1 development line

- Project profile: `standard`
- Agatzub Development Ruleset: `v2.7.0`
- Rules content commit: `ba60623aec67c57d46bda7ce2b291a823de2d4ea`

This branch is based on the latest preserved modern Mahou source lineage and keeps the
tabbed settings UI, a standalone AutoSwitch dictionary, selection conversion,
translation panel, history and advanced layout controls.

## MIXANIZM defaults

- Caps Lock behaves as normal Windows Caps Lock.
- Windows remains responsible for ordinary layout switching.
- Insert is the shared default action for the last word or selected text.
- AutoSwitch is opt-in and uses the dictionary shipped with the build.
- User-defined snippets are removed. A clean profile does not generate `snippets.txt`; existing `snippets.txt` or `.bak` files remain untouched inactive rollback data.
- AutoSwitch is independent of all obsolete snippet settings and treats dictionary values as literal text only.
- The bundled AutoSwitch dictionary is parsed once per configuration load by a forward-only parser; malformed or incomplete dictionaries fail closed without publishing partial rules.
- Smart Caps is opt-in, local-only and corrects accidental uppercase letters inside freshly typed words only through verified direct text adapters. All-caps words are skipped; intentional mixed-case names can be kept through personal exceptions.
- The Smart Caps tab uses Mahou's common Russian/English localization and shows a session counter for corrections performed by Mahou itself.
- JKL is disabled by default until its native helpers receive a separate audit and are
  packaged intentionally.
- Settings and user data are stored in `%APPDATA%\MIXANIZM Mahou`.
- Logs are stored in `%LOCALAPPDATA%\MIXANIZM Mahou\Logs`.

## Network and privacy

The legacy self-updater and public sync/backup services are disabled. The dictionary
button restores the copy packaged with the verified build and does not download or run
an extraction script. The user-defined snippets feature, including all expression and external-process paths, is physically removed. Existing legacy snippet files are left untouched but are never read or executed.

The translator remains an explicit opt-in feature. When enabled, selected text is sent
to the configured online translation service. Translation and speech requests use a
bounded per-operation client with an eight-second timeout and at most three redirects;
input is limited to 5000 characters and raw request URLs or responses are not written to
diagnostic output.

## User data reliability

Configuration is written through its existing temporary-file replacement path. The AutoSwitch dictionary, imported user files and generated default dictionaries are also flushed to a same-directory temporary file and replaced with a `.bak` recovery copy.
High-frequency input-history updates intentionally keep their existing lightweight write
path until physical keyboard-hook performance testing is available.

## Current status

This is still a draft test branch. Selected-text conversion and Smart Caps have
accepted focused Windows evidence at their recorded immutable source commits;
that evidence is not a blanket verification of every retained feature or of the
current head. The independent AutoSwitch candidate, modifier handling, translator behavior,
input history, startup/restart, settings migration, UI scaling and the full
settings UI still require the applicable retained-feature Windows smoke before
Draft PR #2 can leave Draft.

The first independent-AutoSwitch candidate at `5527f662cb844ba90d264f5b93cb27f8334bf295`
is rejected as `USER_SMOKE_FAILED / STARTUP_HANG_HIGH_CPU`. `AGZ-MAH-0020`
replaces its quadratic startup parser and has local executable coverage, but
replacement exact-head CI and physical-Windows startup smoke are still required.

The reconciled status matrix and separate merge/public-release gates are in
`docs/RELEASE-READINESS.md`. A complete replacement body proposal for Draft PR
#2 is in `docs/PR2-DESCRIPTION-PROPOSAL.md`; it is a proposal only and does not
change PR metadata or authorize merge or release. See also
`SECURITY-AUDIT-MODERN.md`.

The current GitHub snapshot is maintained in `PROJECT_STATE.md`. Task status is kept in
`ISSUES.md`, and `UNIVERSAL-CHAT-HANDOFF.md` is the recovery handoff for the active project
supervisor. Short project-specific agent instructions are in `AGENTS.md`; the full
supervisor/executor workflow comes from the pinned central ruleset.

Original Mahou is GPL v2+ software. Original authorship remains credited in the source
history and license; MIXANIZM maintains this modernization branch.

## Test artifact provenance

Test ZIP names include the runtime version, platform, short source commit, and GitHub Actions run ID. Every archive carries its full source commit/tree manifest and SHA-256 inventory, with separate post-upload artifact evidence. A ZIP must pass the fail-closed handoff procedure in `ARTIFACT-PROVENANCE.md`; generic filenames are not valid provenance.
