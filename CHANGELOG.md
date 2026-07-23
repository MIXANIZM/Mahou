# Changelog

## [Unreleased]

### Added

- Immutable artifact names containing runtime version, platform, source commit, and GitHub Actions run ID.
- Full commit/tree build manifests, complete package SHA-256 coverage, post-upload evidence, and a fail-closed handoff verification script.
- Positive, wrong-commit, and legacy-artifact provenance regression checks.
- Exact platform/repository/runtime handoff checks plus wrong-tree, wrong-platform, byte-tamper, and schema-v1 legacy regression cases.
- Opt-in local Smart Caps settings and personal exceptions through verified direct text adapters.
- Mahou-only session counters for performed and reverted Smart Caps corrections.
- Smart Caps strings in the common English and Russian localization dictionaries.

### Changed

- Smart Caps now corrects accidental uppercase letters anywhere after the first letter of each word segment, including a third initial capital and capitals in the middle; all-caps words remain unchanged.

### Security

- Generic artifact names are forbidden; build artifacts are uploaded only after deterministic rebuild, security, manifest, checksum, and provenance checks pass.
- Build workflows use read-only permissions, disable persisted checkout credentials, and never commit or push generated evidence.
- Third-party workflow actions are pinned to full commit SHAs, with readable release-version comments.
- Smart Caps is prohibited from using synthetic selection, keyboard rewriting, clipboard mutation, or unsupported text controls.
- The status counter stores only numeric session totals and never exposes typed word content.

### Fixed

- The Smart Caps tab no longer stays in English when Mahou uses the `Русский` interface language.
- The long Smart Caps description is laid out dynamically instead of being clipped.
- Collapsed-caret `Insert` no longer uses generated blue selection.
- Modern Windows Notepad/RichEdit is contained as a safe no-op until a dedicated direct adapter exists.
