# Changelog

## [Unreleased]

### Added

- Opt-in local Smart Caps settings and personal exceptions through verified direct text adapters.
- Mahou-only session counters for performed and reverted Smart Caps corrections.
- Smart Caps strings in the common English and Russian localization dictionaries.

### Changed

- Smart Caps now corrects accidental uppercase letters anywhere after the first letter of each word segment, including a third initial capital and capitals in the middle; all-caps words remain unchanged.

### Security

- Smart Caps is prohibited from using synthetic selection, keyboard rewriting, clipboard mutation, or unsupported text controls.
- The status counter stores only numeric session totals and never exposes typed word content.

### Fixed

- The Smart Caps tab no longer stays in English when Mahou uses the `Русский` interface language.
- The long Smart Caps description is laid out dynamically instead of being clipped.
- Collapsed-caret `Insert` no longer uses generated blue selection.
- Modern Windows Notepad/RichEdit is contained as a safe no-op until a dedicated direct adapter exists.
